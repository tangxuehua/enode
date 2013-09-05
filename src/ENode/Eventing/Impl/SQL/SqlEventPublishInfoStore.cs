using System;
using ENode.Infrastructure;
using ENode.Infrastructure.Dapper;
using ENode.Infrastructure.Sql;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>The SQL implementation of IEventPublishInfoStore.
    /// </summary>
    public class SqlEventPublishInfoStore : IEventPublishInfoStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly IDbConnectionFactory _connectionFactory;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>Insert the first published event version of aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        public void InsertFirstPublishedVersion(string aggregateRootId)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                var count = connection.GetCount(new { AggregateRootId = aggregateRootId }, _tableName);
                if (count == 0)
                {
                    connection.Insert(new { AggregateRootId = aggregateRootId, PublishedEventStreamVersion = 1 }, _tableName);
                }
            });
        }
        /// <summary>Update the published event version of aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="version"></param>
        public void UpdatePublishedVersion(string aggregateRootId, long version)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                connection.Update(
                    new { PublishedEventStreamVersion = version },
                    new { AggregateRootId = aggregateRootId },
                    _tableName);
            });
        }
        /// <summary>Get the current event published version for the specified aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public long GetEventPublishedVersion(string aggregateRootId)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute(connection => connection.GetValue<long>(new { AggregateRootId = aggregateRootId }, _tableName, "PublishedEventStreamVersion"));
        }
    }
}
