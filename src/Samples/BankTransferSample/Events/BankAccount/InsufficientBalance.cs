using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>余额不足，不允许预转出操作
    /// </summary>
    [Serializable]
    public class InsufficientBalance  : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }

        public InsufficientBalance(string accountId, Guid transactionId) : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
