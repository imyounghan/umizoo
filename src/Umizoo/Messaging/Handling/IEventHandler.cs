
namespace Umizoo.Messaging.Handling
{
    /// <summary>
    /// 表示继承该接口的是溯源事件处理程序
    /// </summary>
    public interface IEventHandler : IHandler
    { }

    /// <summary>
    /// 表示继承该接口的是一个溯源事件处理程序
    /// </summary>
    public interface IEventHandler<TEvent> : IEventHandler
        where TEvent : class, IVersionedEvent
    {
        /// <summary>
        /// 处理事件。
        /// </summary>
        void Handle(IEventContext context, TEvent @event);
    }

    ///// <summary>
    ///// 表示继承此接口的是两个溯源事件的处理器。
    ///// </summary>
    //public interface IEventHandler<TEvent1, TEvent2> : IEventHandler
    //    where TEvent1 : IEvent
    //    where TEvent2 : IEvent
    //{
    //    /// <summary>
    //    /// 处理事件。
    //    /// </summary>
    //    void Handle(IEventContext context, TEvent1 event1, TEvent2 event2);
    //}

    ///// <summary>
    ///// 表示继承此接口的是三个溯源事件的处理器。
    ///// </summary>
    //public interface IEventHandler<TEvent1, TEvent2, TEvent3> : IEventHandler
    //    where TEvent1 : IEvent
    //    where TEvent2 : IEvent
    //    where TEvent3 : IEvent
    //{
    //    /// <summary>
    //    /// 处理事件。
    //    /// </summary>
    //    void Handle(IEventContext context, TEvent1 event1, TEvent2 event2, TEvent3 event3);
    //}

    ///// <summary>
    ///// 表示继承此接口的是四个溯源事件的处理器。
    ///// </summary>
    //public interface IEventHandler<TEvent1, TEvent2, TEvent3, TEvent4> : IEventHandler
    //    where TEvent1 : IEvent
    //    where TEvent2 : IEvent
    //    where TEvent3 : IEvent
    //    where TEvent4 : IEvent
    //{
    //    /// <summary>
    //    /// 处理事件。
    //    /// </summary>
    //    void Handle(IEventContext context, TEvent1 event1, TEvent2 event2, TEvent3 event3, TEvent4 event4);
    //}

    ///// <summary>
    ///// 表示继承此接口的是五个溯源事件的处理器。
    ///// </summary>
    //public interface IEventHandler<TEvent1, TEvent2, TEvent3, TEvent4, TEvent5> : IEventHandler
    //    where TEvent1 : IEvent
    //    where TEvent2 : IEvent
    //    where TEvent3 : IEvent
    //    where TEvent4 : IEvent
    //    where TEvent5 : IEvent
    //{
    //    /// <summary>
    //    /// 处理事件。
    //    /// </summary>
    //    void Handle(IEventContext context, TEvent1 event1, TEvent2 event2, TEvent3 event3, TEvent4 event4, TEvent5 event5);
    //}
}
