using System;
using System.Text;
using ECommon.IoC;
using ECommon.Remoting;
using ECommon.Serializing;
using ENode.Distribute.EventStore.Protocols;
using ENode.Eventing;

namespace ENode.Distribute.EventStore.RequestHandlers
{
    public class GetEventStreamRequestHandler : IRequestHandler
    {
        private readonly IEventStore _eventStore;
        private readonly IBinarySerializer _binarySerializer;

        public GetEventStreamRequestHandler()
        {
            _eventStore = ObjectContainer.Resolve<IEventStore>();
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
        }
        public RemotingResponse HandleRequest(IRequestHandlerContext context, RemotingRequest request)
        {
            try
            {
                var businessRequest = _binarySerializer.Deserialize<GetEventStreamRequest>(request.Body);
                var eventStream = _eventStore.GetEventStream(businessRequest.AggregateRootId, businessRequest.CommitId);
                var responseData = _binarySerializer.Serialize(eventStream);
                return new RemotingResponse((int)ResponseCode.Success, request.Sequence, responseData);
            }
            catch (Exception ex)
            {
                return new RemotingResponse((int)ResponseCode.Failed, request.Sequence, Encoding.UTF8.GetBytes(ex.Message));
            }
        }
    }
}
