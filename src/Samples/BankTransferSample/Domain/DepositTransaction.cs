using System;
using BankTransferSample.DomainEvents;
using ENode.Domain;

namespace BankTransferSample.Domain
{
    /// <summary>聚合根，表示一笔银行存款交易
    /// </summary>
    [Serializable]
    public class DepositTransaction : AggregateRoot<string>
    {
        #region Public Properties

        /// <summary>账户ID
        /// </summary>
        public string AccountId { get; private set; }
        /// <summary>存款金额
        /// </summary>
        public double Amount { get; private set; }
        /// <summary>交易开始时间
        /// </summary>
        public DateTime StartedTime { get; private set; }
        /// <summary>交易状态
        /// </summary>
        public TransactionStatus Status { get; private set; }

        #endregion

        #region Constructors

        /// <summary>构造函数
        /// </summary>
        public DepositTransaction(string transactionId, string accountId, double amount)
            : base(transactionId)
        {
            ApplyEvent(new DepositTransactionStartedEvent(transactionId, accountId, amount));
        }

        #endregion

        #region Public Methods

        /// <summary>确认预存款
        /// </summary>
        public void ConfirmDepositPreparation()
        {
            if (Status == TransactionStatus.Started)
            {
                ApplyEvent(new DepositTransactionPreparationCompletedEvent(Id, AccountId));
            }
        }
        /// <summary>确认存款
        /// </summary>
        public void ConfirmDeposit()
        {
            if (Status == TransactionStatus.PreparationCompleted)
            {
                ApplyEvent(new DepositTransactionCompletedEvent(Id, AccountId));
            }
        }

        #endregion

        #region Handler Methods

        private void Handle(DepositTransactionStartedEvent evnt)
        {
            Id = evnt.AggregateRootId;
            AccountId = evnt.AccountId;
            Amount = evnt.Amount;
            Status = TransactionStatus.Started;
        }
        private void Handle(DepositTransactionPreparationCompletedEvent evnt)
        {
            Status = TransactionStatus.PreparationCompleted;
        }
        private void Handle(DepositTransactionCompletedEvent evnt)
        {
            Status = TransactionStatus.Completed;
        }

        #endregion
    }
}
