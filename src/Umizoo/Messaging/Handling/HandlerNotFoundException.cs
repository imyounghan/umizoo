

namespace Umizoo.Messaging.Handling
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// 表示一个当找不到消息处理程序而引发的异常
    /// </summary>
    [Serializable]
    public class HandlerNotFoundException : ApplicationException
    {
        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public HandlerNotFoundException(Type type)
            : base(string.Format("Handler not found for '{0}'.", type.FullName))
        { }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public HandlerNotFoundException(IEnumerable<Type> types)
            : base(string.Format("Event Handler not found for '{0}'.", string.Join(",", types.Select(p => p.FullName))))
        { }

        /// <summary>
        /// Parameterized constructor.
        /// </summary>
        public HandlerNotFoundException(Type aggregateRootType, Type eventType)
            : base(string.Format("Event Handler not found on '{0}' for '{1}'.", aggregateRootType.FullName, eventType.FullName))
        { }

        //public HandlerNotFoundException(string errorMessage)
        //    : base(errorMessage)
        //{ }
    }
}
