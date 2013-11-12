using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易预转出已确认
    /// </summary>
    [Serializable]
    public class DebitPreparationConfirmed : SourcableDomainEvent<Guid>
    {
        public TransactionInfo TransactionInfo { get; private set; }

        public DebitPreparationConfirmed(Guid transactionId, TransactionInfo transactionInfo) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }
}
