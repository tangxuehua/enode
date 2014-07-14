using BankTransferSample.Commands;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    public class CommandTypeCodeProvider : AbstractTypeCodeProvider, ICommandTypeCodeProvider
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateAccountCommand>(100);
            RegisterType<AddTransactionPreparationCommand>(101);
            RegisterType<CommitTransactionPreparationCommand>(102);
            RegisterType<CancelTransactionPreparationCommand>(103);

            RegisterType<StartDepositTransactionCommand>(201);
            RegisterType<ConfirmDepositPreparationCommand>(202);
            RegisterType<ConfirmDepositCommand>(203);

            RegisterType<StartTransferTransactionCommand>(300);
            RegisterType<ConfirmTransferOutPreparationCommand>(301);
            RegisterType<ConfirmTransferInPreparationCommand>(302);
            RegisterType<ConfirmTransferOutCommand>(303);
            RegisterType<ConfirmTransferInCommand>(304);
            RegisterType<StartCancelTransferTransactionCommand>(305);
            RegisterType<ConfirmTransferOutCanceledCommand>(306);
            RegisterType<ConfirmTransferInCanceledCommand>(307);
        }
    }
}
