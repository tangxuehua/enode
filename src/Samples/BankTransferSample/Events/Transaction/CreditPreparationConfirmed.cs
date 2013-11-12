using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易预转入已确认
    /// </summary>
    [Serializable]
    public class CreditPreparationConfirmed : SourcableDomainEvent<Guid>
    {
        public TransactionInfo TransactionInfo { get; private set; }

        public CreditPreparationConfirmed(Guid transactionId, TransactionInfo transactionInfo) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }
}
