namespace ENode.Eventing
{
    /// <summary>Represents a event handler.
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>Handle the given event.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="evnt"></param>
        void Handle(IEventContext context, object evnt);
        /// <summary>Get the inner event handler.
        /// </summary>
        /// <returns></returns>
        object GetInnerEventHandler();
    }
    /// <summary>Represents a event handler.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<in TEvent> where TEvent : class, IEvent
    {
        /// <summary>Handle the given domain event.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="evnt"></param>
        void Handle(IEventContext context, TEvent evnt);
    }
}
