using System.Reflection;
using ENode;

namespace NoteSample
{
    public class ENodeFrameworkRedisInitializer : IENodeFrameworkInitializer
    {
        public void Initialize()
        {
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

                //使用Redis作为Domain的内存缓存
                .UseRedisMemoryCache("127.0.0.1", 6379)

                .UseAllDefaultProcessors(
                    new string[] { "CommandQueue" },
                    "RetryCommandQueue",
                    new string[] { "EventQueue" })
                .Start();
        }
    }
}
