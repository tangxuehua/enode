using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>交易预转出信息不存在
    /// </summary>
    [Serializable]
    public class DebitPreparationNotExist  : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }

        public DebitPreparationNotExist(string accountId, Guid transactionId) : base(accountId)
        {
            TransactionId = transactionId;
        }
    }
}
