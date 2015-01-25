using System.Collections.Generic;

namespace ENode.EQueue
{
    /// <summary>Represents a topic provider interface.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface ITopicProvider<T>
    {
        /// <summary>Gets the topic by the given source object.
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        string GetTopic(T source);
        /// <summary>Gets all the topics.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetAllTopics();
    }
}
