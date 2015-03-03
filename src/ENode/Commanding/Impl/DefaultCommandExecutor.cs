using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ENode.Domain;
using ENode.Eventing;
using ENode.Exceptions;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandExecutor : ICommandExecutor
    {
        #region Private Variables

        private readonly ICommandStore _commandStore;
        private readonly IEventStore _eventStore;
        private readonly ICommandHandlerProvider _commandHandlerProvider;
        private readonly ITypeCodeProvider _aggregateRootTypeProvider;
        private readonly IEventService _eventService;
        private readonly IPublisher<IPublishableException> _exceptionPublisher;
        private readonly IMemoryCache _memoryCache;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultCommandExecutor(
            ICommandStore commandStore,
            IEventStore eventStore,
            ICommandHandlerProvider commandHandlerProvider,
            ITypeCodeProvider aggregateRootTypeProvider,
            IEventService eventService,
            IPublisher<IPublishableException> exceptionPublisher,
            IMemoryCache memoryCache,
            IOHelper ioHelper,
            ILoggerFactory loggerFactory)
        {
            _commandStore = commandStore;
            _eventStore = eventStore;
            _commandHandlerProvider = commandHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _eventService = eventService;
            _exceptionPublisher = exceptionPublisher;
            _memoryCache = memoryCache;
            _ioHelper = ioHelper;
            _logger = loggerFactory.Create(GetType().FullName);
            _eventService.SetCommandExecutor(this);
        }

        #endregion

        #region Public Methods

        public void ExecuteCommand(ProcessingCommand processingCommand)
        {
            if (processingCommand.Command is IAggregateCommand)
            {
                DoExecuteCommand(processingCommand);
            }
            else
            {
                ProcessCommand(processingCommand, 0);
            }
        }

        #endregion

        #region Private Methods

        private void ProcessCommand(ProcessingCommand processingCommand, int retryTimes)
        {
            var command = processingCommand.Command;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<HandledCommand>>("GetCommandAsync",
            () => _commandStore.GetAsync(command.Id),
            currentRetryTimes => ProcessCommand(processingCommand, currentRetryTimes),
            result =>
            {
                var existingHandledCommand = result.Data;
                if (existingHandledCommand != null)
                {
                    NotifyCommandExecuted(processingCommand, CommandStatus.NothingChanged, null, null);
                    return;
                }
                DoExecuteCommand(processingCommand);
            },
            () => string.Format("[commandId:{0}]", command.Id),
            () => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Get command async failed."),
            retryTimes);
        }
        private void DoExecuteCommand(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Command;

            //获取Command Handler
            var commandHandler = GetCommandHandler(processingCommand);
            if (commandHandler == null)
            {
                return;
            }

            //调用command handler执行当前command
            var handleSuccess = false;
            try
            {
                commandHandler.Handle(processingCommand.CommandExecuteContext, command);
                _logger.DebugFormat("Handle command success. handlerType:{0}, commandType:{1}, commandId:{2}, aggregateRootId:{3}",
                    commandHandler.GetInnerHandler().GetType().Name,
                    command.GetType().Name,
                    command.Id,
                    processingCommand.AggregateRootId);
                handleSuccess = true;
            }
            catch (IOException ex)
            {
                _logger.Error(ex);
                RetryCommand(processingCommand);
                return;
            }
            catch (Exception ex)
            {
                HandleExceptionAsync(processingCommand, commandHandler, ex, 0);
                return;
            }

            //如果command执行成功，则提交执行后的结果
            if (handleSuccess)
            {
                try
                {
                    if (command is IAggregateCommand)
                    {
                        CommitAggregateChanges(processingCommand);
                    }
                    else
                    {
                        CommitChanges(processingCommand);
                    }
                }
                catch (Exception ex)
                {
                    LogCommandExecuteException(processingCommand, commandHandler, ex);
                    NotifyCommandExecuted(processingCommand, CommandStatus.Failed, ex.GetType().Name, "Unknown exception caught when committing changes of command.");
                }
            }
        }
        private ICommandHandler GetCommandHandler(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Command;
            var commandHandlers = _commandHandlerProvider.GetHandlers(command.GetType());
            var handlerCount = commandHandlers.Count();

            if (handlerCount == 0)
            {
                _logger.ErrorFormat("No command handler found, commandType:{0}, commandId:{1}.", command.GetType().FullName, command.Id);
                NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "No command handler found.");
                return null;
            }
            else if (handlerCount > 1)
            {
                _logger.ErrorFormat("Found more than one command handlers, commandType:{0}, commandId:{1}.", command.GetType().FullName, command.Id);
                NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "More than one command handler found.");
                return null;
            }

            return commandHandlers.Single();
        }
        private void CommitAggregateChanges(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Command;
            var context = processingCommand.CommandExecuteContext;
            var trackedAggregateRoots = context.GetTrackedAggregateRoots();
            var dirtyAggregateRootChanges = trackedAggregateRoots.ToDictionary(x => x, x => x.GetChanges()).Where(x => x.Value.Any());
            var dirtyAggregateRootCount = dirtyAggregateRootChanges.Count();

            //如果当前command没有对任何聚合根做修改，则认为当前command已经处理结束，返回command的结果为NothingChanged
            if (dirtyAggregateRootCount == 0)
            {
                _logger.DebugFormat("No aggregate created or modified by command. commandType:{0}, commandId:{1}", command.GetType().Name, command.Id);
                NotifyCommandExecuted(processingCommand, CommandStatus.NothingChanged, null, null);
                return;
            }
            //如果被创建或修改的聚合根多于一个，则认为当前command处理失败，一个command只能创建或修改一个聚合根；
            else if (dirtyAggregateRootCount > 1)
            {
                var dirtyAggregateTypes = string.Join("|", dirtyAggregateRootChanges.Select(x => x.Key.GetType().Name));
                var errorMessage = string.Format("Detected more than one aggregate created or modified by command. commandType:{0}, commandId:{1}, dirty aggregate types:{2}",
                    command.GetType().Name,
                    command.Id,
                    dirtyAggregateTypes);
                _logger.ErrorFormat(errorMessage);
                NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage);
                return;
            }

            //获取当前被修改的聚合根
            var dirtyAggregateRootChange = dirtyAggregateRootChanges.Single();
            var dirtyAggregateRoot = dirtyAggregateRootChange.Key;
            var changedEvents = dirtyAggregateRootChange.Value;

            //构造出一个事件流对象
            var eventStream = BuildDomainEventStream(dirtyAggregateRoot, changedEvents, processingCommand);

            //如果当前command处于并发冲突的重试中，则直接提交该command产生的事件，因为该command肯定已经在command store中了
            if (processingCommand.ConcurrentRetriedCount > 0)
            {
                _eventService.CommitEventAsync(new EventCommittingContext(dirtyAggregateRoot, eventStream, processingCommand));
                return;
            }

            var handledAggregateCommand = new HandledAggregateCommand(command, processingCommand.SourceId, processingCommand.SourceType, eventStream.AggregateRootId, eventStream.AggregateRootTypeCode);
            CommitAggregateChangesAsync(processingCommand, dirtyAggregateRoot, eventStream, handledAggregateCommand, 0);
        }
        private void CommitChanges(ProcessingCommand processingCommand)
        {
            CommitChangesAsync(processingCommand, 0);
        }
        private DomainEventStream BuildDomainEventStream(IAggregateRoot aggregateRoot, IEnumerable<IDomainEvent> changedEvents, ProcessingCommand processingCommand)
        {
            return new DomainEventStream(
                processingCommand.Command.Id,
                aggregateRoot.UniqueId,
                _aggregateRootTypeProvider.GetTypeCode(aggregateRoot.GetType()),
                aggregateRoot.Version + 1,
                DateTime.Now,
                changedEvents,
                processingCommand.Items);
        }
        private void CommitAggregateChangesAsync(ProcessingCommand processingCommand, IAggregateRoot dirtyAggregateRoot, DomainEventStream eventStream, HandledAggregateCommand handledAggregateCommand, int retryTimes)
        {
            var command = processingCommand.Command;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<CommandAddResult>>("AddCommandAsync",
            () => _commandStore.AddAsync(handledAggregateCommand),
            currentRetryTimes => CommitAggregateChangesAsync(processingCommand, dirtyAggregateRoot, eventStream, handledAggregateCommand, currentRetryTimes),
            result =>
            {
                var commandAddResult = result.Data;
                if (commandAddResult == CommandAddResult.Success)
                {
                    _eventService.CommitEventAsync(new EventCommittingContext(dirtyAggregateRoot, eventStream, processingCommand));
                }
                else if (commandAddResult == CommandAddResult.DuplicateCommand)
                {
                    HandleAggregateDuplicatedCommandAsync(processingCommand, dirtyAggregateRoot, eventStream, 0);
                }
            },
            () => string.Format("[handledAggregateCommand:{0}]", handledAggregateCommand),
            () => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Add command async failed."),
            retryTimes);
        }
        private void HandleAggregateDuplicatedCommandAsync(ProcessingCommand processingCommand, IAggregateRoot dirtyAggregateRoot, DomainEventStream eventStream, int retryTimes)
        {
            var command = processingCommand.Command;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<HandledCommand>>("GetCommandAsync",
            () => _commandStore.GetAsync(command.Id),
            currentRetryTimes => HandleAggregateDuplicatedCommandAsync(processingCommand, dirtyAggregateRoot, eventStream, currentRetryTimes),
            result =>
            {
                var existingHandledCommand = result.Data as HandledAggregateCommand;
                if (existingHandledCommand != null)
                {
                    HandleExistingHandledAggregateAsync(processingCommand, dirtyAggregateRoot, eventStream, existingHandledCommand, 0);
                }
                else
                {
                    //到这里，说明当前command想添加到commandStore中时，提示command重复，但是尝试从commandStore中取出该command时却找不到该command。
                    //出现这种情况，我们就无法再做后续处理了，这种错误理论上不会出现，除非commandStore的Add接口和Get接口出现读写不一致的情况；
                    //我们记录错误日志，然后认为当前command已被处理为失败。
                    var errorMessage = string.Format("Command exist in the command store, but we cannot get it from the command store. commandType:{0}, commandId:{1}",
                        command.GetType().Name,
                        command.Id);
                    NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage);
                }
            },
            () => string.Format("[commandId:{0}]", command.Id),
            () => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Get command async failed."),
            retryTimes);
        }
        private void HandleExistingHandledAggregateAsync(ProcessingCommand processingCommand, IAggregateRoot dirtyAggregateRoot, DomainEventStream eventStream, HandledAggregateCommand existingHandledCommand, int retryTimes)
        {
            var command = processingCommand.Command;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<DomainEventStream>>("FindEventStreamByCommandIdAsync",
            () => _eventStore.FindAsync(existingHandledCommand.AggregateRootId, command.Id),
            currentRetryTimes => HandleExistingHandledAggregateAsync(processingCommand, dirtyAggregateRoot, eventStream, existingHandledCommand, currentRetryTimes),
            result =>
            {
                var existingEventStream = result.Data;
                if (existingEventStream != null)
                {
                    //如果当前command已经被持久化过了，且该command产生的事件也已经被持久化了，则只要再做一遍发布事件的操作
                    _eventService.PublishDomainEventAsync(processingCommand, existingEventStream);
                }
                else
                {
                    //如果当前command已经被持久化过了，但事件没有被持久化，则需要重新提交当前command所产生的事件；
                    _eventService.CommitEventAsync(new EventCommittingContext(dirtyAggregateRoot, eventStream, processingCommand));
                }
            },
            () => string.Format("[aggregateRootId:{0}, commandId:{1}]", existingHandledCommand.AggregateRootId, command.Id),
            () => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Find event stream by command id async failed."),
            retryTimes);
        }
        private void CommitChangesAsync(ProcessingCommand processingCommand, int retryTimes)
        {
            var command = processingCommand.Command;
            var evnts = processingCommand.CommandExecuteContext.GetEvents().ToList();
            var handledCommand = new HandledCommand(command, processingCommand.SourceId, processingCommand.SourceType, evnts);

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<CommandAddResult>>("AddCommandAsync",
            () => _commandStore.AddAsync(handledCommand),
            currentRetryTimes => CommitChangesAsync(processingCommand, currentRetryTimes),
            result =>
            {
                var commandAddResult = result.Data;
                if (commandAddResult == CommandAddResult.Success)
                {
                    _eventService.PublishEventAsync(processingCommand, new EventStream(command.Id, evnts, processingCommand.Items));
                }
                else if (commandAddResult == CommandAddResult.DuplicateCommand)
                {
                    HandleDuplicatedCommandAsync(processingCommand, 0);
                }
            },
            () => string.Format("[handledCommand:{0}]", handledCommand),
            () => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Add command async failed."),
            retryTimes);
        }
        private void HandleDuplicatedCommandAsync(ProcessingCommand processingCommand, int retryTimes)
        {
            var command = processingCommand.Command;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<HandledCommand>>("GetCommandAsync",
            () => _commandStore.GetAsync(command.Id),
            currentRetryTimes => HandleDuplicatedCommandAsync(processingCommand, currentRetryTimes),
            result =>
            {
                var existingHandledCommand = result.Data;
                if (existingHandledCommand != null)
                {
                    _eventService.PublishEventAsync(processingCommand, new EventStream(command.Id, existingHandledCommand.Events, processingCommand.Items));
                }
                else
                {
                    //到这里，说明当前command想添加到commandStore中时，提示command重复，但是尝试从commandStore中取出该command时却找不到该command。
                    //出现这种情况，我们就无法再做后续处理了，这种错误理论上不会出现，除非commandStore的Add接口和Get接口出现读写不一致的情况；
                    //我们记录错误日志，然后认为当前command已被处理为失败。
                    var errorMessage = string.Format("Command exist in the command store, but we cannot get it from the command store. commandType:{0}, commandId:{1}",
                        command.GetType().Name,
                        command.Id);
                    NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage);
                }
            },
            () => string.Format("[commandId:{0}]", command.Id),
            () => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Get command async failed."),
            retryTimes);
        }
        private void HandleExceptionAsync(ProcessingCommand processingCommand, ICommandHandler commandHandler, Exception exception, int retryTimes)
        {
            var command = processingCommand.Command;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<HandledCommand>>("GetCommandAsync",
            () => _commandStore.GetAsync(command.Id),
            currentRetryTimes => HandleExceptionAsync(processingCommand, commandHandler, exception, currentRetryTimes),
            result =>
            {
                var existingHandledCommand = result.Data;
                if (existingHandledCommand != null)
                {
                    if (command is IAggregateCommand)
                    {
                        HandleExistingHandledAggregateCommandForExceptionAsync(processingCommand, existingHandledCommand, commandHandler, exception, 0);
                    }
                    else
                    {
                        _eventService.PublishEventAsync(processingCommand, new EventStream(command.Id, existingHandledCommand.Events, processingCommand.Items));
                    }
                }
                else
                {
                    //到这里，说明当前command执行遇到异常，然后当前command之前也没执行过，是第一次被执行。
                    //那就判断当前异常是否是需要被发布出去的异常，如果是，则发布该异常给所有消费者；否则，就记录错误日志；
                    //然后，认为该command处理失败即可；
                    var publishableException = exception as IPublishableException;
                    if (publishableException != null)
                    {
                        PublishExceptionAsync(processingCommand, publishableException, 0);
                    }
                    else
                    {
                        LogCommandExecuteException(processingCommand, commandHandler, exception);
                        NotifyCommandExecuted(processingCommand, CommandStatus.Failed, exception.GetType().Name, exception.Message);
                    }
                }
            },
            () => string.Format("[commandId:{0}]", command.Id),
            () => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Get command async failed."),
            retryTimes);
        }
        private void PublishExceptionAsync(ProcessingCommand processingCommand, IPublishableException exception, int retryTimes)
        {
            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult>("PublishExceptionAsync",
            () => _exceptionPublisher.PublishAsync(exception),
            currentRetryTimes => PublishExceptionAsync(processingCommand, exception, currentRetryTimes),
            result =>
            {
                NotifyCommandExecuted(processingCommand, CommandStatus.Failed, exception.GetType().Name, (exception as Exception).Message);
            },
            () =>
            {
                var serializableInfo = new Dictionary<string, string>();
                exception.SerializeTo(serializableInfo);
                var exceptionInfo = string.Join(",", serializableInfo.Select(x => string.Format("{0}:{1}", x.Key, x.Value)));
                return string.Format("[commandId:{0}, exceptionInfo:{1}]", processingCommand.Command.Id, exceptionInfo);
            },
            () => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Publish exception async failed."),
            retryTimes);
        }
        private void HandleExistingHandledAggregateCommandForExceptionAsync(ProcessingCommand processingCommand, HandledCommand existingHandledCommand, ICommandHandler commandHandler, Exception exception, int retryTimes)
        {
            var command = processingCommand.Command;
            var existingHandledAggregateCommand = (HandledAggregateCommand)existingHandledCommand;
            var aggregateRootId = existingHandledAggregateCommand.AggregateRootId;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<DomainEventStream>>("FindEventStreamByCommandIdAsync",
            () => _eventStore.FindAsync(aggregateRootId, command.Id),
            currentRetryTimes => HandleExistingHandledAggregateCommandForExceptionAsync(processingCommand, existingHandledCommand, commandHandler, exception, currentRetryTimes),
            result =>
            {
                var existingEventStream = result.Data;
                if (existingEventStream != null)
                {
                    _eventService.PublishDomainEventAsync(processingCommand, existingEventStream);
                }
                else
                {
                    LogCommandExecuteException(processingCommand, commandHandler, exception);
                    TryToRetryCommandForExceptionAsync(processingCommand, existingHandledAggregateCommand, commandHandler, exception, 0);
                }
            },
            () => string.Format("[aggregateRootId:{0}, commandId:{1}]", aggregateRootId, command.Id),
            () => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Find event stream by command id async failed."),
            retryTimes);
        }
        private void TryToRetryCommandForExceptionAsync(ProcessingCommand processingCommand, HandledAggregateCommand existingHandledAggregateCommand, ICommandHandler commandHandler, Exception exception, int retryTimes)
        {
            var command = processingCommand.Command;

            //到这里，说明当前command执行遇到异常，然后该command在commandStore中存在，
            //但是在eventStore中不存在，此时可以理解为该command还未被成功执行，此时做如下操作：
            //1.将command从commandStore中移除
            //2.根据eventStore里的事件刷新缓存，目的是为了还原聚合根到最新状态，因为该聚合根的状态有可能已经被污染
            //3.重试该command
            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult>("RemoveCommandAsync",
            () => _commandStore.RemoveAsync(command.Id),
            currentRetryTimes => TryToRetryCommandForExceptionAsync(processingCommand, existingHandledAggregateCommand, commandHandler, exception, currentRetryTimes),
            result =>
            {
                _memoryCache.RefreshAggregateFromEventStore(existingHandledAggregateCommand.AggregateRootTypeCode, existingHandledAggregateCommand.AggregateRootId);
                RetryCommand(processingCommand);
            },
            () => string.Format("[commandId:{0}]", command.Id),
            () => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Remove command async failed."),
            retryTimes);
        }
        private void NotifyCommandExecuted(ProcessingCommand processingCommand, CommandStatus commandStatus, string exceptionTypeName, string errorMessage)
        {
            processingCommand.Complete(new CommandResult(commandStatus, processingCommand.Command.Id, processingCommand.AggregateRootId, exceptionTypeName, errorMessage));
        }
        private void RetryCommand(ProcessingCommand processingCommand)
        {
            processingCommand.CommandExecuteContext.Clear();
            ExecuteCommand(processingCommand);
        }
        private void LogCommandExecuteException(ProcessingCommand processingCommand, ICommandHandler commandHandler, Exception exception)
        {
            var errorMessage = string.Format("{0} raised when {1} handling {2}. commandId:{3}, aggregateRootId:{4}",
                exception.GetType().Name,
                commandHandler.GetInnerHandler().GetType().Name,
                processingCommand.Command.GetType().Name,
                processingCommand.Command.Id,
                processingCommand.AggregateRootId);
            _logger.Error(errorMessage, exception);
        }

        #endregion
    }
}
