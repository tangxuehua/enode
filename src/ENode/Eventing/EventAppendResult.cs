using System.Collections.Generic;

namespace ENode.Eventing
{
    public class EventAppendResult
    {
        public IList<string> SuccessAggregateRootIdList { get; set; }
        public IList<string> DuplicateEventAggregateRootIdList { get; set; }
        public IList<string> DuplicateCommandIdList { get; set; }

        public EventAppendResult()
        {
            SuccessAggregateRootIdList = new List<string>();
            DuplicateCommandIdList = new List<string>();
            DuplicateEventAggregateRootIdList = new List<string>();
        }


        public void AddSuccessAggregateRootId(string aggregateRootId)
        {
            if (!SuccessAggregateRootIdList.Contains(aggregateRootId))
            {
                SuccessAggregateRootIdList.Add(aggregateRootId);
            }
        }
        public void AddDuplicateEventAggregateRootId(string aggregateRootId)
        {
            if (!DuplicateEventAggregateRootIdList.Contains(aggregateRootId))
            {
                DuplicateEventAggregateRootIdList.Add(aggregateRootId);
            }
        }
        public void AddDuplicateCommandId(string commandId)
        {
            if (!DuplicateCommandIdList.Contains(commandId))
            {
                DuplicateCommandIdList.Add(commandId);
            }
        }
    }
}
