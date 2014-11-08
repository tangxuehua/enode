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
        #region Private Variables

        private string _accountId;
        private double _amount;
        private TransactionStatus _status;

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
            if (_status == TransactionStatus.Started)
            {
                ApplyEvent(new DepositTransactionPreparationCompletedEvent(Id, _accountId));
            }
        }
        /// <summary>确认存款
        /// </summary>
        public void ConfirmDeposit()
        {
            if (_status == TransactionStatus.PreparationCompleted)
            {
                ApplyEvent(new DepositTransactionCompletedEvent(Id, _accountId));
            }
        }

        #endregion

        #region Handler Methods

        private void Handle(DepositTransactionStartedEvent evnt)
        {
            _id = evnt.AggregateRootId;
            _accountId = evnt.AccountId;
            _amount = evnt.Amount;
            _status = TransactionStatus.Started;
        }
        private void Handle(DepositTransactionPreparationCompletedEvent evnt)
        {
            _status = TransactionStatus.PreparationCompleted;
        }
        private void Handle(DepositTransactionCompletedEvent evnt)
        {
            _status = TransactionStatus.Completed;
        }

        #endregion
    }
}
