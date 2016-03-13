using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Logging;
using ENode.Configurations;
using ECommonConfiguration = ECommon.Configurations.Configuration;

namespace ENode.Tests
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
            var setting = new ConfigurationSetting
            {
                SqlDefaultConnectionString = ConfigurationManager.AppSettings["connectionString"]
            };
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Domain"),
                Assembly.Load("NoteSample.CommandHandlers"),
                Assembly.Load("NoteSample.Commands"),
                Assembly.GetExecutingAssembly()
            };

            ECommonConfiguration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .CreateENode(setting)
                .RegisterENodeComponents()
                .UseSqlServerEventStore()
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies)
                .UseEQueue()
                .StartEQueue();

            Logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program));
            Logger.Info("ENode framework initialized.");
        }
        static void RunAllTests()
        {
            foreach (var testClass in Assembly.GetExecutingAssembly().GetTypes().Where(x => x.Name.EndsWith("Test")))
            {
                var testInstance = Activator.CreateInstance(testClass);
                foreach (var method in testClass.GetMethods(BindingFlags.Instance | BindingFlags.Public).Where(x => x.Name.EndsWith("_test")))
                {
                    try
                    {
                        Logger.Info("");
                        Logger.InfoFormat("---- {0} start.", method.Name);
                        method.Invoke(testInstance, null);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(string.Format("---- {0} has exception.", method.Name), ex);
                    }
                    finally
                    {
                        Logger.InfoFormat("---- {0} end.", method.Name);
                    }
                }
            }
        }
    }
}
