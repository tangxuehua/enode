using ECommon.Components;
using ENode.Configurations;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.SqlServer
{
    public static class ENodeExtensions
    {
        /// <summary>Use the SqlServerEventStore as the IEventStore.
        /// </summary>
        /// <returns></returns>
        public static ENodeConfiguration UseSqlServerEventStore(this ENodeConfiguration eNodeConfiguration)
        {
            eNodeConfiguration.GetCommonConfiguration().SetDefault<IEventStore, SqlServerEventStore>();
            return eNodeConfiguration;
        }
        /// <summary>Use the SqlServerPublishedVersionStore as the IPublishedVersionStore.
        /// </summary>
        /// <returns></returns>
        public static ENodeConfiguration UseSqlServerPublishedVersionStore(this ENodeConfiguration eNodeConfiguration)
        {
            eNodeConfiguration.GetCommonConfiguration().SetDefault<IPublishedVersionStore, SqlServerPublishedVersionStore>();
            return eNodeConfiguration;
        }
        /// <summary>Use the SqlServerLockService as the ILockService.
        /// </summary>
        /// <returns></returns>
        public static ENodeConfiguration UseSqlServerLockService(this ENodeConfiguration eNodeConfiguration)
        {
            eNodeConfiguration.GetCommonConfiguration().SetDefault<ILockService, SqlServerLockService>();
            return eNodeConfiguration;
        }
        /// <summary>Initialize the SqlServerEventStore with option setting.
        /// </summary>
        /// <param name="optionSetting"></param>
        /// <returns></returns>
        public static ENodeConfiguration InitializeSqlServerEventStore(this ENodeConfiguration eNodeConfiguration, OptionSetting optionSetting = null)
        {
            ((SqlServerEventStore)ObjectContainer.Resolve<IEventStore>()).Initialize(optionSetting);
            return eNodeConfiguration;
        }
        /// <summary>Initialize the SqlServerPublishedVersionStore with option setting.
        /// </summary>
        /// <param name="optionSetting"></param>
        /// <returns></returns>
        public static ENodeConfiguration InitializeSqlServerPublishedVersionStore(this ENodeConfiguration eNodeConfiguration, OptionSetting optionSetting = null)
        {
            ((SqlServerPublishedVersionStore)ObjectContainer.Resolve<IPublishedVersionStore>()).Initialize(optionSetting);
            return eNodeConfiguration;
        }
        /// <summary>Initialize the SqlServerLockService with option setting.
        /// </summary>
        /// <param name="optionSetting"></param>
        /// <returns></returns>
        public static ENodeConfiguration InitializeSqlServerLockService(this ENodeConfiguration eNodeConfiguration, OptionSetting optionSetting = null)
        {
            ((SqlServerLockService)ObjectContainer.Resolve<ILockService>()).Initialize(optionSetting);
            return eNodeConfiguration;
        }
    }
}