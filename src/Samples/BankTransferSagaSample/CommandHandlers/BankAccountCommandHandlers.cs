using BankTransferSagaSample.Commands;
using BankTransferSagaSample.Domain;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSagaSample.CommandHandlers {
    /// <summary>银行账户相关命令处理
    /// </summary>
    [Component]
    public class BankAccountCommandHandlers :
        ICommandHandler<OpenAccount>,         //开户
        ICommandHandler<Deposit>,             //存钱
        ICommandHandler<TransferOut>,         //转出
        ICommandHandler<TransferIn>,          //转入
        ICommandHandler<RollbackTransferOut>  //回滚转出
    {
        public void Handle(ICommandContext context, OpenAccount command) {
            context.Add(new BankAccount(command.AccountId, command.AccountNumber, command.Owner));
        }
        public void Handle(ICommandContext context, Deposit command) {
            context.Get<BankAccount>(command.AccountId).Deposit(command.Amount);
        }
        public void Handle(ICommandContext context, TransferOut command) {
            var sourceAccount = context.Get<BankAccount>(command.TransferInfo.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TransferInfo.TargetAccountId);
            sourceAccount.TransferOut(targetAccount, command.ProcessId, command.TransferInfo);
        }
        public void Handle(ICommandContext context, TransferIn command) {
            var sourceAccount = context.Get<BankAccount>(command.TransferInfo.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TransferInfo.TargetAccountId);
            targetAccount.TransferIn(sourceAccount, command.ProcessId, command.TransferInfo);
        }
        public void Handle(ICommandContext context, RollbackTransferOut command) {
            context.Get<BankAccount>(command.TransferInfo.SourceAccountId).RollbackTransferOut(command.ProcessId, command.TransferInfo);
        }
    }
}
