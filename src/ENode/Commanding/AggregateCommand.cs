using System;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract aggregate command.
    /// </summary>
    [Serializable]
    public abstract class AggregateCommand<TAggregateRootId> : Command, IAggregateCommand
    {
        /// <summary>Represents the aggregate root which is related with the command.
        /// </summary>
        public TAggregateRootId AggregateRootId { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        protected AggregateCommand() : base() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        protected AggregateCommand(TAggregateRootId aggregateRootId) : base()
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            AggregateRootId = aggregateRootId;
        }

        /// <summary>Returns the aggregate root id as the command target.
        /// </summary>
        /// <returns></returns>
        public override object GetTarget()
        {
            return AggregateRootId;
        }

        string IAggregateCommand.AggregateRootId
        {
            get
            {
                if (this.AggregateRootId != null)
                {
                    return this.AggregateRootId.ToString();
                }
                return null;
            }
        }
    }
}
