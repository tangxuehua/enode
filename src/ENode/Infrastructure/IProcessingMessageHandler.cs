using System.Threading.Tasks;

namespace ENode.Infrastructure
{
    public interface IProcessingMessageHandler<X, Y>
        where X : class, IProcessingMessage<X, Y>
        where Y : IMessage
    {
        Task HandleAsync(X processingMessage);
    }
}
