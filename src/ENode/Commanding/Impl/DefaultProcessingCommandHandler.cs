using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ECommon.Extensions;
using ECommon.IO;
using ECommon.Logging;
using ECommon.Serializing;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Messaging;

namespace ENode.Commanding.Impl
{
    public class DefaultProcessingCommandHandler : IProcessingCommandHandler
    {
        #region Private Variables

        private readonly IJsonSerializer _jsonSerializer;
        private readonly IEventStore _eventStore;
        private readonly IMemoryCache _memoryCache;
        private readonly ICommandHandlerProvider _commandHandlerProvider;
        private readonly ITypeNameProvider _typeNameProvider;
        private readonly IEventCommittingService _eventCommittingService;
        private readonly IMessagePublisher<IApplicationMessage> _applicationMessagePublisher;
        private readonly IMessagePublisher<IDomainException> _exceptionPublisher;
        private readonly IOHelper _ioHelper;
        private readonly ILogger _logger;
        private readonly ITimeProvider _timeProvider;

        #endregion

        #region Constructors

        public DefaultProcessingCommandHandler(
            IJsonSerializer jsonSerializer,
            IEventStore eventStore,
            IMemoryCache memoryCache,
            ICommandHandlerProvider commandHandlerProvider,
            ITypeNameProvider typeNameProvider,
            IEventCommittingService eventCommittingService,
            IMessagePublisher<IApplicationMessage> applicationMessagePublisher,
            IMessagePublisher<IDomainException> exceptionPublisher,
            IOHelper ioHelper,
            ILoggerFactory loggerFactory,
            ITimeProvider timeProvider)
        {
            _jsonSerializer = jsonSerializer;
            _eventStore = eventStore;
            _memoryCache = memoryCache;
            _commandHandlerProvider = commandHandlerProvider;
            _typeNameProvider = typeNameProvider;
            _eventCommittingService = eventCommittingService;
            _applicationMessagePublisher = applicationMessagePublisher;
            _exceptionPublisher = exceptionPublisher;
            _ioHelper = ioHelper;
            _logger = loggerFactory.Create(GetType().FullName);
            _timeProvider = timeProvider;
        }

        #endregion

        #region Public Methods

        public async Task HandleAsync(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Message;

            if (string.IsNullOrEmpty(command.AggregateRootId))
            {
                var errorMessage = string.Format("The aggregateRootId of command cannot be null or empty. commandType:{0}, commandId:{1}", command.GetType().Name, command.Id);
                _logger.Error(errorMessage);
                await CompleteCommand(processingCommand, CommandStatus.Failed, typeof(string).FullName, errorMessage).ConfigureAwait(false);
            }

            var findResult = GetCommandHandler(processingCommand, out ICommandHandlerProxy commandHandler);
            if (findResult == HandlerFindResult.Found)
            {
                await HandleCommandInternal(processingCommand, commandHandler, 0, new TaskCompletionSource<bool>()).ConfigureAwait(false);
            }
            else if (findResult == HandlerFindResult.TooManyHandlerData)
            {
                _logger.ErrorFormat("Found more than one command handler data, commandType:{0}, commandId:{1}", command.GetType().FullName, command.Id);
                await CompleteCommand(processingCommand, CommandStatus.Failed, typeof(string).FullName, "More than one command handler data found.").ConfigureAwait(false);
            }
            else if (findResult == HandlerFindResult.TooManyHandler)
            {
                _logger.ErrorFormat("Found more than one command handler, commandType:{0}, commandId:{1}", command.GetType().FullName, command.Id);
                await CompleteCommand(processingCommand, CommandStatus.Failed, typeof(string).FullName, "More than one command handler found.").ConfigureAwait(false);
            }
            else if (findResult == HandlerFindResult.NotFound)
            {
                var errorMessage = string.Format("No command handler found of command. commandType:{0}, commandId:{1}", command.GetType().Name, command.Id);
                _logger.Error(errorMessage);
                await CompleteCommand(processingCommand, CommandStatus.Failed, typeof(string).FullName, errorMessage).ConfigureAwait(false);
            }
        }

        #endregion

        #region Private Methods

