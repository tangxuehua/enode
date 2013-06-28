using System.Reflection;
using ENode;

namespace NoteSample
{
    public class ENodeFrameworkUnitTestInitializer : IENodeFrameworkInitializer
    {
        public void Initialize()
        {
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };

            //全部使用默认配置，一般单元测试时，可以使用该配置

            Configuration
                .Create()
                .UseTinyObjectContainer()
                .RegisterAllDefaultFrameworkComponents()
                .UseLog4Net("log4net.config")
                .UseDefaultCommandHandlerProvider(assemblies)
                .UseDefaultAggregateRootTypeProvider(assemblies)
                .UseDefaultAggregateRootInternalHandlerProvider(assemblies)
                .UseDefaultEventHandlerProvider(assemblies)
                .UseAllDefaultProcessors(
                    new string[] { "CommandQueue" },
                    "RetryCommandQueue",
                    new string[] { "EventQueue" })
                .Start();
        }
    }
}
