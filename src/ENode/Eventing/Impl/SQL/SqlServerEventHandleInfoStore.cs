using System;
using System.Data.SqlClient;
using ECommon.Dapper;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>The Microsoft SqlServer based implementation of IEventHandleInfoStore.
    /// </summary>
    public class SqlServerEventHandleInfoStore : IEventHandleInfoStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <exception cref="ArgumentNullException"></exception>
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

        /// <summary>Insert an event handle info.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeCode"></param>
        /// <param name="eventTypeCode"></param>
        /// <param name="aggregateRootId"></param>
        /// <param name="aggregateRootVersion"></param>
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
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeCode"></param>
        /// <returns></returns>
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
