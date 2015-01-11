using System;
using System.Linq;
using ECommon.Logging;
using ECommon.Retring;
using ENode.Commanding;
using ENode.Configurations;
using ENode.Domain;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;

namespace ENode.Exceptions.Impl
{
    public class DefaultExceptionProcessor : AbstractParallelProcessor<IPublishableException>
    {
        #region Private Variables

        private readonly ITypeCodeProvider<IExceptionHandler> _exceptionHandlerTypeCodeProvider;
        private readonly ITypeCodeProvider<ICommand> _commandTypeCodeProvider;
        private readonly IHandlerProvider<IExceptionHandler> _exceptionHandlerProvider;
        private readonly ICommandService _commandService;
        private readonly IRepository _repository;
        private readonly ILogger _logger;

        #endregion

        #region Constructors

        public DefaultExceptionProcessor(
            ITypeCodeProvider<IExceptionHandler> exceptionHandlerTypeCodeProvider,
            ITypeCodeProvider<ICommand> commandTypeCodeProvider,
            IHandlerProvider<IExceptionHandler> exceptionHandlerProvider,
            ICommandService commandService,
            IRepository repository,
            IActionExecutionService actionExecutionService,
            ILoggerFactory loggerFactory)
            : base(ENodeConfiguration.Instance.Setting.ExceptionProcessorParallelThreadCount, "ProcessPublishableException")
        {
            Name = GetType().Name;
            _exceptionHandlerTypeCodeProvider = exceptionHandlerTypeCodeProvider;
            _commandTypeCodeProvider = commandTypeCodeProvider;
            _exceptionHandlerProvider = exceptionHandlerProvider;
            _commandService = commandService;
            _repository = repository;
            _logger = loggerFactory.Create(GetType().FullName);
        }

        #endregion

        protected override QueueMessage<IPublishableException> CreateQueueMessage(IPublishableException message, IProcessContext<IPublishableException> processContext)
        {
            return new QueueMessage<IPublishableException>(message.Id, message, processContext);
        }
        protected override void HandleQueueMessage(QueueMessage<IPublishableException> queueMessage)
        {
            var success = true;
            var publishableException = queueMessage.Payload;

            foreach (var exceptionHandler in _exceptionHandlerProvider.GetHandlers(publishableException.GetType()))
            {
                if (!DispatchExceptionToHandler(publishableException, exceptionHandler))
                {
                    success = false;
                }
            }

            if (!success)
            {
                _logger.ErrorFormat("Process publishable exception failed, exceptionId:{0}, exceptionType:{1}, retryTimes:{2}", publishableException.Id, publishableException.GetType().Name, queueMessage.RetryTimes);
            }

            OnMessageHandled(success, queueMessage);
        }

        #region Private Methods

        private bool DispatchExceptionToHandler(IPublishableException exception, IExceptionHandler exceptionHandler)
        {
            var handlerType = exceptionHandler.GetInnerHandler().GetType();
            var handlerTypeCode = _exceptionHandlerTypeCodeProvider.GetTypeCode(handlerType);
            var handlingContext = new DefaultHandlingContext(_repository);

            try
            {
                exceptionHandler.Handle(handlingContext, exception);
                var commands = handlingContext.GetCommands();
                if (commands.Any())
                {
                    foreach (var command in commands)
                    {
                        command.Id = BuildCommandId(command, exception, handlerTypeCode);
                        _commandService.Send(command, exception.Id, "Exception");
                        _logger.DebugFormat("Send command success, commandType:{0}, commandId:{1}, exceptionHandlerType:{2}, exceptionType:{3}, exceptionId:{4}",
                            command.GetType().Name,
                            command.Id,
                            handlerType.Name,
                            exception.GetType().Name,
                            exception.Id);
                    }
                }
                _logger.DebugFormat("Handle exception success. exceptionId:{0}, exceptionType:{1}, handlerType:{2}",
                    exception.Id,
                    exception.GetType().Name,
                    handlerType.Name);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error(string.Format("Exception raised when [{0}] handling [{1}].", handlerType.Name, exception.GetType().Name), ex);
                return false;
            }
        }
        private string BuildCommandId(ICommand command, IPublishableException exception, int exceptionHandlerTypeCode)
        {
            var key = command.GetKey();
            var commandKey = key == null ? string.Empty : key.ToString();
            var commandTypeCode = _commandTypeCodeProvider.GetTypeCode(command.GetType());
            return string.Format("{0}{1}{2}{3}", exception.Id, commandKey, exceptionHandlerTypeCode, commandTypeCode);
        }

        #endregion
    }
}
