using ENode.Distribute.EventStore;

namespace ENode.Eventing.Impl
{
    public class EmptyEventStoreServer : IEventStoreServer
    {
        public void Start() { }
        public void Shutdown() { }
    }
}
