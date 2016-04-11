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
        private readonly ConcurrentQueue<EventCommittingContext> _messageQueue;
        private readonly Action<IList<EventCommittingContext>> _handleMessageAction;
        private int _isHandlingMessage;
        private int _batchSize;

        public string AggregateRootId
        {
            get
            {
                return _aggregateRootId;
            }
        }

        public EventMailBox(string aggregateRootId, int batchSize, Action<IList<EventCommittingContext>> handleMessageAction)
        {
            _aggregateRootId = aggregateRootId;
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
        }
        public void Run()
        {
            IList<EventCommittingContext> contextList = null;
            try
            {
                EventCommittingContext context = null;
                while (_messageQueue.TryDequeue(out context))
                {
                    context.EventMailBox = this;
                    if (contextList == null)
                    {
                        contextList = new List<EventCommittingContext>();
                    }
                    contextList.Add(context);

                    if (contextList.Count == _batchSize)
                    {
                        break;
                    }
                }
                if (contextList != null && contextList.Count > 0)
                {
                    _handleMessageAction(contextList);
                }
            }
            finally
            {
                if (contextList == null || contextList.Count == 0)
                {
                    ExitHandlingMessage();
                    if (!_messageQueue.IsEmpty)
                    {
                        RegisterForExecution();
                    }
                }
            }
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

        private bool EnterHandlingMessage()
        {
            return Interlocked.CompareExchange(ref _isHandlingMessage, 1, 0) == 0;
        }
    }
}
