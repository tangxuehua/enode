using System.Reflection;
using ENode;
using ENode.Autofac;
using ENode.JsonNet;
using ENode.Log4Net;

namespace NoteSample
{
    public class ENodeFrameworkUnitTestInitializer : IENodeFrameworkInitializer
    {
        public void Initialize()
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseAutofac()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .UseLog4Net()
                .UseJsonNet()
                .CreateAllDefaultProcessors()
                .Initialize(assemblies)
                .Start();
        }
    }
}
