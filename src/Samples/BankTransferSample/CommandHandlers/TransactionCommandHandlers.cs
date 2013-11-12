using BankTransferSample.Commands;
using BankTransferSample.Domain.Transactions;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.CommandHandlers
{
    /// <summary>银行转账交易相关命令处理
    /// </summary>
    [Component]
    public class TransactionCommandHandlers :
        ICommandHandler<StartTransaction>,                 //开始交易
        ICommandHandler<ConfirmDebitPreparation>,          //确认预转出
        ICommandHandler<ConfirmCreditPreparation>,         //确认预转入
        ICommandHandler<ConfirmDebit>,                     //确认转出
        ICommandHandler<ConfirmCredit>                     //确认转入
    {
        public void Handle(ICommandContext context, StartTransaction command)
        {
            context.Add(new Transaction(command.TransactionInfo));
        }
        public void Handle(ICommandContext context, ConfirmDebitPreparation command)
        {
            context.Get<Transaction>(command.TransactionId).ConfirmDebitPreparation();
        }
        public void Handle(ICommandContext context, ConfirmCreditPreparation command)
        {
            context.Get<Transaction>(command.TransactionId).ConfirmCreditPreparation();
        }
        public void Handle(ICommandContext context, ConfirmDebit command)
        {
            context.Get<Transaction>(command.TransactionId).ConfirmDebit();
        }
        public void Handle(ICommandContext context, ConfirmCredit command)
        {
            context.Get<Transaction>(command.TransactionId).ConfirmCredit();
        }
    }
}
