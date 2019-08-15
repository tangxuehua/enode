using System.Collections.Generic;

namespace ENode.Eventing
{
    public class EventAppendResult
    {
        public IList<string> SuccessAggregateRootIdList { get; set; }
        public IList<string> DuplicateEventAggregateRootIdList { get; set; }
        public IList<string> DuplicateCommandIdList { get; set; }
    }
}
