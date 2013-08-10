using System.Reflection;
using ENode;
using ENode.Autofac;
using ENode.JsonNet;
using ENode.Log4Net;
using ENode.Mongo;

namespace NoteSample {
    public class ENodeFrameworkMongoInitializer : IENodeFrameworkInitializer {
        public void Initialize() {
            var assemblies = new Assembly[] { Assembly.GetExecutingAssembly() };
            var connectionString = "mongodb://localhost/NoteDB";

            Configuration
                .Create()
                .UseAutofac()
                .RegisterFrameworkComponents()
                .RegisterBusinessComponents(assemblies)
                .UseLog4Net()
                .UseJsonNet()
                .UseMongo(connectionString)
                .CreateAllDefaultProcessors()
                .Initialize(assemblies)
                .Start();
        }
    }
}
