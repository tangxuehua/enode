
namespace ENode.Commanding.Impl
{
    public class DefaultCommandRoutingKeyProvider : ICommandRoutingKeyProvider
    {
        public string GetRoutingKey(ICommand command)
        {
            if (!string.IsNullOrEmpty(command.AggregateRootId))
            {
                return command.AggregateRootId;
            }
            return command.Id;
        }
    }
}
