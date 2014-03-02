using System;
using System.Reflection;
using ECommon.Autofac;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ENode.Configurations;

namespace DistributeEventStoreSample.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .CreateENode()
                .RegisterENodeComponents()
                .UseDefaultEventStoreServer()
                .InitializeEventStore()
                .StartEventStoreServer();

            Console.WriteLine("Press Enter to exit...");
            Console.ReadLine();
        }
    }
}
