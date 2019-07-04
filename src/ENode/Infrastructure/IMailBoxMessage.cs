namespace ENode.Infrastructure
{
    public interface IMailBoxMessage<TMessage, TMessageProcessResult>
        where TMessage : class, IMailBoxMessage<TMessage, TMessageProcessResult>
    {
        IMailBox<TMessage, TMessageProcessResult> MailBox { get; set; }
        long Sequence { get; set; }
    }
}
