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
        /// <param name="loggerFactory"></param>
        public DefaultCommandExecutor(
            ICommandStore commandStore,
            IEventStore eventStore,
            IWaitingCommandService waitingCommandService,
            IExecutedCommandService executedCommandService,
            ICommandHandlerProvider commandHandlerProvider,
            IAggregateRootTypeCodeProvider aggregateRootTypeProvider,
            IEventService eventService,
            ILoggerFactory loggerFactory)
        {
            _commandStore = commandStore;
            _eventStore = eventStore;
            _waitingCommandService = waitingCommandService;
            _executedCommandService = executedCommandService;
            _commandHandlerProvider = commandHandlerProvider;
            _aggregateRootTypeProvider = aggregateRootTypeProvider;
            _eventService = eventService;
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

            //Validate command and get command handler.
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
                ProcessFailedCommand(processingCommand, CommandStatus.Failed, ex.GetType().Name, ex.Message);
                return;
            }

            //Try to add command to waiting queue if necessary. If the command is added into the waiting queue, it will be executed later.
            if (context.CheckCommandWaiting && _waitingCommandService.RegisterCommand(processingCommand))
            {
                return;
            }

            //Handle the command.
            try
            {
                commandHandler.Handle(context, command);
                _logger.DebugFormat("Handle command success. commandHandlerType:{0}, commandType:{1}, commandId:{2}, aggregateRootId:{3}",
                    commandHandler.GetInnerCommandHandler().GetType().Name,
                    command.GetType().Name,
                    command.Id,
                    command.AggregateRootId);
            }
            catch (Exception ex)
            {
                //If throws enode exception, we throws it directly as in this case we should retry the command again.
                if (ex is ENodeException)
                {
                    throw;
                }

                //All other exceptions mean the domain occurs exception,
                //in this case we should not retry the command and return the exception error back to user.
                var commandHandlerType = commandHandler.GetInnerCommandHandler().GetType();
                var errorMessage = string.Format("{0} raised when {1} handling {2}. commandId:{3}, aggregateRootId:{4}, exceptionMessage:{5}",
                    ex.GetType().Name,
                    commandHandlerType.Name,
                    command.GetType().Name,
                    command.Id,
                    command.AggregateRootId,
                    ex.Message);
                _logger.Error(errorMessage, ex);
                ProcessFailedCommand(processingCommand, CommandStatus.Failed, ex.GetType().Name, ex.Message);
            }

            //Commit the changes.
            CommitChanges(processingCommand);
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

            if (dirtyAggregateRootCount == 0)
            {
                _logger.DebugFormat("No aggregate created or modified by command. commandType:{0}, commandId:{1}",
                    command.GetType().Name,
                    command.Id);
                ProcessFailedCommand(processingCommand, CommandStatus.NothingChanged, null, null);
                return;
            }
            else if (dirtyAggregateRootCount > 1)
            {
                var dirtyAggregateTypes = string.Join("|", dirtyAggregateRoots.Select(x => x.GetType().Name));
                var errorMessage = string.Format("Detected more than one aggregate created or modified by command. commandType:{0}, commandId:{1}, dirty aggregate types:{2}",
                    command.GetType().Name,
                    command.Id,
                    dirtyAggregateTypes);
                _logger.ErrorFormat(errorMessage);
                ProcessFailedCommand(processingCommand, CommandStatus.Failed, null, errorMessage);
                return;
            }

            var dirtyAggregate = dirtyAggregateRoots.Single();
            var eventStream = BuildEventStream(dirtyAggregate, command);
            var commandAddResult = _commandStore.AddCommand(new HandledCommand(command, eventStream.AggregateRootId, eventStream.AggregateRootTypeCode, eventStream.Version));

            if (commandAddResult == CommandAddResult.DuplicateCommand)
            {
                var existingHandledCommand = _commandStore.Find(command.Id);
                if (existingHandledCommand != null)
                {
                    var existingEventStream = _eventStore.Find(existingHandledCommand.AggregateRootId, existingHandledCommand.Version);
                    if (existingEventStream != null)
                    {
                        _eventService.PublishEvent(processingCommand, existingEventStream);
                    }
                    else
                    {
                        _eventService.CommitEvent(new EventCommittingContext(dirtyAggregate, eventStream, processingCommand));
                    }
                }
                else
                {
                    var errorMessage = string.Format("Command exist in the command store, but it cannot be found from the command store. commandType:{0}, commandId:{1}",
                        command.GetType().Name,
                        command.Id);
                    ProcessFailedCommand(processingCommand, CommandStatus.Failed, null, errorMessage);
                }
            }
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
        private void ProcessFailedCommand(ProcessingCommand processingCommand, CommandStatus commandStatus, string exceptionTypeName, string errorMessage)
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
    }
}
