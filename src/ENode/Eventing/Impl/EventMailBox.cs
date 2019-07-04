using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommon.Logging;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class EventMailBox : DefaultMailBox<EventCommittingContext, bool>
    {
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<string, Byte>> _aggregateDictDict;

        public EventMailBox(string routingKey, int batchSize, Action<IList<EventCommittingContext>> handleMessageAction, ILogger logger)
            : base(routingKey, batchSize, true, null, (x =>
                  {
                      handleMessageAction(x);
                      return Task.CompletedTask;
                  }), logger)
        {
            _aggregateDictDict = new ConcurrentDictionary<string, ConcurrentDictionary<string, Byte>>();
        }

        public override void EnqueueMessage(EventCommittingContext message)
        {
            var eventDict = _aggregateDictDict.GetOrAdd(message.EventStream.AggregateRootId, x => new ConcurrentDictionary<string, byte>());
            if (eventDict.TryAdd(message.EventStream.Id, 1))
            {
                base.EnqueueMessage(message);
            }
        }
        public override async Task CompleteMessage(EventCommittingContext message, bool result)
        {
            await base.CompleteMessage(message, result);
            RemoveEventCommittingContext(message);
        }
        protected override IList<EventCommittingContext> FilterMessages(IList<EventCommittingContext> messages)
        {
            var filterCommittingContextList = new List<EventCommittingContext>();
            if (messages != null && messages.Count > 0)
            {
                foreach (var committingContext in messages)
                {
                    if (ContainsEventCommittingContext(committingContext))
                    {
                        filterCommittingContextList.Add(committingContext);
                    }
                }
            }
            return filterCommittingContextList;
        }
        public bool ContainsEventCommittingContext(EventCommittingContext eventCommittingContext)
        {
            if (_aggregateDictDict.TryGetValue(eventCommittingContext.EventStream.AggregateRootId, out ConcurrentDictionary<string, Byte> eventDict))
            {
                return eventDict.ContainsKey(eventCommittingContext.EventStream.Id);
            }
            return false;
        }
        public void RemoveAggregateAllEventCommittingContexts(string aggregateRootId)
        {
            _aggregateDictDict.TryRemove(aggregateRootId, out ConcurrentDictionary<string, Byte> removed);
        }
        public void RemoveEventCommittingContext(EventCommittingContext eventCommittingContext)
        {
            if (_aggregateDictDict.TryGetValue(eventCommittingContext.EventStream.AggregateRootId, out ConcurrentDictionary<string, Byte> eventDict))
            {
                eventDict.TryRemove(eventCommittingContext.EventStream.Id, out Byte removed);
            }
        }
    }
}