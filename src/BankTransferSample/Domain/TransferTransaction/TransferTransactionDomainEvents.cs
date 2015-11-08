using System;
using BankTransferSample.Domain;
using ENode.Eventing;
using ENode.Infrastructure;

namespace BankTransferSample.Domain
{
    /// <summary>转账交易已开始
    /// </summary>
    [Serializable]
    [Code(1000008)]
    public abstract class AbstractTransferTransactionEvent : DomainEvent<string>
    {
        public TransferTransactionInfo TransactionInfo { get; private set; }

        public AbstractTransferTransactionEvent() { }
        public AbstractTransferTransactionEvent(TransferTransaction transaction, TransferTransactionInfo transactionInfo)
            : base(transaction)
        {
            TransactionInfo = transactionInfo;
        }
    }
    /// <summary>转账交易已开始
    /// </summary>
    [Serializable]
    [Code(1000009)]
    public class TransferTransactionStartedEvent : AbstractTransferTransactionEvent
    {
        public TransferTransactionStartedEvent() { }
        public TransferTransactionStartedEvent(TransferTransaction transaction, TransferTransactionInfo transactionInfo)
            : base(transaction, transactionInfo) { }
    }
    /// <summary>源账户验证通过事件已确认
    /// </summary>
    [Serializable]
    [Code(1000010)]
    public class SourceAccountValidatePassedConfirmedEvent : AbstractTransferTransactionEvent
    {
        public SourceAccountValidatePassedConfirmedEvent() { }
        public SourceAccountValidatePassedConfirmedEvent(TransferTransaction transaction, TransferTransactionInfo transactionInfo)
            : base(transaction, transactionInfo)
        {
        }
    }
    /// <summary>目标账户验证通过事件已确认
    /// </summary>
    [Serializable]
    [Code(1000011)]
    public class TargetAccountValidatePassedConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TargetAccountValidatePassedConfirmedEvent() { }
        public TargetAccountValidatePassedConfirmedEvent(TransferTransaction transaction, TransferTransactionInfo transactionInfo)
            : base(transaction, transactionInfo)
        {
        }
    }
    /// <summary>源账户和目标账户验证通过事件都已确认
    /// </summary>
    [Serializable]
    [Code(1000012)]
    public class AccountValidatePassedConfirmCompletedEvent : AbstractTransferTransactionEvent
    {
        public AccountValidatePassedConfirmCompletedEvent() { }
        public AccountValidatePassedConfirmCompletedEvent(TransferTransaction transaction, TransferTransactionInfo transactionInfo)
            : base(transaction, transactionInfo)
        {
        }
    }
    /// <summary>转账交易预转出已确认
    /// </summary>
    [Serializable]
    [Code(1000013)]
    public class TransferOutPreparationConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TransferOutPreparationConfirmedEvent() { }
        public TransferOutPreparationConfirmedEvent(TransferTransaction transaction, TransferTransactionInfo transactionInfo)
            : base(transaction, transactionInfo)
        {
        }
    }
    /// <summary>转账交易预转入已确认
    /// </summary>
    [Serializable]
    [Code(1000014)]
    public class TransferInPreparationConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TransferInPreparationConfirmedEvent() { }
        public TransferInPreparationConfirmedEvent(TransferTransaction transaction, TransferTransactionInfo transactionInfo)
            : base(transaction, transactionInfo)
        {
        }
    }
    /// <summary>转账交易转出已确认
    /// </summary>
    [Serializable]
    [Code(1000015)]
    public class TransferOutConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TransferOutConfirmedEvent() { }
        public TransferOutConfirmedEvent(TransferTransaction transaction, TransferTransactionInfo transactionInfo)
            : base(transaction, transactionInfo) { }
    }
    /// <summary>转账交易转入已确认
    /// </summary>
    [Serializable]
    [Code(1000016)]
    public class TransferInConfirmedEvent : AbstractTransferTransactionEvent
    {
        public TransferInConfirmedEvent() { }
        public TransferInConfirmedEvent(TransferTransaction transaction, TransferTransactionInfo transactionInfo)
            : base(transaction, transactionInfo) { }
    }
    /// <summary>转账交易已完成
    /// </summary>
    [Serializable]
    [Code(1000017)]
    public class TransferTransactionCompletedEvent : DomainEvent<string>
    {
        public TransferTransactionCompletedEvent() { }
        public TransferTransactionCompletedEvent(TransferTransaction transaction)
            : base(transaction) { }
    }
    /// <summary>转账交易已取消（结束），交易已失败
    /// </summary>
    [Serializable]
    [Code(1000018)]
    public class TransferTransactionCanceledEvent : DomainEvent<string>
    {
        public TransferTransactionCanceledEvent() { }
        public TransferTransactionCanceledEvent(TransferTransaction transaction)
            : base(transaction) { }
    }
}
