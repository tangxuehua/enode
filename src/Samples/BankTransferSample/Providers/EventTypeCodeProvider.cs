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
            RegisterType<TransactionPreparationAddedEvent>(101);
            RegisterType<TransactionPreparationCommittedEvent>(102);
            RegisterType<TransactionPreparationCanceledEvent>(103);
            RegisterType<InsufficientBalanceEvent>(104);

            RegisterType<DepositTransactionStartedEvent>(201);
            RegisterType<DepositTransactionPreparationCompletedEvent>(202);
            RegisterType<DepositTransactionCompletedEvent>(203);

            RegisterType<TransferTransactionStartedEvent>(301);
            RegisterType<TransferOutPreparationConfirmedEvent>(302);
            RegisterType<TransferInPreparationConfirmedEvent>(303);
            RegisterType<TransferOutConfirmedEvent>(304);
            RegisterType<TransferInConfirmedEvent>(305);
            RegisterType<TransferTransactionCompletedEvent>(306);
            RegisterType<TransferTransactionCanceledEvent>(307);
        }
    }
}
