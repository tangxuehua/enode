using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Logging;

namespace ENode.Eventing.Impl
{
    public class EventMailBox
    {
        private readonly ILogger _logger;
        private readonly string _aggregateRootId;
        private readonly ConcurrentQueue<EventCommittingContext> _messageQueue;
        private readonly Action<IList<EventCommittingContext>> _handleMessageAction;
        private int _isRunning;
        private int _batchSize;

        public string AggregateRootId
        {
            get
            {
                return _aggregateRootId;
            }
        }

        public EventMailBox(string aggregateRootId, int batchSize, Action<IList<EventCommittingContext>> handleMessageAction, ILogger logger)
        {
            _aggregateRootId = aggregateRootId;
            _messageQueue = new ConcurrentQueue<EventCommittingContext>();
            _batchSize = batchSize;
            _handleMessageAction = handleMessageAction;
            _logger = logger;
        }

        public void EnqueueMessage(EventCommittingContext message)
        {
            _messageQueue.Enqueue(message);
            TryRun();
        }
        public void TryRun(bool exitFirst = false)
        {
            if (exitFirst)
            {
                Exit();
            }
            if (TryEnter())
            {
                Task.Factory.StartNew(Run);
            }
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
            catch (Exception ex)
            {
                _logger.Error(string.Format("Event mailbox run has unknown exception, aggregateRootId: {0}", AggregateRootId), ex);
                Thread.Sleep(1);
            }
            finally
            {
                if (contextList == null || contextList.Count == 0)
                {
                    Exit();
                    if (!_messageQueue.IsEmpty)
                    {
                        TryRun();
                    }
                }
            }
        }
        public void Exit()
        {
            Interlocked.Exchange(ref _isRunning, 0);
        }
        public void Clear()
        {
            EventCommittingContext message;
            while (_messageQueue.TryDequeue(out message)) { }
        }

        private bool TryEnter()
        {
            return Interlocked.CompareExchange(ref _isRunning, 1, 0) == 0;
        }
    }
}
