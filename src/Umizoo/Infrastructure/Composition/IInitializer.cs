

namespace Umizoo.Infrastructure.Composition
{
    using System.Collections.Generic;
    using System.Reflection;

    /// <summary>
    /// 应用程序初始化接口
    /// </summary>
    public interface IInitializer
    {
        /// <summary>
        /// 当前程序初始化
        /// </summary>
        /// <param name="container">对象容器</param>
        /// <param name="assemblies">程序集集合</param>
        void Initialize(IObjectContainer container, IEnumerable<Assembly> assemblies);
    }
}
