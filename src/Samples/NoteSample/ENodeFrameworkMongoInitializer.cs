using System.Reflection;
using ENode;

namespace NoteSample
{
    public class ENodeFrameworkMongoInitializer : IENodeFrameworkInitializer
    {
        public void Initialize()
        {
            var connectionString = "mongodb://localhost/EventDB";
            var eventCollection = "Event";
            var eventPublishInfoCollection = "EventPublishInfo";
            var eventHandleInfoCollection = "EventHandleInfo";

            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseTinyObjectContainer()
                .UseLog4Net("log4net.config")
                .UseDefaultCommandHandlerProvider(assemblies)
                .UseDefaultAggregateRootTypeProvider(assemblies)
                .UseDefaultAggregateRootInternalHandlerProvider(assemblies)
                .UseDefaultEventHandlerProvider(assemblies)

                //使用MongoDB来支持持久化
                .UseDefaultEventCollectionNameProvider(eventCollection)
                .UseDefaultQueueCollectionNameProvider()
                .UseMongoMessageStore(connectionString)
                .UseMongoEventStore(connectionString)
                .UseMongoEventPublishInfoStore(connectionString, eventPublishInfoCollection)
                .UseMongoEventHandleInfoStore(connectionString, eventHandleInfoCollection)

                .UseAllDefaultProcessors(
                    new string[] { "CommandQueue" },
                    "RetryCommandQueue",
                    new string[] { "EventQueue" })
                .Start();
        }
    }
}
