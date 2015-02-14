using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Logging;
using ENode.Commanding;
using ENode.Configurations;
using NoteSample.Commands;

namespace ENode.SendCommandPerfTests
{
    class Program
    {
        static ILogger _logger;
        static ENodeConfiguration _configuration;
        static int _totalCount = 10000;
        static ManualResetEvent _waitHandle = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandService = ObjectContainer.Resolve<ICommandService>();
            var watch = Stopwatch.StartNew();
            var commands = new List<ICommand>();

            for (var i = 1; i <= _totalCount; i++)
            {
                commands.Add(
                    new CreateNoteCommand
                    {
                        AggregateRootId = i.ToString(),
                        Title = "Sample Note"
                    });
            }

            int _current = 0;
            Console.WriteLine("Start to send commands.");
            foreach (var command in commands)
            {
                commandService.SendAsync(command).ContinueWith(x =>
                {
                    var current = Interlocked.Increment(ref _current);
                    if (current % 1000 == 0)
                    {
                        Console.WriteLine(current);
                    }
                    if (current == _totalCount)
                    {
                        _waitHandle.Set();
                    }
                });
            }
            _waitHandle.WaitOne();
            Console.WriteLine("Commands send completed, time spent: {0}ms", watch.ElapsedMilliseconds);
            Console.ReadLine();
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
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies)
                .UseEQueue()
                .StartEQueue();

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            Console.WriteLine("ENode started...");
        }
    }
}
