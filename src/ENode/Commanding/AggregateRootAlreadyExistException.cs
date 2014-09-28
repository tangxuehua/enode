using System;

namespace ENode.Commanding
{
    /// <summary>Represents an exception when tring to add a duplicated aggregate root into command context.
    /// </summary>
    [Serializable]
    public class AggregateRootAlreadyExistException : Exception
    {
        private const string ExceptionMessage = "Aggregate Root [type={0},id={1}] already exist in command context, cannot be add twice.";

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="id">The aggregate root id.</param>
        /// <param name="type">The aggregate root type.</param>
        public AggregateRootAlreadyExistException(object id, Type type) : base(string.Format(ExceptionMessage, type.Name, id)) { }
    }
}
