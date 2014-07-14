using System;
using BankTransferSample.Domain;
using ENode.Eventing;

namespace BankTransferSample.DomainEvents
{
    /// <summary>转账交易已开始
    /// </summary>
    [Serializable]
    public abstract class AbstractTransferTransactionEvent : DomainEvent<string>
    {
        public TransferTransactionInfo TransactionInfo { get; private set; }

        public AbstractTransferTransactionEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }
    /// <summary>转账交易已开始
    /// </summary>
    [Serializable]
    public class TransferTransactionStartedEvent : AbstractTransferTransactionEvent
    {
        public TransferTransactionStartedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo) { }
    }
    /// <summary>转账交易预转出已确认
    /// </summary>
    [Serializable]
    public class TransferOutPreparationConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TransferOutPreparationConfirmedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo)
        {
        }
    }
    /// <summary>转账交易预转入已确认
    /// </summary>
    [Serializable]
    public class TransferInPreparationConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TransferInPreparationConfirmedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo)
        {
        }
    }
    /// <summary>转账交易预转出和预转入都已确认
    /// </summary>
    [Serializable]
    public class TransferTransactionPreparationCompletedEvent : AbstractTransferTransactionEvent
    {
        public TransferTransactionPreparationCompletedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo)
        {
        }
    }
    /// <summary>转账交易转出已确认
    /// </summary>
    [Serializable]
    public class TransferOutConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TransferOutConfirmedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo) { }
    }
    /// <summary>转账交易转入已确认
    /// </summary>
    [Serializable]
    public class TransferInConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TransferInConfirmedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo) { }
    }
    /// <summary>转账交易已完成
    /// </summary>
    [Serializable]
    public class TransferTransactionCompletedEvent : ProcessCompletedEvent<string>
    {
        public TransferTransactionCompletedEvent(string transactionId)
            : base(transactionId) { }
    }
    /// <summary>转账交易取消已开始
    /// </summary>
    [Serializable]
    public class TransferTransactionCancelStartedEvent : AbstractTransferTransactionEvent
    {
        public TransferTransactionCancelStartedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo) { }
    }
    /// <summary>转账交易取消转出已确认
    /// </summary>
    [Serializable]
    public class TransferOutCanceledConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TransferOutCanceledConfirmedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo) { }
    }
    /// <summary>转账交易取消转入已确认
    /// </summary>
    [Serializable]
    public class TransferInCanceledConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TransferInCanceledConfirmedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo) { }
    }
    /// <summary>转账交易已取消（结束），交易已失败
    /// </summary>
    [Serializable]
    public class TransferTransactionCanceledEvent : ProcessCompletedEvent<string>
    {
        public TransferTransactionCanceledEvent(string transactionId)
            : base(transactionId) { }
    }
}
