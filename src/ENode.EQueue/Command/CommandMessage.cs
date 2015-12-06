using System;

namespace ENode.EQueue
{
    [Serializable]
    public class CommandMessage
    {
        public string CommandData { get; set; }
        public string ReplyAddress { get; set; }
    }
}
