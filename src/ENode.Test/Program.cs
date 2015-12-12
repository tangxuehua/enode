using System;
using System.Linq;
using System.Reflection;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Logging;
using ENode.Configurations;

namespace ENode.Test
{
    class Program
    {
        public static ILogger Logger;

        static void Main(string[] args)
        {
            InitializeECommon();
            RunAllTests();
            Console.ReadLine();
        }

        static void InitializeECommon()
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

            Logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program));
        }
        static void RunAllTests()
        {
            foreach (var testClass in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Name.EndsWith("Test")))
            {
                var testInstance = Activator.CreateInstance(testClass);
                foreach (var method in testClass.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(x => x.Name.StartsWith("test_")))
                {
                    try
                    {
                        Logger.InfoFormat("Start test case: {0}", method.Name);
                        method.Invoke(testInstance, null);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(string.Format("{0} run has exception.", method.Name), ex);
                    }
                    finally
                    {
                        Logger.InfoFormat("End test case: {0}", method.Name);
                    }
                }
            }
        }
    }
}
