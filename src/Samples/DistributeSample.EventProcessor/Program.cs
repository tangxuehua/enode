using System;
using System.Reflection;
using DistributeSample.EventProcessor.EQueueIntegrations;
using ECommon.Autofac;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ENode.Configurations;

namespace DistributeSample.EventProcessor
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
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartEQueue();

            Console.WriteLine("Event Processor started.");
        }
    }
}
