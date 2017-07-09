

namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 为过滤器提供一个注册点。
    /// </summary>
    public static class FilterProviders
    {
        static FilterProviders()
        {
            Providers = new FilterProviderCollection();
            //Providers.Add(new DuplicateExecutionFilterProvider());
            Providers.Add(new HandlerInstanceFilterProvider());
            Providers.Add(new FilterAttributeFilterProvider());
            //Providers.Add(new HandlerCompletedFilterProvider());
        }

        /// <summary>
        /// 为筛选器提供一个注册点。
        /// </summary>
        public static FilterProviderCollection Providers
        {
            get;
            private set;
        }
    }
}
