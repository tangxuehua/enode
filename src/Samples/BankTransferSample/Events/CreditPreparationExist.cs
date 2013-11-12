using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易预转入信息已存在
    /// </summary>
    [Serializable]
    public class CreditPreparationExist  : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }

        public CreditPreparationExist(string accountId, Guid transactionId) : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
