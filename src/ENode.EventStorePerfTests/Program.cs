using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.IO;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Utilities;
using ENode.Configurations;
using ENode.Domain;
using ENode.Eventing;
using NoteSample.Domain;
using ECommonConfiguration = ECommon.Configurations.Configuration;

namespace ENode.EventStorePerfTests
{
    class Program
    {
        static ENodeConfiguration _configuration;

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var note = new Note(ObjectId.GenerateNewStringId(), "Sample Note Title");
            var evnts = (note as IAggregateRoot).GetChanges();
            var count = int.Parse(ConfigurationManager.AppSettings["count"]);
            var mode = ConfigurationManager.AppSettings["mode"];
            var eventStore = ObjectContainer.Resolve<IEventStore>();
            var watch = Stopwatch.StartNew();

            if (mode == "async")
            {
                var current = 0;
                for (var i = 1; i <= count; i++)
                {
                    eventStore.AppendAsync(new DomainEventStream(ObjectId.GenerateNewStringId(), note.Id, 100, i, DateTime.Now, evnts)).ContinueWith(t =>
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
                        batch.Add(new DomainEventStream(ObjectId.GenerateNewStringId(), note.Id, 100, i, DateTime.Now, evnts));
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
                SqlServerDefaultConnectionString = ConfigurationManager.AppSettings["connectionString"],
                EnableGroupCommitEvent = true
            };
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Domain")
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
                .RegisterAllTypeCodes()
                .UseSqlServerEventStore()
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies);

            Console.WriteLine("ENode started...");
        }
    }
}
