using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ECommon.Scheduling;
using ENode.Commanding;
using ENode.Domain;
using ENode.Exceptions;
using ENode.Infrastructure;

namespace ENode.Exceptions.Impl
{
    public class DefaultExceptionProcessor : IMessageProcessor<IException>
    {
        #region Private Variables

        private const int WorkerCount = 1;
        private readonly ITypeCodeProvider<IExceptionHandler> _exceptionHandlerTypeCodeProvider;
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
        private readonly IMessageHandlerProvider<IExceptionHandler> _exceptionHandlerProvider;
        private readonly IProcessCommandSender _processCommandSender;
        private readonly IRepository _repository;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ILogger _logger;
        private readonly IList<BlockingCollection<ExceptionProcessingContext>> _queueList;
        private readonly IList<Worker> _workerList;

        #endregion

        #region Constructors

        /// <summary>Parameterized constructor.
        /// </summary>
        /// <param name="exceptionHandlerTypeCodeProvider"></param>
        /// <param name="commandTypeCodeProvider"></param>
        /// <param name="exceptionHandlerProvider"></param>
        /// <param name="processCommandSender"></param>
        /// <param name="repository"></param>
        /// <param name="actionExecutionService"></param>
        /// <param name="loggerFactory"></param>
        public DefaultExceptionProcessor(
            ITypeCodeProvider<IExceptionHandler> exceptionHandlerTypeCodeProvider,
            ITypeCodeProvider<ICommand> commandTypeCodeProvider,
            IMessageHandlerProvider<IExceptionHandler> exceptionHandlerProvider,
            IProcessCommandSender processCommandSender,
            IRepository repository,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            _exceptionHandlerTypeCodeProvider = exceptionHandlerTypeCodeProvider;
            _commandTypeCodeProvider = commandTypeCodeProvider;
            _exceptionHandlerProvider = exceptionHandlerProvider;
            _processCommandSender = processCommandSender;
            _repository = repository;
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().FullName);
            _queueList = new List<BlockingCollection<ExceptionProcessingContext>>();
            for (var index = 0; index < WorkerCount; index++)
            {
                _queueList.Add(new BlockingCollection<ExceptionProcessingContext>(new ConcurrentQueue<ExceptionProcessingContext>()));
            }

            _workerList = new List<Worker>();
            for (var index = 0; index < WorkerCount; index++)
            {
                var queue = _queueList[index];
                var worker = new Worker("ProcessException", () =>
                {
                    ProcessException(queue.Take());
                });
                _workerList.Add(worker);
                worker.Start();
            }
        }

        #endregion

        public void Process(IException exception, IMessageProcessContext<IException> context)
        {
            var processingContext = new ExceptionProcessingContext(exception, context);
            var queueIndex = processingContext.Exception.UniqueId.GetHashCode() % WorkerCount;
            if (queueIndex < 0)
            {
                queueIndex = Math.Abs(queueIndex);
            }
            _queueList[queueIndex].Add(processingContext);
        }

        #region Private Methods

        private void ProcessException(ExceptionProcessingContext context)
        {
            _actionExecutionService.TryAction(
                "DispatchException",
                () => DispatchException(context),
                3,
                new ActionInfo("DispatchExceptionCallback", DispatchExceptionCallback, context, null));
        }
        private bool DispatchException(ExceptionProcessingContext context)
        {
            var exception = context.Exception;
            var success = true;

            foreach (var exceptionHandler in _exceptionHandlerProvider.GetMessageHandlers(exception.GetType()))
            {
                if (!DispatchExceptionToHandler(context.Exception, exceptionHandler))
                {
                    success = false;
                }
            }

            return success;
        }
        private bool DispatchExceptionCallback(object obj)
        {
            var processingContext = obj as ExceptionProcessingContext;
            processingContext.Context.OnMessageProcessed(processingContext.Exception);
            return true;
        }
        private bool DispatchExceptionToHandler(IException exception, IExceptionHandler exceptionHandler)
        {
            var exceptionHandlerTypeCode = _exceptionHandlerTypeCodeProvider.GetTypeCode(exceptionHandler.GetType());
            var exceptionHandlerType = exceptionHandler.GetInnerHandler().GetType();
            var exceptionHandlingContext = new ExceptionHandlingContext(_repository);

            try
            {
                exceptionHandler.Handle(exceptionHandlingContext, exception);
                var processCommands = exceptionHandlingContext.GetCommands();
                if (processCommands.Any())
                {
                    var processId = exception.Items["ProcessId"];
                    if (string.IsNullOrEmpty(processId))
                    {
                        throw new ENodeException("ProcessId cannot be null or empty if the exception handler generates commands. exceptionId:{0}, exceptionType:{1}, handlerType:{2}",
                            exception.UniqueId,
                            exception.GetType().Name,
                            exceptionHandlerType.Name);
                    }
                    foreach (var processCommand in processCommands)
                    {
                        processCommand.Id = BuildCommandId(processCommand, exception, exceptionHandlerTypeCode);
                        processCommand.Items["ProcessId"] = processId;
                        _processCommandSender.SendProcessCommand(processCommand, null, exception.UniqueId);
                        _logger.DebugFormat("Send process command success, commandType:{0}, commandId:{1}, exceptionHandlerType:{2}, exceptionType:{3}, exceptionId:{4}, processId:{5}",
                            processCommand.GetType().Name,
                            processCommand.Id,
                            exceptionHandlerType.Name,
                            exception.GetType().Name,
                            exception.UniqueId,
                            processId);
                    }
                }
                _logger.DebugFormat("Handle exception success. exceptionId:{0}, exceptionType:{1}, handlerType:{2}",
                    exception.UniqueId,
                    exception.GetType().Name,
                    exceptionHandlerType.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when [{0}] handling [{1}].", exceptionHandlerType.Name, exception.GetType().Name), ex);
                return false;
            }
        }
        private string BuildCommandId(ICommand command, IException exception, int exceptionHandlerTypeCode)
        {
            var key = command.GetKey();
            var commandKey = key == null ? string.Empty : key.ToString();
            var commandTypeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            return string.Format("{0}{1}{2}{3}", exception.UniqueId, commandKey, exceptionHandlerTypeCode, commandTypeCode);
        }

        #endregion

        class ExceptionProcessingContext
        {
            public IException Exception { get; private set; }
            public IMessageProcessContext<IException> Context { get; private set; }

            public ExceptionProcessingContext(IException exception, IMessageProcessContext<IException> context)
            {
                Exception = exception;
                Context = context;
            }
        }
        class ExceptionHandlingContext : IExceptionHandlingContext
        {
            private readonly List<ICommand> _commands = new List<ICommand>();
            private readonly IRepository _repository;

            public ExceptionHandlingContext(IRepository repository)
            {
                _repository = repository;
            }

            public T Get<T>(object aggregateRootId) where T : class, IAggregateRoot
            {
                return _repository.Get<T>(aggregateRootId);
            }
            public void AddCommand(ICommand command)
            {
                _commands.Add(command);
            }
            public IEnumerable<ICommand> GetCommands()
            {
                return _commands;
            }
        }
    }
}
