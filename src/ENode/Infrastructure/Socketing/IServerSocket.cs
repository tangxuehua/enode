using System;

namespace ENode.Infrastructure.Socketing
{
    public interface IServerSocket
    {
        IServerSocket Listen(int backlog);
        IServerSocket Bind(string address, int port);
        IClientSocket Start(Action<ReceiveContext> messageReceivedCallback);
    }
}
