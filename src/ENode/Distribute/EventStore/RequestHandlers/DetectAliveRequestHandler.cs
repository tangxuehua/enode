using System;
using ECommon.IoC;
using ECommon.Remoting;
using ENode.Distribute.EventStore.Protocols;
using ENode.Eventing;

namespace ENode.Distribute.EventStore.RequestHandlers
{
    public class DetectAliveRequestHandler : IRequestHandler
    {
        private readonly IEventStore _eventStore;

        public DetectAliveRequestHandler()
        {
            _eventStore = ObjectContainer.Resolve<IEventStore>();
        }
        public RemotingResponse HandleRequest(IRequestHandlerContext context, RemotingRequest request)
        {
            var data = BitConverter.GetBytes(_eventStore.IsAvailable ? 1 : 0);
            return new RemotingResponse((int)ResponseCode.Success, request.Sequence, data);
        }
    }
}
