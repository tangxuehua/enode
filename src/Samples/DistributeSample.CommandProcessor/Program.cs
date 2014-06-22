using System;
using System.Reflection;
using DistributeSample.CommandProcessor.EQueueIntegrations;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Logging;
using ENode.Configurations;

namespace DistributeSample.CommandProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeENodeFramework();
            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            var assemblies = new[] { Assembly.GetExecutingAssembly() };

            Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .SetProviders()
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartRetryCommandService()
                .StartWaitingCommandService()
                .StartEQueue();

            ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name).Info("Command Processor started.");
        }
    }
}
