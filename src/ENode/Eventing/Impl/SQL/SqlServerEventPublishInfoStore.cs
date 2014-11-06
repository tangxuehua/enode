using System.Data.SqlClient;
using System.Linq;
using ECommon.Dapper;
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
                connection.Update(new { PublishedVersion = version }, new { EventProcessorName = eventProcessorName, AggregateRootId = aggregateRootId }, _tableName);
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
