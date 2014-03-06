using System;
using ECommon.IoC;
using ENode.Infrastructure.Dapper;
using ENode.Infrastructure.Sql;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>The Microsoft SqlServer based implementation of IEventHandleInfoStore.
    /// </summary>
    public class SqlServerEventHandleInfoStore : IEventHandleInfoStore
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
            _connectionFactory = ObjectContainer.Resolve<IDbConnectionFactory>();
        }

        #endregion

        /// <summary>Insert an event handle info.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeCode"></param>
        public void AddEventHandleInfo(string eventId, int eventHandlerTypeCode)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                var key = new { EventHandlerTypeCode = eventHandlerTypeCode, EventId = eventId };
                var count = connection.GetCount(key, _tableName);
                if (count == 0)
                {
                    connection.Insert(key, _tableName);
                }
            });
        }
        /// <summary>Check whether the given event was handled by the given event handler.
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="eventHandlerTypeCode"></param>
        /// <returns></returns>
        public bool IsEventHandleInfoExist(string eventId, int eventHandlerTypeCode)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute(connection => connection.GetCount(new { EventHandlerTypeCode = eventHandlerTypeCode, EventId = eventId }, _tableName) > 0);
        }
    }
}
