using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class CommandHandlerWrapper<TCommand> : MessageHandlerWrapper<ICommandContext, TCommand, ICommand>, ICommandHandler
        where TCommand : class, ICommand
    {
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandHandler"></param>
        public CommandHandlerWrapper(IMessageHandler<ICommandContext, TCommand> commandHandler) : base(commandHandler)
        {
        }
    }
}
