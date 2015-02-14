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
using ENode.Eventing;
using NoteSample.Commands;

namespace ENode.CommandProcessorPerfTests
{
    class Program
    {
        static ILogger _logger;
        static ENodeConfiguration _configuration;
        static int _totalCount = 100000;
        static ManualResetEvent _waitHandle = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            var commandProcessor = ObjectContainer.Resolve<ICommandProcessor>();
            var repository = ObjectContainer.Resolve<IRepository>();
            var watch = Stopwatch.StartNew();
            var commands = new List<ProcessingCommand>();

            for (var i = 1; i <= _totalCount; i++)
            {
                commands.Add(new ProcessingCommand(
                    new CreateNoteCommand
                    {
                        AggregateRootId = i.ToString(),
                        Title = "Sample Note"
                    }, new CommandExecuteContext(_logger, repository), null, null, new Dictionary<string, string>()));
            }

            Console.WriteLine("Start to process commands.");
            foreach (var command in commands)
            {
                commandProcessor.Process(command);
            }
            _waitHandle.WaitOne();
            Console.WriteLine("Commands process completed, time spent: {0}ms", watch.ElapsedMilliseconds);
            Console.ReadLine();
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
                .CreateENode()
                .RegisterENodeComponents()
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies)
                .StartENode(NodeType.CommandProcessor);

            Console.WriteLine(string.Empty);

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(Program).Name);
            _logger.Info("ENode started...");
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<string, IAggregateRoot> _aggregateRoots;
            private readonly ConcurrentDictionary<string, IEvent> _events;
            private readonly IRepository _repository;
            private readonly ILogger _logger;
            private static int _executedCount;

            public CommandExecuteContext(ILogger logger, IRepository repository)
            {
                _aggregateRoots = new ConcurrentDictionary<string, IAggregateRoot>();
                _events = new ConcurrentDictionary<string, IEvent>();
                _logger = logger;
                _repository = repository;
            }

            public void OnCommandExecuted(CommandResult commandResult)
            {
                var currentCount = Interlocked.Increment(ref _executedCount);
                if (currentCount % 1000 == 0)
                {
                    Console.WriteLine("Executed {0} commands.", currentCount);
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
            public void Add(IEvent evnt)
            {
                if (evnt == null)
                {
                    throw new ArgumentNullException("evnt");
                }
                if (!_events.TryAdd(evnt.Id, evnt))
                {
                    throw new EventAlreadyExistException(evnt.Id, evnt.GetType());
                }
            }
            public IEnumerable<IAggregateRoot> GetTrackedAggregateRoots()
            {
                return _aggregateRoots.Values;
            }
            public IEnumerable<IEvent> GetEvents()
            {
                return _events.Values;
            }
            public void Clear()
            {
                _aggregateRoots.Clear();
                _events.Clear();
            }
        }
    }
}
