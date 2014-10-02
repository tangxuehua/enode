namespace ENode.EQueue
{
    public enum MessageTypeCode
    {
        CommandMessage = 1,
        CommandExecutedMessage = 2,
        EventMessage = 3,
        DomainEventMessage = 4,
        DomainEventHandledMessage = 5,
        ExceptionMessage = 6,
    }
}
