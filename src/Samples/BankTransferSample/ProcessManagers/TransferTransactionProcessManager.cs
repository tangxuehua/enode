using System.Linq;
using System.Threading.Tasks;
using BankTransferSample.ApplicationMessages;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using ENode.Messaging;

namespace BankTransferSample.ProcessManagers
{
    /// <summary>银行转账交易流程管理器，用于协调银行转账交易流程中各个参与者聚合根之间的消息交互。
    /// </summary>
    public class TransferTransactionProcessManager :
        IMessageHandler<TransferTransactionStartedEvent>,                  //转账交易已开始
        IMessageHandler<AccountValidatePassedMessage>,                     //账户验证已通过
        IMessageHandler<AccountValidateFailedMessage>,                     //账户验证未通过
        IMessageHandler<AccountValidatePassedConfirmCompletedEvent>,       //两个账户的验证通过事件都已确认
        IMessageHandler<TransactionPreparationAddedEvent>,                 //账户预操作已添加
        IMessageHandler<InsufficientBalanceException>,                     //账户余额不足
        IMessageHandler<TransferOutPreparationConfirmedEvent>,             //转账交易预转出已确认
        IMessageHandler<TransferInPreparationConfirmedEvent>,              //转账交易预转入已确认
        IMessageHandler<TransactionPreparationCommittedEvent>              //账户预操作已提交
    {
        private ICommandService _commandService;

        public TransferTransactionProcessManager(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public async Task HandleAsync(TransferTransactionStartedEvent evnt)
        {
            var task1 = _commandService.SendAsync(new ValidateAccountCommand(evnt.TransactionInfo.SourceAccountId, evnt.AggregateRootId) { Id = evnt.Id, Items = evnt.Items });
            var task2 = _commandService.SendAsync(new ValidateAccountCommand(evnt.TransactionInfo.TargetAccountId, evnt.AggregateRootId) { Id = evnt.Id, Items = evnt.Items });
            await Task.WhenAll(task1, task2).ConfigureAwait(false);
        }
        public async Task HandleAsync(AccountValidatePassedMessage message)
        {
            await _commandService.SendAsync(new ConfirmAccountValidatePassedCommand(message.TransactionId, message.AccountId) { Id = message.Id, Items = message.Items });
        }
        public async Task HandleAsync(AccountValidateFailedMessage message)
        {
            await _commandService.SendAsync(new CancelTransferTransactionCommand(message.TransactionId) { Id = message.Id, Items = message.Items });
        }
        public async Task HandleAsync(AccountValidatePassedConfirmCompletedEvent evnt)
        {
            await _commandService.SendAsync(new AddTransactionPreparationCommand(
                evnt.TransactionInfo.SourceAccountId,
                evnt.AggregateRootId,
                TransactionType.TransferTransaction,
                PreparationType.DebitPreparation,
                evnt.TransactionInfo.Amount)
            {
                Id = evnt.Id,
                Items = evnt.Items
            });
        }
        public async Task HandleAsync(TransactionPreparationAddedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    await _commandService.SendAsync(new ConfirmTransferOutPreparationCommand(evnt.TransactionPreparation.TransactionId) { Id = evnt.Id, Items = evnt.Items });
                }
                else if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    await _commandService.SendAsync(new ConfirmTransferInPreparationCommand(evnt.TransactionPreparation.TransactionId) { Id = evnt.Id, Items = evnt.Items });
                }
            }
        }
        public async Task HandleAsync(InsufficientBalanceException exception)
        {
            if (exception.TransactionType == TransactionType.TransferTransaction)
            {
                await _commandService.SendAsync(new CancelTransferTransactionCommand(exception.TransactionId) { Id = exception.Id, Items = exception.Items });
            }
        }
        public async Task HandleAsync(TransferOutPreparationConfirmedEvent evnt)
        {
            await _commandService.SendAsync(new AddTransactionPreparationCommand(
                evnt.TransactionInfo.TargetAccountId,
                evnt.AggregateRootId,
                TransactionType.TransferTransaction,
                PreparationType.CreditPreparation,
                evnt.TransactionInfo.Amount)
            {
                Id = evnt.Id,
                Items = evnt.Items
            });
        }
        public async Task HandleAsync(TransferInPreparationConfirmedEvent evnt)
        {
            var task1 = _commandService.SendAsync(new CommitTransactionPreparationCommand(evnt.TransactionInfo.SourceAccountId, evnt.AggregateRootId) { Id = evnt.Id, Items = evnt.Items });
            var task2 = _commandService.SendAsync(new CommitTransactionPreparationCommand(evnt.TransactionInfo.TargetAccountId, evnt.AggregateRootId) { Id = evnt.Id, Items = evnt.Items });
            await Task.WhenAll(task1, task2).ConfigureAwait(false);
        }
        public async Task HandleAsync(TransactionPreparationCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    await _commandService.SendAsync(new ConfirmTransferOutCommand(evnt.TransactionPreparation.TransactionId) { Id = evnt.Id, Items = evnt.Items });
                }
                else if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    await _commandService.SendAsync(new ConfirmTransferInCommand(evnt.TransactionPreparation.TransactionId) { Id = evnt.Id, Items = evnt.Items });
                }
            }
        }
    }
}
