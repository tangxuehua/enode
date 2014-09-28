using System;
using BankTransferSample.DomainEvents;
using ENode.Domain;

namespace BankTransferSample.Domain
{
    /// <summary>聚合根，表示一笔银行内账户之间的转账交易
    /// </summary>
    [Serializable]
    public class TransferTransaction : AggregateRoot<string>
    {
        #region Public Properties

        /// <summary>交易基本信息
        /// </summary>
        public TransferTransactionInfo TransactionInfo { get; private set; }
        /// <summary>交易开始时间
        /// </summary>
        public DateTime StartedTime { get; private set; }
        /// <summary>源账户验证通过
        /// </summary>
        public bool IsSourceAccountValidatePassed { get; private set; }
        /// <summary>目标账户验证通过
        /// </summary>
        public bool IsTargetAccountValidatePassed { get; private set; }
        /// <summary>预转出已确认
        /// </summary>
        public bool IsTransferOutPreparationConfirmed { get; private set; }
        /// <summary>预转入已确认
        /// </summary>
        public bool IsTransferInPreparationConfirmed { get; private set; }
        /// <summary>转出已确认
        /// </summary>
        public bool IsTransferOutConfirmed { get; private set; }
        /// <summary>转入已确认
        /// </summary>
        public bool IsTransferInConfirmed { get; private set; }
        /// <summary>交易状态
        /// </summary>
        public TransactionStatus Status { get; private set; }

        #endregion

        #region Constructors

        /// <summary>构造函数
        /// </summary>
        public TransferTransaction(string transactionId, TransferTransactionInfo transactionInfo)
            : base(transactionId)
        {
            ApplyEvent(new TransferTransactionStartedEvent(transactionId, transactionInfo));
        }

        #endregion

        #region Public Methods

        /// <summary>确认账户验证通过
        /// </summary>
        public void ConfirmAccountValidatePassed(string accountId)
        {
            if (Status == TransactionStatus.Started)
            {
                if (accountId == TransactionInfo.SourceAccountId)
                {
                    if (!IsSourceAccountValidatePassed)
                    {
                        ApplyEvent(new SourceAccountValidatePassedConfirmedEvent(Id, TransactionInfo));
                        if (IsTargetAccountValidatePassed)
                        {
                            ApplyEvent(new AccountValidatePassedConfirmCompletedEvent(Id, TransactionInfo));
                        }
                    }
                }
                else if (accountId == TransactionInfo.TargetAccountId)
                {
                    if (!IsTargetAccountValidatePassed)
                    {
                        ApplyEvent(new TargetAccountValidatePassedConfirmedEvent(Id, TransactionInfo));
                        if (IsSourceAccountValidatePassed)
                        {
                            ApplyEvent(new AccountValidatePassedConfirmCompletedEvent(Id, TransactionInfo));
                        }
                    }
                }
            }
        }
        /// <summary>确认预转出
        /// </summary>
        public void ConfirmTransferOutPreparation()
        {
            if (Status == TransactionStatus.AccountValidateCompleted)
            {
                if (!IsTransferOutPreparationConfirmed)
                {
                    ApplyEvent(new TransferOutPreparationConfirmedEvent(Id, TransactionInfo));
                }
            }
        }
        /// <summary>确认预转入
        /// </summary>
        public void ConfirmTransferInPreparation()
        {
            if (Status == TransactionStatus.AccountValidateCompleted)
            {
                if (!IsTransferInPreparationConfirmed)
                {
                    ApplyEvent(new TransferInPreparationConfirmedEvent(Id, TransactionInfo));
                }
            }
        }
        /// <summary>确认转出
        /// </summary>
        public void ConfirmTransferOut()
        {
            if (Status == TransactionStatus.PreparationCompleted)
            {
                if (!IsTransferOutConfirmed)
                {
                    ApplyEvent(new TransferOutConfirmedEvent(Id, TransactionInfo));
                    if (IsTransferInConfirmed)
                    {
                        ApplyEvent(new TransferTransactionCompletedEvent(Id));
                    }
                }
            }
        }
        /// <summary>确认转入
        /// </summary>
        public void ConfirmTransferIn()
        {
            if (Status == TransactionStatus.PreparationCompleted)
            {
                if (!IsTransferInConfirmed)
                {
                    ApplyEvent(new TransferInConfirmedEvent(Id, TransactionInfo));
                    if (IsTransferOutConfirmed)
                    {
                        ApplyEvent(new TransferTransactionCompletedEvent(Id));
                    }
                }
            }
        }
        /// <summary>取消转账交易
        /// </summary>
        public void Cancel()
        {
            if (Status == TransactionStatus.AccountValidateCompleted)
            {
                ApplyEvent(new TransferTransactionCanceledEvent(Id));
            }
        }

        #endregion

        #region Handler Methods

        private void Handle(TransferTransactionStartedEvent evnt)
        {
            Id = evnt.AggregateRootId;
            TransactionInfo = evnt.TransactionInfo;
            Status = TransactionStatus.Started;
        }
        private void Handle(SourceAccountValidatePassedConfirmedEvent evnt)
        {
            IsSourceAccountValidatePassed = true;
        }
        private void Handle(TargetAccountValidatePassedConfirmedEvent evnt)
        {
            IsTargetAccountValidatePassed = true;
        }
        private void Handle(AccountValidatePassedConfirmCompletedEvent evnt)
        {
            Status = TransactionStatus.AccountValidateCompleted;
        }
        private void Handle(TransferOutPreparationConfirmedEvent evnt)
        {
            IsTransferOutPreparationConfirmed = true;
        }
        private void Handle(TransferInPreparationConfirmedEvent evnt)
        {
            IsTransferInPreparationConfirmed = true;
            Status = TransactionStatus.PreparationCompleted;
        }
        private void Handle(TransferOutConfirmedEvent evnt)
        {
            IsTransferOutConfirmed = true;
        }
        private void Handle(TransferInConfirmedEvent evnt)
        {
            IsTransferInConfirmed = true;
        }
        private void Handle(TransferTransactionCompletedEvent evnt)
        {
            Status = TransactionStatus.Completed;
        }
        private void Handle(TransferTransactionCanceledEvent evnt)
        {
            Status = TransactionStatus.Canceled;
        }

        #endregion
    }
}
