using System;
using System.Data.SqlClient;
using ECommon.Dapper;

namespace ENode.Eventing.Impl.SQL
{
    public class SqlServerEventHandleInfoStore : IEventHandleInfoStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;

        #endregion

        #region Constructors

        public SqlServerEventHandleInfoStore(string connectionString, string tableName)
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
        }

        #endregion

        public void AddEventHandleInfo(string eventId, int eventHandlerTypeCode, int eventTypeCode, string aggregateRootId, int aggregateRootVersion)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                connection.Insert(
                new
                {
                    EventHandlerTypeCode = eventHandlerTypeCode,
                    EventId = eventId,
                    EventTypeCode = eventTypeCode,
                    AggregateRootId = aggregateRootId,
                    AggregateRootVersion = aggregateRootVersion
                }, _tableName);
            }
        }
        public bool IsEventHandleInfoExist(string eventId, int eventHandlerTypeCode)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                return connection.GetCount(new { EventHandlerTypeCode = eventHandlerTypeCode, EventId = eventId }, _tableName) > 0;
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
