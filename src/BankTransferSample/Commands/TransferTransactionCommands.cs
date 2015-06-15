using System;
using BankTransferSample.Domain;
using ENode.Commanding;

namespace BankTransferSample.Commands
{
    /// <summary>发起一笔转账交易
    /// </summary>
    [Serializable]
    public class StartTransferTransactionCommand : Command
    {
        /// <summary>转账交易信息
        /// </summary>
        public TransferTransactionInfo TransactionInfo { get; set; }

        public StartTransferTransactionCommand() { }
        public StartTransferTransactionCommand(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId)
        {
            TransactionInfo = transactionInfo;
        }
    }
    /// <summary>确认账户验证已通过
    /// </summary>
    [Serializable]
    public class ConfirmAccountValidatePassedCommand : Command
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
    public class ConfirmTransferOutPreparationCommand : Command
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
    public class ConfirmTransferInPreparationCommand : Command
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
    public class ConfirmTransferOutCommand : Command
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
    public class ConfirmTransferInCommand : Command
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
    public class CancelTransferTransactionCommand : Command
    {
        public CancelTransferTransactionCommand() { }
        public CancelTransferTransactionCommand(string transactionId)
            : base(transactionId)
        {
        }
    }
}
