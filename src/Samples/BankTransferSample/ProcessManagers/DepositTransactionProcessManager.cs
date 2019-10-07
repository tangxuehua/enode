using System.Threading.Tasks;
using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ECommon.IO;
using ENode.Commanding;
using ENode.Messaging;

namespace BankTransferSample.ProcessManagers
{
    /// <summary>银行存款交易流程管理器，用于协调银行存款交易流程中各个参与者聚合根之间的消息交互。
    /// </summary>
    public class DepositTransactionProcessManager :
        IMessageHandler<DepositTransactionStartedEvent>,                    //存款交易已开始
        IMessageHandler<DepositTransactionPreparationCompletedEvent>,       //存款交易已提交
        IMessageHandler<TransactionPreparationAddedEvent>,                  //账户预操作已添加
        IMessageHandler<TransactionPreparationCommittedEvent>               //账户预操作已提交
    {
        private ICommandService _commandService;

        public DepositTransactionProcessManager(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public Task<AsyncTaskResult> HandleAsync(DepositTransactionStartedEvent evnt)
        {
            return _commandService.SendAsync(new AddTransactionPreparationCommand(
                evnt.AccountId,
                evnt.AggregateRootId,
                TransactionType.DepositTransaction,
                PreparationType.CreditPreparation,
                evnt.Amount)
            {
                Id = evnt.Id
            });
        }
        public Task<AsyncTaskResult> HandleAsync(TransactionPreparationAddedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.DepositTransaction &&
                evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
            {
                return _commandService.SendAsync(new ConfirmDepositPreparationCommand(evnt.TransactionPreparation.TransactionId) { Id = evnt.Id });
            }
            return Task.FromResult(AsyncTaskResult.Success);
        }
        public Task<AsyncTaskResult> HandleAsync(DepositTransactionPreparationCompletedEvent evnt)
        {
            return _commandService.SendAsync(new CommitTransactionPreparationCommand(evnt.AccountId, evnt.AggregateRootId) { Id = evnt.Id });
        }
        public Task<AsyncTaskResult> HandleAsync(TransactionPreparationCommittedEvent evnt)
        {
            if (evnt.TransactionPreparation.TransactionType == TransactionType.DepositTransaction &&
                evnt.TransactionPreparation.PreparationType == PreparationType.CreditPreparation)
            {
                return _commandService.SendAsync(new ConfirmDepositCommand(evnt.TransactionPreparation.TransactionId) { Id = evnt.Id });
            }
            return Task.FromResult(AsyncTaskResult.Success);
        }
    }
}
