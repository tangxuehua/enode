using BankTransferSample.Commands;
using BankTransferSample.DomainEvents.BankAccount;
using BankTransferSample.DomainEvents.Transaction;
using ENode.Commanding;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.ProcessManagers
{
    /// <summary>银行转账交易流程管理器，用于协调银行转账交易流程中各个参与者聚合根之间的消息交互。
    /// </summary>
    [Component]
    public class TransactionProcessManager :
        IEventHandler<TransactionStarted>,                  //交易已开始
        IEventHandler<DebitPrepared>,                       //交易预转出成功
        IEventHandler<CreditPrepared>,                      //交易预转入成功
        IEventHandler<TransactionCommitted>,                //交易已提交
        IEventHandler<DebitCompleted>,                      //交易转出成功
        IEventHandler<CreditCompleted>                      //交易转入成功
    {
        private readonly ICommandService _commandService;

        public TransactionProcessManager(ICommandService commandService)
        {
            _commandService = commandService;
        }

        public void Handle(TransactionStarted evnt)
        {
            _commandService.Send(new PrepareDebit(evnt.TransactionInfo.SourceAccountId, evnt.SourceId, evnt.TransactionInfo.Amount));
            _commandService.Send(new PrepareCredit(evnt.TransactionInfo.TargetAccountId, evnt.SourceId, evnt.TransactionInfo.Amount));
        }
        public void Handle(DebitPrepared evnt)
        {
            _commandService.Send(new ConfirmDebitPreparation(evnt.TransactionId));
        }
        public void Handle(CreditPrepared evnt)
        {
            _commandService.Send(new ConfirmCreditPreparation(evnt.TransactionId));
        }
        public void Handle(TransactionCommitted evnt)
        {
            _commandService.Send(new CompleteDebit(evnt.TransactionInfo.SourceAccountId, evnt.SourceId));
            _commandService.Send(new CompleteCredit(evnt.TransactionInfo.TargetAccountId, evnt.SourceId));
        }
        public void Handle(DebitCompleted evnt)
        {
            _commandService.Send(new ConfirmDebit(evnt.TransactionId));
        }
        public void Handle(CreditCompleted evnt)
        {
            _commandService.Send(new ConfirmCredit(evnt.TransactionId));
        }
    }
}
