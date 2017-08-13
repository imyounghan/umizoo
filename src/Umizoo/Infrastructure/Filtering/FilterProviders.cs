// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

namespace Umizoo.Infrastructure.Filtering
{
    /// <summary>
    ///     为过滤器提供一个注册点。
    /// </summary>
    public static class FilterProviders
    {
        static FilterProviders()
        {
            Providers = new FilterProviderCollection();
        }

        /// <summary>
        ///     为筛选器提供一个注册点。
        /// </summary>
        public static FilterProviderCollection Providers { get; }
    }
}