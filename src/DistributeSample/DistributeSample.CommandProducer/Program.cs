using System;
using System.Reflection;
using DistributeSample.CommandProducer.EQueueIntegrations;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Extensions;
using ECommon.IO;
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
        static ILogger _logger;

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();

            for (var index = 1; index <= 10; index++)
            {
                var result = commandService.ExecuteAsync(new CreateNoteCommand { AggregateRootId = ObjectId.GenerateNewStringId(), Title = "Sample Note" + index }).WaitResult<AsyncTaskResult<CommandResult>>(5000000);
                if (result.Data.Status == CommandStatus.Success)
                {
                    _logger.InfoFormat("Execute command success, title: {0}", "Sample Note" + index);
                }
                else
                {
                    _logger.ErrorFormat("Execute command failed, title: {0}, errorMsg: {1}", "Sample Note" + index, result.Data.ErrorMessage);
                }
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
                .RegisterAllTypeCodes()
                .UseEQueue()
                .InitializeBusinessAssemblies(assemblies)
                .StartEQueue();

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            _logger.Info("Command Producer started.");
        }
    }
}
