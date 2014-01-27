using System;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents.BankAccount
{
    /// <summary>交易转入已终止
    /// </summary>
    [Serializable]
    public class CreditAborted : SourcingEvent<string>
    {
        public Guid TransactionId { get; private set; }
        public double Amount { get; private set; }
        public DateTime AbortedTime { get; private set; }

        public CreditAborted(string accountId, Guid transactionId, double amount, DateTime abortedTime) : base(accountId)
        {
            TransactionId = transactionId;
            Amount = amount;
            AbortedTime = abortedTime;
        }
    }
}
