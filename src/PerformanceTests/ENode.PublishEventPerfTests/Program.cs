using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Serilog;
using ECommon.Utilities;
using ENode.Configurations;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Messaging;
using NoteSample.Domain;

namespace ENode.PublishEventPerfTests
{
    class Program
    {
        static ENodeConfiguration _configuration;

        static void Main(string[] args)
        {
            InitializeENodeFramework();
            PublishEventAsync(100000);
            Console.ReadLine();
        }

        public class TestEvent : DomainEvent<string>
        {
            public string Title { get; set; }
        }

        static void PublishEventAsync(int eventCount)
        {
            var printSize = eventCount / 10;
            var eventPublisher = ObjectContainer.Resolve<IMessagePublisher<DomainEventStreamMessage>>();
            var eventStreams = new List<DomainEventStreamMessage>();
            var commandId = ObjectId.GenerateNewStringId();
            var note = new Note(ObjectId.GenerateNewStringId(), "Sample Note");
            var evnt = new TestEvent
            {
                AggregateRootId = note.Id,
                Title = "Sample Note",
                Version = 1
            };
            var evnts = new List<IDomainEvent> { evnt };
            var waitHandle = new ManualResetEvent(false);

            for (var i = 1; i <= eventCount; i++)
            {
                eventStreams.Add(new DomainEventStreamMessage(commandId, note.Id, 1, note.GetType().FullName, evnts, new Dictionary<string, string>()));
            }

            var watch = Stopwatch.StartNew();
            var publishedEventCount = 0;
            var asyncAction = new Action<DomainEventStreamMessage>(async eventStream =>
            {
                await eventPublisher.PublishAsync(eventStream).ConfigureAwait(false);
                var currentCount = Interlocked.Increment(ref publishedEventCount);
                if (currentCount % printSize == 0)
                {
                    Console.WriteLine("----Published {0} events async, time spent: {1}ms", publishedEventCount, watch.ElapsedMilliseconds);
                }
                if (currentCount == eventCount)
                {
                    waitHandle.Set();
                }
            });

            Console.WriteLine("--Start to publish event async, total count: {0}.", eventCount);
            foreach (var eventStream in eventStreams)
            {
                asyncAction(eventStream);
            }
            waitHandle.WaitOne();
            Console.WriteLine("--Event publish async completed, throughput: {0}/s", eventCount * 1000 / watch.ElapsedMilliseconds);
        }
        static void InitializeENodeFramework()
        {
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Domain"),
                Assembly.GetExecutingAssembly()
            };
            var loggerFactory = new SerilogLoggerFactory()
                .AddFileLogger("ECommon", "logs\\ecommon")
                .AddFileLogger("EQueue", "logs\\equeue")
                .AddFileLogger("ENode", "logs\\enode", minimumLevel: Serilog.Events.LogEventLevel.Error);
            _configuration = Configuration
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
