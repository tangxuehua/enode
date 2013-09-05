using System;

namespace ENode.Commanding
{
    /// <summary>Represents an exception when found a duplicated command handler of command.
    /// </summary>
    [Serializable]
    public class DuplicatedCommandHandlerException : Exception
    {
        private const string ExceptionMessage = "Found duplicated command handler {0} of {1}.";

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandType">The command type.</param>
        /// <param name="commandHandlerType">The command handler type.</param>
        public DuplicatedCommandHandlerException(Type commandType, Type commandHandlerType) : base(string.Format(ExceptionMessage, commandHandlerType.Name, commandType.Name)) { }
    }
}
