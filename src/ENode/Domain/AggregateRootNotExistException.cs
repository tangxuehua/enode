using System;

namespace ENode.Domain
{
    /// <summary>Represents an exception when tring to get a not existing aggregate root.
    /// </summary>
    [Serializable]
    public class AggregateRootNotExistException : Exception
    {
        private const string ExceptionMessage = "Aggregate root [type={0},id={1}] not exist.";

        /// <summary>Parameterized constructor.
        /// </summary>
        public AggregateRootNotExistException(object id, Type type) : base(string.Format(ExceptionMessage, type.Name, id)) { }
    }
}
