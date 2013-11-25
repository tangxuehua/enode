using System.Threading.Tasks;

namespace EQueue
{
    public interface IProducer
    {
        void Start();
        void Shutdown();
        Task<SendResult> Send(Message message);
    }
}
