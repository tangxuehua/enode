using System;
using System.Collections.Generic;
using System.Linq;

namespace ENode.EQueue
{
    public abstract class AbstractTopicProvider<T> : ITopicProvider<T>
    {
        private readonly IDictionary<Type, string> _topicDict = new Dictionary<Type, string>();

        public virtual string GetTopic(T source)
        {
            return _topicDict[source.GetType()];
        }
        public IEnumerable<string> GetAllTopics()
        {
            return _topicDict.Values.Distinct();
        }

        protected IEnumerable<Type> GetAllTypes()
        {
            return _topicDict.Keys;
        }
        protected void RegisterTopic(string topic, params Type[] types)
        {
            foreach (var type in types)
            {
                _topicDict.Add(type, topic);
            }
        }
    }
}
