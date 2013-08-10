namespace ENode.Commanding {
    /// <summary>Represents a command handler interface.
    /// </summary>
    public interface ICommandHandler {
        void Handle(ICommandContext context, ICommand command);
        object GetInnerCommandHandler();
    }
    /// <summary>Represents a command handler interface.
    /// </summary>
    /// <typeparam name="TCommand"></typeparam>
    public interface ICommandHandler<TCommand> where TCommand : class, ICommand {
        void Handle(ICommandContext context, TCommand command);
    }
}
