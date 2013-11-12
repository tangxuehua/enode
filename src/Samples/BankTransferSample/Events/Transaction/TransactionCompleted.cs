using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易已完成
    /// </summary>
    [Serializable]
    public class TransactionCompleted : SourcableDomainEvent<Guid>
    {
        public TransactionInfo TransactionInfo { get; private set; }

        public TransactionCompleted(Guid transactionId, TransactionInfo transactionInfo) : base(transactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }
}
