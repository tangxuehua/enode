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
        ICommandHandler<HandleTransferedIn>
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
            var sourceAccount = context.Get<BankAccount>(command.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TargetAccountId);
            context.Add(new TransferProcessManager(sourceAccount, targetAccount, command.Amount));
        }
        public void Handle(ICommandContext context, TransferOut command)
        {
            var sourceAccount = context.Get<BankAccount>(command.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TargetAccountId);
            sourceAccount.TransferOut(targetAccount, command.Amount, command.ProcessId);
        }
        public void Handle(ICommandContext context, HandleTransferedOut command)
        {
            context.Get<TransferProcessManager>(command.Event.ProcessId).HandleTransferedOut(command.Event);
        }
        public void Handle(ICommandContext context, TransferIn command)
        {
            var sourceAccount = context.Get<BankAccount>(command.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TargetAccountId);
            targetAccount.TransferIn(sourceAccount, command.Amount, command.ProcessId);
        }
        public void Handle(ICommandContext context, HandleTransferedIn command)
        {
            context.Get<TransferProcessManager>(command.Event.ProcessId).HandleTransferedIn(command.Event);
        }
    }
}
