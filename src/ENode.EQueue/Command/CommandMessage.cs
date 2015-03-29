using System;

namespace ENode.EQueue
{
    [Serializable]
    public class CommandMessage
    {
        public int CommandTypeCode { get; set; }
        public string CommandData { get; set; }
        public string CommandExecutedMessageTopic { get; set; }
        public string DomainEventHandledMessageTopic { get; set; }
    }
}
