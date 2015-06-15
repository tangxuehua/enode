using System;
using System.Collections.Generic;
using System.Linq;
using ECommon.IO;
using ECommon.Logging;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class DefaultProcessingCommandHandler : IProcessingMessageHandler<ProcessingCommand, ICommand, CommandResult>
    {
        #region Private Variables

        private readonly ICommandStore _commandStore;
        private readonly IEventStore _eventStore;
        private readonly ICommandHandlerProvider _commandHandlerProvider;
        private readonly ICommandAsyncHandlerProvider _commandAsyncHandlerProvider;
        private readonly ITypeCodeProvider _aggregateRootTypeProvider;
        private readonly IEventService _eventService;
        private readonly IMessagePublisher<IApplicationMessage> _messagePublisher;
        private readonly IMessagePublisher<IPublishableException> _exceptionPublisher;
        private readonly IMemoryCache _memoryCache;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultProcessingCommandHandler(
            ICommandStore commandStore,
            IEventStore eventStore,
            ICommandHandlerProvider commandHandlerProvider,
            ICommandAsyncHandlerProvider commandAsyncHandlerProvider,
            ITypeCodeProvider aggregateRootTypeProvider,
            IEventService eventService,
            IMessagePublisher<IApplicationMessage> messagePublisher,
            IMessagePublisher<IPublishableException> exceptionPublisher,
            IMemoryCache memoryCache,
            IOHelper ioHelper,
            ILoggerFactory loggerFactory)
        {
            _commandStore = commandStore;
            _eventStore = eventStore;
            _commandHandlerProvider = commandHandlerProvider;
            _commandAsyncHandlerProvider = commandAsyncHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _eventService = eventService;
            _messagePublisher = messagePublisher;
            _exceptionPublisher = exceptionPublisher;
            _memoryCache = memoryCache;
            _ioHelper = ioHelper;
            _logger = loggerFactory.Create(GetType().FullName);
            _eventService.SetProcessingCommandHandler(this);
        }

        #endregion

        #region Public Methods

        public void HandleAsync(ProcessingCommand processingCommand)
        {
            if (string.IsNullOrEmpty(processingCommand.Message.AggregateRootId))
            {
                NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null,
                    string.Format("The aggregateRootId of command cannot be null or empty. commandType:{0}, commandId:{1}",
                    processingCommand.Message.GetType().Name,
                    processingCommand.Message.Id));
                return;
            }
            var commandHandler = GetCommandHandler(processingCommand);
            var commandAsyncHandler = default(ICommandAsyncHandlerProxy);

            if (commandHandler == null)
            {
                commandAsyncHandler = GetCommandAsyncHandler(processingCommand);
            }

            if (commandHandler == null && commandAsyncHandler == null)
            {
                NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null,
                    string.Format("No command handler found of command. commandType:{0}, commandId:{1}",
                    processingCommand.Message.GetType().Name,
                    processingCommand.Message.Id));
                return;
            }

            if (commandHandler != null)
            {
                HandleCommand(processingCommand, commandHandler);
            }
            else if (commandAsyncHandler != null)
            {
                ProcessCommand(processingCommand, commandAsyncHandler, 0);
            }
        }

        #endregion

        #region Command Handler Helper Methods

        private ICommandHandlerProxy GetCommandHandler(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Message;
            var commandHandlers = _commandHandlerProvider.GetHandlers(command.GetType());

            if (commandHandlers.Count() > 1)
            {
                _logger.ErrorFormat("Found more than one command handlers, commandType:{0}, commandId:{1}.", command.GetType().FullName, command.Id);
                NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "More than one command handlers found.");
                return null;
            }

            return commandHandlers.SingleOrDefault();
        }
        private void HandleCommand(ProcessingCommand processingCommand, ICommandHandlerProxy commandHandler)
        {
            var command = processingCommand.Message;

            //调用command handler执行当前command
            var handleSuccess = false;
            try
            {
                commandHandler.Handle(processingCommand.CommandExecuteContext, command);
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Handle command success. handlerType:{0}, commandType:{1}, commandId:{2}, aggregateRootId:{3}",
                        commandHandler.GetInnerHandler().GetType().Name,
                        command.GetType().Name,
                        command.Id,
                        command.AggregateRootId);
                }
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
                    CommitAggregateChanges(processingCommand);
                }
                catch (Exception ex)
                {
                    LogCommandExecuteException(processingCommand, commandHandler, ex);
                    NotifyCommandExecuted(processingCommand, CommandStatus.Failed, ex.GetType().Name, "Unknown exception caught when committing changes of command.");
                }
            }
        }
        private void CommitAggregateChanges(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Message;
            var context = processingCommand.CommandExecuteContext;
            var trackedAggregateRoots = context.GetTrackedAggregateRoots();
            var dirtyAggregateRootChanges = trackedAggregateRoots.ToDictionary(x => x, x => x.GetChanges()).Where(x => x.Value.Any());
            var dirtyAggregateRootCount = dirtyAggregateRootChanges.Count();

            //如果当前command没有对任何聚合根做修改，则认为当前command已经处理结束，返回command的结果为NothingChanged
            if (dirtyAggregateRootCount == 0)
            {
                _logger.InfoFormat("No aggregate created or modified by command. commandType:{0}, commandId:{1}", command.GetType().Name, command.Id);
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

            //将事件流提交到EventStore
            _eventService.CommitDomainEventAsync(new EventCommittingContext(dirtyAggregateRoot, eventStream, processingCommand));
        }
        private DomainEventStream BuildDomainEventStream(IAggregateRoot aggregateRoot, IEnumerable<IDomainEvent> changedEvents, ProcessingCommand processingCommand)
        {
            return new DomainEventStream(
                processingCommand.Message.Id,
                aggregateRoot.UniqueId,
                _aggregateRootTypeProvider.GetTypeCode(aggregateRoot.GetType()),
                aggregateRoot.Version + 1,
                DateTime.Now,
                changedEvents,
                processingCommand.Items);
        }
        private void HandleExceptionAsync(ProcessingCommand processingCommand, ICommandHandlerProxy commandHandler, Exception exception, int retryTimes)
        {
            var command = processingCommand.Message;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<DomainEventStream>>("FindEventStreamByCommandIdAsync",
            () => _eventStore.FindAsync(command.AggregateRootId, command.Id),
            currentRetryTimes => HandleExceptionAsync(processingCommand, commandHandler, exception, currentRetryTimes),
            result =>
            {
                var existingEventStream = result.Data;
                if (existingEventStream != null)
                {
                    //这里，我们需要再重新做一遍更新内存缓存以及发布事件这两个操作；
                    //之所以要这样做是因为虽然该command产生的事件已经持久化成功，但并不表示已经内存也更新了或者事件已经发布出去了；
                    //因为有可能事件持久化成功了，但那时正好机器断电了，则更新内存和发布事件都没有做；
                    _memoryCache.RefreshAggregateFromEventStore(existingEventStream.AggregateRootTypeCode, existingEventStream.AggregateRootId);
                    _eventService.PublishDomainEventAsync(processingCommand, existingEventStream);
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
            errorMessage => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage ?? "Get command async failed."),
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
                return string.Format("[commandId:{0}, exceptionInfo:{1}]", processingCommand.Message.Id, exceptionInfo);
            },
            errorMessage => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage ?? "Publish exception async failed."),
            retryTimes);
        }
        private void NotifyCommandExecuted(ProcessingCommand processingCommand, CommandStatus commandStatus, string exceptionTypeName, string errorMessage)
        {
            processingCommand.Complete(new CommandResult(commandStatus, processingCommand.Message.Id, processingCommand.Message.AggregateRootId, exceptionTypeName, errorMessage));
        }
        private void RetryCommand(ProcessingCommand processingCommand)
        {
            processingCommand.CommandExecuteContext.Clear();
            HandleAsync(processingCommand);
        }
        private void LogCommandExecuteException(ProcessingCommand processingCommand, ICommandHandlerProxy commandHandler, Exception exception)
        {
            var errorMessage = string.Format("{0} raised when {1} handling {2}. commandId:{3}, aggregateRootId:{4}",
                exception.GetType().Name,
                commandHandler.GetInnerHandler().GetType().Name,
                processingCommand.Message.GetType().Name,
                processingCommand.Message.Id,
                processingCommand.Message.AggregateRootId);
            _logger.Error(errorMessage, exception);
        }

        #endregion

        #region Command Async Handler Help Methods

        private ICommandAsyncHandlerProxy GetCommandAsyncHandler(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Message;
            var commandAsyncHandlers = _commandAsyncHandlerProvider.GetHandlers(command.GetType());

            if (commandAsyncHandlers.Count() > 1)
            {
                _logger.ErrorFormat("Found more than one command handlers, commandType:{0}, commandId:{1}.", command.GetType().FullName, command.Id);
                NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "More than one command handlers found.");
                return null;
            }

            return commandAsyncHandlers.SingleOrDefault();
        }
        private void ProcessCommand(ProcessingCommand processingCommand, ICommandAsyncHandlerProxy commandAsyncHandler, int retryTimes)
        {
            var command = processingCommand.Message;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<HandledCommand>>("GetCommandAsync",
            () => _commandStore.GetAsync(command.Id),
            currentRetryTimes => ProcessCommand(processingCommand, commandAsyncHandler, currentRetryTimes),
            result =>
            {
                var existingHandledCommand = result.Data;
                if (existingHandledCommand != null)
                {
                    NotifyCommandExecuted(processingCommand, CommandStatus.NothingChanged, null, null);
                    return;
                }
                _logger.DebugFormat("Duplicate command execution, commandId:{0},commandType:{1}", command.Id, command.GetType().Name);
                HandleCommandAsync(processingCommand, commandAsyncHandler, 0);
            },
            () => string.Format("[commandId:{0},commandType:{1}]", command.Id, command.GetType().Name),
            errorMessage => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage ?? "Get command async failed."),
            retryTimes);
        }
        private void HandleCommandAsync(ProcessingCommand processingCommand, ICommandAsyncHandlerProxy commandHandler, int retryTimes)
        {
            var command = processingCommand.Message;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<IApplicationMessage>>("HandleCommandAsync",
            () => commandHandler.HandleAsync(command),
            currentRetryTimes => HandleCommandAsync(processingCommand, commandHandler, currentRetryTimes),
            result =>
            {
                if (_logger.IsDebugEnabled)
                {
                    _logger.DebugFormat("Handle command success. handlerType:{0}, commandType:{1}, commandId:{2}, aggregateRootId:{3}",
                        commandHandler.GetInnerHandler().GetType().Name,
                        command.GetType().Name,
                        command.Id,
                        command.AggregateRootId);
                }
                CommitChangesAsync(processingCommand, result.Data, 0);
            },
            () => string.Format("[command:[id:{0},type:{1}],handlerType:{2}]", command.Id, command.GetType().Name, commandHandler.GetInnerHandler().GetType().Name),
            errorMessage => HandleFaildCommandAsync(processingCommand, commandHandler, 0),
            retryTimes);
        }
        private void CommitChangesAsync(ProcessingCommand processingCommand, IApplicationMessage message, int retryTimes)
        {
            var command = processingCommand.Message;
            var handledCommand = new HandledCommand(command.Id, command.AggregateRootId, message);

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<CommandAddResult>>("AddCommandAsync",
            () => _commandStore.AddAsync(handledCommand),
            currentRetryTimes => CommitChangesAsync(processingCommand, message, currentRetryTimes),
            result =>
            {
                var commandAddResult = result.Data;
                if (commandAddResult == CommandAddResult.Success)
                {
                    PublishMessageAsync(processingCommand, message, 0);
                }
                else if (commandAddResult == CommandAddResult.DuplicateCommand)
                {
                    HandleDuplicatedCommandAsync(processingCommand, 0);
                }
                else
                {
                    _logger.ErrorFormat("Add command async failed, commandType:{0}, commandId:{1}, aggregateRootId:{2}", command.GetType().Name, command.Id, command.AggregateRootId);
                    NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Add command async failed.");
                }
            },
            () => string.Format("[handledCommand:{0}]", handledCommand),
            errorMessage => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage ?? "Add command async failed."),
            retryTimes);
        }
        private void PublishMessageAsync(ProcessingCommand processingCommand, IApplicationMessage message, int retryTimes)
        {
            var command = processingCommand.Message;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult>("PublishApplicationMessageAsync",
            () => _messagePublisher.PublishAsync(message),
            currentRetryTimes => PublishMessageAsync(processingCommand, message, currentRetryTimes),
            result =>
            {
                NotifyCommandExecuted(processingCommand, CommandStatus.Success, null, null);
            },
            () => string.Format("[application message:[id:{0},type:{1}],command:[id:{2},type:{3}]]", message.Id, message.GetType().Name, command.Id, command.GetType().Name),
            errorMessage => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage ?? "Publish application message async failed."),
            retryTimes);
        }
        private void HandleDuplicatedCommandAsync(ProcessingCommand processingCommand, int retryTimes)
        {
            var command = processingCommand.Message;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<HandledCommand>>("GetCommandAsync",
            () => _commandStore.GetAsync(command.Id),
            currentRetryTimes => HandleDuplicatedCommandAsync(processingCommand, currentRetryTimes),
            result =>
            {
                var existingHandledCommand = result.Data;
                if (existingHandledCommand != null)
                {
                    if (existingHandledCommand.Message != null)
                    {
                        PublishMessageAsync(processingCommand, existingHandledCommand.Message, 0);
                    }
                    else
                    {
                        NotifyCommandExecuted(processingCommand, CommandStatus.Success, null, null);
                    }
                }
                else
                {
                    //到这里，说明当前command想添加到commandStore中时，提示command重复，但是尝试从commandStore中取出该command时却找不到该command。
                    //出现这种情况，我们就无法再做后续处理了，这种错误理论上不会出现，除非commandStore的Add接口和Get接口出现读写不一致的情况；
                    //我们记录错误日志，然后认为当前command已被处理为失败。
                    var errorMessage = string.Format("Command exist in the command store, but we cannot get it from the command store. commandType:{0}, commandId:{1}, aggregateRootId:{2}",
                        command.GetType().Name,
                        command.Id,
                        command.AggregateRootId);
                    _logger.Error(errorMessage);
                    NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage);
                }
            },
            () => string.Format("[command:[id:{0},type:{1}]", command.Id, command.GetType().Name),
            errorMessage => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage ?? "Get command async failed."),
            retryTimes);
        }
        private void HandleFaildCommandAsync(ProcessingCommand processingCommand, ICommandAsyncHandlerProxy commandHandler, int retryTimes)
        {
            var command = processingCommand.Message;

            _ioHelper.TryAsyncActionRecursively<AsyncTaskResult<HandledCommand>>("GetCommandAsync",
            () => _commandStore.GetAsync(command.Id),
            currentRetryTimes => HandleFaildCommandAsync(processingCommand, commandHandler, currentRetryTimes),
            result =>
            {
                var existingHandledCommand = result.Data;
                if (existingHandledCommand != null)
                {
                    if (existingHandledCommand.Message != null)
                    {
                        PublishMessageAsync(processingCommand, existingHandledCommand.Message, 0);
                    }
                    else
                    {
                        NotifyCommandExecuted(processingCommand, CommandStatus.Success, null, null);
                    }
                }
                else
                {
                    NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Handle command failed.");
                }
            },
            () => string.Format("[command:[id:{0},type:{1}]", command.Id, command.GetType().Name),
            errorMessage => NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, errorMessage ?? "Get command async failed."),
            retryTimes);
        }

        #endregion
    }
}
