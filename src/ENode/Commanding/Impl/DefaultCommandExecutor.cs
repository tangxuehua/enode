using System;
using System.Linq;
using ECommon.Logging;
using ENode.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

namespace ENode.Commanding.Impl
{
    public class DefaultCommandExecutor : ICommandExecutor
    {
        #region Private Variables

        private readonly ICommandStore _commandStore;
        private readonly IEventStore _eventStore;
        private readonly IWaitingCommandService _waitingCommandService;
        private readonly IExecutedCommandService _executedCommandService;
        private readonly ICommandHandlerProvider _commandHandlerProvider;
        private readonly IAggregateRootTypeCodeProvider _aggregateRootTypeProvider;
        private readonly IEventService _eventService;
        private readonly IEventPublishInfoStore _eventPublishInfoStore;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="commandStore"></param>
        /// <param name="eventStore"></param>
        /// <param name="waitingCommandService"></param>
        /// <param name="executedCommandService"></param>
        /// <param name="commandHandlerProvider"></param>
        /// <param name="aggregateRootTypeProvider"></param>
        /// <param name="eventService"></param>
        /// <param name="eventPublishInfoStore"></param>
        /// <param name="loggerFactory"></param>
        public DefaultCommandExecutor(
            ICommandStore commandStore,
            IEventStore eventStore,
            IWaitingCommandService waitingCommandService,
            IExecutedCommandService executedCommandService,
            ICommandHandlerProvider commandHandlerProvider,
            IAggregateRootTypeCodeProvider aggregateRootTypeProvider,
            IEventService eventService,
            IEventPublishInfoStore eventPublishInfoStore,
            ILoggerFactory loggerFactory)
        {
            _commandStore = commandStore;
            _eventStore = eventStore;
            _waitingCommandService = waitingCommandService;
            _executedCommandService = executedCommandService;
            _commandHandlerProvider = commandHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _eventService = eventService;
            _eventPublishInfoStore = eventPublishInfoStore;
            _logger = loggerFactory.Create(GetType().FullName);
            _waitingCommandService.SetCommandExecutor(this);
        }

        #endregion

        #region Public Methods

        public void Execute(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Command;
            var context = processingCommand.CommandExecuteContext;
            var commandHandler = default(ICommandHandler);

            //首先验证command的合法性以及command handler是否存在
            try
            {
                if (!(command is ICreatingAggregateCommand) && string.IsNullOrEmpty(command.AggregateRootId))
                {
                    throw new CommandAggregateRootIdMissingException(command);
                }

                commandHandler = _commandHandlerProvider.GetCommandHandler(command);
                if (commandHandler == null)
                {
                    throw new CommandHandlerNotFoundException(command);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex.Message);
                NotifyCommandExecuteFailedOrNothingChanged(processingCommand, CommandStatus.Failed, ex.GetType().Name, ex.Message);
                return;
            }

            //判断当前command是否需要排队，如果需要排队，则会先排队；
            //ENode框架会对每个被执行的command按照聚合根id为分组进行必要的排队，如果发现一个聚合根有两个或以上的command要执行，
            //那会将第二个command以及之后的command全部排队；这样的设计可以确保对同一个聚合根的执行不会导致eventStore的并发冲突而导致不必要的重试；
            if (context.CheckCommandWaiting && _waitingCommandService.RegisterCommand(processingCommand))
            {
                return;
            }

            //调用command handler执行当前command
            var handleSuccess = false;
            try
            {
                commandHandler.Handle(context, command);
                _logger.DebugFormat("Handle command success. commandHandlerType:{0}, commandType:{1}, commandId:{2}, aggregateRootId:{3}",
                    commandHandler.GetInnerCommandHandler().GetType().Name,
                    command.GetType().Name,
                    command.Id,
                    command.AggregateRootId);
                handleSuccess = true;
            }
            catch (ENodeException)
            {
                //如果是ENodeException，则认为是由于网络问题，不是领域内产生的异常，比如是用户在通过context.Get<T>方法获取聚合根时，遇到异常；
                //这种异常是由ENode框架内部抛出来的，通常是由于memoryCache不可用或者eventStore不可用导致，所以此时我们应该直接throw该异常，
                //然后EQueue会认为当前消息处理失败，会继续重试。
                throw;
            }
            catch (Exception ex)
            {
                var result = HandleCommandException(processingCommand, commandHandler, ex);
                if (result == CommandExceptionHandleResult.ThrowDirectly)
                {
                    throw;
                }
            }

            //如果command执行成功，则提交执行后的结果，ENode框架在这里会保证一个command只能对一个聚合根做修改。
            if (handleSuccess)
            {
                CommitChanges(processingCommand);
            }
        }

        #endregion

        #region Private Methods

