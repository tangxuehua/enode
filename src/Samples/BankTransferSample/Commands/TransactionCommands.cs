using System;
using BankTransferSample.DomainEvents.Transaction;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>创建一笔转账交易
    /// </summary>
    [Serializable]
    public class CreateTransaction : Command, ICreatingAggregateCommand, IStartProcessCommand
    {
        public TransactionInfo TransactionInfo { get; private set; }
        public object ProcessId { get; private set; }

        public CreateTransaction(TransactionInfo transactionInfo)
            : base(transactionInfo.TransactionId)
        {
            TransactionInfo = transactionInfo;
            ProcessId = transactionInfo.TransactionId;
        }
    }
    /// <summary>发起转账交易
    /// </summary>
    [Serializable]
    public class StartTransaction : Command
    {
        public Guid TransactionId { get; private set; }

        public StartTransaction(Guid transactionId)
            : base(transactionId)
        {
            TransactionId = transactionId;
        }
    }
    /// <summary>确认预转出
    /// </summary>
    [Serializable]
    public class ConfirmDebitPreparation : Command
    {
        public Guid TransactionId { get; private set; }

        public ConfirmDebitPreparation(Guid transactionId)
            : base(transactionId)
        {
            TransactionId = transactionId;
        }
    }
    /// <summary>确认预转入
    /// </summary>
    [Serializable]
    public class ConfirmCreditPreparation : Command
    {
        public Guid TransactionId { get; private set; }

        public ConfirmCreditPreparation(Guid transactionId)
            : base(transactionId)
        {
            TransactionId = transactionId;
        }
    }
    /// <summary>确认转出
    /// </summary>
    [Serializable]
    public class ConfirmDebit : Command
    {
        public Guid TransactionId { get; private set; }

        public ConfirmDebit(Guid transactionId)
            : base(transactionId)
        {
            TransactionId = transactionId;
        }
    }
    /// <summary>确认转入
    /// </summary>
    [Serializable]
    public class ConfirmCredit : Command
    {
        public Guid TransactionId { get; private set; }

        public ConfirmCredit(Guid transactionId)
            : base(transactionId)
        {
            TransactionId = transactionId;
        }
    }
    /// <summary>终止转账交易
    /// </summary>
    [Serializable]
    public class AbortTransaction : Command
    {
        public Guid TransactionId { get; private set; }

        public AbortTransaction(Guid transactionId)
            : base(transactionId)
        {
            TransactionId = transactionId;
        }
    }
}
