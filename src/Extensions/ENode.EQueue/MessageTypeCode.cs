namespace ENode.EQueue
{
    public enum MessageTypeCode
    {
        CommandMessage = 1,
        CommandExecutedMessage = 2,
        EventStreamMessage = 3,
        DomainEventStreamMessage = 4,
        DomainEventHandledMessage = 5,
        ExceptionMessage = 6,
        EventMessage = 7,
    }
}
