using BankTransferSample.DomainEvents;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.Providers
{
    public class EventTypeCodeProvider : AbstractTypeCodeProvider, IEventTypeCodeProvider
    {
        public EventTypeCodeProvider()
        {
            RegisterType<AccountCreatedEvent>(100);
            RegisterType<AccountValidatePassedEvent>(101);
            RegisterType<TransactionPreparationAddedEvent>(102);
            RegisterType<TransactionPreparationCommittedEvent>(103);
            RegisterType<TransactionPreparationCanceledEvent>(104);
            RegisterType<InsufficientBalanceEvent>(105);

            RegisterType<DepositTransactionStartedEvent>(201);
            RegisterType<DepositTransactionPreparationCompletedEvent>(202);
            RegisterType<DepositTransactionCompletedEvent>(203);

            RegisterType<TransferTransactionStartedEvent>(301);
            RegisterType<SourceAccountValidatePassedConfirmedEvent>(302);
            RegisterType<TargetAccountValidatePassedConfirmedEvent>(303);
            RegisterType<AccountValidatePassedConfirmCompletedEvent>(304);
            RegisterType<TransferOutPreparationConfirmedEvent>(305);
            RegisterType<TransferInPreparationConfirmedEvent>(306);
            RegisterType<TransferOutConfirmedEvent>(307);
            RegisterType<TransferInConfirmedEvent>(308);
            RegisterType<TransferTransactionCompletedEvent>(309);
            RegisterType<TransferTransactionCanceledEvent>(310);
        }
    }
}
