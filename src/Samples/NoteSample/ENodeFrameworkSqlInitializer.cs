using System.Reflection;
using ENode;
using ENode.Autofac;
using ENode.JsonNet;
using ENode.Log4Net;

namespace NoteSample {
    public class ENodeFrameworkSqlInitializer : IENodeFrameworkInitializer {
        public void Initialize() {
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };
            var connectionString = "Data Source=.;Initial Catalog=EventDB;Integrated Security=True;Connect Timeout=30;Min Pool Size=10;Max Pool Size=100";

            Configuration
                .Create()
                .UseAutofac()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .UseLog4Net()
                .UseJsonNet()
                .UseSql(connectionString)
                .CreateAllDefaultProcessors()
                .Initialize(assemblies)
                .Start();
        }
    }
}
