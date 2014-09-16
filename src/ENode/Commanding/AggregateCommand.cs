using System;

namespace ENode.Commanding
{
    /// <summary>Represents an abstract command.
    /// </summary>
    [Serializable]
    public abstract class AggregateCommand<TAggregateRootId> : Command, IAggregateCommand
    {
        #region Private Variables

        private TAggregateRootId _aggregateRootId;
        private string _aggregateRootStringId;

        #endregion

        #region Public Properties

        /// <summary>Represents the id of aggregate root which is created or updated by the command.
        /// </summary>
        public TAggregateRootId AggregateRootId
        {
            get
            {
                return _aggregateRootId;
            }
        }
        /// <summary>Represents the id of the aggregate root, this property is only used by framework.
        /// </summary>
        string IAggregateCommand.AggregateRootId
        {
            get
            {
                if (_aggregateRootStringId == null && _aggregateRootId != null)
                {
                    _aggregateRootStringId = _aggregateRootId.ToString();
                }
                return _aggregateRootStringId;
            }
        }

        #endregion

        #region Constructors

        /// <summary>Default constructor.
        /// </summary>
        protected AggregateCommand() : this(default(TAggregateRootId)) { }
        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        protected AggregateCommand(TAggregateRootId aggregateRootId) : base()
        {
            _aggregateRootId = aggregateRootId;
            if (aggregateRootId != null)
            {
                _aggregateRootStringId = aggregateRootId.ToString();
            }
        }

        #endregion

        #region Public Methods

        /// <summary>Returns the aggregate root id as the key.
        /// </summary>
        /// <returns></returns>
        public override object GetKey()
        {
            return _aggregateRootId;
        }

        #endregion
    }
}
