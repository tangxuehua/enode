using System;
using System.Reflection;
using System.Threading;
using DistributeSample.CommandProducer.EQueueIntegrations;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using NoteSample.Commands;

namespace DistributeSample.CommandProducer
{
    class Program
    {
        static int _count;
        static ILogger _logger;

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            for (var index = 1; index <= 10; index++)
            {
                commandService.SendAsync(new CreateNoteCommand { AggregateRootId = ObjectId.GenerateNewStringId(), Title = "Sample Note" + index }).ContinueWith(task =>
                {
                    if (task.Result.Status == CommandSendStatus.Success)
                    {
                        _logger.InfoFormat("Sent command{0}", Interlocked.Increment(ref _count));
                    }
                });
            }

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
                .RegisterUnhandledExceptionHandler()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartEQueue();

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            _logger.Info("Command Producer started.");
        }
    }
}
