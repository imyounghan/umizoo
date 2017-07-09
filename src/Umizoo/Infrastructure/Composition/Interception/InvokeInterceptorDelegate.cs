
namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// 表示调用拦截器的委托
    /// </summary>
    public delegate IMethodReturn InvokeInterceptorDelegate(IMethodInvocation input, GetNextInterceptorDelegate getNext);
}
