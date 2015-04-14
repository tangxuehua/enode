using System.Threading.Tasks;
using ECommon.IO;

namespace ENode.Infrastructure.Impl
{
    public class MessageHandlerProxy<T> : IMessageHandlerProxy where T : class, IMessage
    {
        private IMessageHandler<T> _handler;

        public MessageHandlerProxy(IMessageHandler<T> handler)
        {
            _handler = handler;
        }

        public Task<AsyncTaskResult> HandleAsync(IMessage message)
        {
            return _handler.HandleAsync(message as T);
        }
        public object GetInnerHandler()
        {
            return _handler;
        }
    }
}
