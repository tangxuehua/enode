using System.Data.SqlClient;
using ECommon.Dapper;
using ECommon.Utilities;
using ENode.Configurations;

namespace ENode.Eventing.Impl.SQL
{
    public class SqlServerEventHandleInfoStore : IEventHandleInfoStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;

        #endregion

        #region Constructors

        public SqlServerEventHandleInfoStore()
        {
            var setting = ENodeConfiguration.Instance.Setting.SqlServerEventHandleInfoStoreSetting;
            Ensure.NotNull(setting, "SqlServerEventHandleInfoStoreSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerEventHandleInfoStoreSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerEventHandleInfoStoreSetting.TableName");

            _connectionString = setting.ConnectionString;
            _tableName = setting.TableName;
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
