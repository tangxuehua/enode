namespace ENode.Eventing
{
    /// <summary>Represents a event handler.
    /// </summary>
    public interface IEventHandler
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evnt"></param>
        void Handle(object evnt);
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        object GetInnerEventHandler();
    }
    /// <summary>Represents a event handler.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    public interface IEventHandler<in TEvent> where TEvent : class, IEvent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evnt"></param>
        void Handle(TEvent evnt);
    }
}
