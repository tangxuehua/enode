using BankTransferSample.Commands;
using BankTransferSample.Domain;
using BankTransferSample.DomainEvents;
using BankTransferSample.Exceptions;
using ECommon.Components;
using ENode.Commanding;

namespace BankTransferSample.CommandHandlers
{
    /// <summary>银行账户相关命令处理
    /// </summary>
    [Component]
    public class BankAccountCommandHandlers :
        ICommandHandler<CreateAccountCommand>,                       //开户
        ICommandHandler<ValidateAccountCommand>,                     //验证账户是否合法
        ICommandHandler<AddTransactionPreparationCommand>,           //添加预操作
        ICommandHandler<CommitTransactionPreparationCommand>         //提交预操作
    {
        public void Handle(ICommandContext context, CreateAccountCommand command)
        {
            context.Add(new BankAccount(command.AggregateRootId, command.Owner));
        }
        public void Handle(ICommandContext context, ValidateAccountCommand command)
        {
            if (command.AccountId == "00003")
            {
                throw new InvalidAccountException(command.AccountId, command.TransactionId, "账户不可以是00003.");
            }
            context.Add(new AccountValidatePassedEvent(command.AccountId, command.TransactionId));
        }
        public void Handle(ICommandContext context, AddTransactionPreparationCommand command)
        {
            context.Get<BankAccount>(command.AggregateRootId).AddTransactionPreparation(command.TransactionId, command.TransactionType, command.PreparationType, command.Amount);
        }
        public void Handle(ICommandContext context, CommitTransactionPreparationCommand command)
        {
            context.Get<BankAccount>(command.AggregateRootId).CommitTransactionPreparation(command.TransactionId);
        }
    }
}
