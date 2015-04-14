using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ECommon.Dapper;
using ECommon.IO;
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

        #endregion

        #region Constructors

        public SqlServerSequenceMessagePublishedVersionStore()
        {
            var setting = ENodeConfiguration.Instance.Setting.SqlServerSequenceMessagePublishedVersionStoreSetting;
            Ensure.NotNull(setting, "SqlServerAggregatePublishVersionStoreSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerAggregatePublishVersionStoreSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerAggregatePublishVersionStoreSetting.TableName");
            Ensure.NotNull(setting.PrimaryKeyName, "SqlServerAggregatePublishVersionStoreSetting.PrimaryKeyName");

            _connectionString = setting.ConnectionString;
            _tableName = setting.TableName;
            _primaryKeyName = setting.PrimaryKeyName;
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
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
                }
            }
            else
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
        }
        public async Task<AsyncTaskResult<int>> GetPublishedVersionAsync(string processorName, int aggregateRootTypeCode, string aggregateRootId)
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

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
