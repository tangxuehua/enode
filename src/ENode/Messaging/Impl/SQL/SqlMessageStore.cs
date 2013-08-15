using System;
using System.Collections.Generic;
using Dapper;
using ENode.Infrastructure;

namespace ENode.Messaging
{
    public class SqlMessageStore : IMessageStore
    {
        #region Private Variables

        private string _connectionString;
        private IDbConnectionFactory _connectionFactory;
        private IQueueTableNameProvider _queueTableNameProvider;
        private IBinarySerializer _binarySerializer;

        #endregion

        #region Constructors

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

        public void Initialize(string queueName) { }
        public void AddMessage(string queueName, IMessage message)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute((connection) =>
            {
                var tableName = _queueTableNameProvider.GetTable(queueName);
                var messageData = _binarySerializer.Serialize(message);
                connection.Insert(new { MessageId = message.Id, MessageData = messageData }, tableName);
            });
        }
        public void RemoveMessage(string queueName, IMessage message)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute((connection) =>
            {
                var tableName = _queueTableNameProvider.GetTable(queueName);
                connection.Delete(new { MessageId = message.Id }, tableName);
            });
        }
        public IEnumerable<T> GetMessages<T>(string queueName) where T : class, IMessage
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<IEnumerable<T>>((connection) =>
            {
                var tableName = _queueTableNameProvider.GetTable(queueName);
                var items = connection.QueryAll(tableName);
                var messages = new List<T>();

                foreach (var item in items)
                {
                    messages.Add(_binarySerializer.Deserialize<T>((byte[])item.MessageData));
                }

                return messages;
            });
        }
    }
}
