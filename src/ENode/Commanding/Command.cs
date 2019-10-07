using System;
using System.Collections.Generic;
using ENode.Messaging;

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
        public Command(string aggregateRootId) : this(aggregateRootId, null)
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="items"></param>
        public Command(string aggregateRootId, IDictionary<string, string> items)
        {
            AggregateRootId = aggregateRootId ?? throw new ArgumentNullException("aggregateRootId");
            Items = items;
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
        public Command(TAggregateRootId aggregateRootId) : this(aggregateRootId, null)
        {
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="items"></param>
        public Command(TAggregateRootId aggregateRootId, IDictionary<string, string> items)
        {
            if (aggregateRootId == null)
            {
                throw new ArgumentNullException("aggregateRootId");
            }
            AggregateRootId = aggregateRootId;
            Items = items;
        }

        string ICommand.AggregateRootId
        {
            get
            {
                if (AggregateRootId != null)
                {
                    return AggregateRootId.ToString();
                }
                return null;
            }
        }
    }
}
