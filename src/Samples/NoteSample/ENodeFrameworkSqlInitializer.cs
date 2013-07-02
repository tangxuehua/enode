using System.Reflection;
using ENode;

namespace NoteSample
{
    public class ENodeFrameworkSqlInitializer : IENodeFrameworkInitializer
    {
        public void Initialize()
        {
            var connectionString = "Data Source=.;Initial Catalog=EventDB;Integrated Security=True;Connect Timeout=30;Min Pool Size=10;Max Pool Size=100";
            var eventTable = "Event";
            var eventPublishInfoTable = "EventPublishInfo";
            var eventHandleInfoTable = "EventHandleInfo";

            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseTinyObjectContainer()
                .RegisterAllDefaultFrameworkComponents()
                .UseLog4Net("log4net.config")
                .UseDefaultCommandHandlerProvider(assemblies)
                .UseDefaultAggregateRootTypeProvider(assemblies)
                .UseDefaultAggregateRootInternalHandlerProvider(assemblies)
                .UseDefaultEventHandlerProvider(assemblies)
                .UseDefaultEventPersistenceSynchronizerProvider(assemblies)

                //使用Sql来支持持久化
                .UseDefaultEventTableNameProvider(eventTable)
                .UseDefaultQueueTableNameProvider()
                .UseSqlMessageStore(connectionString)
                .UseSqlEventStore(connectionString)
                .UseSqlEventPublishInfoStore(connectionString, eventPublishInfoTable)
                .UseSqlEventHandleInfoStore(connectionString, eventHandleInfoTable)

                .UseAllDefaultProcessors(
                    new string[] { "CommandQueue" },
                    "RetryCommandQueue",
                    new string[] { "EventQueue" })
                .Start();
        }
    }
}
