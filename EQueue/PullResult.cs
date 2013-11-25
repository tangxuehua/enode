using System.Collections.Generic;

namespace EQueue
{
    public class PullResult
    {
        public PullStatus PullStatus { get; set; }
        public long NextBeginOffset { get; set; }
        public long MinOffset { get; set; }
        public long MaxOffset { get; set; }
        public IEnumerable<Message> FoundMessages { get; set; }
    }
}
