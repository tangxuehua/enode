using ENode.Eventing;

namespace ENode.Commanding
{
    /// <summary>Represents a manager which manages all the completion domain event types.
    /// </summary>
    public interface ICommandCompletionEventManager
    {
        /// <summary>Check whether the given domain event represents the end of a business process.
        /// </summary>
        /// <param name="domainEvent"></param>
        /// <returns></returns>
        bool IsCompletionEvent(IDomainEvent domainEvent);
    }
}
