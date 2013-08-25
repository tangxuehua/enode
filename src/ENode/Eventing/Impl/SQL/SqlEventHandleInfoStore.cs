using System;
using ENode.Infrastructure;
using ENode.Infrastructure.Dapper;
using ENode.Infrastructure.Sql;

namespace ENode.Eventing.Impl.SQL
{
    /// <summary>The SQL implementation of IEventHandleInfoStore.
    /// </summary>
    public class SqlEventHandleInfoStore : IEventHandleInfoStore
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
        public SqlEventHandleInfoStore(string connectionString, string tableName)
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
        /// <param name="eventHandlerTypeName"></param>
        public void AddEventHandleInfo(Guid eventId, string eventHandlerTypeName)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute(connection =>
            {
                var key = new { EventHandlerTypeName = eventHandlerTypeName, EventId = eventId };
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
        /// <param name="eventHandlerTypeName"></param>
        /// <returns></returns>
        public bool IsEventHandleInfoExist(Guid eventId, string eventHandlerTypeName)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute(connection => connection.GetCount(new { EventHandlerTypeName = eventHandlerTypeName, EventId = eventId }, _tableName) > 0);
        }
    }
}
