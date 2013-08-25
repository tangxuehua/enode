using System.Reflection;
using ENode;
using ENode.Autofac;
using ENode.JsonNet;
using ENode.Log4Net;

namespace NoteSample
{
    public class ENodeFrameworkSqlInitializer : IENodeFrameworkInitializer
    {
        private const string ConnectionString = "Data Source=.;Initial Catalog=EventDB;Integrated Security=True;Connect Timeout=30;Min Pool Size=10;Max Pool Size=100";

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
                .UseSql(ConnectionString)
                .CreateAllDefaultProcessors()
                .Initialize(assemblies)
                .Start();
        }
    }
}
