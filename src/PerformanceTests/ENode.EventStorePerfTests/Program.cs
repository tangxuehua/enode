using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Serilog;
using ECommon.Utilities;
using ENode.Configurations;
using ENode.Eventing;
using ENode.SqlServer;
using ECommonConfiguration = ECommon.Configurations.Configuration;

namespace ENode.EventStorePerfTests
{
    class Program
    {
        static ENodeConfiguration _configuration;

        class TestEvent : DomainEvent<string> { }

        static void Main(string[] args)
        {
            InitializeENodeFramework();
            BatchAppendAsyncTest();
            Console.ReadLine();
        }
        static void BatchAppendAsyncTest()
        {
            Console.WriteLine("");

            var aggreagateRootId1 = ObjectId.GenerateNewStringId();
            var aggreagateRootId2 = ObjectId.GenerateNewStringId();
            var count = int.Parse(ConfigurationManager.AppSettings["batchAppendAsyncount"]);
            var batchSize = int.Parse(ConfigurationManager.AppSettings["batchSize"]);
            var printSize = count / 10;
            var finishedCount = 0;
            var eventStore = ObjectContainer.Resolve<IEventStore>();
            var eventQueue = new Queue<DomainEventStream>();

            for (var i = 1; i <= count; i++)
            {
                var aggregateRootId = i % 2 == 0 ? aggreagateRootId1 : aggreagateRootId2;
                var evnt = new TestEvent
                {
                    AggregateRootId = aggregateRootId,
                    AggregateRootStringId = aggregateRootId,
                    Version = i
                };
                var eventStream = new DomainEventStream(ObjectId.GenerateNewStringId(), aggregateRootId, "SampleAggregateRootTypeName", DateTime.Now, new IDomainEvent[] { evnt });
                eventQueue.Enqueue(eventStream);
            }

            var watch = Stopwatch.StartNew();
            var context = new BatchAppendContext
            {
                BatchSize = batchSize,
                PrintSize = printSize,
                FinishedCount = finishedCount,
                EventStore = eventStore,
                EventQueue = eventQueue,
                Watch = watch
            };

            Console.WriteLine("start to batch append test, totalCount:" + count);

            DoBatchAppendAsync(context).Wait();
        }
        static async Task DoBatchAppendAsync(BatchAppendContext context)
        {
            var eventList = new List<DomainEventStream>();

            while (context.EventQueue.Count > 0)
            {
                var evnt = context.EventQueue.Dequeue();
                eventList.Add(evnt);
                if (eventList.Count == context.BatchSize)
                {
                    context.EventList = eventList;
                    await BatchAppendAsync(context);
                    return;
                }
            }

            if (eventList.Count > 0)
            {
                context.EventList = eventList;
                await BatchAppendAsync(context);
            }

            Console.WriteLine("batch append throughput: {0} events/s", 1000 * context.FinishedCount / context.Watch.ElapsedMilliseconds);
        }
        static async Task BatchAppendAsync(BatchAppendContext context)
        {
            var result = await context.EventStore.BatchAppendAsync(context.EventList);

            if (result.DuplicateEventAggregateRootIdList.Count > 0)
            {
                Console.WriteLine("duplicated event stream.");
                return;
            }
            else if (result.DuplicateCommandAggregateRootIdList.Count > 0)
            {
                Console.WriteLine("duplicated command execution.");
                return;
            }
            var local = Interlocked.Add(ref context.FinishedCount, context.EventList.Count);
            if (local % context.PrintSize == 0)
            {
                Console.WriteLine("batch appended {0}, time:{1}", local, context.Watch.ElapsedMilliseconds);
            }

            await DoBatchAppendAsync(context);
        }
        static void InitializeENodeFramework()
        {
            var connectionString = ConfigurationManager.AppSettings["connectionString"];
            var loggerFactory = new SerilogLoggerFactory()
                .AddFileLogger("ECommon", "logs\\ecommon")
                .AddFileLogger("EQueue", "logs\\equeue")
                .AddFileLogger("ENode", "logs\\enode", minimumLevel: Serilog.Events.LogEventLevel.Error);
            _configuration = ECommonConfiguration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseSerilog(loggerFactory)
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .CreateENode()
                .RegisterENodeComponents()
                .UseSqlServerEventStore()
                .BuildContainer()
                .InitializeSqlServerEventStore(connectionString);

            Console.WriteLine("ENode started...");
        }
        class BatchAppendContext
        {
            public int BatchSize;
            public int PrintSize;
            public int FinishedCount;
            public IEventStore EventStore;
            public Queue<DomainEventStream> EventQueue;
            public Stopwatch Watch;
            public IList<DomainEventStream> EventList;
        }
    }
}
