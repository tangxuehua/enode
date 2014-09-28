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
    /// <summary>源账户验证通过事件已确认
    /// </summary>
    [Serializable]
    public class SourceAccountValidatePassedConfirmedEvent : AbstractTransferTransactionEvent
    {
        public SourceAccountValidatePassedConfirmedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo)
        {
        }
    }
    /// <summary>目标账户验证通过事件已确认
    /// </summary>
    [Serializable]
    public class TargetAccountValidatePassedConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TargetAccountValidatePassedConfirmedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo)
        {
        }
    }
    /// <summary>源账户和目标账户验证通过事件都已确认
    /// </summary>
    [Serializable]
    public class AccountValidatePassedConfirmCompletedEvent : AbstractTransferTransactionEvent
    {
        public AccountValidatePassedConfirmCompletedEvent(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId, transactionInfo)
        {
        }
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
    /// <summary>转账交易已取消（结束），交易已失败
    /// </summary>
    [Serializable]
    public class TransferTransactionCanceledEvent : ProcessCompletedEvent<string>
    {
        public TransferTransactionCanceledEvent(string transactionId)
            : base(transactionId) { }
    }
}
