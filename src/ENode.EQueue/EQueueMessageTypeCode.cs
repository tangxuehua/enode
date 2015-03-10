namespace ENode.EQueue
{
    public enum EQueueMessageTypeCode
    {
        CommandMessage = 1,
        CommandExecutedMessage = 2,
        DomainEventStreamMessage = 3,
        DomainEventHandledMessage = 4,
        ExceptionMessage = 5,
    }
}
