namespace ENode.Commanding
{
    /// <summary>A provider which can return a routing key for the given command.
    /// </summary>
    public interface ICommandRoutingKeyProvider
    {
        /// <summary>Returns a routing key for the given command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        string GetRoutingKey(ICommand command);
    }
}
