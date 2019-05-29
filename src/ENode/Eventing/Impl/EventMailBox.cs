using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommon.Logging;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl
{
    public class EventMailBox : AggregateMessageMailBox<EventCommittingContext, bool>
    {
        public EventMailBox(string aggregateRootId, int batchSize, Action<IList<EventCommittingContext>> handleMessageAction, ILogger logger)
            : base(aggregateRootId, batchSize, true, null, (x =>
                  {
                      handleMessageAction(x);
                      return Task.CompletedTask;
                  }), logger)
        {

        }
    }
}