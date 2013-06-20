using BankTransferSagaSample.Commands;
using BankTransferSagaSample.Domain;
using ENode.Commanding;

namespace BankTransferSagaSample.CommandHandlers
{
    public class CommandHandlers :
        ICommandHandler<OpenAccount>,
        ICommandHandler<Deposit>,
        ICommandHandler<Transfer>,
        ICommandHandler<TransferOut>,
        ICommandHandler<HandleTransferedOut>,
        ICommandHandler<TransferIn>,
        ICommandHandler<HandleTransferedIn>,
        ICommandHandler<HandleTransferOutFail>,
        ICommandHandler<HandleTransferInFail>,
        ICommandHandler<RollbackTransferOut>,
        ICommandHandler<CompleteTransfer>
    {
        public void Handle(ICommandContext context, OpenAccount command)
        {
            context.Add(new BankAccount(command.AccountId, command.AccountNumber, command.Customer));
        }
        public void Handle(ICommandContext context, Deposit command)
        {
            context.Get<BankAccount>(command.AccountId).Deposit(command.Amount);
        }
        public void Handle(ICommandContext context, Transfer command)
        {
            var sourceAccount = context.Get<BankAccount>(command.TransferInfo.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TransferInfo.TargetAccountId);
            context.Add(new TransferProcessManager(sourceAccount, targetAccount, command.TransferInfo));
        }
        public void Handle(ICommandContext context, TransferOut command)
        {
            var sourceAccount = context.Get<BankAccount>(command.TransferInfo.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TransferInfo.TargetAccountId);
            sourceAccount.TransferOut(targetAccount, command.ProcessId, command.TransferInfo);
        }
        public void Handle(ICommandContext context, HandleTransferedOut command)
        {
            context.Get<TransferProcessManager>(command.ProcessId).HandleTransferedOut(command.TransferInfo);
        }
        public void Handle(ICommandContext context, TransferIn command)
        {
            var sourceAccount = context.Get<BankAccount>(command.TransferInfo.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TransferInfo.TargetAccountId);
            targetAccount.TransferIn(sourceAccount, command.ProcessId, command.TransferInfo);
        }
        public void Handle(ICommandContext context, HandleTransferedIn command)
        {
            context.Get<TransferProcessManager>(command.ProcessId).HandleTransferedIn(command.TransferInfo);
        }
        public void Handle(ICommandContext context, HandleTransferOutFail command)
        {
            context.Get<TransferProcessManager>(command.ProcessId).HandleTransferOutFail(command.TransferInfo, command.ErrorMessage);
        }
        public void Handle(ICommandContext context, HandleTransferInFail command)
        {
            context.Get<TransferProcessManager>(command.ProcessId).HandleTransferInFail(command.TransferInfo, command.ErrorMessage);
        }
        public void Handle(ICommandContext context, RollbackTransferOut command)
        {
            context.Get<BankAccount>(command.TransferInfo.SourceAccountId).RollbackTransferOut(command.ProcessId, command.TransferInfo);
        }
        public void Handle(ICommandContext context, CompleteTransfer command)
        {
            context.Get<TransferProcessManager>(command.ProcessId).Complete(command.TransferInfo);
        }
    }
}
