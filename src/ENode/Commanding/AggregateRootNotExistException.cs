using System;

namespace ENode.Commanding
{
    /// <summary>Represents an exception when tring to get a not existing aggregate root.
    /// </summary>
    [Serializable]
    public class AggregateRootNotExistException : Exception
    {
        private const string ExceptionMessage = "Aggregate Root [type={0},id={1}] not exist.";

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="id">The aggregate root id.</param>
        /// <param name="type">The aggregate root type.</param>
        public AggregateRootNotExistException(object id, Type type) : base(string.Format(ExceptionMessage, type.Name, id)) { }
    }
}
