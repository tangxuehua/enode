using System;

namespace ENode.Commanding
{
    /// <summary>Represents an exception when tring to get a not existing aggregate root.
    /// </summary>
    [Serializable]
    public class AggregateRootNotFoundException : Exception
    {
        private const string ExceptionMessage = "Aggregate root [type={0},id={1}] not exist.";

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="id">The aggregate root id.</param>
        /// <param name="type">The aggregate root type.</param>
        public AggregateRootNotFoundException(object id, Type type) : base(string.Format(ExceptionMessage, type.Name, id)) { }
    }
}
