using System;

namespace ENode.Commanding
{
    /// <summary>Represents an exception when tring to add a duplicated aggregate root into the command context.
    /// </summary>
    [Serializable]
    public class AggregateRootAlreadyExistException : Exception
    {
        private const string ExceptionMessage = "Aggregate root [type={0},id={1}] already exist in command context, cannot be added again.";

        /// <summary>Parameterized constructor.
        /// </summary>
        public AggregateRootAlreadyExistException(object id, Type type) : base(string.Format(ExceptionMessage, type.Name, id)) { }
    }
}
