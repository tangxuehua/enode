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
    public class SqlServerPublishedVersionStore : IPublishedVersionStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _uniqueIndexName;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public SqlServerPublishedVersionStore(OptionSetting optionSetting)
        {
            if (optionSetting != null)
            {
                _connectionString = optionSetting.GetOptionValue<string>("ConnectionString");
                _tableName = optionSetting.GetOptionValue<string>("TableName");
                _uniqueIndexName = optionSetting.GetOptionValue<string>("UniqueIndexName");
            }
            else
            {
                var setting = ENodeConfiguration.Instance.Setting.DefaultDBConfigurationSetting;
                _connectionString = setting.ConnectionString;
                _tableName = setting.PublishedVersionTableName;
                _uniqueIndexName = setting.PublishedVersionUniqueIndexName;
            }

            Ensure.NotNull(_connectionString, "_connectionString");
            Ensure.NotNull(_tableName, "_tableName");
            Ensure.NotNull(_uniqueIndexName, "_uniqueIndexName");

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        #endregion

        public async Task<AsyncTaskResult> UpdatePublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId, int publishedVersion)
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
                            AggregateRootTypeName = aggregateRootTypeName,
                            AggregateRootId = aggregateRootId,
                            Version = 1,
                            CreatedOn = DateTime.Now
                        }, _tableName);
                        return AsyncTaskResult.Success;
                    }
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2601 && ex.Message.Contains(_uniqueIndexName))
                    {
                        return AsyncTaskResult.Success;
                    }
                    _logger.Error("Insert aggregate published version has sql exception.", ex);
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error("Insert aggregate published version has unknown exception.", ex);
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
                            Version = publishedVersion,
                            CreatedOn = DateTime.Now
                        },
                        new
                        {
                            ProcessorName = processorName,
                            AggregateRootId = aggregateRootId,
                            Version = publishedVersion - 1
                        }, _tableName);
                        return AsyncTaskResult.Success;
                    }
                }
                catch (SqlException ex)
                {
                    _logger.Error("Update aggregate published version has sql exception.", ex);
                    return new AsyncTaskResult(AsyncTaskStatus.IOException, ex.Message);
                }
                catch (Exception ex)
                {
                    _logger.Error("Update aggregate published version has unknown exception.", ex);
                    return new AsyncTaskResult(AsyncTaskStatus.Failed, ex.Message);
                }
            }
        }
        public async Task<AsyncTaskResult<int>> GetPublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId)
        {
            try
            {
                using (var connection = GetConnection())
                {
                    var result = await connection.QueryListAsync<int>(new
                    {
                        ProcessorName = processorName,
                        AggregateRootId = aggregateRootId
                    }, _tableName, "Version");
                    return new AsyncTaskResult<int>(AsyncTaskStatus.Success, result.SingleOrDefault());
                }
            }
            catch (SqlException ex)
            {
                _logger.Error("Get aggregate published version has sql exception.", ex);
                return new AsyncTaskResult<int>(AsyncTaskStatus.IOException, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Error("Get aggregate published version has unknown exception.", ex);
                return new AsyncTaskResult<int>(AsyncTaskStatus.Failed, ex.Message);
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
