using System;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.SqlServer
{
    public class SqlServerPublishedVersionStore : IPublishedVersionStore
    {
        private const string EventSingleTableNameFormat = "[{0}]";
        private const string EventTableNameFormat = "[{0}_{1}]";

        #region Private Variables

        private string _connectionString;
        private string _tableName;
        private int _tableCount;
        private string _uniqueIndexName;
        private ILogger _logger;
        private ITimeProvider _timeProvider;
        private IOHelper _ioHelper;

        #endregion

        public SqlServerPublishedVersionStore Initialize(string connectionString, string tableName = "PublishedVersion", int tableCount = 1, string uniqueIndexName = "IX_PublishedVersion_AggId_Version")
        {
            _connectionString = connectionString;
            _tableName = tableName;
            _tableCount = tableCount;
            _uniqueIndexName = uniqueIndexName;

            Ensure.NotNull(_connectionString, "_connectionString");
            Ensure.NotNull(_tableName, "_tableName");
            Ensure.Positive(_tableCount, "_tableCount");
            Ensure.NotNull(_uniqueIndexName, "_uniqueIndexName");

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _timeProvider = ObjectContainer.Resolve<ITimeProvider>();
            _ioHelper = ObjectContainer.Resolve<IOHelper>();

            return this;
        }
        public async Task UpdatePublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId, int publishedVersion)
        {
            await _ioHelper.TryIOActionAsync(async () =>
            {
                if (publishedVersion == 1)
                {

                    try
                    {
                        using (var connection = GetConnection())
                        {
                            var currentTime = _timeProvider.GetCurrentTime();
                            await connection.OpenAsync().ConfigureAwait(false);
                            await connection.InsertAsync(new
                            {
                                ProcessorName = processorName,
                                AggregateRootTypeName = aggregateRootTypeName,
                                AggregateRootId = aggregateRootId,
                                Version = 1,
                                CreatedOn = currentTime,
                                UpdatedOn = currentTime
                            }, GetTableName(aggregateRootId)).ConfigureAwait(false);
                        }
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2601 && ex.Message.Contains(_uniqueIndexName))
                        {
                            return;
                        }
                        var errorMessage = string.Format("Insert aggregate published version has sql exception, aggregateRootType: {0}, aggregateRootId: {1}", aggregateRootTypeName, aggregateRootId);
                        _logger.Error(errorMessage, ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = string.Format("Insert aggregate published version has unknown exception, aggregateRootType: {0}, aggregateRootId: {1}", aggregateRootTypeName, aggregateRootId);
                        _logger.Error(errorMessage, ex);
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        using (var connection = GetConnection())
                        {
                            await connection.OpenAsync().ConfigureAwait(false);
                            await connection.UpdateAsync(
                            new
                            {
                                Version = publishedVersion,
                                UpdatedOn = _timeProvider.GetCurrentTime()
                            },
                            new
                            {
                                ProcessorName = processorName,
                                AggregateRootId = aggregateRootId,
                                Version = publishedVersion - 1
                            }, GetTableName(aggregateRootId)).ConfigureAwait(false);
                        }
                    }
                    catch (SqlException ex)
                    {
                        var errorMessage = string.Format("Update aggregate published version has sql exception, aggregateRootType: {0}, aggregateRootId: {1}", aggregateRootTypeName, aggregateRootId);
                        _logger.Error(errorMessage, ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = string.Format("Update aggregate published version has unknown exception, aggregateRootType: {0}, aggregateRootId: {1}", aggregateRootTypeName, aggregateRootId);
                        _logger.Error(errorMessage, ex);
                        throw;
                    }
                }
            }, "UpdatePublishedVersionAsync");
        }
        public async Task<int> GetPublishedVersionAsync(string processorName, string aggregateRootTypeName, string aggregateRootId)
        {
            return await _ioHelper.TryIOFuncAsync(async () =>
            {
                try
                {
                    using (var connection = GetConnection())
                    {
                        await connection.OpenAsync().ConfigureAwait(false);
                        var result = await connection.QueryListAsync<int>(new
                        {
                            ProcessorName = processorName,
                            AggregateRootId = aggregateRootId
                        }, GetTableName(aggregateRootId), "Version").ConfigureAwait(false);
                        return result.SingleOrDefault();
                    }
                }
                catch (SqlException ex)
                {
                    var errorMessage = string.Format("Get aggregate published version has sql exception, aggregateRootType: {0}, aggregateRootId: {1}", aggregateRootTypeName, aggregateRootId);
                    _logger.Error(errorMessage, ex);
                    throw;
                }
                catch (Exception ex)
                {
                    var errorMessage = string.Format("Get aggregate published version has unknown exception, aggregateRootType: {0}, aggregateRootId: {1}", aggregateRootTypeName, aggregateRootId);
                    _logger.Error(errorMessage, ex);
                    throw;
                }
            }, "GetPublishedVersionAsync");
        }

        private int GetTableIndex(string aggregateRootId)
        {
            int hash = 23;
            foreach (char c in aggregateRootId)
            {
                hash = (hash << 5) - hash + c;
            }
            if (hash < 0)
            {
                hash = Math.Abs(hash);
            }
            return hash % _tableCount;
        }
        private string GetTableName(string aggregateRootId)
        {
            if (_tableCount <= 1)
            {
                return string.Format(EventSingleTableNameFormat, _tableName);
            }

            var tableIndex = GetTableIndex(aggregateRootId);

            return string.Format(EventTableNameFormat, _tableName, tableIndex);
        }
        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
