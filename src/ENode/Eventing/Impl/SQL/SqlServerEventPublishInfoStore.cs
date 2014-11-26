using System;
using System.Data.SqlClient;
using System.Linq;
using ECommon.Components;
using ECommon.Dapper;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Configurations;

namespace ENode.Eventing.Impl.SQL
{
    public class SqlServerEventPublishInfoStore : IEventPublishInfoStore
    {
        #region Private Variables

        private readonly string _connectionString;
        private readonly string _tableName;
        private readonly string _primaryKeyName;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public SqlServerEventPublishInfoStore()
        {
            var setting = ENodeConfiguration.Instance.Setting.SqlServerEventPublishInfoStoreSetting;
            Ensure.NotNull(setting, "SqlServerEventPublishInfoStoreSetting");
            Ensure.NotNull(setting.ConnectionString, "SqlServerEventPublishInfoStoreSetting.ConnectionString");
            Ensure.NotNull(setting.TableName, "SqlServerEventPublishInfoStoreSetting.TableName");
            Ensure.NotNull(setting.PrimaryKeyName, "SqlServerEventPublishInfoStoreSetting.PrimaryKeyName");

            _connectionString = setting.ConnectionString;
            _tableName = setting.TableName;
            _primaryKeyName = setting.PrimaryKeyName;
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(GetType().FullName);
        }

        #endregion

        public void InsertPublishedVersion(string eventProcessorName, string aggregateRootId)
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
                        _logger.Error(string.Format("Failed to insert duplicate aggregate publish record, EventProcessorName:{0}, AggregateRootId:{1}", eventProcessorName, aggregateRootId), ex);
                        return;
                    }
                    throw;
                }
            }
        }
        public void UpdatePublishedVersion(string eventProcessorName, string aggregateRootId, int version)
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
        }
        public int GetEventPublishedVersion(string eventProcessorName, string aggregateRootId)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                return connection.QueryList<int>(new { EventProcessorName = eventProcessorName, AggregateRootId = aggregateRootId }, _tableName, "PublishedVersion").SingleOrDefault();
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
