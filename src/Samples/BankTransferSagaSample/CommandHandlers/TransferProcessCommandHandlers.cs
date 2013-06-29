using BankTransferSagaSample.Commands;
using BankTransferSagaSample.Domain;
using ENode.Commanding;

namespace BankTransferSagaSample.CommandHandlers
{
    /// <summary>银行转账流程相关命令处理
    /// </summary>
    public class TransferProcessCommandHandlers :
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
            context.Get<TransferProcess>(command.ProcessId).HandleFailedTransferOut(command.TransferInfo, command.ErrorMessage);
        }
        public void Handle(ICommandContext context, HandleFailedTransferIn command)
        {
            context.Get<TransferProcess>(command.ProcessId).HandleFailedTransferIn(command.TransferInfo, command.ErrorMessage);
        }
        public void Handle(ICommandContext context, HandleTransferOutRolledback command)
        {
            context.Get<TransferProcess>(command.ProcessId).HandleTransferOutRolledback(command.TransferInfo);
        }
    }
}
