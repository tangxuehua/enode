using System.Collections.Generic;

namespace ENode.Eventing
{
    public class EventAppendResult
    {
        private readonly object _lockObj = new object();
        public IList<string> SuccessAggregateRootIdList { get; set; }
        public IList<string> DuplicateEventAggregateRootIdList { get; set; }
        public IDictionary<string, IList<string>> DuplicateCommandAggregateRootIdList { get; set; }

        public EventAppendResult()
        {
            SuccessAggregateRootIdList = new List<string>();
            DuplicateEventAggregateRootIdList = new List<string>();
            DuplicateCommandAggregateRootIdList = new Dictionary<string, IList<string>>();
        }


        public void AddSuccessAggregateRootId(string aggregateRootId)
        {
            lock (_lockObj)
            {
                if (!SuccessAggregateRootIdList.Contains(aggregateRootId))
                {
                    SuccessAggregateRootIdList.Add(aggregateRootId);
                }
            }
        }
        public void AddDuplicateEventAggregateRootId(string aggregateRootId)
        {
            lock (_lockObj)
            {
                if (!DuplicateEventAggregateRootIdList.Contains(aggregateRootId))
                {
                    DuplicateEventAggregateRootIdList.Add(aggregateRootId);
                }
            }
        }
        public void AddDuplicateCommandIds(string aggregateRootId, IList<string> aggregateDuplicateCommandIdList)
        {
            lock (_lockObj)
            {
                if (!DuplicateCommandAggregateRootIdList.ContainsKey(aggregateRootId))
                {
                    DuplicateCommandAggregateRootIdList.Add(aggregateRootId, aggregateDuplicateCommandIdList);
                }
            }
        }
    }
}
