using System;

namespace ENode.Distribute.EventStore.Protocols
{
    [Serializable]
    public class GetEventStreamRequest
    {
        public string AggregateRootId { get; private set; }
        public Guid CommandId { get; private set; }

        public GetEventStreamRequest(string aggregateRootId, Guid commandId)
        {
            AggregateRootId = aggregateRootId;
            CommandId = commandId;
        }

        public override string ToString()
        {
            return string.Format("[AggregateRootId:{0}, CommandId:{1}]", AggregateRootId, CommandId);
        }
    }
}
