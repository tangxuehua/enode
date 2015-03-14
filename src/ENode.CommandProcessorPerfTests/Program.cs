using System;
using System.Collections.Concurrent;
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
using ENode.Domain;
using ENode.Infrastructure;
using NoteSample.Commands;

namespace ENode.CommandProcessorPerfTests
{
    class Program
    {
        static ENodeConfiguration _configuration;
        static ManualResetEvent _waitHandle = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            InitializeENodeFramework();
            ProcessCreateAggregateCommands(100000);
            Console.ReadLine();
        }

        static void ProcessCreateAggregateCommands(int commandCount)
        {
            var commandProcessor = ObjectContainer.Resolve<IMessageProcessor<ProcessingCommand, ICommand, CommandResult>>();
            var repository = ObjectContainer.Resolve<IRepository>();
            var watch = Stopwatch.StartNew();
            var commands = new List<ProcessingCommand>();
            var logger = ObjectContainer.Resolve<ILoggerFactory>().Create("main");

            for (var i = 1; i <= commandCount; i++)
            {
                commands.Add(new ProcessingCommand(new CreateNoteCommand
                {
                    AggregateRootId = i.ToString(),
                    Title = "Sample Note"
                }, new CommandExecuteContext(watch, commandCount, logger, repository), null, null, new Dictionary<string, string>()));
            }

            Console.WriteLine("--Start to process commands, total count: {0}.", commandCount);
            foreach (var command in commands)
            {
                commandProcessor.Process(command);
            }
            _waitHandle.WaitOne();
            Console.WriteLine("--Commands process completed, throughput: {0}/s", commandCount * 1000 / watch.ElapsedMilliseconds);
        }
        static void InitializeENodeFramework()
        {
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Domain"),
                Assembly.Load("NoteSample.Commands"),
                Assembly.Load("NoteSample.CommandHandlers"),
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
                .RegisterAllTypeCodes()
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies);

            Console.WriteLine("ENode started...");
        }
        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<string, IAggregateRoot> _aggregateRoots;
            private readonly IRepository _repository;
            private readonly ILogger _logger;
            private readonly Stopwatch _watch;
            private readonly int _totalCount;
            private readonly int _printSize;
            private static int _executedCount;

            public CommandExecuteContext(Stopwatch watch, int totalCount, ILogger logger, IRepository repository)
            {
                _watch = watch;
                _aggregateRoots = new ConcurrentDictionary<string, IAggregateRoot>();
                _logger = logger;
                _repository = repository;
                _totalCount = totalCount;
                _printSize = totalCount / 10;
            }

            public void OnCommandExecuted(CommandResult commandResult)
            {
                var currentCount = Interlocked.Increment(ref _executedCount);
                if (currentCount % _printSize == 0)
                {
                    Console.WriteLine("----Processed {0} commands, timespent:{1}ms", currentCount, _watch.ElapsedMilliseconds);
                }
                if (currentCount == _totalCount)
                {
                    _waitHandle.Set();
                }
            }
            public void Add(IAggregateRoot aggregateRoot)
            {
                if (aggregateRoot == null)
                {
                    throw new ArgumentNullException("aggregateRoot");
                }
                if (!_aggregateRoots.TryAdd(aggregateRoot.UniqueId, aggregateRoot))
                {
                    throw new AggregateRootAlreadyExistException(aggregateRoot.UniqueId, aggregateRoot.GetType());
                }
            }
            public T Get<T>(object id) where T : class, IAggregateRoot
            {
                if (id == null)
                {
                    throw new ArgumentNullException("id");
                }

                IAggregateRoot aggregateRoot = null;
                if (_aggregateRoots.TryGetValue(id.ToString(), out aggregateRoot))
                {
                    return aggregateRoot as T;
                }

                aggregateRoot = _repository.Get<T>(id);

                if (aggregateRoot != null)
                {
                    _aggregateRoots.TryAdd(aggregateRoot.UniqueId, aggregateRoot);
                    return aggregateRoot as T;
                }

                return null;
            }
            public IEnumerable<IAggregateRoot> GetTrackedAggregateRoots()
            {
                return _aggregateRoots.Values;
            }
            public void Clear()
            {
                _aggregateRoots.Clear();

            }
        }
    }
}
