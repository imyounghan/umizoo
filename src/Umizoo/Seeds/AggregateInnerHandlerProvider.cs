// Copyright © 2015 ~ 2017 Sunsoft Studio, All rights reserved.
// Umizoo is a framework can help you develop DDD and CQRS style applications.
// 
// Created by young.han with Visual Studio 2017 on 2017-08-08.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Umizoo.Infrastructure;
using Umizoo.Messaging;

namespace Umizoo.Seeds
{
    public sealed class AggregateInnerHandlerProvider
    {
        public static readonly AggregateInnerHandlerProvider Current = new AggregateInnerHandlerProvider();

        private readonly Dictionary<Type, IDictionary<Type, MethodInfo>> _innerHandlers;
        private bool initialized;

        public AggregateInnerHandlerProvider()
        {
            _innerHandlers = new Dictionary<Type, IDictionary<Type, MethodInfo>>();
        }

        private void RegisterInnerHandler(Type aggregateRootType)
        {
            var eventHandlerDic = aggregateRootType
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Where(method => method.Name.Equals("handle", StringComparison.CurrentCultureIgnoreCase) &&
                                 method.ReturnType == typeof(void) && method.GetParameters().Length == 1)
                .Select(method => new {MethodInfo = method, method.GetParameters().First().ParameterType})
                .Where(entry => typeof(IEvent).IsAssignableFrom(entry.ParameterType))
                .ToDictionary(entry => entry.ParameterType, entry => entry.MethodInfo);


            _innerHandlers.Add(aggregateRootType, eventHandlerDic);
        }

        /// <summary>
        ///     初始化聚合根内部处理器并提供缓存能力。
        /// </summary>
        public void Initialize(IEnumerable<Type> types)
        {
            if (initialized) return;

            types.Where(typeof(IAggregateRoot).IsAssignableFrom).ForEach(RegisterInnerHandler);
            initialized = true;
        }

        /// <summary>
        ///     获取聚合内部事件处理器
        /// </summary>
        public bool TryGetHandler(Type aggregateRootType, Type eventType,
            out Action<IAggregateRoot, IEvent> innerHandler)
        {
            IDictionary<Type, MethodInfo> eventHandlerDic;
            MethodInfo targetMethod;
            if (!_innerHandlers.TryGetValue(aggregateRootType, out eventHandlerDic) ||
                eventHandlerDic == null || !eventHandlerDic.TryGetValue(eventType, out targetMethod))
            {
                innerHandler = delegate { };
                return false;
            }

            innerHandler = delegate(IAggregateRoot aggregateRoot, IEvent @event)
            {
                targetMethod.Invoke(aggregateRoot, new[] {@event});
            };
            return true;
        }
    }
}