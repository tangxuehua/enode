using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Configurations;

namespace ENode.Infrastructure.Impl.SQL
{
    public class SqlServerMessageHandleRecordStore : IMessageHandleRecordStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _primaryKeyName;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public SqlServerMessageHandleRecordStore()
        {
            var setting = ENodeConfiguration.Instance.Setting.SqlServerMessageHandleRecordStoreSetting;
            Ensure.NotNull(setting, "SqlServerMessageHandleRecordStoreSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerMessageHandleRecordStoreSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerMessageHandleRecordStoreSetting.TableName");
            Ensure.NotNull(setting.PrimaryKeyName, "SqlServerMessageHandleRecordStoreSetting.PrimaryKeyName");

            _connectionString = setting.ConnectionString;
            _tableName = setting.TableName;
            _primaryKeyName = setting.PrimaryKeyName;
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        #endregion

        public async Task<AsyncTaskResult> AddRecordAsync(MessageHandleRecord record)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    await connection.InsertAsync(new
                    {
                        HandlerTypeCode = record.HandlerTypeCode,
                        MessageId = record.MessageId,
                        MessageTypeCode = record.MessageTypeCode,
                        AggregateRootTypeCode = record.AggregateRootTypeCode,
                        AggregateRootId = record.AggregateRootId,
                        Version = record.Version,
                        Timestamp = record.Timestamp
                    }, _tableName);
                    return AsyncTaskResult.Success;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 && ex.Message.Contains(_primaryKeyName))
                {
                    return AsyncTaskResult.Success;
                }
                _logger.Error("Insert message handle record has sql exception.", ex);
                return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error("Insert message handle record has unknown exception.", ex);
                return new AsyncTaskResult(AsyncTaskStatus.Failed, ex.Message);
            }
        }
        public async Task<AsyncTaskResult<bool>> IsRecordExistAsync(string messageId, int handlerTypeCode, int aggregateRootTypeCode)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    var count = await connection.GetCountAsync(new { MessageId = messageId, HandlerTypeCode = handlerTypeCode }, _tableName);
                    return new AsyncTaskResult<bool>(AsyncTaskStatus.Success, count > 0);
                }
            }
            catch (SqlException ex)
            {
                _logger.Error("Get message handle record has sql exception.", ex);
                return new AsyncTaskResult<bool>(AsyncTaskStatus.IOException, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error("Get message handle record has unknown exception.", ex);
                return new AsyncTaskResult<bool>(AsyncTaskStatus.Failed, ex.Message);
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
