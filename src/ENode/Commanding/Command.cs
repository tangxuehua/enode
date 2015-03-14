using System;
using ENode.Infrastructure;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract command.
    /// </summary>
    [Serializable]
    public abstract class Command : Message, ICommand
    {
        /// <summary>Represents the associated aggregate root id.
        /// </summary>
        public string AggregateRootId { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public Command() : base() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        public Command(string aggregateRootId)
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            AggregateRootId = aggregateRootId;
        }

        /// <summary>Returns the aggregate root id by default.
        /// </summary>
        /// <returns></returns>
        public override string GetRoutingKey()
        {
            return AggregateRootId;
        }
    }
    /// <summary>Represents an abstract command with generic aggregate root id.
    /// </summary>
    [Serializable]
    public abstract class Command<TAggregateRootId> : Message, ICommand
    {
        /// <summary>Represents the associated aggregate root id.
        /// </summary>
        public TAggregateRootId AggregateRootId { get; set; }

        /// <summary>Default constructor.
        /// </summary>
        public Command() : base() { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        public Command(TAggregateRootId aggregateRootId)
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            AggregateRootId = aggregateRootId;
        }

        string ICommand.AggregateRootId
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

        /// <summary>Returns the aggregate root id by default.
        /// </summary>
        /// <returns></returns>
        public override string GetRoutingKey()
        {
            if (!object.Equals(AggregateRootId, default(TAggregateRootId)))
            {
                return ((ICommand)this).AggregateRootId;
            }
            return null;
        }
    }
}
