using BankTransferSample.Commands;
using BankTransferSample.Domain;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.CommandHandlers {
    [Component]
    public class OpenAccountCommandHandler : ICommandHandler<OpenAccount> {
        public void Handle(ICommandContext context, OpenAccount command) {
            context.Add(new BankAccount(command.AccountId, command.AccountNumber, command.Owner));
        }
    }
}
