

namespace Umizoo.Infrastructure.Async
{
    using System;

    internal delegate void EndInvokeDelegate(IAsyncResult asyncResult);

    internal delegate TResult EndInvokeDelegate<TResult>(IAsyncResult asyncResult);
}
