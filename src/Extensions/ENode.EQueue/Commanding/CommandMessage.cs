using System;
using ENode.Commanding;

namespace ENode.EQueue
{
    [Serializable]
    public class CommandMessage
    {
        public byte[] CommandData { get; set; }
        public string CommandResultTopic { get; set; }
    }
}
