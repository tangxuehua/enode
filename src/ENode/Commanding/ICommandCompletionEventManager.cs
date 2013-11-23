using ENode.Eventing;

namespace ENode.Commanding
{
    /// <summary>Represents a manager which manages all the completion domain event types.
    /// </summary>
    public interface ICommandCompletionEventManager
    {
        bool IsCompletionEvent(IDomainEvent domainEvent);
    }
}
