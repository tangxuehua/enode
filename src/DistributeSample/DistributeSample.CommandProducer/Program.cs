using System;
using System.Reflection;
using System.Threading;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
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
            var sentCount = 0;

            for (var index = 1; index <= 10; index++)
            {
                commandService.SendAsync(new CreateNoteCommand { AggregateRootId = ObjectId.GenerateNewStringId(), Title = "Sample Note" + index }).ContinueWith(t =>
                {
                    if (t.Result.Status == AsyncTaskStatus.Success)
                    {
                        _logger.InfoFormat("Send command success, sentCount: {0}", Interlocked.Increment(ref sentCount));
                    }
                    else
                    {
                        _logger.InfoFormat("Send command failed, errorMessage: {0}", t.Result.ErrorMessage);
                    }
                });
            }

            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Commands"),
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
                .StartEQueue();

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            _logger.Info("Command Producer started.");
        }
    }
}
