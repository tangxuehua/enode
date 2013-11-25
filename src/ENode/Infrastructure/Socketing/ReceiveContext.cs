using System;
using System.Net.Sockets;

namespace ENode.Infrastructure.Socketing
{
    public class ReceiveContext
    {
        public Socket TargetSocket;
        public byte[] Message;
        public byte[] ReplyMessage;
        public Action<ReceiveContext> MessageProcessedCallback;
    }
}
