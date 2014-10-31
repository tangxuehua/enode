using BankTransferSample.Commands;
using ECommon.Components;
using ENode.Commanding;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    [Component(LifeStyle.Singleton)]
    public class CommandTypeCodeProvider : AbstractTypeCodeProvider<ICommand>
    {
        public CommandTypeCodeProvider()
        {
            RegisterType<CreateAccountCommand>(100);
            RegisterType<ValidateAccountCommand>(101);
            RegisterType<AddTransactionPreparationCommand>(102);
            RegisterType<CommitTransactionPreparationCommand>(103);

            RegisterType<StartDepositTransactionCommand>(201);
            RegisterType<ConfirmDepositPreparationCommand>(202);
            RegisterType<ConfirmDepositCommand>(203);

            RegisterType<StartTransferTransactionCommand>(300);
            RegisterType<ConfirmAccountValidatePassedCommand>(301);
            RegisterType<ConfirmTransferOutPreparationCommand>(302);
            RegisterType<ConfirmTransferInPreparationCommand>(303);
            RegisterType<ConfirmTransferOutCommand>(304);
            RegisterType<ConfirmTransferInCommand>(305);
            RegisterType<CancelTransferTransactionCommand>(306);
        }
    }
}
