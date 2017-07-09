using System;

namespace Umizoo.Infrastructure
{
    /// <summary>
    /// 表示定义缓存区域策略的特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheRegionAttribute : Attribute
    {
        /// <summary>
        /// 默认的缓存分区名称
        /// </summary>
        public const string DefaultRegionName = "ThinkCache";


        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public CacheRegionAttribute(string regionName = null)
        {
            this.CacheRegion = regionName.IfEmpty(DefaultRegionName);
        }

        /// <summary>
        /// 区域名称
        /// </summary>
        public string CacheRegion { get; set; }
    }
}
