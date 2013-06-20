using BankTransferSagaSample.Commands;
using BankTransferSagaSample.Domain;
using ENode.Commanding;

namespace BankTransferSagaSample.CommandHandlers
{
    /// <summary>银行账户相关命令处理
    /// </summary>
    public class BankAccountCommandHandlers :
        ICommandHandler<OpenAccount>,         //开户
        ICommandHandler<Deposit>,             //存钱
        ICommandHandler<TransferOut>,         //转出
        ICommandHandler<TransferIn>,          //转入
        ICommandHandler<RollbackTransferOut>  //回滚转出
    {
        public void Handle(ICommandContext context, OpenAccount command)
        {
            context.Add(new BankAccount(command.AccountId, command.AccountNumber, command.Owner));
        }
        public void Handle(ICommandContext context, Deposit command)
        {
            context.Get<BankAccount>(command.AccountId).Deposit(command.Amount);
        }
        public void Handle(ICommandContext context, TransferOut command)
        {
            var sourceAccount = context.Get<BankAccount>(command.TransferInfo.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TransferInfo.TargetAccountId);
            sourceAccount.TransferOut(targetAccount, command.ProcessId, command.TransferInfo);
        }
        public void Handle(ICommandContext context, TransferIn command)
        {
            var sourceAccount = context.Get<BankAccount>(command.TransferInfo.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TransferInfo.TargetAccountId);
            targetAccount.TransferIn(sourceAccount, command.ProcessId, command.TransferInfo);
        }
        public void Handle(ICommandContext context, RollbackTransferOut command)
        {
            context.Get<BankAccount>(command.TransferInfo.SourceAccountId).RollbackTransferOut(command.ProcessId, command.TransferInfo);
        }
    }

    /// <summary>银行转账相关命令处理
    /// </summary>
    public class TransferRelatedCommandHandlers :
        ICommandHandler<StartTransfer>,                //开始转账
        ICommandHandler<HandleTransferedOut>,          //处理已转出事件
        ICommandHandler<HandleTransferedIn>,           //处理已转入事件
        ICommandHandler<HandleFailedTransferOut>,      //处理转出失败
        ICommandHandler<HandleFailedTransferIn>,       //处理转入失败
        ICommandHandler<HandleTransferOutRolledback>   //处理转出已回滚事件
    {
        public void Handle(ICommandContext context, StartTransfer command)
        {
            var sourceAccount = context.Get<BankAccount>(command.TransferInfo.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TransferInfo.TargetAccountId);
            context.Add(new TransferProcess(sourceAccount, targetAccount, command.TransferInfo));
        }
        public void Handle(ICommandContext context, HandleTransferedOut command)
        {
            context.Get<TransferProcess>(command.ProcessId).HandleTransferedOut(command.TransferInfo);
        }
        public void Handle(ICommandContext context, HandleTransferedIn command)
        {
            context.Get<TransferProcess>(command.ProcessId).HandleTransferedIn(command.TransferInfo);
        }
        public void Handle(ICommandContext context, HandleFailedTransferOut command)
        {
            context.Get<TransferProcess>(command.ProcessId).HandleFailedTransferOut(command.TransferInfo);
        }
        public void Handle(ICommandContext context, HandleFailedTransferIn command)
        {
            context.Get<TransferProcess>(command.ProcessId).HandleFailedTransferIn(command.TransferInfo);
        }
        public void Handle(ICommandContext context, HandleTransferOutRolledback command)
        {
            context.Get<TransferProcess>(command.ProcessId).HandleTransferOutRolledback(command.TransferInfo);
        }
    }
}
