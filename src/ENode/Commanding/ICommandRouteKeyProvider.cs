namespace ENode.Commanding
{
    /// <summary>A provider which can return a route key for the given command.
    /// </summary>
    public interface ICommandRouteKeyProvider
    {
        /// <summary>Returns a route key for the given command.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        string GetRouteKey(ICommand command);
    }
}
