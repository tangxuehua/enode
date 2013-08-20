using System;
using ENode.Infrastructure;
using ENode.Infrastructure.Dapper;
using ENode.Infrastructure.Sql;

namespace ENode.Eventing.Impl.SQL
{
    public class SqlEventPublishInfoStore : IEventPublishInfoStore
    {
        #region Private Variables

        private string _connectionString;
        private string _tableName;
        private IDbConnectionFactory _connectionFactory;

        #endregion

        #region Constructors

        public SqlEventPublishInfoStore(string connectionString, string tableName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }

            _connectionString = connectionString;
            _tableName = tableName;
            _connectionFactory = ObjectContainer.Resolve<IDbConnectionFactory>();
        }

        #endregion

        public void InsertFirstPublishedVersion(string aggregateRootId)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute((connection) =>
            {
                var count = connection.GetCount(new { AggregateRootId = aggregateRootId }, _tableName);
                if (count == 0)
                {
                    connection.Insert(new { AggregateRootId = aggregateRootId, PublishedEventStreamVersion = 1 }, _tableName);
                }
            });
        }
        public void UpdatePublishedVersion(string aggregateRootId, long version)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute((connection) =>
            {
                connection.Update(
                    new { PublishedEventStreamVersion = version },
                    new { AggregateRootId = aggregateRootId },
                    _tableName);
            });
        }
        public long GetEventPublishedVersion(string aggregateRootId)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<long>((connection) =>
            {
                return connection.GetValue<long>(new { AggregateRootId = aggregateRootId }, _tableName, "PublishedEventStreamVersion");
            });
        }
    }
}
