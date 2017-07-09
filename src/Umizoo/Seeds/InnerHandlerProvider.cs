
namespace Umizoo.Seeds
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    using Umizoo.Messaging;

    /// <summary>
    /// 表示这是一个获取聚合根内部处理器的提供者
    /// </summary>
    public sealed class InnerHandlerProvider
    {
        /// <summary>
        /// <see cref="AggregateRootInnerHandlerProvider"/> 的一个实例
        /// </summary>
        public static readonly InnerHandlerProvider Instance = new InnerHandlerProvider();

        private readonly Dictionary<Type, IDictionary<Type, MethodInfo>> _innerHandlers;

        private InnerHandlerProvider()
        {
            this._innerHandlers = new Dictionary<Type, IDictionary<Type, MethodInfo>>();
        }

        private IDictionary<Type, MethodInfo> FindInnerHandlers(Type type)
        {
            var eventHandlerDic = new Dictionary<Type, MethodInfo>();

            var entries = from method in type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                          let returnType = method.ReturnType
                          let parameters = method.GetParameters()
                          let parameter = parameters.FirstOrDefault()
                          where method.Name.ToLower() == "handle"
                            && returnType == typeof(void)
                            && parameters.Length == 1
                            && typeof(IEvent).IsAssignableFrom(parameter.ParameterType)
                          select new { Method = method, EventType = parameter.ParameterType };
            foreach(var entry in entries) {
                if(eventHandlerDic.ContainsKey(entry.EventType)) {
                    var errorMessage = string.Format("found duplicated handler from '{0}' on '{1}'.",
                        entry.EventType.FullName, type.FullName);
                    throw new SystemException(errorMessage);
                }
                eventHandlerDic.Add(entry.EventType, entry.Method);
            }

            return eventHandlerDic;
        }

        private static bool IsAggregateRoot(Type type)
        {
            return type.IsClass && !type.IsAbstract && typeof(IAggregateRoot).IsAssignableFrom(type);
        }

        /// <summary>
        /// 初始化聚合根内部处理器并提供缓存能力。
        /// </summary>
        public void Initialize(IEnumerable<Type> types)
        {
            foreach(var type in types.Where(IsAggregateRoot)) {
                if(!type.IsSerializable) {
                    string message = string.Format("{0} should be marked as serializable.", type.FullName);
                    throw new SystemException(message);
                }

                var handlers = FindInnerHandlers(type);
                _innerHandlers.TryAdd(type, handlers);
            }
        }

        /// <summary>
        /// 获取聚合内部事件处理器
        /// </summary>
        public bool TryGetHandler(Type aggregateRootType, Type eventType, out Action<IAggregateRoot, IEvent> innerHandler)
        {
            IDictionary<Type, MethodInfo> eventHandlerDic;
            MethodInfo targetMethod;
            if(!_innerHandlers.TryGetValue(aggregateRootType, out eventHandlerDic) ||
                eventHandlerDic == null || !eventHandlerDic.TryGetValue(eventType, out targetMethod)) {
                innerHandler = delegate { };
                return false;
            }

            innerHandler = delegate(IAggregateRoot aggregateRoot, IEvent @event) {
                targetMethod.Invoke(aggregateRoot, new[] { @event });
            };
            return true;
        }
    }
}
