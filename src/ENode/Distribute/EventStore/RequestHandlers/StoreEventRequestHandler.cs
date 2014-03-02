using System;
using System.Text;
using ECommon.IoC;
using ECommon.Remoting;
using ECommon.Serializing;
using ENode.Distribute.EventStore.Protocols;
using ENode.Eventing;

namespace ENode.Distribute.EventStore.RequestHandlers
{
    public class StoreEventRequestHandler : IRequestHandler
    {
        private readonly IEventStore _eventStore;
        private readonly IBinarySerializer _binarySerializer;

        public StoreEventRequestHandler()
        {
            _eventStore = ObjectContainer.Resolve<IEventStore>();
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }
        public RemotingResponse HandleRequest(IRequestHandlerContext context, RemotingRequest request)
        {
            try
            {
                var eventStream = _binarySerializer.Deserialize<EventByteStream>(request.Body);
                var commitStatus = _eventStore.Commit(eventStream);
                var data = BitConverter.GetBytes((int)commitStatus);
                return new RemotingResponse((int)ResponseCode.Success, request.Sequence, data);
            }
            catch (Exception ex)
            {
                return new RemotingResponse((int)ResponseCode.Failed, request.Sequence, Encoding.UTF8.GetBytes(ex.Message));
            }
        }
    }
}
