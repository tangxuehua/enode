using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Threading;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.IO;
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

            var aggreagateRootId = ObjectId.GenerateNewStringId();
            var count = int.Parse(ConfigurationManager.AppSettings["count"]);
            var mode = ConfigurationManager.AppSettings["mode"];
            var eventStore = ObjectContainer.Resolve<IEventStore>();
            var watch = Stopwatch.StartNew();

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

            if (mode == "async")
            {
                var current = 0;

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
                        if (local % 1000 == 0)
                        {
                            Console.WriteLine("{0}, time:{1}", local, watch.ElapsedMilliseconds);
                        }
                    });
                }
            }
            else if (mode == "batchAsync")
            {
                var current = 0;
                var batchSize = 1000;
                var batch = new List<DomainEventStream>();
                for (var i = 1; i <= count; i++)
                {
                    if (i % batchSize == 0)
                    {
                        eventStore.BatchAppendAsync(batch).ContinueWith(t =>
                        {
                            if (t.Result.Status != AsyncTaskStatus.Success)
                            {
                                Console.WriteLine(t.Result.ErrorMessage);
                                return;
                            }
                            Console.WriteLine("{0}, time:{1}", Interlocked.Increment(ref current) * batchSize, watch.ElapsedMilliseconds);
                        });
                        batch = new List<DomainEventStream>();
                    }
                    else
                    {
                        batch.Add(createEventStream(i));
                    }
                }
                if (batch.Count > 0)
                {
                    eventStore.BatchAppendAsync(batch).ContinueWith(t =>
                    {
                        if (t.Result.Status != AsyncTaskStatus.Success)
                        {
                            Console.WriteLine(t.Result.ErrorMessage);
                            return;
                        }
                        Console.WriteLine("{0}, time:{1}", Interlocked.Increment(ref current) * batchSize, watch.ElapsedMilliseconds);
                    });
                }
            }

            Console.ReadLine();
        }
        static void InitializeENodeFramework()
        {
            var setting = new ConfigurationSetting
            {
                SqlDefaultConnectionString = ConfigurationManager.AppSettings["connectionString"],
                EnableGroupCommitEvent = true
            };
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
    }
}
