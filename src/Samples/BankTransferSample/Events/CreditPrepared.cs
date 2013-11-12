using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易预转入成功
    /// </summary>
    [Serializable]
    public class CreditPrepared  : SourcableDomainEvent<string>
    {
        public Guid TransactionId { get; private set; }
        public double Amount { get; private set; }

        public CreditPrepared(string accountId, Guid transactionId, double amount) : base(accountId)
        {
            TransactionId = transactionId;
            Amount = amount;
        }
    }
}
