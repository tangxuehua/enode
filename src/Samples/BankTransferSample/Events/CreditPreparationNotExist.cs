using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易预转入信息不存在
    /// </summary>
    [Serializable]
    public class CreditPreparationNotExist  : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }

        public CreditPreparationNotExist(string accountId, Guid transactionId) : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
