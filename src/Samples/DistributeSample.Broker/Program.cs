using System;
using ECommon.Autofac;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Log4Net;
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

            Console.WriteLine("Press Enter to exit...");
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
