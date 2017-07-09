

namespace Umizoo.Infrastructure.Async
{
    using System;
    using System.Threading;
    using Umizoo.Infrastructure;

    sealed class WrappedAsyncResult<TResult> : IAsyncResult
    {

        private readonly BeginInvokeDelegate _beginDelegate;
        private readonly object _beginDelegateLockObj = new object();
        private readonly EndInvokeDelegate<TResult> _endDelegate;
        private readonly SingleEntryGate _endExecutedGate = new SingleEntryGate(); // prevent End() from being called twice
        private readonly SingleEntryGate _handleCallbackGate = new SingleEntryGate(); // prevent callback from being handled multiple times
        private IAsyncResult _innerAsyncResult;
        private AsyncCallback _originalCallback;
        private readonly object _tag; // prevent an instance of this type from being passed to the wrong End() method
        private volatile bool _timedOut;
        private Timer _timer;

        public WrappedAsyncResult(BeginInvokeDelegate beginDelegate, EndInvokeDelegate<TResult> endDelegate, object tag)
        {
            _beginDelegate = beginDelegate;
            _endDelegate = endDelegate;
            _tag = tag;
        }

        public object AsyncState
        {
            get
            {
                return _innerAsyncResult.AsyncState;
            }
        }

        public WaitHandle AsyncWaitHandle
        {
            get
            {
                return _innerAsyncResult.AsyncWaitHandle;
            }
        }

        public bool CompletedSynchronously
        {
            get
            {
                return _innerAsyncResult.CompletedSynchronously;
            }
        }

        public bool IsCompleted
        {
            get
            {
                return _innerAsyncResult.IsCompleted;
            }
        }

        // kicks off the process, instantiates a timer if requested
        public void Begin(AsyncCallback callback, object state, int timeout)
        {
            _originalCallback = callback;
            bool completedSynchronously;

            // Force the target Begin() operation to complete before the callback can continue,
            // since the target operation might perform post-processing of the data.
            lock(_beginDelegateLockObj) {
                _innerAsyncResult = _beginDelegate(HandleAsynchronousCompletion, state);

                completedSynchronously = _innerAsyncResult.CompletedSynchronously;
                if(!completedSynchronously) {
                    if(timeout > Timeout.Infinite) {
                        CreateTimer(timeout);
                    }
                }
            }

            if(completedSynchronously) {
                if(callback != null) {
                    callback(this);
                }
            }
        }

        public static WrappedAsyncResult<TResult> Cast(IAsyncResult asyncResult, object tag)
        {
            if(asyncResult == null) {
                throw new ArgumentNullException("asyncResult");
            }

            WrappedAsyncResult<TResult> castResult = asyncResult as WrappedAsyncResult<TResult>;
            if(castResult != null && Object.Equals(castResult._tag, tag)) {
                return castResult;
            }
            else {
                throw new ArgumentException("The provided IAsyncResult is not valid for this method.");
            }
        }

        private void CreateTimer(int timeout)
        {
            // this method should be called within a lock(_beginDelegateLockObj)
            _timer = new Timer(HandleTimeout, null, timeout, Timeout.Infinite /* disable periodic signaling */);
        }

        public TResult End()
        {
            if(!_endExecutedGate.TryEnter()) {
                throw new InvalidOperationException("The provided IAsyncResult has already been consumed.");
            }

            if(_timedOut) {
                throw new TimeoutException();
            }
            WaitForBeginToCompleteAndDestroyTimer();

            return _endDelegate(_innerAsyncResult);
        }

        private void ExecuteAsynchronousCallback(bool timedOut)
        {
            WaitForBeginToCompleteAndDestroyTimer();

            if(_handleCallbackGate.TryEnter()) {
                _timedOut = timedOut;
                if(_originalCallback != null) {
                    _originalCallback(this);
                }
            }
        }

        private void HandleAsynchronousCompletion(IAsyncResult asyncResult)
        {
            if(asyncResult.CompletedSynchronously) {
                // If the operation completed synchronously, the WrappedAsyncResult.Begin() method will handle it.
                return;
            }

            ExecuteAsynchronousCallback(false /* timedOut */);
        }

        private void HandleTimeout(object state)
        {
            ExecuteAsynchronousCallback(true /* timedOut */);
        }

        private void WaitForBeginToCompleteAndDestroyTimer()
        {
            lock(_beginDelegateLockObj) {
                // Wait for the target Begin() method to complete, as it might be performing
                // post-processing. This also forces a memory barrier, so _innerAsyncResult
                // is guaranteed to be non-null at this point.

                if(_timer != null) {
                    _timer.Dispose();
                }
                _timer = null;
            }
        }


        public static IAsyncResult Begin(AsyncCallback callback, object state, BeginInvokeDelegate beginDelegate, EndInvokeDelegate<TResult> endDelegate)
        {
            return Begin(callback, state, beginDelegate, endDelegate, null /* tag */);
        }

        public static IAsyncResult Begin(AsyncCallback callback, object state, BeginInvokeDelegate beginDelegate, EndInvokeDelegate<TResult> endDelegate, object tag)
        {
            return Begin(callback, state, beginDelegate, endDelegate, tag, Timeout.Infinite);
        }

        public static IAsyncResult Begin(AsyncCallback callback, object state, BeginInvokeDelegate beginDelegate, EndInvokeDelegate<TResult> endDelegate, object tag, int timeout)
        {
            WrappedAsyncResult<TResult> asyncResult = new WrappedAsyncResult<TResult>(beginDelegate, endDelegate, tag);
            asyncResult.Begin(callback, state, timeout);
            return asyncResult;
        }
    }
}
