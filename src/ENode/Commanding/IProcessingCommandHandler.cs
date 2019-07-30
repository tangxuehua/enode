using System.Threading.Tasks;

namespace ENode.Commanding
{
    public interface IProcessingCommandHandler
    {
       Task HandleAsync(ProcessingCommand processingCommand);
    }
}
