using System;
using System.Threading.Tasks;
using BankTransferSample.ApplicationMessages;
using BankTransferSample.Domain;
using ECommon.Logging;
using ENode.Messaging;

namespace BankTransferSample.EventHandlers
{
    public class ConsoleLogger :
        IMessageHandler<AccountCreatedEvent>,
        IMessageHandler<AccountValidatePassedMessage>,
        IMessageHandler<AccountValidateFailedMessage>,
        IMessageHandler<TransactionPreparationAddedEvent>,
        IMessageHandler<TransactionPreparationCommittedEvent>,
        IMessageHandler<TransferTransactionStartedEvent>,
        IMessageHandler<TransferOutPreparationConfirmedEvent>,
        IMessageHandler<TransferInPreparationConfirmedEvent>,
        IMessageHandler<TransferTransactionCompletedEvent>,
        IMessageHandler<InsufficientBalanceException>,
        IMessageHandler<TransferTransactionCanceledEvent>
    {
        private ILogger _logger;

        public bool IsPerformanceTest { get; set; }

        public ConsoleLogger(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(ConsoleLogger).Name);
        }

        public Task HandleAsync(AccountCreatedEvent evnt)
        {
            LogFormat("账户已创建，账户：{0}，所有者：{1}", evnt.AggregateRootId, evnt.Owner);
            return Task.CompletedTask;
        }
        public Task HandleAsync(AccountValidatePassedMessage message)
        {
            LogFormat("账户验证已通过，交易ID：{0}，账户：{1}", message.TransactionId, message.AccountId);
            return Task.CompletedTask;
        }
        public Task HandleAsync(AccountValidateFailedMessage message)
        {
            LogFormat("无效的银行账户，交易ID：{0}，账户：{1}，理由：{2}", message.TransactionId, message.AccountId, message.Reason);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransactionPreparationAddedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    LogFormat("账户预转出成功，交易ID：{0}，账户：{1}，金额：{2}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount);
                }
                else if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    LogFormat("账户预转入成功，交易ID：{0}，账户：{1}，金额：{2}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount);
                }
            }
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransactionPreparationCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.DepositTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    LogFormat("账户存款已成功，账户：{0}，金额：{1}，当前余额：{2}", evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
                }
            }
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    LogFormat("账户转出已成功，交易ID：{0}，账户：{1}，金额：{2}，当前余额：{3}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
                }
                if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    LogFormat("账户转入已成功，交易ID：{0}，账户：{1}，金额：{2}，当前余额：{3}", evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation.AccountId, evnt.TransactionPreparation.Amount, evnt.CurrentBalance);
                }
            }
            return Task.CompletedTask;
        }

        public Task HandleAsync(TransferTransactionStartedEvent evnt)
        {
            LogFormat("转账交易已开始，交易ID：{0}，源账户：{1}，目标账户：{2}，转账金额：{3}", evnt.AggregateRootId, evnt.TransactionInfo.SourceAccountId, evnt.TransactionInfo.TargetAccountId, evnt.TransactionInfo.Amount);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferOutPreparationConfirmedEvent evnt)
        {
            LogFormat("预转出确认成功，交易ID：{0}，账户：{1}", evnt.AggregateRootId, evnt.TransactionInfo.SourceAccountId);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferInPreparationConfirmedEvent evnt)
        {
            LogFormat("预转入确认成功，交易ID：{0}，账户：{1}", evnt.AggregateRootId, evnt.TransactionInfo.TargetAccountId);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferTransactionCompletedEvent evnt)
        {
            LogFormat("转账交易已完成，交易ID：{0}", evnt.AggregateRootId);
            return Task.CompletedTask;
        }

        public Task HandleAsync(InsufficientBalanceException exception)
        {
            LogFormat("账户的余额不足，交易ID：{0}，账户：{1}，可用余额：{2}，转出金额：{3}", exception.TransactionId, exception.AccountId, exception.CurrentAvailableBalance, exception.Amount);
            return Task.CompletedTask;
        }
        public Task HandleAsync(TransferTransactionCanceledEvent evnt)
        {
            _logger.InfoFormat("转账交易已取消，交易ID：{0}", evnt.AggregateRootId);
            return Task.CompletedTask;
        }

        private void LogFormat(string format, params object[] args)
        {
            if (IsPerformanceTest)
            {
                return;
            }
            _logger.InfoFormat(format, args);
        }
    }
}
