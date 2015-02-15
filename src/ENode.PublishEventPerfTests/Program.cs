using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.JsonNet;
using ECommon.Utilities;
using ENode.Configurations;
using ENode.Eventing;
using ENode.Infrastructure;
using NoteSample.Domain;

namespace ENode.PublishEventPerfTests
{
    class Program
    {
        static ENodeConfiguration _configuration;

        static void Main(string[] args)
        {
            InitializeENodeFramework();
            PublishEventSync(10000);
            Console.ReadLine();
        }

        static void PublishEventSync(int eventCount)
        {
            var printSize = eventCount / 10;
            var eventPublisher = ObjectContainer.Resolve<IPublisher<DomainEventStream>>();
            var eventStreams = new List<DomainEventStream>();
            var commandId = ObjectId.GenerateNewStringId();
            var noteId = ObjectId.GenerateNewStringId();
            var evnt = new NoteCreated(noteId, "Sample Note");
            var evnts = new List<IDomainEvent> { evnt };

            for (var i = 1; i <= eventCount; i++)
            {
                eventStreams.Add(new DomainEventStream(commandId, noteId, 100, 1, DateTime.Now, evnts, new Dictionary<string, string>()));
            }

            int publishedEventCount = 0;
            Console.WriteLine("--Start to send events, total count: {0}.", eventCount);
            var watch = Stopwatch.StartNew();
            foreach (var eventStream in eventStreams)
            {
                eventPublisher.Publish(eventStream);
                publishedEventCount++;
                if (publishedEventCount % printSize == 0)
                {
                    Console.WriteLine("----Sent {0} events, time spent: {1}ms", publishedEventCount, watch.ElapsedMilliseconds);
                }
            }
            Console.WriteLine("--Events send completed, average speed: {0}/s", eventCount * 1000 / watch.ElapsedMilliseconds);
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
