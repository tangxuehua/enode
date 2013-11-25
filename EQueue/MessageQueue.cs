using System;

namespace EQueue
{
    [Serializable]
    public class MessageQueue
    {
        public string Topic { get; set; }
        public string BrokerName { get; set; }
        public int QueueId { get; set; }

        public MessageQueue(string topic, string brokerName, int queueId)
        {
            Topic = topic;
            BrokerName = brokerName;
            QueueId = queueId;
        }
    }
}
