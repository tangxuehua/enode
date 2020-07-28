using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Serilog;
using ENode.Commanding;
using ENode.Configurations;
using NoteSample.Commands;

namespace ENode.SendCommandPerfTests
{
    class Program
    {
        static void Main(string[] args)
        {
            InitializeENodeFramework();
            var count = int.Parse(ConfigurationManager.AppSettings["count"]);
            SendCommandAsync(count);
            Console.ReadLine();
        }

        static IEnumerable<ICommand> CreateCommands(int commandCount)
        {
            var commands = new List<ICommand>();
            for (var i = 1; i <= commandCount; i++)
            {
                commands.Add(new CreateNoteCommand
                {
                    AggregateRootId = i.ToString(),
                    Title = "Sample Note"
                });
            }
            return commands;
        }
        static void SendCommandAsync(int commandCount)
        {
            var commands = CreateCommands(commandCount);
            var watch = Stopwatch.StartNew();
            var sequence = 0;
            var printSize = commandCount / 10;
            var commandService = ObjectContainer.Resolve<ICommandService>();
            var waitHandle = new ManualResetEvent(false);
            var asyncAction = new Action<ICommand>(async command =>
            {
                await commandService.SendAsync(command).ConfigureAwait(false);
                var current = Interlocked.Increment(ref sequence);
                if (current % printSize == 0)
                {
                    Console.WriteLine("----Sent {0} commands async, time spent: {1}ms", current, watch.ElapsedMilliseconds);
                }
                if (current == commandCount)
                {
                    waitHandle.Set();
                }
            });

            Console.WriteLine("--Start to send commands asynchronously, total count: {0}.", commandCount);
            foreach (var command in commands)
            {
                asyncAction(command);
            }
            waitHandle.WaitOne();
            Console.WriteLine("--Commands send async completed, throughput: {0}/s", commandCount * 1000 / watch.ElapsedMilliseconds);
        }
        static void InitializeENodeFramework()
        {
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Commands"),
                Assembly.GetExecutingAssembly()
            };
            var loggerFactory = new SerilogLoggerFactory()
                .AddFileLogger("ECommon", "logs\\ecommon")
                .AddFileLogger("EQueue", "logs\\equeue")
                .AddFileLogger("ENode", "logs\\enode", minimumLevel: Serilog.Events.LogEventLevel.Error);
            ECommon.Configurations.Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseSerilog(loggerFactory)
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .UseEQueue()
                .BuildContainer()
                .InitializeBusinessAssemblies(assemblies)
                .StartEQueue();

            Console.WriteLine("ENode started...");
        }
    }
}
