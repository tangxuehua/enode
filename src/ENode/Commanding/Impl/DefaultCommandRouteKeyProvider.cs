using System;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandRouteKeyProvider : ICommandRouteKeyProvider
    {
        public string GetRouteKey(ICommand command)
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
