using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易转入成功
    /// </summary>
    [Serializable]
    public class CreditCompleted  : SourcableDomainEvent<string>
    {
        public Guid TransactionId { get; private set; }
        public double Amount { get; private set; }

        public CreditCompleted(string accountId, Guid transactionId, double amount) : base(accountId)
        {
            TransactionId = transactionId;
            Amount = amount;
        }
    }
}