        private Task HandleCommandInternal(ProcessingCommand processingCommand, ICommandHandlerProxy commandHandler, int retryTimes, TaskCompletionSource<bool> taskSource)
        {
            var command = processingCommand.Message;
            var commandContext = processingCommand.CommandExecuteContext;

            commandContext.Clear();

            if (processingCommand.IsDuplicated)
            {
                return RepublishCommandEvents(processingCommand, 0, new TaskCompletionSource<bool>());
            }

            _ioHelper.TryAsyncActionRecursivelyWithoutResult("HandleCommandAsync",
            async () =>
            {
                await commandHandler.HandleAsync(commandContext, command).ConfigureAwait(false);
            },
            currentRetryTimes => HandleCommandInternal(processingCommand, commandHandler, currentRetryTimes, taskSource),
            async () =>
            {
                _logger.InfoFormat("Handle command success. handlerType:{0}, commandType:{1}, commandId:{2}, aggregateRootId:{3}",
                    commandHandler.GetInnerObject().GetType().Name,
                    command.GetType().Name,
                    command.Id,
                    command.AggregateRootId);
                if (commandContext.GetApplicationMessage() != null)
                {
                    await CommitChangesAsync(processingCommand, true, commandContext.GetApplicationMessage(), null, new TaskCompletionSource<bool>()).ConfigureAwait(false);
                    taskSource.SetResult(true);
                }
                else
                {
                    try
                    {
                        await CommitAggregateChanges(processingCommand).ConfigureAwait(false);
                        taskSource.SetResult(true);
                    }
                    catch (AggregateRootReferenceChangedException aggregateRootReferenceChangedException)
                    {
                        _logger.InfoFormat("Aggregate root reference changed when processing command, try to re-handle the command. aggregateRootId: {0}, aggregateRootType: {1}, commandId: {2}, commandType: {3}, handlerType: {4}",
                            aggregateRootReferenceChangedException.AggregateRoot.UniqueId,
                            aggregateRootReferenceChangedException.AggregateRoot.GetType().Name,
                            command.Id,
                            command.GetType().Name,
                            commandHandler.GetInnerObject().GetType().Name
                        );
                        await HandleCommandInternal(processingCommand, commandHandler, 0, taskSource).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(string.Format("Commit aggregate changes has unknown exception, this should not be happen, and we just complete the command, handlerType:{0}, commandType:{1}, commandId:{2}, aggregateRootId:{3}",
                            commandHandler.GetInnerObject().GetType().Name,
                            command.GetType().Name,
                            command.Id,
                            command.AggregateRootId), ex);
                        await CompleteCommand(processingCommand, CommandStatus.Failed, ex.GetType().Name, "Unknown exception caught when committing changes of command.").ConfigureAwait(false);
                        taskSource.SetResult(true);
                    }
                }
            },
            () => string.Format("[command:[id:{0},type:{1}],handlerType:{2},aggregateRootId:{3}]", command.Id, command.GetType().Name, commandHandler.GetInnerObject().GetType().Name, command.AggregateRootId),
            async (ex, errorMessage) =>
            {
                await HandleExceptionAsync(processingCommand, commandHandler, ex, errorMessage, 0, new TaskCompletionSource<bool>()).ConfigureAwait(false);
                taskSource.SetResult(true);
            },
            retryTimes);

            return taskSource.Task;
        }
        private async Task CommitAggregateChanges(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Message;
            var context = processingCommand.CommandExecuteContext;
            var trackedAggregateRoots = context.GetTrackedAggregateRoots();
            var dirtyAggregateRootCount = 0;
            var dirtyAggregateRoot = default(IAggregateRoot);
            var changedEvents = default(IEnumerable<IDomainEvent>);

            foreach (var aggregateRoot in trackedAggregateRoots)
            {
                var events = aggregateRoot.GetChanges();
                if (events.Any())
                {
                    dirtyAggregateRootCount++;
                    if (dirtyAggregateRootCount > 1)
                    {
                        var errorMessage = string.Format("Detected more than one aggregate created or modified by command. commandType:{0}, commandId:{1}",
                            command.GetType().Name,
                            command.Id);
                        _logger.ErrorFormat(errorMessage);
                        await CompleteCommand(processingCommand, CommandStatus.Failed, typeof(string).FullName, errorMessage).ConfigureAwait(false);
                        return;
                    }
                    dirtyAggregateRoot = aggregateRoot;
                    changedEvents = events;
                }
            }

            //如果当前command没有对任何聚合根做修改，框架仍然需要尝试获取该command之前是否有产生事件，
            //如果有，则需要将事件再次发布到MQ；如果没有，则完成命令，返回command的结果为NothingChanged。
            //之所以要这样做是因为有可能当前command上次执行的结果可能是事件持久化完成，但是发布到MQ未完成，然后那时正好机器断电宕机了；
            //这种情况下，如果机器重启，当前command对应的聚合根从eventstore恢复的聚合根是被当前command处理过后的；
            //所以如果该command再次被处理，可能对应的聚合根就不会再产生事件了；
            //所以，我们要考虑到这种情况，尝试再次发布该命令产生的事件到MQ；
            //否则，如果我们直接将当前command设置为完成，即对MQ进行ack操作，那该command的事件就永远不会再发布到MQ了，这样就无法保证CQRS数据的最终一致性了。
            if (dirtyAggregateRootCount == 0 || changedEvents == null || changedEvents.Count() == 0)
            {
                await RepublishCommandEvents(processingCommand, 0, new TaskCompletionSource<bool>()).ConfigureAwait(false);
                return;
            }

            //构造出一个事件流对象
            var commandResult = processingCommand.CommandExecuteContext.GetResult();
            if (commandResult != null)
            {
                processingCommand.Items["CommandResult"] = commandResult;
            }
            var eventStream = new DomainEventStream(
                processingCommand.Message.Id,
                dirtyAggregateRoot.UniqueId,
                _typeNameProvider.GetTypeName(dirtyAggregateRoot.GetType()),
                _timeProvider.GetCurrentTime(),
                changedEvents,
                command.Items);

            //内存先接受聚合根的更新，需要检查聚合根引用是否已变化，如果已变化，会抛出异常
            await _memoryCache.AcceptAggregateRootChanges(dirtyAggregateRoot).ConfigureAwait(false);

            //提交事件流进行后续的处理
            _eventCommittingService.CommitDomainEventAsync(new EventCommittingContext(eventStream, processingCommand));
        }
        private Task RepublishCommandEvents(ProcessingCommand processingCommand, int retryTimes, TaskCompletionSource<bool> taskSource)
        {
            var command = processingCommand.Message;

            _ioHelper.TryAsyncActionRecursively("ProcessIfNoEventsOfCommand",
            () => _eventStore.FindAsync(command.AggregateRootId, command.Id),
            currentRetryTimes => RepublishCommandEvents(processingCommand, currentRetryTimes, taskSource),
            async result =>
            {
                var existingEventStream = result;
                if (existingEventStream != null)
                {
                    _eventCommittingService.PublishDomainEventAsync(processingCommand, existingEventStream);
                    taskSource.SetResult(true);
                }
                else
                {
                    await CompleteCommand(processingCommand, CommandStatus.NothingChanged, typeof(string).FullName, processingCommand.CommandExecuteContext.GetResult()).ConfigureAwait(false);
                    taskSource.SetResult(true);
                }
            },
            () => string.Format("[commandId:{0}]", command.Id),
            null,
            retryTimes, true);

            return taskSource.Task;
        }
        private Task HandleExceptionAsync(ProcessingCommand processingCommand, ICommandHandlerProxy commandHandler, Exception exception, string errorMessage, int retryTimes, TaskCompletionSource<bool> taskSource)
        {
            var command = processingCommand.Message;

            _ioHelper.TryAsyncActionRecursively("FindEventByCommandIdAsync",
            () => _eventStore.FindAsync(command.AggregateRootId, command.Id),
            currentRetryTimes => HandleExceptionAsync(processingCommand, commandHandler, exception, errorMessage, currentRetryTimes, taskSource),
            async result =>
            {
                var existingEventStream = result;
                if (existingEventStream != null)
                {
                    //这里，我们需要再重新做一遍发布事件这个操作；
                    //之所以要这样做是因为虽然该command产生的事件已经持久化成功，但并不表示事件已经发布出去了；
                    //因为有可能事件持久化成功了，但那时正好机器断电了，则发布事件就没有做；
                    _eventCommittingService.PublishDomainEventAsync(processingCommand, existingEventStream);
                    taskSource.SetResult(true);
                }
                else
                {
                    //到这里，说明当前command执行遇到异常，然后当前command之前也没执行过，是第一次被执行。
                    //那就判断当前异常是否是需要被发布出去的异常，如果是，则发布该异常给所有消费者；
                    //否则，就记录错误日志，然后认为该command处理失败即可；
                    var realException = GetRealException(exception);
                    if (realException is IDomainException)
                    {
                        await PublishExceptionAsync(processingCommand, realException as IDomainException, 0, new TaskCompletionSource<bool>()).ConfigureAwait(false);
                        taskSource.SetResult(true);
                    }
                    else
                    {
                        await CompleteCommand(processingCommand, CommandStatus.Failed, realException.GetType().Name, exception != null ? realException.Message : errorMessage).ConfigureAwait(false);
                        taskSource.SetResult(true);
                    }
                }
            },
            () => string.Format("[command:[id:{0},type:{1}],handlerType:{2},aggregateRootId:{3}]", command.Id, command.GetType().Name, commandHandler.GetInnerObject().GetType().Name, command.AggregateRootId),
            null,
            retryTimes, true);

            return taskSource.Task;
        }
        private Exception GetRealException(Exception exception)
        {
            if (exception is AggregateException && ((AggregateException)exception).InnerExceptions.IsNotEmpty())
            {
                return ((AggregateException)exception).InnerExceptions.First();
            }
            return exception;
        }
        private Task PublishExceptionAsync(ProcessingCommand processingCommand, IDomainException exception, int retryTimes, TaskCompletionSource<bool> taskSource)
        {
            exception.MergeItems(processingCommand.Message.Items);

            _ioHelper.TryAsyncActionRecursivelyWithoutResult("PublishExceptionAsync",
            () => _exceptionPublisher.PublishAsync(exception),
            currentRetryTimes => PublishExceptionAsync(processingCommand, exception, currentRetryTimes, taskSource),
            async () =>
            {
                await CompleteCommand(processingCommand, CommandStatus.Failed, exception.GetType().Name, (exception as Exception).Message).ConfigureAwait(false);
                taskSource.SetResult(true);
            },
            () =>
            {
                var serializableInfo = new Dictionary<string, string>();
                exception.SerializeTo(serializableInfo);
                var exceptionInfo = string.Join(",", serializableInfo.Select(x => string.Format("{0}:{1}", x.Key, x.Value)));
                return string.Format("[commandId:{0}, exceptionInfo:{1}]", processingCommand.Message.Id, exceptionInfo);
            },
            null,
            retryTimes, true);

            return taskSource.Task;
        }
        private async Task CommitChangesAsync(ProcessingCommand processingCommand, bool success, IApplicationMessage message, string errorMessage, TaskCompletionSource<bool> taskSource)
        {
            if (success)
            {
                if (message != null)
                {
                    message.MergeItems(processingCommand.Message.Items);
                    await PublishMessageAsync(processingCommand, message, 0, new TaskCompletionSource<bool>()).ConfigureAwait(false);
                    taskSource.SetResult(true);
                }
                else
                {
                    await CompleteCommand(processingCommand, CommandStatus.Success, null, null).ConfigureAwait(false);
                    taskSource.SetResult(true);
                }
            }
            else
            {
                await CompleteCommand(processingCommand, CommandStatus.Failed, typeof(string).FullName, errorMessage).ConfigureAwait(false);
                taskSource.SetResult(true);
            }
        }
        private Task PublishMessageAsync(ProcessingCommand processingCommand, IApplicationMessage message, int retryTimes, TaskCompletionSource<bool> taskSource)
        {
            var command = processingCommand.Message;

            _ioHelper.TryAsyncActionRecursivelyWithoutResult("PublishApplicationMessageAsync",
            () => _applicationMessagePublisher.PublishAsync(message),
            currentRetryTimes => PublishMessageAsync(processingCommand, message, currentRetryTimes, taskSource),
            async () =>
            {
                await CompleteCommand(processingCommand, CommandStatus.Success, message.GetType().FullName, _jsonSerializer.Serialize(message)).ConfigureAwait(false);
                taskSource.SetResult(true);
            },
            () => string.Format("[application message:[id:{0},type:{1}],command:[id:{2},type:{3}]]", message.Id, message.GetType().Name, command.Id, command.GetType().Name),
            null,
            retryTimes, true);

            return taskSource.Task;
        }
        private HandlerFindResult GetCommandHandler<T>(ProcessingCommand processingCommand, out T handlerProxy) where T : class, IObjectProxy
        {
            handlerProxy = null;

            var command = processingCommand.Message;
            var handlerDataList = _commandHandlerProvider.GetHandlers(command.GetType());

            if (handlerDataList == null || handlerDataList.Count() == 0)
            {
                return HandlerFindResult.NotFound;
            }
            else if (handlerDataList.Count() > 1)
            {
                return HandlerFindResult.TooManyHandlerData;
            }

            var handlerData = handlerDataList.Single();
            if (handlerData.ListHandlers == null || handlerData.ListHandlers.Count() == 0)
            {
                return HandlerFindResult.NotFound;
            }
            else if (handlerData.ListHandlers.Count() > 1)
            {
                return HandlerFindResult.TooManyHandler;
            }

            handlerProxy = handlerData.ListHandlers.Single() as T;

            return HandlerFindResult.Found;
        }
        private async Task CompleteCommand(ProcessingCommand processingCommand, CommandStatus commandStatus, string resultType, string result)
        {
            var commandResult = new CommandResult(commandStatus, processingCommand.Message.Id, processingCommand.Message.AggregateRootId, result, resultType);
            await processingCommand.MailBox.CompleteMessage(processingCommand, commandResult).ConfigureAwait(false);
        }
        private enum HandlerFindResult
        {
            NotFound,
            Found,
            TooManyHandlerData,
            TooManyHandler
        }

        #endregion
    }
}
