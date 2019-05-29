namespace ENode.Infrastructure
{
    public interface IAggregateMessageMailBoxMessage<TMessage, TMessageProcessResult>
        where TMessage : class, IAggregateMessageMailBoxMessage<TMessage, TMessageProcessResult>
    {
        IAggregateMessageMailBox<TMessage, TMessageProcessResult> MailBox { get; set; }
        long Sequence { get; set; }
    }
}
