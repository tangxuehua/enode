using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ECommon.Scheduling;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Infrastructure;

namespace ENode.Exceptions.Impl
{
    public class DefaultExceptionProcessor : IMessageProcessor<IPublishableException>
    {
        #region Private Variables

        private int _workerCount = 1;
        private readonly ITypeCodeProvider<IExceptionHandler> _exceptionHandlerTypeCodeProvider;
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
        private readonly IMessageHandlerProvider<IExceptionHandler> _exceptionHandlerProvider;
        private readonly ICommandService _commandService;
        private readonly IRepository _repository;
        private readonly IActionExecutionService _actionExecutionService;
        private readonly ILogger _logger;
        private readonly IList<BlockingCollection<ExceptionProcessingContext>> _queueList;
        private readonly IList<Worker> _workerList;

        #endregion

        public string Name { get; set; }

        #region Constructors

        public DefaultExceptionProcessor(
            ITypeCodeProvider<IExceptionHandler> exceptionHandlerTypeCodeProvider,
            ITypeCodeProvider<ICommand> commandTypeCodeProvider,
            IMessageHandlerProvider<IExceptionHandler> exceptionHandlerProvider,
            ICommandService commandService,
            IRepository repository,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
        {
            Name = GetType().Name;
            _exceptionHandlerTypeCodeProvider = exceptionHandlerTypeCodeProvider;
            _commandTypeCodeProvider = commandTypeCodeProvider;
            _exceptionHandlerProvider = exceptionHandlerProvider;
            _commandService = commandService;
            _repository = repository;
            _actionExecutionService = actionExecutionService;
            _logger = loggerFactory.Create(GetType().FullName);
            _queueList = new List<BlockingCollection<ExceptionProcessingContext>>();
            _workerCount = ENodeConfiguration.Instance.Setting.ExceptionProcessorParallelThreadCount;
            for (var index = 0; index < _workerCount; index++)
            {
                _queueList.Add(new BlockingCollection<ExceptionProcessingContext>(new ConcurrentQueue<ExceptionProcessingContext>()));
            }

            _workerList = new List<Worker>();
            for (var index = 0; index < _workerCount; index++)
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

        public void Process(IPublishableException exception, IMessageProcessContext<IPublishableException> context)
        {
            var processingContext = new ExceptionProcessingContext(exception, context);
            var queueIndex = processingContext.Exception.UniqueId.GetHashCode() % _workerCount;
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
        private bool DispatchExceptionToHandler(IPublishableException exception, IExceptionHandler exceptionHandler)
        {
            var exceptionHandlerType = exceptionHandler.GetInnerHandler().GetType();
            var exceptionHandlerTypeCode = _exceptionHandlerTypeCodeProvider.GetTypeCode(exceptionHandlerType);
            var exceptionHandlingContext = new ExceptionHandlingContext(_repository);

            try
            {
                exceptionHandler.Handle(exceptionHandlingContext, exception);
                var commands = exceptionHandlingContext.GetCommands();
                if (commands.Any())
                {
                    foreach (var command in commands)
                    {
                        command.Id = BuildCommandId(command, exception, exceptionHandlerTypeCode);
                        _commandService.Send(command, null, exception.UniqueId);
                        _logger.DebugFormat("Send command success, commandType:{0}, commandId:{1}, exceptionHandlerType:{2}, exceptionType:{3}, exceptionId:{4}",
                            command.GetType().Name,
                            command.Id,
                            exceptionHandlerType.Name,
                            exception.GetType().Name,
                            exception.UniqueId);
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
        private string BuildCommandId(ICommand command, IPublishableException exception, int exceptionHandlerTypeCode)
        {
            var key = command.GetKey();
            var commandKey = key == null ? string.Empty : key.ToString();
            var commandTypeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            return string.Format("{0}{1}{2}{3}", exception.UniqueId, commandKey, exceptionHandlerTypeCode, commandTypeCode);
        }

        #endregion

        class ExceptionProcessingContext
        {
            public IPublishableException Exception { get; private set; }
            public IMessageProcessContext<IPublishableException> Context { get; private set; }

            public ExceptionProcessingContext(IPublishableException exception, IMessageProcessContext<IPublishableException> context)
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
