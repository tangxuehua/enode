using System;

namespace ENode.Commanding.Impl
{
    /// <summary>The default implementation of ICommandRouteKeyProvider.
    /// </summary>
    public class DefaultCommandRouteKeyProvider : ICommandRouteKeyProvider
    {
        public string GetRouteKey(ICommand command)
        {
            if (string.IsNullOrEmpty(command.AggregateRootId))
            {
                return command.Id;
            }
            return command.AggregateRootId;
        }
    }
}
