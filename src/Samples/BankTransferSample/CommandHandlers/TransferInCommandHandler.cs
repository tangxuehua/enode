using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;

namespace BankTransferSample.CommandHandlers
{
    public class TransferInCommandHandler : ICommandHandler<TransferIn>
    {
        public void Handle(ICommandContext context, TransferIn command)
        {
            var sourceAccount = context.Get<BankAccount>(command.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TargetAccountId);
            targetAccount.TransferIn(sourceAccount, command.Amount);
        }
    }
}
