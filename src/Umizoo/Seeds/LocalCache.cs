
namespace Umizoo.Seeds
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.IO;
    using System.Runtime.Caching;
    using System.Runtime.Serialization.Formatters.Binary;

    using Umizoo.Infrastructure;

    /// <summary>
    /// <see cref="ICache"/> 的本机缓存
    /// </summary>
    public class LocalCache : ICache
    {
        private readonly MemoryCache _cache;

        private readonly CacheItemPolicy _policy;

        private readonly BinaryFormatter _serializer;
        private readonly bool _enabled;

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public LocalCache()
        {
            this._serializer = new BinaryFormatter();
            this._enabled = ConfigurationManager.AppSettings["thinkcfg.caching_enabled"].ChangeIfError(false);

            this._cache = MemoryCache.Default;
            this._policy = new CacheItemPolicy()
                               {
                                   Priority = CacheItemPriority.Default,
                                   SlidingExpiration = TimeSpan.FromMinutes(5)
                               };
        }


        /// <summary>
        /// 从缓存中获取该类型的实例。
        /// </summary>
        public bool TryGet<T>(Type modelType, object modelId, out T model)
            where T : class
        {
            model = default(T);
            if (!_enabled)
                return false;

            Ensure.NotNull(modelType, "modelType");
            Ensure.NotNull(modelId, "modelId");

            if (modelType.IsAbstract || !modelType.IsClass)
                return false;

            string cacheRegion = GetCacheRegion(modelType);
            string cacheKey = BuildCacheKey(modelType, modelId);

            //if (_keys.Contains(cacheKey))
            //    return true;

            object data = _cache.Get(cacheKey, cacheRegion);
            if (data == null)
                return false;

            var de = (DictionaryEntry)data;
            if (modelId.ToString() == de.Key.ToString()) {
                using (var stream = new MemoryStream((byte[])de.Value))
                {
                    model = (T)_serializer.Deserialize(stream);
                }
                return true;
            }
            else {
                return false;
            }            
        }
        /// <summary>
        /// 设置实例到缓存
        /// </summary>
        public void Set(object model, object modelId)
        {
            if (!_enabled)
                return;

            Ensure.NotNull(modelId, "modelId");

            var type = model.GetType();

            string cacheRegion = GetCacheRegion(type);
            string cacheKey = BuildCacheKey(type, modelId);

            using(var stream = new MemoryStream())
            {
                _serializer.Serialize(stream, model);
                var data = new DictionaryEntry(modelId, stream.ToArray());
                _cache.Set(cacheKey, data, _policy, cacheRegion);
            }
        }
        /// <summary>
        /// 从缓存中移除
        /// </summary>
        public void Remove(Type modelType, object modelId)
        {
            if (!_enabled)
                return;

            Ensure.NotNull(modelType, "modelType");
            Ensure.NotNull(modelId, "modelId");

            string cacheRegion = GetCacheRegion(modelType);
            string cacheKey = BuildCacheKey(modelType, modelId);

            _cache.Remove(cacheKey, cacheRegion);
        }

        private static string GetCacheRegion(Type type)
        {
            var attr = type.GetSingleAttribute<CacheRegionAttribute>(false);
            if (attr == null)
                return CacheRegionAttribute.DefaultRegionName;

            return attr.CacheRegion;
        }
        private static string BuildCacheKey(Type type, object key)
        {
            return string.Format("Model:{0}:{1}", type.FullName, key);
        }
    }
}
