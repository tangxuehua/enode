using System.Linq;
using System.Threading.Tasks;
using BankTransferSample.ApplicationMessages;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ECommon.IO;
using ENode.Commanding;
using ENode.Infrastructure;

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

        public async Task<AsyncTaskResult> HandleAsync(TransferTransactionStartedEvent evnt)
        {
            var task1 = _commandService.SendAsync(new ValidateAccountCommand(evnt.TransactionInfo.SourceAccountId, evnt.AggregateRootId));
            var task2 = _commandService.SendAsync(new ValidateAccountCommand(evnt.TransactionInfo.TargetAccountId, evnt.AggregateRootId));
            var totalResult = await Task.WhenAll(task1, task2).ConfigureAwait(false);

            var failedResults = totalResult.Where(x => x.Status == AsyncTaskStatus.Failed);
            if (failedResults.Count() > 0)
            {
                return new AsyncTaskResult(AsyncTaskStatus.Failed, string.Join("|", failedResults.Select(x => x.ErrorMessage)));
            }

            var ioExceptionResults = totalResult.Where(x => x.Status == AsyncTaskStatus.IOException);
            if (ioExceptionResults.Count() > 0)
            {
                return new AsyncTaskResult(AsyncTaskStatus.IOException, string.Join("|", ioExceptionResults.Select(x => x.ErrorMessage)));
            }

            return AsyncTaskResult.Success;
        }
        public Task<AsyncTaskResult> HandleAsync(AccountValidatePassedMessage message)
        {
            return _commandService.SendAsync(new ConfirmAccountValidatePassedCommand(message.TransactionId, message.AccountId));
        }
        public Task<AsyncTaskResult> HandleAsync(AccountValidateFailedMessage message)
        {
            return _commandService.SendAsync(new CancelTransferTransactionCommand(message.TransactionId));
        }
        public Task<AsyncTaskResult> HandleAsync(AccountValidatePassedConfirmCompletedEvent evnt)
        {
            return _commandService.SendAsync(new AddTransactionPreparationCommand(
                evnt.TransactionInfo.SourceAccountId,
                evnt.AggregateRootId,
                TransactionType.TransferTransaction,
                PreparationType.DebitPreparation,
                evnt.TransactionInfo.Amount));
        }
        public Task<AsyncTaskResult> HandleAsync(TransactionPreparationAddedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    return _commandService.SendAsync(new ConfirmTransferOutPreparationCommand(evnt.TransactionPreparation.TransactionId));
                }
                else if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    return _commandService.SendAsync(new ConfirmTransferInPreparationCommand(evnt.TransactionPreparation.TransactionId));
                }
            }
            return Task.FromResult(AsyncTaskResult.Success);
        }
        public Task<AsyncTaskResult> HandleAsync(InsufficientBalanceException exception)
        {
            if (exception.TransactionType == TransactionType.TransferTransaction)
            {
                return _commandService.SendAsync(new CancelTransferTransactionCommand(exception.TransactionId));
            }
            return Task.FromResult(AsyncTaskResult.Success);
        }
        public Task<AsyncTaskResult> HandleAsync(TransferOutPreparationConfirmedEvent evnt)
        {
            return _commandService.SendAsync(new AddTransactionPreparationCommand(
                evnt.TransactionInfo.TargetAccountId,
                evnt.AggregateRootId,
                TransactionType.TransferTransaction,
                PreparationType.CreditPreparation,
                evnt.TransactionInfo.Amount));
        }
        public async Task<AsyncTaskResult> HandleAsync(TransferInPreparationConfirmedEvent evnt)
        {
            var task1 = _commandService.SendAsync(new CommitTransactionPreparationCommand(evnt.TransactionInfo.SourceAccountId, evnt.AggregateRootId));
            var task2 = _commandService.SendAsync(new CommitTransactionPreparationCommand(evnt.TransactionInfo.TargetAccountId, evnt.AggregateRootId));
            var totalResult = await Task.WhenAll(task1, task2).ConfigureAwait(false);

            var failedResults = totalResult.Where(x => x.Status == AsyncTaskStatus.Failed);
            if (failedResults.Count() > 0)
            {
                return new AsyncTaskResult(AsyncTaskStatus.Failed, string.Join("|", failedResults.Select(x => x.ErrorMessage)));
            }

            var ioExceptionResults = totalResult.Where(x => x.Status == AsyncTaskStatus.IOException);
            if (ioExceptionResults.Count() > 0)
            {
                return new AsyncTaskResult(AsyncTaskStatus.IOException, string.Join("|", ioExceptionResults.Select(x => x.ErrorMessage)));
            }

            return AsyncTaskResult.Success;
        }
        public Task<AsyncTaskResult> HandleAsync(TransactionPreparationCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.TransferTransaction)
            {
                if (evnt.TransactionPreparation.PreparationType == PreparationType.DebitPreparation)
                {
                    return _commandService.SendAsync(new ConfirmTransferOutCommand(evnt.TransactionPreparation.TransactionId));
                }
                else if (evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
                {
                    return _commandService.SendAsync(new ConfirmTransferInCommand(evnt.TransactionPreparation.TransactionId));
                }
            }
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
}
