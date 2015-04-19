using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Configurations;

namespace ENode.Infrastructure.Impl.SQL
{
    public class SqlServerSequenceMessagePublishedVersionStore : ISequenceMessagePublishedVersionStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _primaryKeyName;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public SqlServerSequenceMessagePublishedVersionStore()
        {
            var setting = ENodeConfiguration.Instance.Setting.SqlServerSequenceMessagePublishedVersionStoreSetting;
            Ensure.NotNull(setting, "SqlServerSequenceMessagePublishedVersionStoreSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerSequenceMessagePublishedVersionStoreSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerSequenceMessagePublishedVersionStoreSetting.TableName");
            Ensure.NotNull(setting.PrimaryKeyName, "SqlServerSequenceMessagePublishedVersionStoreSetting.PrimaryKeyName");

            _connectionString = setting.ConnectionString;
            _tableName = setting.TableName;
            _primaryKeyName = setting.PrimaryKeyName;
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        #endregion

        public async Task<AsyncTaskResult> UpdatePublishedVersionAsync(string processorName, int aggregateRootTypeCode, string aggregateRootId, int publishedVersion)
        {
            if (publishedVersion == 1)
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.InsertAsync(new
                        {
                            ProcessorName = processorName,
                            AggregateRootTypeCode = aggregateRootTypeCode,
                            AggregateRootId = aggregateRootId,
                            PublishedVersion = 1,
                            Timestamp = DateTime.Now
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
                    _logger.Error("Insert sequence message published version has sql exception.", ex);
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error("Insert sequence message published version has unknown exception.", ex);
                    return new AsyncTaskResult(AsyncTaskStatus.Failed, ex.Message);
                }
            }
            else
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.UpdateAsync(
                        new
                        {
                            PublishedVersion = publishedVersion,
                            Timestamp = DateTime.Now
                        },
                        new
                        {
                            ProcessorName = processorName,
                            AggregateRootId = aggregateRootId,
                            PublishedVersion = publishedVersion - 1
                        }, _tableName);
                        return AsyncTaskResult.Success;
                    }
                }
                catch (SqlException ex)
                {
                    _logger.Error("Update sequence message published version has sql exception.", ex);
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error("Update sequence message published version has unknown exception.", ex);
                    return new AsyncTaskResult(AsyncTaskStatus.Failed, ex.Message);
                }
            }
        }
        public async Task<AsyncTaskResult<int>> GetPublishedVersionAsync(string processorName, int aggregateRootTypeCode, string aggregateRootId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    var result = await connection.QueryListAsync<int>(new
                    {
                        ProcessorName = processorName,
                        AggregateRootId = aggregateRootId
                    }, _tableName, "PublishedVersion");
                    return new AsyncTaskResult<int>(AsyncTaskStatus.Success, result.SingleOrDefault());
                }
            }
            catch (SqlException ex)
            {
                _logger.Error("Get sequence message published version has sql exception.", ex);
                return new AsyncTaskResult<int>(AsyncTaskStatus.IOException, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error("Get sequence message published version has unknown exception.", ex);
                return new AsyncTaskResult<int>(AsyncTaskStatus.Failed, ex.Message);
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
