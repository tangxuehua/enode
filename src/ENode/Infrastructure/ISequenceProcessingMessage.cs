using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    public interface ISequenceProcessingMessage
    {
        void AddToWaitingList();
    }
}
