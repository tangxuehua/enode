using System;
using System.Data.SqlClient;
using System.Linq;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Configurations;
using ENode.Infrastructure;

namespace ENode.Eventing.Impl.SQL
{
    public class SqlServerAggregatePublishVersionStore : IAggregatePublishVersionStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _primaryKeyName;
        private readonly ILogger _logger;
        private readonly IOHelper _ioHelper;

        #endregion

        #region Constructors

        public SqlServerAggregatePublishVersionStore()
        {
            var setting = ENodeConfiguration.Instance.Setting.SqlServerAggregatePublishVersionStoreSetting;
            Ensure.NotNull(setting, "SqlServerAggregatePublishVersionStoreSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerAggregatePublishVersionStoreSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerAggregatePublishVersionStoreSetting.TableName");
            Ensure.NotNull(setting.PrimaryKeyName, "SqlServerAggregatePublishVersionStoreSetting.PrimaryKeyName");

            _connectionString = setting.ConnectionString;
            _tableName = setting.TableName;
            _primaryKeyName = setting.PrimaryKeyName;
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
            _ioHelper = ObjectContainer.Resolve<IOHelper>();
        }

        #endregion

        public void InsertFirstVersion(string eventProcessorName, string aggregateRootId)
        {
            _ioHelper.TryIOAction(() =>
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    try
                    {
                        connection.Insert(new { EventProcessorName = eventProcessorName, AggregateRootId = aggregateRootId, PublishedVersion = 1 }, _tableName);
                    }
                    catch (SqlException ex)
                    {
                        if (ex.Number == 2627 && ex.Message.Contains(_primaryKeyName))
                        {
                            _logger.Error(string.Format("Duplicate aggregate publish version record insert, EventProcessorName:{0}, AggregateRootId:{1}", eventProcessorName, aggregateRootId), ex);
                            return;
                        }
                        throw;
                    }
                }
            }, "InsertAggregatePublishedVersion");
        }
        public void UpdateVersion(string eventProcessorName, string aggregateRootId, int version)
        {
            _ioHelper.TryIOAction(() =>
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    var effectedRows = connection.Update(new { PublishedVersion = version }, new { EventProcessorName = eventProcessorName, AggregateRootId = aggregateRootId, PublishedVersion = version - 1 }, _tableName);
                    if (effectedRows != 1)
                    {
                        throw new Exception(string.Format("Update aggregate publish version failed, EventProcessorName:{0}, AggregateRootId:{1}, target version:{2}", eventProcessorName, aggregateRootId, version));
                    }
                }
            }, "UpdateAggregatePublishedVersion");
        }
        public int GetVersion(string eventProcessorName, string aggregateRootId)
        {
            return _ioHelper.TryIOFunc(() =>
            {
                using (var connection = GetConnection())
                {
                    connection.Open();
                    return connection.QueryList<int>(new { EventProcessorName = eventProcessorName, AggregateRootId = aggregateRootId }, _tableName, "PublishedVersion").SingleOrDefault();
                }
            }, "GetAggregatePublishedVersion");
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
