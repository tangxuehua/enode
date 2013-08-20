using System;
using ENode.Infrastructure;
using ENode.Infrastructure.Dapper;
using ENode.Infrastructure.Sql;

namespace ENode.Eventing.Impl.SQL
{
    public class SqlEventHandleInfoStore : IEventHandleInfoStore
    {
        #region Private Variables

        private string _connectionString;
        private string _tableName;
        private IDbConnectionFactory _connectionFactory;

        #endregion

        #region Constructors

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

        public void AddEventHandleInfo(Guid eventId, string eventHandlerTypeName)
        {
            _connectionFactory.CreateConnection(_connectionString).TryExecute((connection) =>
            {
                var key = new { EventHandlerTypeName = eventHandlerTypeName, EventId = eventId };
                var count = connection.GetCount(key, _tableName);
                if (count == 0)
                {
                    connection.Insert(key, _tableName);
                }
            });
        }
        public bool IsEventHandleInfoExist(Guid eventId, string eventHandlerTypeName)
        {
            return _connectionFactory.CreateConnection(_connectionString).TryExecute<bool>((connection) =>
            {
                return connection.GetCount(new { EventHandlerTypeName = eventHandlerTypeName, EventId = eventId }, _tableName) > 0;
            });
        }
    }
}
