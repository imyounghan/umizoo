// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-07.

using System.Collections.Generic;

namespace Umizoo.Infrastructure.Composition.Interception
{
    /// <summary>
    ///     拦截器的管道
    /// </summary>
    public class InterceptorPipeline
    {
        /// <summary>
        ///     一个空的拦截器的管道
        /// </summary>
        public static readonly InterceptorPipeline Empty = new InterceptorPipeline();

        private readonly IList<IInterceptor> _interceptors;

        /// <summary>
        ///     Default constructor.
        /// </summary>
        public InterceptorPipeline()
        {
            _interceptors = new List<IInterceptor>();
        }

        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        public InterceptorPipeline(IEnumerable<IInterceptor> interceptors)
        {
            _interceptors = new List<IInterceptor>(interceptors);
        }

        /// <summary>
        ///     拦截器数据
        /// </summary>
        public int Count => _interceptors.Count;

        /// <summary>
        ///     调用结果
        /// </summary>
        public IMethodReturn Invoke(IMethodInvocation input, InvokeInterceptorDelegate target)
        {
            if (Count == 0)
                return target(input, null);

            var index = 0;

            var result = _interceptors[0].Invoke(input, delegate
            {
                ++index;
                if (index < Count) return _interceptors[index].Invoke;
                return target;
            });

            return result;
        }
    }
}