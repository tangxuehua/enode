using System.Linq;
using System.Threading;
using BankTransferSample.ApplicationMessages;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using BankTransferSample.EventHandlers;
using BankTransferSample.ProcessManagers;
using ECommon.Components;
using ECommon.Logging;
using ECommon.Scheduling;
using ENode.Commanding;
using ENode.Configurations;
using ENode.EQueue;
using ENode.EQueue.Commanding;
using ENode.Eventing;
using ENode.Infrastructure;
using ENode.Infrastructure.Impl;
using EQueue.Broker;
using EQueue.Configurations;

namespace BankTransferSample
{
    public static class ENodeExtensions
    {
        private static BrokerController _broker;
        private static CommandService _commandService;
        private static CommandResultProcessor _commandResultProcessor;
        private static CommandConsumer _commandConsumer;
        private static ApplicationMessagePublisher _applicationMessagePublisher;
        private static ApplicationMessageConsumer _applicationMessageConsumer;
        private static DomainEventPublisher _domainEventPublisher;
        private static DomainEventConsumer _eventConsumer;
        private static PublishableExceptionPublisher _exceptionPublisher;
        private static PublishableExceptionConsumer _exceptionConsumer;

        public static ENodeConfiguration UseEQueue(this ENodeConfiguration enodeConfiguration)
        {
            var configuration = enodeConfiguration.GetCommonConfiguration();

            configuration.RegisterEQueueComponents();

            _broker = new BrokerController();

            _commandResultProcessor = new CommandResultProcessor();
            _commandService = new CommandService(_commandResultProcessor);
            _applicationMessagePublisher = new ApplicationMessagePublisher();
            _domainEventPublisher = new DomainEventPublisher();
            _exceptionPublisher = new PublishableExceptionPublisher();

            configuration.SetDefault<ICommandService, CommandService>(_commandService);
            configuration.SetDefault<IMessagePublisher<IApplicationMessage>, ApplicationMessagePublisher>(_applicationMessagePublisher);
            configuration.SetDefault<IMessagePublisher<DomainEventStreamMessage>, DomainEventPublisher>(_domainEventPublisher);
            configuration.SetDefault<IMessagePublisher<IPublishableException>, PublishableExceptionPublisher>(_exceptionPublisher);

            _commandConsumer = new CommandConsumer().Subscribe("BankTransferCommandTopic");
            _applicationMessageConsumer = new ApplicationMessageConsumer().Subscribe("BankTransferApplicationMessageTopic");
            _eventConsumer = new DomainEventConsumer().Subscribe("BankTransferEventTopic");
            _exceptionConsumer = new PublishableExceptionConsumer().Subscribe("BankTransferExceptionTopic");

            return enodeConfiguration;
        }
        public static ENodeConfiguration StartEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _broker.Start();
            _exceptionConsumer.Start();
            _eventConsumer.Start();
            _applicationMessageConsumer.Start();
            _commandConsumer.Start();
            _applicationMessagePublisher.Start();
            _domainEventPublisher.Start();
            _exceptionPublisher.Start();
            _commandService.Start();
            _commandResultProcessor.Start();

            WaitAllConsumerLoadBalanceComplete();

            return enodeConfiguration;
        }
        public static ENodeConfiguration ShutdownEQueue(this ENodeConfiguration enodeConfiguration)
        {
            _commandResultProcessor.Shutdown();
            _commandService.Shutdown();
            _applicationMessagePublisher.Shutdown();
            _domainEventPublisher.Shutdown();
            _exceptionPublisher.Shutdown();
            _commandConsumer.Shutdown();
            _applicationMessageConsumer.Shutdown();
            _eventConsumer.Shutdown();
            _exceptionConsumer.Shutdown();
            _broker.Shutdown();
            return enodeConfiguration;
        }

