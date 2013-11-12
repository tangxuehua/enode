using BankTransferSample.Commands;
using BankTransferSample.Domain.BankAccounts;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.CommandHandlers
{
    /// <summary>银行账户相关命令处理
    /// </summary>
    [Component]
    public class BankAccountCommandHandlers :
        ICommandHandler<CreateAccount>,         //开户
        ICommandHandler<Deposit>,               //存钱
        ICommandHandler<Withdraw>,              //取钱
        ICommandHandler<PrepareDebit>,          //预转出
        ICommandHandler<PrepareCredit>,         //预转入
        ICommandHandler<CompleteDebit>,         //完成转出
        ICommandHandler<CompleteCredit>         //完成转入
    {
        public void Handle(ICommandContext context, CreateAccount command)
        {
            context.Add(new BankAccount(command.AccountId, command.Owner));
        }
        public void Handle(ICommandContext context, Deposit command)
        {
            context.Get<BankAccount>(command.AccountId).Deposit(command.Amount);
        }
        public void Handle(ICommandContext context, Withdraw command)
        {
            context.Get<BankAccount>(command.AccountId).Withdraw(command.Amount);
        }
        public void Handle(ICommandContext context, PrepareDebit command)
        {
            context.Get<BankAccount>(command.AccountId).PrepareDebit(command.TransactionId, command.Amount);
        }
        public void Handle(ICommandContext context, PrepareCredit command)
        {
            context.Get<BankAccount>(command.AccountId).PrepareCredit(command.TransactionId, command.Amount);
        }
        public void Handle(ICommandContext context, CompleteDebit command)
        {
            context.Get<BankAccount>(command.AccountId).CompleteDebit(command.TransactionId);
        }
        public void Handle(ICommandContext context, CompleteCredit command)
        {
            context.Get<BankAccount>(command.AccountId).CompleteCredit(command.TransactionId);
        }
    }
}