        private void CommitChanges(ProcessingCommand processingCommand)
        {
            var command = processingCommand.Command;
            var context = processingCommand.CommandExecuteContext;
            var trackedAggregateRoots = context.GetTrackedAggregateRoots();
            var dirtyAggregateRoots = trackedAggregateRoots.Where(x => x.GetUncommittedEvents().Any()).ToList();
            var dirtyAggregateRootCount = dirtyAggregateRoots.Count();

            //如果当前command没有对任何聚合根做修改，则认为当前command已经处理结束，返回command的结果为NothingChanged
            if (dirtyAggregateRootCount == 0)
            {
                _logger.DebugFormat("No aggregate created or modified by command. commandType:{0}, commandId:{1}",
                    command.GetType().Name,
                    command.Id);
                NotifyCommandExecuteFailedOrNothingChanged(processingCommand, CommandStatus.NothingChanged, null, null);
                return;
            }
            //如果发现被修改的聚合根操作一个，则认为当前command处理失败；
            else if (dirtyAggregateRootCount > 1)
            {
                var dirtyAggregateTypes = string.Join("|", dirtyAggregateRoots.Select(x => x.GetType().Name));
                var errorMessage = string.Format("Detected more than one aggregate created or modified by command. commandType:{0}, commandId:{1}, dirty aggregate types:{2}",
                    command.GetType().Name,
                    command.Id,
                    dirtyAggregateTypes);
                _logger.ErrorFormat(errorMessage);
                NotifyCommandExecuteFailedOrNothingChanged(processingCommand, CommandStatus.Failed, null, errorMessage);
                return;
            }

            //获取当前被修改的聚合根
            var dirtyAggregate = dirtyAggregateRoots.Single();

            //从该聚合根获取所有产生的领域事件，构造出一个EventStream对象
            var eventStream = BuildEventStream(dirtyAggregate, command);

            //尝试将当前已执行的command添加到commandStore
            var commandAddResult = _commandStore.AddCommand(
                new HandledCommand(
                    command,
                    eventStream.AggregateRootId,
                    eventStream.AggregateRootTypeCode));

            //如果添加的结果是command重复，则做如下处理
            if (commandAddResult == CommandAddResult.DuplicateCommand)
            {
                var existingHandledCommand = _commandStore.Find(command.Id);
                if (existingHandledCommand != null)
                {
                    var existingEventStream = _eventStore.Find(existingHandledCommand.AggregateRootId, command.Id);
                    if (existingEventStream != null)
                    {
                        var publishedVersion = _eventPublishInfoStore.GetEventPublishedVersion(existingHandledCommand.AggregateRootId);
                        if (publishedVersion < existingEventStream.Version)
                        {
                            //到这里，说明当前command在commandStore中已经存在，在eventStore中也存在，
                            //但是该command对应的eventStream还没被所有的消费者消费，所以我们需要再重新publish该command对应的eventStream。
                            _eventService.PublishEvent(processingCommand, existingEventStream);
                        }
                        else
                        {
                            //到这里，说明当前command被重复执行了，且上一次执行是成功的，因为
                            //1.该command已经在commandStore中存在;
                            //2.该command在eventStore中也存在;
                            //3.该command的eventStream也已经被所有的消费者消费了;
                            //所以，我们就可以忽略当前command的执行，不再需要做后续处理。
                            NotifyCommandExecuteFailedOrNothingChanged(processingCommand, CommandStatus.DuplicateAndIgnored, null, null);
                        }
                    }
                    else
                    {
                        //到这里，说明当前command在commandStore中存在，但是在eventStore中不存在，说明该command上次虽然执行了，
                        //加到commandStore成功了，但是该command的产生的事件持久化到eventStore中失败或者还未来得及持久化服务器就被重启了，
                        //这种情况，我们需要重新将当前command产生的事件提交的eventService进行后续处理。
                        _eventService.CommitEvent(new EventCommittingContext(dirtyAggregate, eventStream, processingCommand));
                    }
                }
                else
                {
                    //到这里，说明当前command想添加到commandStore中时，提示command重复，但是尝试从commandStore中取出该command时却找不到该command。
                    //出现这种情况，我们就无法再做后续处理了，这种错误理论上不会出现，除非commandStore的AddCommand接口和Find接口出现读写不一致的情况；
                    //我们记录错误日志，然后认为当前command已被处理为失败。
                    var errorMessage = string.Format("Command exist in the command store, but it cannot be found from the command store. commandType:{0}, commandId:{1}",
                        command.GetType().Name,
                        command.Id);
                    NotifyCommandExecuteFailedOrNothingChanged(processingCommand, CommandStatus.Failed, null, errorMessage);
                }
            }
            //如果添加成功，说明该command是第一次执行，那就将当前eventStream交给eventService做后续处理
            else if (commandAddResult == CommandAddResult.Success)
            {
                _eventService.CommitEvent(new EventCommittingContext(dirtyAggregate, eventStream, processingCommand));
            }
        }
        private EventStream BuildEventStream(IAggregateRoot aggregateRoot, ICommand command)
        {
            var uncommittedEvents = aggregateRoot.GetUncommittedEvents().ToList();
            aggregateRoot.ClearUncommittedEvents();

            var aggregateRootTypeCode = _aggregateRootTypeProvider.GetTypeCode(aggregateRoot.GetType());
            var nextVersion = aggregateRoot.Version + 1;
            var currentTime = DateTime.Now;
            var processId = command is IProcessCommand ? ((IProcessCommand)command).ProcessId : null;

            foreach (var evnt in uncommittedEvents)
            {
                evnt.Version = nextVersion;
                evnt.Timestamp = currentTime;
            }

            return new EventStream(
                command.Id,
                aggregateRoot.UniqueId,
                aggregateRootTypeCode,
                processId,
                nextVersion,
                currentTime,
                uncommittedEvents,
                command.Items);
        }
        private CommandExceptionHandleResult HandleCommandException(ProcessingCommand processingCommand, ICommandHandler commandHandler, Exception exception)
        {
            var command = processingCommand.Command;

            //到这里，说明当前command执行时遇到异常，且该异常是由于领域内用户自己抛出的异常；此时我们需要做如下的处理；
            //主要是判断当前command之前有没有被执行过，
            //如果有执行过，则继续判断是否后续所有的步骤都执行成功了，对每一种情况做相应的处理；
            //如果没有执行过，则也做相应处理。
            var existingHandledCommand = _commandStore.Find(command.Id);
            if (existingHandledCommand != null)
            {
                var existingEventStream = _eventStore.Find(existingHandledCommand.AggregateRootId, command.Id);
                if (existingEventStream != null)
                {
                    var publishedVersion = _eventPublishInfoStore.GetEventPublishedVersion(existingHandledCommand.AggregateRootId);
                    if (publishedVersion < existingEventStream.Version)
                    {
                        //到这里，说明当前command执行遇到异常，然后该command在commandStore中存在，
                        //在eventStore中也存在，但是该command对应的eventStream还没被所有的消费者消费。
                        //所以我们需要再重新publish该command对应的eventStream。
                        _eventService.PublishEvent(processingCommand, existingEventStream);
                    }
                    else
                    {
                        //到这里，说明当前command虽然执行遇到异常，但是因为
                        //1.该command已经在commandStore中存在;
                        //2.该command在eventStore中也存在;
                        //3.该command的eventStream也已经被所有的消费者消费了;
                        //因此，该command可以认为之前已经被成功处理了，所以，我们就可以吞掉当前异常，相当于是忽略当前command的执行；
                        NotifyCommandExecuteFailedOrNothingChanged(processingCommand, CommandStatus.DuplicateAndIgnored, exception.GetType().Name, exception.Message);
                    }
                }
                else
                {
                    //到这里，说明当前command遇到异常，然后该command在commandStore中存在，
                    //但是在eventStore中不存在，此时可以理解为该command还没被执行，所以需要将其从commandStore中移除，
                    //然后让其再重新被执行，我们只要简单的将当前异常继续throw出去，那equeue就是自动再次重试该command。
                    _commandStore.Remove(command.Id);
                    return CommandExceptionHandleResult.ThrowDirectly;
                }
            }
            else
            {
                //到这里，说明当前command执行遇到领域内的异常，然后当前command之前也没执行过，是第一次被执行，那我们就记录错误日志
                //然后认为该command处理失败即可。
                var commandHandlerType = commandHandler.GetInnerCommandHandler().GetType();
                var errorMessage = string.Format("{0} raised when {1} handling {2}. commandId:{3}, aggregateRootId:{4}, exceptionMessage:{5}",
                    exception.GetType().Name,
                    commandHandlerType.Name,
                    command.GetType().Name,
                    command.Id,
                    command.AggregateRootId,
                    exception.Message);
                _logger.Error(errorMessage, exception);
                NotifyCommandExecuteFailedOrNothingChanged(processingCommand, CommandStatus.Failed, exception.GetType().Name, exception.Message);
            }

            return CommandExceptionHandleResult.Success;
        }
        private void NotifyCommandExecuteFailedOrNothingChanged(ProcessingCommand processingCommand, CommandStatus commandStatus, string exceptionTypeName, string errorMessage)
        {
            var command = processingCommand.Command;
            var aggregateRootId = command.AggregateRootId;
            var processId = command is IProcessCommand ? ((IProcessCommand)command).ProcessId : null;

            _executedCommandService.ProcessExecutedCommand(
                processingCommand.CommandExecuteContext,
                processingCommand.Command,
                commandStatus,
                processId,
                aggregateRootId,
                exceptionTypeName,
                errorMessage);
        }

        #endregion

        enum CommandExceptionHandleResult
        {
            Success,
            ThrowDirectly
        }
    }
}