        public static ENodeConfiguration RegisterAllTypeCodes(this ENodeConfiguration enodeConfiguration)
        {
            var provider = ObjectContainer.Resolve<ITypeCodeProvider>() as DefaultTypeCodeProvider;

            //aggregates
            provider.RegisterType<BankAccount>(100);
            provider.RegisterType<DepositTransaction>(101);
            provider.RegisterType<TransferTransaction>(102);

            //commands
            provider.RegisterType<CreateAccountCommand>(200);
            provider.RegisterType<ValidateAccountCommand>(201);
            provider.RegisterType<AddTransactionPreparationCommand>(202);
            provider.RegisterType<CommitTransactionPreparationCommand>(203);

            provider.RegisterType<StartDepositTransactionCommand>(204);
            provider.RegisterType<ConfirmDepositPreparationCommand>(205);
            provider.RegisterType<ConfirmDepositCommand>(206);

            provider.RegisterType<StartTransferTransactionCommand>(207);
            provider.RegisterType<ConfirmAccountValidatePassedCommand>(208);
            provider.RegisterType<ConfirmTransferOutPreparationCommand>(209);
            provider.RegisterType<ConfirmTransferInPreparationCommand>(210);
            provider.RegisterType<ConfirmTransferOutCommand>(211);
            provider.RegisterType<ConfirmTransferInCommand>(212);
            provider.RegisterType<CancelTransferTransactionCommand>(213);

            //application messages
            provider.RegisterType<AccountValidatePassedMessage>(300);
            provider.RegisterType<AccountValidateFailedMessage>(301);

            //domain events
            provider.RegisterType<AccountCreatedEvent>(400);
            provider.RegisterType<TransactionPreparationAddedEvent>(401);
            provider.RegisterType<TransactionPreparationCommittedEvent>(402);
            provider.RegisterType<TransactionPreparationCanceledEvent>(403);

            provider.RegisterType<DepositTransactionStartedEvent>(404);
            provider.RegisterType<DepositTransactionPreparationCompletedEvent>(405);
            provider.RegisterType<DepositTransactionCompletedEvent>(406);

            provider.RegisterType<TransferTransactionStartedEvent>(407);
            provider.RegisterType<SourceAccountValidatePassedConfirmedEvent>(408);
            provider.RegisterType<TargetAccountValidatePassedConfirmedEvent>(409);
            provider.RegisterType<AccountValidatePassedConfirmCompletedEvent>(410);
            provider.RegisterType<TransferOutPreparationConfirmedEvent>(411);
            provider.RegisterType<TransferInPreparationConfirmedEvent>(412);
            provider.RegisterType<TransferOutConfirmedEvent>(413);
            provider.RegisterType<TransferInConfirmedEvent>(414);
            provider.RegisterType<TransferTransactionCompletedEvent>(415);
            provider.RegisterType<TransferTransactionCanceledEvent>(416);

            //publishable exceptions
            provider.RegisterType<InsufficientBalanceException>(500);

            //application message and domain event handlers
            provider.RegisterType<DepositTransactionProcessManager>(600);
            provider.RegisterType<TransferTransactionProcessManager>(601);
            provider.RegisterType<ConsoleLogger>(602);
            provider.RegisterType<SyncHelper>(603);
            provider.RegisterType<CountSyncHelper>(604);

            return enodeConfiguration;
        }

        private static void WaitAllConsumerLoadBalanceComplete()
        {
            var logger = ObjectContainer.Resolve<ILoggerFactory>().Create(typeof(ENodeExtensions).Name);
            var scheduleService = ObjectContainer.Resolve<IScheduleService>();
            var waitHandle = new ManualResetEvent(false);

            logger.Info("Waiting for all consumer load balance complete, please wait for a moment...");
            var taskId = scheduleService.ScheduleTask("WaitAllConsumerLoadBalanceComplete", () =>
            {
                var eventConsumerAllocatedQueues = _eventConsumer.Consumer.GetCurrentQueues();
                var commandConsumerAllocatedQueues = _commandConsumer.Consumer.GetCurrentQueues();
                var exceptionConsumerAllocatedQueues = _exceptionConsumer.Consumer.GetCurrentQueues();
                var commandExecutedMessageConsumerAllocatedQueues = _commandResultProcessor.CommandExecutedMessageConsumer.GetCurrentQueues();
                var domainEventHandledMessageConsumerAllocatedQueues = _commandResultProcessor.DomainEventHandledMessageConsumer.GetCurrentQueues();
                if (eventConsumerAllocatedQueues.Count() == 4
                    && commandConsumerAllocatedQueues.Count() == 4
                    && exceptionConsumerAllocatedQueues.Count() == 4
                    && commandExecutedMessageConsumerAllocatedQueues.Count() == 4
                    && domainEventHandledMessageConsumerAllocatedQueues.Count() == 4)
                {
                    waitHandle.Set();
                }
            }, 1000, 1000);

            waitHandle.WaitOne();
            scheduleService.ShutdownTask(taskId);
            logger.Info("All consumer load balance completed.");
        }
    }
}
