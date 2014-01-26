using System;
using ENode.Eventing;
using EQueue.Protocols;

namespace ENode.EQueue
{
    public class EventProcessContext : IEventProcessContext
    {
        public Action<EventStream, QueueMessage> EventProcessedAction { get; private set; }
        public QueueMessage QueueMessage { get; private set; }

        public EventProcessContext(QueueMessage queueMessage, Action<EventStream, QueueMessage> eventProcessedAction)
        {
            QueueMessage = queueMessage;
            EventProcessedAction = eventProcessedAction;
        }

        public void OnEventProcessed(EventStream eventStream)
        {
            EventProcessedAction(eventStream, QueueMessage);
        }
    }
}
