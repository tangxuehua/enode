using System;

namespace ENode.EQueue
{
    [Serializable]
    public class CommandMessage
    {
        public byte[] CommandData { get; set; }
        public string CommandExecutedMessageTopic { get; set; }
        public string DomainEventHandledMessageTopic { get; set; }
        public string SourceId { get; set; }
        public string SourceType { get; set; }
    }
}
