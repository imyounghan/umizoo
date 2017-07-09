
namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// 表现一个拦截器的接口
    /// </summary>
    public interface IInterceptor
    {
        /// <summary>
        /// 调用结果
        /// </summary>
        IMethodReturn Invoke(IMethodInvocation input, GetNextInterceptorDelegate getNext);
    }
}
