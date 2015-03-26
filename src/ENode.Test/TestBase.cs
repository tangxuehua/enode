using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Logging;
using ECommon.Remoting;
using ECommon.Utilities;
using ENode.Configurations;

namespace ENode.Test
{
    public abstract class TestBase
    {
        protected static ILogger _logger;

        static TestBase()
        {
            InitializeECommon();
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create("system");
        }

        private static void InitializeECommon()
        {
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Domain"),
                Assembly.GetExecutingAssembly()
            };

            Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies);
        }
    }
}
