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
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Domain"),
                Assembly.Load("NoteSample.Commands"),
                Assembly.Load("NoteSample.CommandHandlers"),
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
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartENode(NodeType.CommandProcessor)
                .StartEQueue();

            ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name).Info("Command Processor started.");
        }
    }
}
