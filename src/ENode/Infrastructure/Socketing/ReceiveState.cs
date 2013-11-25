using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace ENode.Infrastructure.Socketing
{
    public class ReceiveState
    {
        public Socket SourceSocket = null;
        public const int BufferSize = 1024;
        public byte[] Buffer = new byte[BufferSize];
        public List<byte> Data = new List<byte>();
        public int? MessageSize;
        public Action<byte[]> MessageReceivedCallback;
    }
}
