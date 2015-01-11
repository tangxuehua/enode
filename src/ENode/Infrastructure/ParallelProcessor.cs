using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ECommon.Extensions;

namespace ENode.Infrastructure
{
    public class ParallelProcessor<T> where T : class
    {
        private readonly TaskFactory _taskFactory;
        private readonly int _parallelThreadCount = 1;
        private readonly IList<MailboxWorker<QueueMessage<T>>> _mailboxWorkerList;
        private bool _isStarted;

        public ParallelProcessor(int parallelThreadCount, string actionName, Action<QueueMessage<T>> action)
        {
            if (parallelThreadCount <= 0)
            {
                throw new ArgumentException("parallelThreadCount must > 0");
            }

            _taskFactory = new TaskFactory();
            _parallelThreadCount = parallelThreadCount;
            _mailboxWorkerList = new List<MailboxWorker<QueueMessage<T>>>();
            for (var index = 0; index < _parallelThreadCount; index++)
            {
                _mailboxWorkerList.Add(new MailboxWorker<QueueMessage<T>>(actionName, action));
            }
        }

        public void Start()
        {
            lock (this)
            {
                if (_isStarted) return;
                foreach (var mailboxWorker in _mailboxWorkerList)
                {
                    mailboxWorker.Start();
                }
                _isStarted = true;
            }
        }
        public void Stop()
        {
            lock (this)
            {
                foreach (var mailboxWorker in _mailboxWorkerList)
                {
                    mailboxWorker.Stop();
                }
            }
        }
        public void EnqueueMessage(QueueMessage<T> queueMessage)
        {
            var queueIndex = queueMessage.HashKey.GetHashCode() % _parallelThreadCount;
            if (queueIndex < 0)
            {
                queueIndex = Math.Abs(queueIndex);
            }
            _mailboxWorkerList[queueIndex].EnqueueMessage(queueMessage);
        }
        public void RetryMessage(QueueMessage<T> queueMessage)
        {
            _taskFactory.StartDelayedTask(1000, () => EnqueueMessage(queueMessage));
        }
    }
}
