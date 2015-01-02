using System;
using System.Collections.Concurrent;
using ECommon.Scheduling;

namespace ENode.Infrastructure
{
    public class MailboxWorker<T>
    {
        private readonly BlockingCollection<T> _queue = new BlockingCollection<T>();
        private readonly Worker _worker;

        public MailboxWorker(string actionName, Action<T> action)
        {
            _queue = new BlockingCollection<T>();
            _worker = new Worker(actionName, () => action(_queue.Take()));
        }

        public void EnqueueMessage(T message)
        {
            _queue.Add(message);
        }
        public void Start()
        {
            _worker.Start();
        }
        public void Stop()
        {
            _worker.Stop();
        }
    }
}
