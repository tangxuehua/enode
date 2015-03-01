
namespace ENode.Commanding.Impl
{
    public class DefaultCommandRoutingKeyProvider : ICommandRoutingKeyProvider
    {
        public string GetRoutingKey(ICommand command)
        {
            var aggregateCommand = command as IAggregateCommand;
            if (aggregateCommand != null && !string.IsNullOrEmpty(aggregateCommand.AggregateRootId))
            {
                return aggregateCommand.AggregateRootId;
            }
            return command.Id;
        }
    }
}
