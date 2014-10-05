using System;
using System.Data.SqlClient;
using System.Linq;
using ECommon.Dapper;

namespace ENode.Eventing.Impl.SQL
{
    public class SqlServerEventPublishInfoStore : IEventPublishInfoStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _primaryKeyName;

        #endregion

        #region Constructors

        public SqlServerEventPublishInfoStore(string connectionString, string tableName, string primaryKeyName)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("connectionString");
            }
            if (string.IsNullOrEmpty(tableName))
            {
                throw new ArgumentNullException("tableName");
            }
            if (string.IsNullOrEmpty(primaryKeyName))
            {
                throw new ArgumentNullException("primaryKeyName");
            }

            _connectionString = connectionString;
            _tableName = tableName;
            _primaryKeyName = primaryKeyName;
        }

        #endregion

        public void InsertPublishedVersion(string eventProcessorName, string aggregateRootId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                try
                {
                    connection.Insert(new { EventProcessorName = eventProcessorName, AggregateRootId = aggregateRootId, PublishedVersion = 1 }, _tableName);
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627 && ex.Message.Contains(_primaryKeyName))
                    {
                        return;
                    }
                    throw;
                }
            }
        }
        public void UpdatePublishedVersion(string eventProcessorName, string aggregateRootId, int version)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                connection.Update(new { PublishedVersion = version }, new { EventProcessorName = eventProcessorName, AggregateRootId = aggregateRootId }, _tableName);
            }
        }
        public int GetEventPublishedVersion(string eventProcessorName, string aggregateRootId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                return connection.QueryList<int>(new { EventProcessorName = eventProcessorName, AggregateRootId = aggregateRootId }, _tableName, "PublishedVersion").SingleOrDefault();
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
