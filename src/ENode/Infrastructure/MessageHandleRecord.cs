using System.Threading.Tasks;
namespace ENode.Infrastructure
{
    public class MessageHandleRecord
    {
        public string MessageId { get; set; }
        public int HandlerTypeCode { get; set; }
        public int MessageTypeCode { get; set; }
        public string AggregateRootId { get; set; }
        public int Version { get; set; }
    }
}
