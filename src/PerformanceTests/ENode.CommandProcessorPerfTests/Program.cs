using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ECommon.Components;
using ECommon.Configurations;
using ECommon.Logging;
using ECommon.Serilog;
using ECommon.Utilities;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Eventing;
using ENode.Messaging;
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
        static IEventCommittingService _eventService;
        static int _commandCount;
        static int _executedCount;
        static int _totalCommandCount;
        static bool _isUpdating;

        static void Main(string[] args)
        {
            _commandCount = int.Parse(ConfigurationManager.AppSettings["count"]);

            InitializeENodeFramework();

            var aggregateRootId = ObjectId.GenerateNewStringId();
            var createCommand = new ProcessingCommand(new CreateNoteCommand
            {
                AggregateRootId = aggregateRootId,
                Title = "Sample Note"
            }, new CommandExecuteContext(_commandCount), new Dictionary<string, string>());
            var updateCommands = new List<ProcessingCommand>();
            for (var i = 0; i < _commandCount; i++)
            {
                updateCommands.Add(new ProcessingCommand(new ChangeNoteTitleCommand
                {
                    AggregateRootId = aggregateRootId,
                    Title = "Changed Note Title"
                }, new CommandExecuteContext(_commandCount), new Dictionary<string, string>()));
            }

            _totalCommandCount = 1;
            _waitHandle = new ManualResetEvent(false);
            _commandProcessor.Process(createCommand);
            _waitHandle.WaitOne();

            _isUpdating = true;
            _executedCount = 0;
            _totalCommandCount = updateCommands.Count;
            _waitHandle = new ManualResetEvent(false);
            _watch = Stopwatch.StartNew();
            Console.WriteLine("");
            Console.WriteLine("--Start to update aggregate concurrently, total count: {0}.", _totalCommandCount);

            Task.Factory.StartNew(() =>
            {
                foreach (var updateCommand in updateCommands)
                {
                    _commandProcessor.Process(updateCommand);
                }
            });

            _waitHandle.WaitOne();
            Console.WriteLine("--Completed, throughput: {0}/s", _totalCommandCount * 1000 / _watch.ElapsedMilliseconds);

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
                .RegisterBusinessComponents(assemblies)
                .BuildContainer()
                .InitializeBusinessAssemblies(assemblies);
            _eventService = ObjectContainer.Resolve<IEventCommittingService>();

            _logger = ObjectContainer.Resolve<ILoggerFactory>().Create("main");
            _repository = ObjectContainer.Resolve<IRepository>();
            _commandProcessor = ObjectContainer.Resolve<ICommandProcessor>();

            Console.WriteLine("ENode started...");
        }

        class CommandExecuteContext : ICommandExecuteContext
        {
            private readonly ConcurrentDictionary<string, IAggregateRoot> _aggregateRoots;
            private readonly int _printSize;
            private string _result;
            private IApplicationMessage _applicationMessage;

            public CommandExecuteContext(int commandCount)
            {
                _aggregateRoots = new ConcurrentDictionary<string, IAggregateRoot>();
                _printSize = commandCount / 10;
            }

            public Task OnCommandExecutedAsync(CommandResult commandResult)
            {
                if (commandResult.Status != CommandStatus.Success)
                {
                    _logger.Info("Command execute failed.");
                    return Task.CompletedTask;
                }
                var currentCount = Interlocked.Increment(ref _executedCount);
                if (_isUpdating && currentCount % _printSize == 0)
                {
                    Console.WriteLine("----Processed {0} commands, timespent:{1}ms", currentCount, _watch.ElapsedMilliseconds);
                }
                if (currentCount == _totalCommandCount)
                {
                    _waitHandle.Set();
                }
                return Task.CompletedTask;
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
            public Task AddAsync(IAggregateRoot aggregateRoot)
            {
                Add(aggregateRoot);
                return Task.CompletedTask;
            }
            public async Task<T> GetAsync<T>(object id, bool firstFormCache = true) where T : class, IAggregateRoot
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

                aggregateRoot = await _repository.GetAsync<T>(id);

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

            public void SetApplicationMessage(IApplicationMessage applicationMessage)
            {
                _applicationMessage = applicationMessage;
            }

            public IApplicationMessage GetApplicationMessage()
            {
                return _applicationMessage;
            }
        }
    }
}
