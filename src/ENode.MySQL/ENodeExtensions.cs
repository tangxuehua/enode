using ECommon.Components;
using ENode.Configurations;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.MySQL
{
    public static class ENodeExtensions
    {
        /// <summary>Use the MySqlEventStore as the IEventStore.
        /// </summary>
        /// <returns></returns>
        public static ENodeConfiguration UseMySqlEventStore(this ENodeConfiguration eNodeConfiguration)
        {
            eNodeConfiguration.GetCommonConfiguration().SetDefault<IEventStore, MySqlEventStore>();
            return eNodeConfiguration;
        }
        /// <summary>Use the MySqlPublishedVersionStore as the IPublishedVersionStore.
        /// </summary>
        /// <returns></returns>
        public static ENodeConfiguration UseMySqlPublishedVersionStore(this ENodeConfiguration eNodeConfiguration)
        {
            eNodeConfiguration.GetCommonConfiguration().SetDefault<IPublishedVersionStore, MySqlPublishedVersionStore>();
            return eNodeConfiguration;
        }
        /// <summary>Use the MySqlLockService as the ILockService.
        /// </summary>
        /// <returns></returns>
        public static ENodeConfiguration UseMySqlLockService(this ENodeConfiguration eNodeConfiguration)
        {
            eNodeConfiguration.GetCommonConfiguration().SetDefault<ILockService, MySqlLockService>();
            return eNodeConfiguration;
        }
        /// <summary>Initialize the MySqlEventStore with option setting.
        /// </summary>
        /// <param name="eNodeConfiguration"></param>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <param name="tableCount"></param>
        /// <param name="versionIndexName"></param>
        /// <param name="commandIndexName"></param>
        /// <param name="batchInsertTimeoutSeconds"></param>
        /// <returns></returns>
        public static ENodeConfiguration InitializeMySqlEventStore(this ENodeConfiguration eNodeConfiguration,
            string connectionString,
            string tableName = "EventStream",
            int tableCount = 1,
            string versionIndexName = "IX_EventStream_AggId_Version",
            string commandIndexName = "IX_EventStream_AggId_CommandId",
            int batchInsertTimeoutSeconds = 60)
        {
            ((MySqlEventStore)ObjectContainer.Resolve<IEventStore>()).Initialize(
                connectionString,
                tableName,
                tableCount,
                versionIndexName,
                commandIndexName,
                batchInsertTimeoutSeconds);
            return eNodeConfiguration;
        }
        /// <summary>Initialize the MySqlPublishedVersionStore with option setting.
        /// </summary>
        /// <param name="eNodeConfiguration"></param>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <param name="tableCount"></param>
        /// <param name="uniqueIndexName"></param>
        /// <returns></returns>
        public static ENodeConfiguration InitializeMySqlPublishedVersionStore(this ENodeConfiguration eNodeConfiguration,
            string connectionString,
            string tableName = "PublishedVersion",
            int tableCount = 1,
            string uniqueIndexName = "IX_PublishedVersion_AggId_Version")
        {
            ((MySqlPublishedVersionStore)ObjectContainer.Resolve<IPublishedVersionStore>()).Initialize(
                connectionString,
                tableName,
                tableCount,
                uniqueIndexName);
            return eNodeConfiguration;
        }
        /// <summary>Initialize the MySqlLockService with option setting.
        /// </summary>
        /// <param name="eNodeConfiguration"></param>
        /// <param name="connectionString"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public static ENodeConfiguration InitializeMySqlLockService(this ENodeConfiguration eNodeConfiguration,
            string connectionString,
            string tableName = "LockKey")
        {
            ((MySqlLockService)ObjectContainer.Resolve<ILockService>()).Initialize(connectionString, tableName);
            return eNodeConfiguration;
        }
    }
}