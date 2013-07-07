using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.CommandHandlers
{
    [Component]
    public class TransferOutCommandHandler : ICommandHandler<TransferOut>
    {
        public void Handle(ICommandContext context, TransferOut command)
        {
            var sourceAccount = context.Get<BankAccount>(command.SourceAccountId);
            var targetAccount = context.Get<BankAccount>(command.TargetAccountId);
            sourceAccount.TransferOut(targetAccount, command.Amount);
        }
    }
}
