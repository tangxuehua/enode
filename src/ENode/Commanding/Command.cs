using System;
using ECommon.Utilities;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract command.
    /// </summary>
    [Serializable]
    public abstract class Command : ICommand
    {
        /// <summary>Represents the unique identifier of the command.
        /// </summary>
        public string Id { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        protected Command()
        {
            Id = ObjectId.GenerateNewStringId();
        }

        /// <summary>Returns null by default.
        /// </summary>
        /// <returns></returns>
        public virtual object GetTarget()
        {
            return null;
        }
    }
}
