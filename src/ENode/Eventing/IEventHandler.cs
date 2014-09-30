using ENode.Infrastructure;

namespace ENode.Eventing
{
    /// <summary>Represents an event handler.
    /// </summary>
    public interface IEventHandler : IMessageHandler
    {
    }
    /// <summary>Represents an event handler.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<in TEvent> : IMessageHandler<IEventContext, TEvent> where TEvent : class, IEvent
    {
    }
}
