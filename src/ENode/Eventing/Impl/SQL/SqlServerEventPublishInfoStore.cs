using System;
using ECommon.IoC;
using ENode.Infrastructure.Dapper;
using ENode.Infrastructure.Sql;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>The Microsoft SqlServer based implementation of IEventPublishInfoStore.
    /// </summary>
    public class SqlServerEventPublishInfoStore : IEventPublishInfoStore
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
        public SqlServerEventPublishInfoStore(string connectionString, string tableName)
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
                    connection.Insert(new { AggregateRootId = aggregateRootId, PublishedVersion = 1 }, _tableName);
                }
            });
        }
        /// <summary>Update the published event version of aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <param name="version"></param>
        public void UpdatePublishedVersion(string aggregateRootId, int version)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                connection.Update(
                    new { PublishedVersion = version },
                    new { AggregateRootId = aggregateRootId },
                    _tableName);
            });
        }
        /// <summary>Get the current event published version for the specified aggregate.
        /// </summary>
        /// <param name="aggregateRootId"></param>
        /// <returns></returns>
        public int GetEventPublishedVersion(string aggregateRootId)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute(connection => connection.GetValue<int>(new { AggregateRootId = aggregateRootId }, _tableName, "PublishedVersion"));
        }
    }
}
