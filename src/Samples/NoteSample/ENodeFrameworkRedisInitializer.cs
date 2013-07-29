using System.Reflection;
using ENode;
using ENode.Domain;
using ENode.Infrastructure;

namespace NoteSample
{
    public class ENodeFrameworkRedisInitializer : IENodeFrameworkInitializer
    {
        public void Initialize()
        {
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseAutofacContainer()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .SetDefault<ILoggerFactory, Log4NetLoggerFactory>(new Log4NetLoggerFactory("log4net.config"))
                .SetDefault<IMemoryCache, RedisMemoryCache>(new RedisMemoryCache("127.0.0.1", 6379))
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
