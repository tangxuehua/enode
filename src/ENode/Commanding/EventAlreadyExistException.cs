using System;

namespace ENode.Commanding
{
    /// <summary>Represents an exception when tring to add a duplicated event into command context.
    /// </summary>
    [Serializable]
    public class EventAlreadyExistException : Exception
    {
        private const string ExceptionMessage = "Event [type={0},id={1}] already exist in command context, cannot be add twice.";

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="id">The event id.</param>
        /// <param name="type">The event type.</param>
        public EventAlreadyExistException(object id, Type type) : base(string.Format(ExceptionMessage, type.Name, id)) { }
    }
}
