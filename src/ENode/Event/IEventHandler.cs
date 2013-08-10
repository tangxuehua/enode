namespace ENode.Eventing {
    /// <summary>Represents a event handler.
    /// </summary>
    public interface IEventHandler {
        void Handle(object evnt);
        object GetInnerEventHandler();
    }
    /// <summary>Represents a event handler.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<TEvent> where TEvent : class, IEvent {
        void Handle(TEvent evnt);
    }
}
