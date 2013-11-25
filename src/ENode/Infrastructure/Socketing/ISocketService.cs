using System;
using System.Net.Sockets;

namespace ENode.Infrastructure.Socketing
{
    public interface ISocketService
    {
        void SendMessage(Socket targetSocket, byte[] message, Action<byte[]> messageSentCallback);
        void ReceiveMessage(Socket sourceSocket, Action<byte[]> messageReceivedCallback);
    }
}
