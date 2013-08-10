using System.Reflection;
using ENode;
using ENode.Autofac;
using ENode.JsonNet;
using ENode.Log4Net;
using ENode.Redis;

namespace NoteSample {
    public class ENodeFrameworkRedisInitializer : IENodeFrameworkInitializer {
        public void Initialize() {
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseAutofac()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .UseLog4Net()
                .UseJsonNet()
                .UseRedis()
                .CreateAllDefaultProcessors()
                .Initialize(assemblies)
                .Start();
        }
    }
}
