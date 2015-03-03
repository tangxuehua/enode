using System.Threading.Tasks;

namespace ENode.Infrastructure.Impl
{
    public class DefaultProxyHandler<T> : IProxyHandler where T : class
    {
        private IHandler<T> _handler;

        public DefaultProxyHandler(IHandler<T> handler)
        {
            _handler = handler;
        }

        public Task<AsyncTaskResult> Handle(object message)
        {
            return _handler.Handle(message as T);
        }
        public object GetInnerHandler()
        {
            return _handler;
        }
    }
}
