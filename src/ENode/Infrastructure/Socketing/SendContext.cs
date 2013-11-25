using System;
using System.Net.Sockets;

namespace ENode.Infrastructure.Socketing
{
    public class SendContext
    {
        public Socket TargetSocket;
        public byte[] Message;
        public Action<byte[]> MessageSentCallback;
    }
}
