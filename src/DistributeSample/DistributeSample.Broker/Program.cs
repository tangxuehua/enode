using System;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Logging;
using EQueue.Broker;
using EQueue.Configurations;

namespace DistributeSample.Broker
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeEQueue();
            new BrokerController().Start();

            ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name).Info("Broker started, press Enter to exit...");
            Console.ReadLine();
        }

        static void InitializeEQueue()
        {
            Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .RegisterEQueueComponents();
        }
    }
}
