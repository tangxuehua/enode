using System;
using ENode.Commanding;

namespace ENode.EQueue
{
    [Serializable]
    public class CommandMessage
    {
        public byte[] CommandData { get; set; }
        public string CommandExecutedMessageTopic { get; set; }
        public string DomainEventHandledMessageTopic { get; set; }
        public string SourceEventId { get; set; }
        public string SourceExceptionId { get; set; }
    }
}
