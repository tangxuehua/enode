using ENode.Eventing;
using ENode.Messaging;

namespace ENode.Mongo
{
    /// <summary>ENode configuration class Mongo extensions.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>Use MongoDB as the storage for the enode framework.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="connectionString">The connection string of the mongodb server.</param>
        /// <returns></returns>
        public static Configuration UseMongo(this Configuration configuration, string connectionString)
        {
            return UseMongo(configuration, connectionString, "Event", null, "EventPublishInfo", "EventHandleInfo");
        }

        /// <summary>Use MongoDB as the storage for the enode framework.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="connectionString">The connection string of the mongodb server.</param>
        /// <param name="eventCollectionName">The mongo collection used to store all the domain event.</param>
        /// <param name="queueNameFormat">The format of the queue name.</param>
        /// <param name="eventPublishInfoCollectionName">The collection used to store all the event publish information.</param>
        /// <param name="eventHandleInfoCollectionName">The collection used to store all the event handle information.</param>
        /// <returns></returns>
        public static Configuration UseMongo(this Configuration configuration, string connectionString, string eventCollectionName, string queueNameFormat, string eventPublishInfoCollectionName, string eventHandleInfoCollectionName)
        {
            configuration.SetDefault<IEventCollectionNameProvider, DefaultEventCollectionNameProvider>(new DefaultEventCollectionNameProvider(eventCollectionName));
            configuration.SetDefault<IQueueCollectionNameProvider, DefaultQueueCollectionNameProvider>(new DefaultQueueCollectionNameProvider(queueNameFormat));
            configuration.SetDefault<IMessageStore, MongoMessageStore>(new MongoMessageStore(connectionString));
            configuration.SetDefault<IEventStore, MongoEventStore>(new MongoEventStore(connectionString));
            configuration.SetDefault<IEventPublishInfoStore, MongoEventPublishInfoStore>(new MongoEventPublishInfoStore(connectionString, eventPublishInfoCollectionName));
            configuration.SetDefault<IEventHandleInfoStore, MongoEventHandleInfoStore>(new MongoEventHandleInfoStore(connectionString, eventHandleInfoCollectionName));
            return configuration;
        }
    }
}