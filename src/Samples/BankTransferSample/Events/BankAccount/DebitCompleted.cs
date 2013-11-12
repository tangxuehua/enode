using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易转出成功
    /// </summary>
    [Serializable]
    public class DebitCompleted  : SourcableDomainEvent<string>
    {
        public Guid TransactionId { get; private set; }
        public double Amount { get; private set; }

        public DebitCompleted(string accountId, Guid transactionId, double amount) : base(accountId)
        {
            TransactionId = transactionId;
            Amount = amount;
        }
    }
}
