using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using ECommon.Autofac;
using ECommon.Components;
using ECommon.JsonNet;
using ECommon.Log4Net;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Infrastructure;
using NoteSample.Commands;
using ECommonConfiguration = ECommon.Configurations.Configuration;

namespace ENode.CommandProcessorPerfTests
{
    class Program
    {
        static ENodeConfiguration _configuration;
        static ManualResetEvent _waitHandle;
        static ILogger _logger;
        static Stopwatch _watch;
        static IRepository _repository;
        static IMessageProcessor<ProcessingCommand, ICommand, CommandResult> _commandProcessor;
        static int _commandCount;
        static int _executedCount;

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            _commandCount = int.Parse(ConfigurationManager.AppSettings["count"]);
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create("main");
            _repository = ObjectContainer.Resolve<IRepository>();
            _commandProcessor = ObjectContainer.Resolve<IMessageProcessor<ProcessingCommand, ICommand, CommandResult>>();

            var createCommands = new List<ProcessingCommand>();
            var updateCommands = new List<ProcessingCommand>();
            for (var i = 0; i < _commandCount; i++)
            {
                var noteId = ObjectId.GenerateNewStringId();
                createCommands.Add(new ProcessingCommand(new CreateNoteCommand
                {
                    AggregateRootId = noteId,
                    Title = "Sample Note"
                }, new CommandExecuteContext(), new Dictionary<string, string>()));
                updateCommands.Add(new ProcessingCommand(new ChangeNoteTitleCommand
                {
                    AggregateRootId = noteId,
                    Title = "Changed Note Title"
                }, new CommandExecuteContext(), new Dictionary<string, string>()));
            }

            _waitHandle = new ManualResetEvent(false);
            _watch = Stopwatch.StartNew();
            Console.WriteLine("--Start to process create aggregate commands, total count: {0}.", _commandCount);
            foreach (var command in createCommands)
            {
                _commandProcessor.Process(command);
            }
            _waitHandle.WaitOne();
            Console.WriteLine("--Commands process completed, throughput: {0}/s", _commandCount * 1000 / _watch.ElapsedMilliseconds);

            _executedCount = 0;
            _waitHandle = new ManualResetEvent(false);
            _watch = Stopwatch.StartNew();
            Console.WriteLine("");
            Console.WriteLine("--Start to process update aggregate commands, total count: {0}.", _commandCount);
            foreach (var command in updateCommands)
            {
                _commandProcessor.Process(command);
            }
            _waitHandle.WaitOne();
            Console.WriteLine("--Commands process completed, throughput: {0}/s", _commandCount * 1000 / _watch.ElapsedMilliseconds);

            Console.ReadLine();
        }

        static void InitializeENodeFramework()
        {
            var setting = new ConfigurationSetting
            {
                SqlServerDefaultConnectionString = ConfigurationManager.AppSettings["connectionString"],
                EnableGroupCommitEvent = bool.Parse(ConfigurationManager.AppSettings["batchCommit"]),
                GroupCommitMaxSize = int.Parse(ConfigurationManager.AppSettings["batchSize"])
            };
            var assemblies = new[]
            {
                Assembly.Load("NoteSample.Domain"),
                Assembly.Load("NoteSample.Commands"),
                Assembly.Load("NoteSample.CommandHandlers"),
                Assembly.GetExecutingAssembly()
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
        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<string, IAggregateRoot> _aggregateRoots;
            private readonly int _printSize;

            public CommandExecuteContext()
            {
                _aggregateRoots = new ConcurrentDictionary<string, IAggregateRoot>();
                _printSize = _commandCount / 10;
            }

            public void OnCommandExecuted(CommandResult commandResult)
            {
                if (commandResult.Status != CommandStatus.Success)
                {
                    _logger.Info("Command execute failed.");
                    return;
                }
                var currentCount = Interlocked.Increment(ref _executedCount);
                if (currentCount % _printSize == 0)
                {
                    Console.WriteLine("----Processed {0} commands, timespent:{1}ms", currentCount, _watch.ElapsedMilliseconds);
                }
                if (currentCount == _commandCount)
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
