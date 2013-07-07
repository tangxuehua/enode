using System.Reflection;
using ENode;
using ENode.Infrastructure;

namespace NoteSample
{
    public class ENodeFrameworkMongoInitializer : IENodeFrameworkInitializer
    {
        public void Initialize()
        {
            var connectionString = "mongodb://localhost/NoteDB";
            var eventCollection = "Event";
            var eventPublishInfoCollection = "EventPublishInfo";
            var eventHandleInfoCollection = "EventHandleInfo";

            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseAutofacContainer()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .SetDefault<ILoggerFactory, Log4NetLoggerFactory>(new Log4NetLoggerFactory("log4net.config"))
                .UseMongoAsStorage(connectionString, eventCollection, null, eventPublishInfoCollection, eventHandleInfoCollection)
                .CreateAllDefaultProcessors(
                    new string[] { "CommandQueue" },
                    "RetryCommandQueue",
                    new string[] { "EventQueue" })
                .Initialize(assemblies)
                .Start();
        }
    }
}
