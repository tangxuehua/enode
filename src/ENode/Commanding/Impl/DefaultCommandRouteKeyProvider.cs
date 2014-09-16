using System;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandRouteKeyProvider.
    /// </summary>
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
