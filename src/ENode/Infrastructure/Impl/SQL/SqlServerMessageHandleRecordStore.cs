using System.Data.SqlClient;
using ECommon.Dapper;
using ECommon.Utilities;
using ENode.Configurations;

namespace ENode.Infrastructure.Impl.SQL
{
    public class SqlServerMessageHandleRecordStore : IMessageHandleRecordStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;

        #endregion

        #region Constructors

        public SqlServerMessageHandleRecordStore()
        {
            var setting = ENodeConfiguration.Instance.Setting.SqlServerMessageHandleRecordStoreSetting;
            Ensure.NotNull(setting, "SqlServerMessageHandleRecordStoreSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerMessageHandleRecordStoreSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerMessageHandleRecordStoreSetting.TableName");

            _connectionString = setting.ConnectionString;
            _tableName = setting.TableName;
        }

        #endregion

        public void AddRecord(MessageHandleRecord record)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                connection.Insert(new
                {
                    Type = (int)record.Type,
                    HandlerTypeCode = record.HandlerTypeCode,
                    MessageId = record.MessageId,
                    MessageTypeCode = record.MessageTypeCode,
                    AggregateRootId = record.AggregateRootId,
                    AggregateRootVersion = record.AggregateRootVersion
                }, _tableName);
            }
        }
        public bool IsRecordExist(MessageHandleRecordType type, string messageId, int handlerTypeCode)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                return connection.GetCount(new { MessageId = messageId, HandlerTypeCode = handlerTypeCode }, _tableName) > 0;
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
