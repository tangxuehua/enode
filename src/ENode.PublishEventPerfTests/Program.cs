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
using ECommon.Utilities;
using ENode.Configurations;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.Domain;

namespace ENode.PublishEventPerfTests
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

            var eventPublisher = ObjectContainer.Resolve<IPublisher<DomainEventStream>>();
            var watch = Stopwatch.StartNew();
            var eventStreams = new List<DomainEventStream>();
            var commandId = ObjectId.GenerateNewStringId();
            var noteId = ObjectId.GenerateNewStringId();
            var evnt = new NoteCreated(noteId, "Sample Note");
            var evnts = new List<IDomainEvent> { evnt };

            for (var i = 1; i <= _totalCount; i++)
            {
                eventStreams.Add(new DomainEventStream(commandId, noteId, 100, 1, DateTime.Now, evnts, new Dictionary<string, string>()));
            }

            int _current = 0;
            Console.WriteLine("Start to send event stream.");
            foreach (var eventStream in eventStreams)
            {
                eventPublisher.Publish(eventStream);
                var current = Interlocked.Increment(ref _current);
                if (current % 1000 == 0)
                {
                    Console.WriteLine(current);
                }
                if (current == _totalCount)
                {
                    _waitHandle.Set();
                }
            }
            _waitHandle.WaitOne();
            Console.WriteLine("Event stream send completed, time spent: {0}ms", watch.ElapsedMilliseconds);
            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Domain"),
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
