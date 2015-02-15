using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.JsonNet;
using ENode.Commanding;
using ENode.Configurations;
using NoteSample.Commands;

namespace ENode.SendCommandPerfTests
{
    class Program
    {
        static ENodeConfiguration _configuration;
        static ICommandService _commandService;

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            _commandService = ObjectContainer.Resolve<ICommandService>();

            SendCommandAsync(100000);
            SendCommandSync(50000);

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

            Console.WriteLine("");
            Console.WriteLine("--Start to send commands asynchronously, total count: {0}.", commandCount);
            foreach (var command in commands)
            {
                _commandService.SendAsync(command).ContinueWith(x =>
                {
                    var current = Interlocked.Increment(ref sequence);
                    if (current % printSize == 0)
                    {
                        Console.WriteLine("----Sent {0} commands, time spent: {1}ms", current, watch.ElapsedMilliseconds);
                    }
                    if (current == commandCount)
                    {
                        Console.WriteLine("--Commands send completed, average speed: {0}/s", commandCount * 1000 / watch.ElapsedMilliseconds);
                    }
                });
            }
        }
        static void SendCommandSync(int commandCount)
        {
            var commands = CreateCommands(commandCount);
            var watch = Stopwatch.StartNew();
            var sentCount = 0;
            var printSize = commandCount / 10;

            Console.WriteLine("");
            Console.WriteLine("--Start to send commands synchronously, total count: {0}.", commandCount);
            foreach (var command in commands)
            {
                _commandService.Send(command);
                sentCount++;
                if (sentCount % printSize == 0)
                {
                    Console.WriteLine("----Sent {0} commands, time spent: {1}ms", sentCount, watch.ElapsedMilliseconds);
                }
            }
            Console.WriteLine("--Commands send completed, average speed: {0}/s", commandCount * 1000 / watch.ElapsedMilliseconds);
        }
        static void InitializeENodeFramework()
        {
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Commands"),
                Assembly.GetExecutingAssembly()
            };
            _configuration = Configuration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies)
                .UseEQueue()
                .StartEQueue();

            Console.WriteLine("ENode started...");
        }
    }
}
