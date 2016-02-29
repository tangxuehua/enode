using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ENode.Eventing.Impl
{
    public class EventMailBox
    {
        private readonly string _aggregateRootId;
        private readonly IList<EventCommittingContext> _committingContexts;
        private readonly ConcurrentQueue<EventCommittingContext> _messageQueue;
        private readonly Action<EventMailBox> _handleMessageAction;
        private int _isHandlingMessage;
        private int _batchSize;

        public string AggregateRootId
        {
            get
            {
                return _aggregateRootId;
            }
        }
        public IList<EventCommittingContext> CommittingContexts
        {
            get
            {
                return _committingContexts;
            }
        }

        public EventMailBox(string aggregateRootId, int batchSize, Action<EventMailBox> handleMessageAction)
        {
            _aggregateRootId = aggregateRootId;
            _committingContexts = new List<EventCommittingContext>();
            _messageQueue = new ConcurrentQueue<EventCommittingContext>();
            _batchSize = batchSize;
            _handleMessageAction = handleMessageAction;
        }

        public void EnqueueMessage(EventCommittingContext message)
        {
            _messageQueue.Enqueue(message);
            RegisterForExecution();
        }
        public void Clear()
        {
            EventCommittingContext message;
            while (_messageQueue.TryDequeue(out message)) { }
            _committingContexts.Clear();
        }
        public void Run()
        {
            _committingContexts.Clear();
            try
            {
                EventCommittingContext context = null;
                while (_messageQueue.TryDequeue(out context))
                {
                    context.EventMailBox = this;
                    _committingContexts.Add(context);

                    if (_committingContexts.Count == _batchSize)
                    {
                        break;
                    }
                }
                if (_committingContexts.Count > 0)
                {
                    _handleMessageAction(this);
                }
            }
            finally
            {
                if (_committingContexts.Count == 0)
                {
                    ExitHandlingMessage();
                    if (!_messageQueue.IsEmpty)
                    {
                        RegisterForExecution();
                    }
                }
            }
        }
        public bool EnterHandlingMessage()
        {
            return Interlocked.CompareExchange(ref _isHandlingMessage, 1, 0) == 0;
        }
        public void ExitHandlingMessage()
        {
            Interlocked.Exchange(ref _isHandlingMessage, 0);
        }

        public void RegisterForExecution(bool exitFirst = false)
        {
            if (exitFirst)
            {
                ExitHandlingMessage();
            }
            if (EnterHandlingMessage())
            {
                Task.Factory.StartNew(Run);
            }
        }
    }
}
