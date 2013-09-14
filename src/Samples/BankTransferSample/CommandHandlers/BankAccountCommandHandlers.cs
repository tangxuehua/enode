using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.CommandHandlers
{
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
        public void Handle(ICommandContext context, OpenAccount command)
        {
            context.Add(new BankAccount(command.AccountNumber, command.Owner));
        }
        public void Handle(ICommandContext context, Deposit command)
        {
            context.Get<BankAccount>(command.AccountNumber).Deposit(command.Amount);
        }
        public void Handle(ICommandContext context, TransferOut command)
        {
            context.Get<BankAccount>(command.TransferInfo.SourceAccountNumber).TransferOut(command.ProcessId, command.TransferInfo);
        }
        public void Handle(ICommandContext context, TransferIn command)
        {
            context.Get<BankAccount>(command.TransferInfo.TargetAccountNumber).TransferIn(command.ProcessId, command.TransferInfo);
        }
        public void Handle(ICommandContext context, RollbackTransferOut command)
        {
            context.Get<BankAccount>(command.TransferInfo.SourceAccountNumber).RollbackTransferOut(command.ProcessId, command.TransferInfo);
        }
    }
}
