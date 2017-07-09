using System.Collections.Generic;
using System.Reflection;

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    /// 表示一个获取拦截器的接口
    /// </summary>
    public interface IInterceptorProvider
    {
        /// <summary>
        /// 获取 <paramref name="method"/> 上的拦截器
        /// </summary>
        IEnumerable<IInterceptor> GetInterceptors(MethodInfo method);
    }
}
