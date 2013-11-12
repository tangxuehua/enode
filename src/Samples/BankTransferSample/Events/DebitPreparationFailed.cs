using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易预转出失败
    /// </summary>
    [Serializable]
    public class DebitPreparationFailed  : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }
        public string Reason { get; private set; }

        public DebitPreparationFailed(string accountId, Guid transactionId, string reason) : base(accountId)
        {
            TransactionId = transactionId;
            Reason = reason;
        }
    }
}
