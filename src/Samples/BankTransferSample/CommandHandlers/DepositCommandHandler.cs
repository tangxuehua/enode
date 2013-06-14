using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;

namespace BankTransferSample.CommandHandlers
{
    public class DepositCommandHandler : ICommandHandler<Deposit>
    {
        public void Handle(ICommandContext context, Deposit command)
        {
            context.Get<BankAccount>(command.AccountId).Deposit(command.Amount);
        }
    }
}
