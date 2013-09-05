using System;
using System.Collections.Generic;
using System.Linq;
using ENode.Infrastructure;
using ENode.Infrastructure.Dapper;
using ENode.Infrastructure.Serializing;
using ENode.Infrastructure.Sql;

namespace ENode.Messaging.Impl.SQL
{
    /// <summary>The SQL implementation of IMessageStore.
    /// </summary>
    public class SqlMessageStore : IMessageStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly IQueueTableNameProvider _queueTableNameProvider;
        private readonly IBinarySerializer _binarySerializer;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public SqlMessageStore(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }

            _connectionString = connectionString;
            _queueTableNameProvider = ObjectContainer.Resolve<IQueueTableNameProvider>();
            _binarySerializer = ObjectContainer.Resolve<IBinarySerializer>();
            _connectionFactory = ObjectContainer.Resolve<IDbConnectionFactory>();
        }

        #endregion

        /// <summary>Initialize the message store with the given queue name.
        /// </summary>
        /// <param name="queueName"></param>
        public void Initialize(string queueName) { }
        /// <summary>Persist a new message to the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        public void AddMessage(string queueName, IMessage message)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                var tableName = _queueTableNameProvider.GetTable(queueName);
                var messageData = _binarySerializer.Serialize(message);
                connection.Insert(new { MessageId = message.Id, MessageData = messageData }, tableName);
            });
        }
        /// <summary>Remove a existing message from the queue.
        /// </summary>
        /// <param name="queueName"></param>
        /// <param name="message"></param>
        public void RemoveMessage(string queueName, IMessage message)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                var tableName = _queueTableNameProvider.GetTable(queueName);
                connection.Delete(new { MessageId = message.Id }, tableName);
            });
        }
        /// <summary>Get all the existing messages of the queue.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="queueName"></param>
        /// <returns></returns>
        public IEnumerable<T> GetMessages<T>(string queueName) where T : class, IMessage
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<IEnumerable<T>>(connection =>
            {
                var tableName = _queueTableNameProvider.GetTable(queueName);
                var items = connection.QueryAll(tableName);

                return items.Select(item => _binarySerializer.Deserialize<T>((byte[]) item.MessageData)).ToList();
            });
        }
    }
}
