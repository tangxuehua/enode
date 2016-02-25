using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Logging;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Eventing;
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
        static ICommandProcessor _commandProcessor;
        static IEventService _eventService;
        static int _commandCount;
        static int _executedCount;

        static void Main(string[] args)
        {
            InitializeENodeFramework();

            _commandCount = int.Parse(ConfigurationManager.AppSettings["count"]);
            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create("main");
            _repository = ObjectContainer.Resolve<IRepository>();
            _commandProcessor = ObjectContainer.Resolve<ICommandProcessor>();

            var noteId = ObjectId.GenerateNewStringId();
            var updateCommands = new List<ProcessingCommand>();
            var createCommand = new ProcessingCommand(new CreateNoteCommand
            {
                AggregateRootId = noteId,
                Title = "Sample Note"
            }, new CommandExecuteContext(_commandCount), new Dictionary<string, string>());

            for (var i = 0; i < _commandCount; i++)
            {
                updateCommands.Add(new ProcessingCommand(new ChangeNoteTitleCommand
                {
                    AggregateRootId = noteId,
                    Title = "Changed Note Title"
                }, new CommandExecuteContext(_commandCount), new Dictionary<string, string>()));
            }

            _waitHandle = new ManualResetEvent(false);
            _commandProcessor.Process(createCommand);
            _waitHandle.WaitOne();

            _waitHandle = new ManualResetEvent(false);
            _watch = Stopwatch.StartNew();
            Console.WriteLine("");
            Console.WriteLine("--Start to process aggregate commands, total count: {0}.", _commandCount + 1);

            foreach (var updateCommand in updateCommands)
            {
                _commandProcessor.Process(updateCommand);
            }
            _waitHandle.WaitOne();
            Console.WriteLine("--Commands process completed, throughput: {0}/s", _commandCount * 1000 / _watch.ElapsedMilliseconds);

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
            var connectionString = ConfigurationManager.AppSettings["connectionString"];
            var setting = new ConfigurationSetting
            {
                SqlDefaultConnectionString = connectionString,
                EventMailBoxPersistenceMaxBatchSize = 1000
            };
            var optionSetting = new OptionSetting(
                new KeyValuePair<string, object>("ConnectionString", connectionString),
                new KeyValuePair<string, object>("TableName", "EventStream"),
                new KeyValuePair<string, object>("VersionIndexName", "IX_EventStream_AggId_Version"),
                new KeyValuePair<string, object>("CommandIndexName", "IX_EventStream_AggId_CommandId"),
                new KeyValuePair<string, object>("TableCount", int.Parse(ConfigurationManager.AppSettings["tableCount"])),
                new KeyValuePair<string, object>("BulkCopyBatchSize", 1000),
                new KeyValuePair<string, object>("BulkCopyTimeout", 60));

            _configuration = ECommonConfiguration
                .Create()
                .UseAutofac()
                .RegisterCommonComponents()
                .UseLog4Net()
                .UseJsonNet()
                .RegisterUnhandledExceptionHandler()
                .CreateENode(setting)
                .RegisterENodeComponents()
                .UseSqlServerEventStore(optionSetting)
                .RegisterBusinessComponents(assemblies)
                .InitializeBusinessAssemblies(assemblies);
            _eventService = ObjectContainer.Resolve<IEventService>();

            Console.WriteLine("ENode started...");
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<string, IAggregateRoot> _aggregateRoots;
            private readonly int _printSize;
            private string _result;

            public CommandExecuteContext(int commandCount)
            {
                _aggregateRoots = new ConcurrentDictionary<string, IAggregateRoot>();
                _printSize = commandCount / 10;
            }

            public void OnCommandExecuted(CommandResult commandResult)
            {
                if (commandResult.Status != CommandStatus.Success)
                {
                    _logger.Info("Command execute failed.");
                    return;
                }
                var currentCount = Interlocked.Increment(ref _executedCount);
                if (currentCount == 1)
                {
                    _waitHandle.Set();
                }
                if (currentCount > 1 && (currentCount - 1) % _printSize == 0)
                {
                    Console.WriteLine("----Processed {0} commands, timespent:{1}ms", currentCount, _watch.ElapsedMilliseconds);
                }
                if ((currentCount - 1) == _commandCount)
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
            public T Get<T>(object id, bool firstFormCache = true) where T : class, IAggregateRoot
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
                _result = null;
            }
            public void SetResult(string result)
            {
                _result = result;
            }
            public string GetResult()
            {
                return _result;
            }
        }
    }
}
