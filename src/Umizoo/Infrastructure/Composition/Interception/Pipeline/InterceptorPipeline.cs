using System.Collections.Generic;

namespace Umizoo.Infrastructure.Composition.Interception.Pipeline
{
    /// <summary>
    /// 拦截器的管道
    /// </summary>
    public class InterceptorPipeline
    {
        /// <summary>
        /// 一个空的拦截器的管道
        /// </summary>
        public static readonly InterceptorPipeline Empty = new InterceptorPipeline();

        private readonly IList<IInterceptor> _interceptors;

        /// <summary>
        /// Default constructor.
        /// </summary>
        public InterceptorPipeline()
        {
            this._interceptors = new List<IInterceptor>();
        }
        /// <summary>
        ///  Parameterized constructor.
        /// </summary>
        public InterceptorPipeline(IEnumerable<IInterceptor> interceptors)
        {
            this._interceptors = new List<IInterceptor>(interceptors);
        }

        /// <summary>
        /// 拦截器数据
        /// </summary>
        public int Count { get { return _interceptors.Count; } }

        /// <summary>
        /// 调用结果
        /// </summary>
        public IMethodReturn Invoke(IMethodInvocation input, InvokeInterceptorDelegate target)
        {
            if (this.Count == 0)
                return target(input, null);

            int index = 0;

            IMethodReturn result = _interceptors[0].Invoke(input, delegate {
                ++index;
                if (index < this.Count) {
                    return _interceptors[index].Invoke;
                }
                else {
                    return target;
                }
            });

            return result;
        }
    }
}
