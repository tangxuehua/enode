using System;
using BankTransferSample.DomainEvents.Transaction;
using ENode.Domain;

namespace BankTransferSample.Domain.Transactions
{
    /// <summary>银行转账交易聚合根，封装一次转账交易的数据一致性
    /// </summary>
    [Serializable]
    public class Transaction : AggregateRoot<Guid>
    {
        #region Public Properties

        /// <summary>交易基本信息
        /// </summary>
        public TransactionInfo TransactionInfo { get; private set; }
        /// <summary>预转出已确认
        /// </summary>
        public bool DebitPreparationConfirmed { get; private set; }
        /// <summary>预转入已确认
        /// </summary>
        public bool CreditPreparationConfirmed { get; private set; }
        /// <summary>转出已确认
        /// </summary>
        public bool DebitConfirmed { get; private set; }
        /// <summary>转入已确认
        /// </summary>
        public bool CreditConfirmed { get; private set; }
        /// <summary>当前状态
        /// </summary>
        public TransactionStatus Status { get; private set; }

        #endregion

        #region Constructors

        /// <summary>构造函数
        /// </summary>
        /// <param name="transactionInfo"></param>
        public Transaction(TransactionInfo transactionInfo)
        {
            RaiseEvent(new TransactionCreated(Guid.NewGuid(), transactionInfo));
        }

        #endregion

        #region Public Methods

        /// <summary>开始交易
        /// </summary>
        public void Start()
        {
            if (Status == TransactionStatus.Created)
            {
                RaiseEvent(new TransactionStarted(Id, TransactionInfo, DateTime.Now));
            }
        }
        /// <summary>确认预转出
        /// </summary>
        public void ConfirmDebitPreparation()
        {
            if (Status == TransactionStatus.Started)
            {
                if (!DebitPreparationConfirmed)
                {
                    RaiseEvent(new DebitPreparationConfirmed(Id, TransactionInfo, DateTime.Now));
                    if (CreditPreparationConfirmed)
                    {
                        RaiseEvent(new TransactionCommitted(Id, TransactionInfo, DateTime.Now));
                    }
                }
            }
        }
        /// <summary>确认预转入
        /// </summary>
        public void ConfirmCreditPreparation()
        {
            if (Status == TransactionStatus.Started)
            {
                if (!CreditPreparationConfirmed)
                {
                    RaiseEvent(new CreditPreparationConfirmed(Id, TransactionInfo, DateTime.Now));
                    if (DebitPreparationConfirmed)
                    {
                        RaiseEvent(new TransactionCommitted(Id, TransactionInfo, DateTime.Now));
                    }
                }
            }
        }
        /// <summary>确认转出
        /// </summary>
        public void ConfirmDebit()
        {
            if (Status == TransactionStatus.Committed)
            {
                if (!DebitConfirmed)
                {
                    RaiseEvent(new DebitConfirmed(Id, TransactionInfo, DateTime.Now));
                    if (CreditConfirmed)
                    {
                        RaiseEvent(new TransactionCompleted(Id, TransactionInfo, DateTime.Now));
                    }
                }
            }
        }
        /// <summary>确认转入
        /// </summary>
        public void ConfirmCredit()
        {
            if (Status == TransactionStatus.Committed)
            {
                if (!CreditConfirmed)
                {
                    RaiseEvent(new CreditConfirmed(Id, TransactionInfo, DateTime.Now));
                    if (DebitConfirmed)
                    {
                        RaiseEvent(new TransactionCompleted(Id, TransactionInfo, DateTime.Now));
                    }
                }
            }
        }
        /// <summary>终止交易
        /// </summary>
        public void Abort()
        {
            if (Status == TransactionStatus.Started)
            {
                RaiseEvent(new TransactionAborted(Id, TransactionInfo, DateTime.Now));
            }
        }

        #endregion

        #region Handler Methods

        private void Handle(TransactionCreated evnt)
        {
            Id = evnt.SourceId;
            TransactionInfo = evnt.TransactionInfo;
            Status = TransactionStatus.Created;
        }
        private void Handle(TransactionStarted evnt)
        {
            Status = TransactionStatus.Started;
        }
        private void Handle(DebitPreparationConfirmed evnt)
        {
            DebitPreparationConfirmed = true;
        }
        private void Handle(CreditPreparationConfirmed evnt)
        {
            CreditPreparationConfirmed = true;
        }
        private void Handle(DebitConfirmed evnt)
        {
            DebitConfirmed = true;
        }
        private void Handle(CreditConfirmed evnt)
        {
            CreditConfirmed = true;
        }
        private void Handle(TransactionCommitted evnt)
        {
            Status = TransactionStatus.Committed;
        }
        private void Handle(TransactionCompleted evnt)
        {
            Status = TransactionStatus.Completed;
        }

        #endregion
    }
}
