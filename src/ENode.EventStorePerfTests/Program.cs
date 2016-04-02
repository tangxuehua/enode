using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Utilities;
using ENode.Configurations;
using ENode.Eventing;
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
            AppendAsyncTest();
            BatchAppendAsyncTest();
            Console.ReadLine();
        }
        static void AppendAsyncTest()
        {
            var aggreagateRootId = ObjectId.GenerateNewStringId();
            var count = int.Parse(ConfigurationManager.AppSettings["appendAsyncCount"]);
            var printSize = count / 10;
            var eventStore = ObjectContainer.Resolve<IEventStore>();
            var createEventStream = new Func<int, DomainEventStream>(version =>
            {
                var evnt = new TestEvent
                {
                    AggregateRootId = aggreagateRootId,
                    Version = version
                };
                var eventStream = new DomainEventStream(ObjectId.GenerateNewStringId(), aggreagateRootId, "SampleAggregateRootTypeName", version, DateTime.Now, new IDomainEvent[] { evnt });
                return eventStream;
            });

            Console.WriteLine("start to append test, totalCount:" + count);

            var current = 0;
            var watch = Stopwatch.StartNew();
            var waitHandle = new ManualResetEvent(false);

            for (var i = 1; i <= count; i++)
            {
                eventStore.AppendAsync(createEventStream(i)).ContinueWith(t =>
                {
                    if (t.Result.Data == EventAppendResult.DuplicateEvent)
                    {
                        Console.WriteLine("duplicated event stream.");
                        return;
                    }
                    else if (t.Result.Data == EventAppendResult.DuplicateCommand)
                    {
                        Console.WriteLine("duplicated command execution.");
                        return;
                    }
                    var local = Interlocked.Increment(ref current);
                    if (local % printSize == 0)
                    {
                        Console.WriteLine("appended {0}, time:{1}", local, watch.ElapsedMilliseconds);
                        if (local == count)
                        {
                            Console.WriteLine("append throughput: {0} events/s", 1000 * local / watch.ElapsedMilliseconds);
                            waitHandle.Set();
                        }
                    }
                });
            }

            waitHandle.WaitOne();
        }
        static void BatchAppendAsyncTest()
        {
            Console.WriteLine("");

            var aggreagateRootId = ObjectId.GenerateNewStringId();
            var count = int.Parse(ConfigurationManager.AppSettings["batchAppendAsyncount"]);
            var batchSize = int.Parse(ConfigurationManager.AppSettings["batchSize"]);
            var printSize = count / 10;
            var finishedCount = 0;
            var eventStore = ObjectContainer.Resolve<IEventStore>();
            var eventQueue = new Queue<DomainEventStream>();

            for (var i = 1; i <= count; i++)
            {
                var evnt = new TestEvent
                {
                    AggregateRootId = aggreagateRootId,
                    Version = i
                };
                var eventStream = new DomainEventStream(ObjectId.GenerateNewStringId(), aggreagateRootId, "SampleAggregateRootTypeName", i, DateTime.Now, new IDomainEvent[] { evnt });
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

            DoBatchAppend(context);
        }
        static void DoBatchAppend(BatchAppendContext context)
        {
            var eventList = new List<DomainEventStream>();

            while (context.EventQueue.Count > 0)
            {
                var evnt = context.EventQueue.Dequeue();
                eventList.Add(evnt);
                if (eventList.Count == context.BatchSize)
                {
                    context.EventList = eventList;
                    BatchAppendAsync(context);
                    return;
                }
            }

            if (eventList.Count > 0)
            {
                context.EventList = eventList;
                BatchAppendAsync(context);
            }

            Console.WriteLine("batch append throughput: {0} events/s", 1000 * context.FinishedCount / context.Watch.ElapsedMilliseconds);
        }
        static async void BatchAppendAsync(BatchAppendContext context)
        {
            var result = await context.EventStore.BatchAppendAsync(context.EventList);

            if (result.Data == EventAppendResult.DuplicateEvent)
            {
                Console.WriteLine("duplicated event stream.");
                return;
            }
            else if (result.Data == EventAppendResult.DuplicateCommand)
            {
                Console.WriteLine("duplicated command execution.");
                return;
            }
            var local = Interlocked.Add(ref context.FinishedCount, context.EventList.Count);
            if (local % context.PrintSize == 0)
            {
                Console.WriteLine("batch appended {0}, time:{1}", local, context.Watch.ElapsedMilliseconds);
            }

            DoBatchAppend(context);
        }
        static void InitializeENodeFramework()
        {
            var setting = new ConfigurationSetting(ConfigurationManager.AppSettings["connectionString"]);
            _configuration = ECommonConfiguration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .CreateENode(setting)
                .RegisterENodeComponents()
                .UseSqlServerEventStore();

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
