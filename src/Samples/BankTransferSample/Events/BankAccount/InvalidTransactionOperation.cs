using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>在转账交易已完成后进行了无效的操作
    /// </summary>
    [Serializable]
    public class InvalidTransactionOperation : DomainEvent<string>
    {
        public Guid TransactionId { get; private set; }
        public TransactionOperation Operation { get; private set; }

        public InvalidTransactionOperation(string accountId, Guid transactionId, TransactionOperation operation) : base(accountId)
        {
            TransactionId = transactionId;
            Operation = operation;
        }
    }
    /// <summary>转账交易操作类型枚举
    /// </summary>
    public enum TransactionOperation
    {
        PrepareDebit,
        PrepareCredit,
        CompleteDebit,
        CompleteCredit
    }
}
