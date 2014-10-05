using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class CommandHandlerWrapper<TCommand> : MessageHandlerWrapper<ICommandContext, TCommand, ICommand>, ICommandHandler
        where TCommand : class, ICommand
    {
        public CommandHandlerWrapper(IMessageHandler<ICommandContext, TCommand> commandHandler) : base(commandHandler)
        {
        }
    }
}
