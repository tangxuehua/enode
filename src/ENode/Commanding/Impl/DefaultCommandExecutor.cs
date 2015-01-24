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
        private readonly IHandlerProvider<ICommandHandler> _commandHandlerProvider;
        private readonly ITypeCodeProvider<IAggregateRoot> _aggregateRootTypeProvider;
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
            IHandlerProvider<ICommandHandler> commandHandlerProvider,
            ITypeCodeProvider<IAggregateRoot> aggregateRootTypeProvider,
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
            var command = processingCommand.Command;

            //如果是非操作聚合根的command，且该command已检测到被处理过，则直接认为command已成功处理。
            if (!(command is IAggregateCommand))
            {
                var result = _ioHelper.TryIOFuncRecursively<HandledCommand>("GetHandledCommand", () => command.Id, () =>
                {
                    return _commandStore.Get(command.Id);
                });
                var existingHandledCommand = result.Data;

                if (existingHandledCommand != null)
                {
                    NotifyCommandExecuted(processingCommand, CommandStatus.NothingChanged, null, null);
                    return;
                }
            }

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
                _logger.DebugFormat("Command was handled. handlerType:{0}, commandType:{1}, commandId:{2}, aggregateRootId:{3}",
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
                HandleException(processingCommand, commandHandler, ex);
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

        #endregion

        #region Private Methods

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

            //如果当前command处于并发冲突的重试中，则直接提交该command产生的事件，因为该command肯定已经在command store中了
            if (processingCommand.ConcurrentRetriedCount > 0)
            {
                _eventService.CommitEvent(new EventCommittingContext(dirtyAggregateRoot, eventStream, processingCommand));
                return;
            }

            //尝试将当前已执行的command添加到commandStore
            string sourceId;
            string sourceType;
            processingCommand.Items.TryGetValue("SourceId", out sourceId);
            processingCommand.Items.TryGetValue("SourceType", out sourceType);

            var handledAggregateCommand = new HandledAggregateCommand(command, sourceId, sourceType, eventStream.AggregateRootId, eventStream.AggregateRootTypeCode);

            var addCommandIoResult = _ioHelper.TryIOFuncRecursively<CommandAddResult>("AddHandledAggregateCommand", () => handledAggregateCommand.ToString(), () =>
            {
                return _commandStore.Add(handledAggregateCommand);
            });
            var commandAddResult = addCommandIoResult.Data;

            //如果command添加成功，则提交该command产生的事件
            if (commandAddResult == CommandAddResult.Success)
            {
                _eventService.CommitEvent(new EventCommittingContext(dirtyAggregateRoot, eventStream, processingCommand));
            }
            //如果添加的结果是command重复，则做如下处理
            else if (commandAddResult == CommandAddResult.DuplicateCommand)
            {
                var getCommandIoResult = _ioHelper.TryIOFuncRecursively<HandledAggregateCommand>("GetHandledAggregateCommand", () => command.Id, () =>
                {
                    return _commandStore.Get(command.Id) as HandledAggregateCommand;
                });
                var existingHandledCommand = getCommandIoResult.Data;

                if (existingHandledCommand != null)
                {
                    var result = _ioHelper.TryIOFuncRecursively<DomainEventStream>("FindEventByCommandId", () =>
                    {
                        return string.Format("[aggregateRootId:{0},commandId:{1},commandType:{2}]", existingHandledCommand.AggregateRootId, command.Id, command.GetType().Name);
                    }, () =>
                    {
                        return _eventStore.Find(existingHandledCommand.AggregateRootId, command.Id);
                    });

                    if (!result.Success)
                    {
                        NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Command persist duplicated, but try to find domain event from event store by commandId failed.");
                        return;
                    }

                    var existingEventStream = result.Data;
                    if (existingEventStream != null)
                    {
                        //如果当前command已经被持久化过了，且该command产生的事件也已经被持久化了，则只要再做一遍发布事件的操作
                        _eventService.PublishDomainEvent(processingCommand, existingEventStream);
                    }
                    else
                    {
                        //如果当前command已经被持久化过了，但事件没有被持久化，则需要重新提交当前command所产生的事件；
                        _eventService.CommitEvent(new EventCommittingContext(dirtyAggregateRoot, eventStream, processingCommand));
                    }
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
            }
        }
        private void CommitChanges(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Command;
            string sourceId;
            string sourceType;
            processingCommand.Items.TryGetValue("SourceId", out sourceId);
            processingCommand.Items.TryGetValue("SourceType", out sourceType);
            var evnts = processingCommand.CommandExecuteContext.GetEvents().ToList();
            var handledCommand = new HandledCommand(command, sourceId, sourceType, evnts);

            var addCommandIoResult = _ioHelper.TryIOFuncRecursively<CommandAddResult>("AddHandledCommand", () => handledCommand.ToString(), () =>
            {
                return _commandStore.Add(handledCommand);
            });
            var commandAddResult = addCommandIoResult.Data;

            if (commandAddResult == CommandAddResult.Success)
            {
                _eventService.PublishEvent(processingCommand, new EventStream(command.Id, evnts, processingCommand.Items));
            }
            else if (commandAddResult == CommandAddResult.DuplicateCommand)
            {
                var getCommandIoResult = _ioHelper.TryIOFuncRecursively<HandledCommand>("GetHandledCommand", () => command.Id, () =>
                {
                    return _commandStore.Get(command.Id);
                });
                var existingHandledCommand = getCommandIoResult.Data;

                if (existingHandledCommand != null)
                {
                    _eventService.PublishEvent(processingCommand, new EventStream(command.Id, existingHandledCommand.Events, processingCommand.Items));
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
            }
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
        private void HandleException(ProcessingCommand processingCommand, ICommandHandler commandHandler, Exception exception)
        {
            var command = processingCommand.Command;
            try
            {
                var getCommandIoResult = _ioHelper.TryIOFuncRecursively<HandledCommand>("GetHandledCommand", () => command.Id, () =>
                {
                    return _commandStore.Get(command.Id);
                });
                var existingHandledCommand = getCommandIoResult.Data;

                if (existingHandledCommand != null)
                {
                    if (command is IAggregateCommand)
                    {
                        var existingHandledAggregateCommand = (HandledAggregateCommand)existingHandledCommand;
                        var aggregateRootId = existingHandledAggregateCommand.AggregateRootId;
                        var result = _ioHelper.TryIOFuncRecursively<DomainEventStream>("FindEventByCommandId", () =>
                        {
                            return string.Format("[aggregateRootId:{0},commandId:{1},commandType:{2}]", aggregateRootId, command.Id, command.GetType().Name);
                        }, () =>
                        {
                            return _eventStore.Find(aggregateRootId, command.Id);
                        });

                        if (!result.Success)
                        {
                            NotifyCommandExecuted(processingCommand, CommandStatus.Failed, null, "Command handling has exception, and the command is persisted previously, but try to find event from event store by commandId failed.");
                            return;
                        }

                        var existingEventStream = result.Data;
                        if (existingEventStream != null)
                        {
                            _eventService.PublishDomainEvent(processingCommand, existingEventStream);
                        }
                        else
                        {
                            //到这里，说明当前command执行遇到异常，然后该command在commandStore中存在，
                            //但是在eventStore中不存在，此时可以理解为该command还未被成功执行，此时做如下操作：
                            //1.记录command执行的错误日志
                            //2.将command从commandStore中移除
                            //3.根据eventStore里的事件刷新缓存，目的是为了还原聚合根到最新状态，因为该聚合根的状态有可能已经被污染
                            //4.重试该command
                            LogCommandExecuteException(processingCommand, commandHandler, exception);
                            _ioHelper.TryIOActionRecursively("RemoveHandledCommand", () => command.Id, () =>
                            {
                                _commandStore.Remove(command.Id);
                            });
                            _memoryCache.RefreshAggregateFromEventStore(existingHandledAggregateCommand.AggregateRootTypeCode, existingHandledAggregateCommand.AggregateRootId);
                            RetryCommand(processingCommand);
                        }
                    }
                    else
                    {
                        _eventService.PublishEvent(processingCommand, new EventStream(command.Id, existingHandledCommand.Events, processingCommand.Items));
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
                        _exceptionPublisher.Publish(publishableException);
                    }
                    else
                    {
                        LogCommandExecuteException(processingCommand, commandHandler, exception);
                    }
                    NotifyCommandExecuted(processingCommand, CommandStatus.Failed, exception.GetType().Name, exception.Message);
                }
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Failed to handle command execute exception. commandId:{0}, commandType:{1}, handlerType:{2}, aggregateRootId:{3}, originalExceptionType:{4}, originalExceptionMessage:{5}",
                    command.Id,
                    command.GetType().Name,
                    commandHandler.GetInnerHandler().GetType().Name,
                    processingCommand.AggregateRootId,
                    exception.GetType().Name,
                    exception.Message);
                _logger.Error(errorMessage, ex);
                RetryCommand(processingCommand);
            }
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
