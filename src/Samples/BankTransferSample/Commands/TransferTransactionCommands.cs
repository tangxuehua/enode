using System;
using BankTransferSample.Domain;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>发起一笔转账交易
    /// </summary>
    [Serializable]
    public class StartTransferTransactionCommand : AggregateCommand<string>, ICreatingAggregateCommand
    {
        /// <summary>转账交易信息
        /// </summary>
        public TransferTransactionInfo TransactionInfo { get; set; }

        public StartTransferTransactionCommand() { }
        public StartTransferTransactionCommand(TransferTransactionInfo transactionInfo)
        {
            TransactionInfo = transactionInfo;
        }
    }
    /// <summary>确认账户验证已通过
    /// </summary>
    [Serializable]
    public class ConfirmAccountValidatePassedCommand : AggregateCommand<string>
    {
        /// <summary>账户ID
        /// </summary>
        public string AccountId { get; set; }

        public ConfirmAccountValidatePassedCommand() { }
        public ConfirmAccountValidatePassedCommand(string transactionId, string accountId)
            : base(transactionId)
        {
            AccountId = accountId;
        }
    }
    /// <summary>确认预转出
    /// </summary>
    [Serializable]
    public class ConfirmTransferOutPreparationCommand : AggregateCommand<string>
    {
        public ConfirmTransferOutPreparationCommand() { }
        public ConfirmTransferOutPreparationCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认预转入
    /// </summary>
    [Serializable]
    public class ConfirmTransferInPreparationCommand : AggregateCommand<string>
    {
        public ConfirmTransferInPreparationCommand() { }
        public ConfirmTransferInPreparationCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认转出
    /// </summary>
    [Serializable]
    public class ConfirmTransferOutCommand : AggregateCommand<string>
    {
        public ConfirmTransferOutCommand() { }
        public ConfirmTransferOutCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>确认转入
    /// </summary>
    [Serializable]
    public class ConfirmTransferInCommand : AggregateCommand<string>
    {
        public ConfirmTransferInCommand() { }
        public ConfirmTransferInCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
    /// <summary>取消转账交易
    /// </summary>
    [Serializable]
    public class CancelTransferTransactionCommand : AggregateCommand<string>
    {
        public CancelTransferTransactionCommand() { }
        public CancelTransferTransactionCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
}
