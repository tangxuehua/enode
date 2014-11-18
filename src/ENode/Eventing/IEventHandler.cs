using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents an event handler.
    /// </summary>
    public interface IEventHandler : IProxyHandler
    {
        /// <summary>Handle the given event.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="evnt"></param>
        void Handle(IHandlingContext context, object evnt);
    }
    /// <summary>Represents an event handler.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<in TEvent> where TEvent : class, IEvent
    {
        /// <summary>Handle the given event.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="evnt"></param>
        void Handle(IHandlingContext context, TEvent evnt);
    }
}
