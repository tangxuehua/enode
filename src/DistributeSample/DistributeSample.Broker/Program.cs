using System;
using System.IO;
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

            var brokerStorePath = @"d:\equeue-store";

            if (Directory.Exists(brokerStorePath))
            {
                Directory.Delete(brokerStorePath, true);
            }

            BrokerController.Create().Start();

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
