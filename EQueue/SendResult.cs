namespace EQueue
{
    public class SendResult
    {
        public SendStatus SendStatus { get; set; }
        public string MessageId { get; set; }
        public MessageQueue MessageQueue { get; set; }
        public long QueueOffset { get; set; }
    }
}
