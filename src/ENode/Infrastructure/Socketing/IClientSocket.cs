using System;

namespace ENode.Infrastructure.Socketing
{
    public interface IClientSocket
    {
        IClientSocket Connect(string address, int port);
        IClientSocket Start(Action<byte[]> replyMessageReceivedCallback);
        IClientSocket Shutdown();
        IClientSocket SendMessage(byte[] message, Action<byte[]> messageSentCallback);
    }
}
