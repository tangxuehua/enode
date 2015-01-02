using System;
using System.Collections.Generic;

namespace ENode.Infrastructure
{
    public class ParallelProcessor<T>
    {
        private readonly int _parallelThreadCount = 1;
        private readonly IList<MailboxWorker<T>> _mailboxWorkerList;
        private bool _isStarted;

        public ParallelProcessor(int parallelThreadCount, string actionName, Action<T> action)
        {
            if (parallelThreadCount <= 0)
            {
                throw new NotSupportedException("parallelThreadCount must > 0");
            }

            _parallelThreadCount = parallelThreadCount;
            _mailboxWorkerList = new List<MailboxWorker<T>>();
            for (var index = 0; index < _parallelThreadCount; index++)
            {
                _mailboxWorkerList.Add(new MailboxWorker<T>(actionName, action));
            }
        }

        public void Start()
        {
            if (_isStarted) return;
            foreach (var mailboxWorker in _mailboxWorkerList)
            {
                mailboxWorker.Start();
            }
            _isStarted = true;
        }
        public void Stop()
        {
            foreach (var mailboxWorker in _mailboxWorkerList)
            {
                mailboxWorker.Stop();
            }
        }
        public void EnqueueMessage(object hashKey, T message)
        {
            var queueIndex = hashKey.GetHashCode() % _parallelThreadCount;
            if (queueIndex < 0)
            {
                queueIndex = Math.Abs(queueIndex);
            }
            _mailboxWorkerList[queueIndex].EnqueueMessage(message);
        }
    }
}
