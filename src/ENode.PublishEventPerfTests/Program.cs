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
            PublishEventAsync(100000);
            PublishEventSync(20000);
            Console.ReadLine();
        }

        static void PublishEventAsync(int eventCount)
        {
            var printSize = eventCount / 10;
            var eventPublisher = ObjectContainer.Resolve<IMessagePublisher<DomainEventStream>>();
            var eventStreams = new List<DomainEventStream>();
            var commandId = ObjectId.GenerateNewStringId();
            var noteId = ObjectId.GenerateNewStringId();
            var evnt = new NoteCreated(noteId, "Sample Note");
            var evnts = new List<IDomainEvent> { evnt };
            var waitHandle = new ManualResetEvent(false);

            for (var i = 1; i <= eventCount; i++)
            {
                eventStreams.Add(new DomainEventStream(commandId, noteId, 100, 1, DateTime.Now, evnts, new Dictionary<string, string>()));
            }

            var watch = Stopwatch.StartNew();
            var publishedEventCount = 0;
            var asyncAction = new Action<DomainEventStream>(async eventStream =>
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
        static void PublishEventSync(int eventCount)
        {
            var printSize = eventCount / 10;
            var eventPublisher = ObjectContainer.Resolve<IMessagePublisher<DomainEventStream>>();
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
            Console.WriteLine("--Start to publish event sync, total count: {0}.", eventCount);
            var watch = Stopwatch.StartNew();
            foreach (var eventStream in eventStreams)
            {
                eventPublisher.Publish(eventStream);
                publishedEventCount++;
                if (publishedEventCount % printSize == 0)
                {
                    Console.WriteLine("----Sent {0} events sync, time spent: {1}ms", publishedEventCount, watch.ElapsedMilliseconds);
                }
            }
            Console.WriteLine("--Event publish sync completed, throughput: {0}/s", eventCount * 1000 / watch.ElapsedMilliseconds);
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
