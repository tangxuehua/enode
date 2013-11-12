using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易已终止
    /// </summary>
    [Serializable]
    public class TransactionAborted : SourcableDomainEvent<Guid>
    {
        public TransactionInfo TransactionInfo { get; private set; }

        public TransactionAborted(Guid transactionId, TransactionInfo transactionInfo) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }
}
