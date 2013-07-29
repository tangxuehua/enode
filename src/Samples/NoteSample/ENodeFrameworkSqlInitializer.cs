using System.Reflection;
using ENode;
using ENode.Infrastructure;

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
                .UseAutofacContainer()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .SetDefault<ILoggerFactory, Log4NetLoggerFactory>(new Log4NetLoggerFactory("log4net.config"))
                .UseSqlAsStorage(connectionString, eventTable, null, eventPublishInfoTable, eventHandleInfoTable)
                .CreateAllDefaultProcessors(
                    new string[] { "CommandQueue" },
                    "RetryCommandQueue",
                    new string[] { "UncommittedEventQueue" },
                    new string[] { "CommittedEventQueue" })
                .Initialize(assemblies)
                .Start();
        }
    }
}
