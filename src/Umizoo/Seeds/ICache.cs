

namespace Umizoo.Seeds
{
    using System;

    /// <summary>
    /// 设置或获取实体的本地缓存接口
    /// </summary>
    public interface ICache
    {
        /// <summary>
        /// 从缓存获取聚合实例
        /// </summary>
        bool TryGet<T>(Type modelType, object modelId, out T model) where T : class;

        /// <summary>
        /// 设置一个聚合实例入缓存。不存在加入缓存，存在更新缓存
        /// </summary>
        void Set(object model, object modelId);

        /// <summary>
        /// 从缓存中移除聚合根
        /// </summary>
        void Remove(Type modelType, object modelId);
    }    
}
