using System;
using BankTransferSample.Events;
using ENode.Domain;

namespace BankTransferSample.Domain
{
    /// <summary>银行转账交易聚合根，封装一次转账交易的数据一致性
    /// </summary>
    [Serializable]
    public class Transaction : AggregateRoot<Guid>
    {
        #region Private Variables

        private bool _debitPreparationConfirmed;
        private bool _creditPreparationConfirmed;

        #endregion

        #region Public Properties

        /// <summary>当前转账交易的转账基本信息
        /// </summary>
        public TransactionInfo TransactionInfo { get; private set; }
        /// <summary>当前转账交易的状态
        /// </summary>
        public TransactionStatus Status { get; private set; }

        #endregion

        #region Constructors

        /// <summary>构造函数
        /// </summary>
        /// <param name="transferInfo"></param>
        public Transaction(TransactionInfo transferInfo)
        {
            RaiseEvent(new TransactionStarted(Guid.NewGuid(), transferInfo));
        }

        #endregion

        #region Public Methods

        /// <summary>确认预转出
        /// </summary>
        public void ConfirmDebitPreparation()
        {
            if (!_debitPreparationConfirmed)
            {
                RaiseEvent(new DebitPreparationConfirmed(Id, TransactionInfo));
            }
            if (_creditPreparationConfirmed)
            {
                RaiseEvent(new TransactionCompleted(Id, TransactionInfo));
            }
        }
        /// <summary>确认预转入
        /// </summary>
        public void ConfirmCreditPreparation()
        {
            if (!_creditPreparationConfirmed)
            {
                RaiseEvent(new CreditPreparationConfirmed(Id, TransactionInfo));
            }
            if (_debitPreparationConfirmed)
            {
                RaiseEvent(new TransactionCompleted(Id, TransactionInfo));
            }
        }
        /// <summary>终止转账交易
        /// </summary>
        public void Abort()
        {
            if (Status != TransactionStatus.Aborted && Status != TransactionStatus.Completed)
            {
                RaiseEvent(new TransactionAborted(Id, TransactionInfo));
            }
        }

        #endregion

        #region Private Methods

        private void Handle(TransactionStarted evnt)
        {
            Status = TransactionStatus.Started;
        }
        private void Handle(DebitPreparationConfirmed evnt)
        {
            Status = TransactionStatus.DebitPreparationConfirmed;
            _debitPreparationConfirmed = true;
        }
        private void Handle(CreditPreparationConfirmed evnt)
        {
            Status = TransactionStatus.CreditPreparationConfirmed;
            _creditPreparationConfirmed = true;
        }
        private void Handle(TransactionCompleted evnt)
        {
            Status = TransactionStatus.Completed;
        }
        private void Handle(TransactionAborted evnt)
        {
            Status = TransactionStatus.Aborted;
        }

        #endregion
    }
}
