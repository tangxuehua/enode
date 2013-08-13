namespace ENode.Commanding
{
    /// <summary>Represents a command queue router to route a given command to an appropriate command queue.
    /// </summary>
    public interface ICommandQueueRouter
    {
        /// <summary>Route a given command to an appropriate command queue.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        ICommandQueue Route(ICommand command);
    }
}
