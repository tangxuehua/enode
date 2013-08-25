using System.Reflection;
using ENode;
using ENode.Autofac;
using ENode.JsonNet;
using ENode.Log4Net;
using ENode.Mongo;

namespace NoteSample
{
    public class ENodeFrameworkMongoInitializer : IENodeFrameworkInitializer
    {
        private const string ConnectionString = "mongodb://localhost/NoteDB";

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
                .UseMongo(ConnectionString)
                .CreateAllDefaultProcessors()
                .Initialize(assemblies)
                .Start();
        }
    }
}
