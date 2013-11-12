using System;
using System.Collections.Generic;
using System.Linq;
using BankTransferSample.Events;
using ENode.Domain;

namespace BankTransferSample.Domain.BankAccounts
{
    /// <summary>银行账号聚合根
    /// </summary>
    [Serializable]
    public class BankAccount : AggregateRoot<string>
    {
        #region Private Variables

        private IList<DebitPreparation> _debitPreparations;
        private IList<CreditPreparation> _creditPreparations;

        #endregion

        #region Public Properties

        /// <summary>拥有者
        /// </summary>
        public string Owner { get; private set; }
        /// <summary>当前余额
        /// </summary>
        public double Balance { get; private set; }

        #endregion

        #region Constructors

        /// <summary>构造函数
        /// </summary>
        /// <param name="accountId"></param>
        /// <param name="owner"></param>
        public BankAccount(string accountId, string owner)
        {
            RaiseEvent(new AccountOpened(accountId, owner));
        }

        #endregion

        #region Protected Methods

        /// <summary>初始化账号聚合根
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            _debitPreparations = new List<DebitPreparation>();
            _creditPreparations = new List<CreditPreparation>();
        }

        #endregion

        #region Public Methods

        /// <summary>存款
        /// </summary>
        /// <param name="amount"></param>
        public void Deposit(double amount)
        {
            RaiseEvent(new Deposited(Id, amount));
        }
        /// <summary>预转出
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="amount"></param>
        public void PrepareDebit(Guid transactionId, double amount)
        {
            if (_debitPreparations.Any(x => x.TransactionId == transactionId))
            {
                RaiseEvent(new DebitPreparationExist(Id, transactionId));
            }
            else
            {
                if (Balance < amount)
                {
                    RaiseEvent(new DebitPreparationFailed(Id, transactionId, "账户余额不足"));
                }
                else
                {
                    RaiseEvent(new DebitPrepared(Id, transactionId, amount));
                }
            }
        }
        /// <summary>预转入
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="amount"></param>
        public void PrepareCredit(Guid transactionId, double amount)
        {
            if (_creditPreparations.Any(x => x.TransactionId == transactionId))
            {
                RaiseEvent(new CreditPreparationExist(Id, transactionId));
            }
            else
            {
                RaiseEvent(new CreditPrepared(Id, transactionId, amount));
            }
        }
        /// <summary>完成转出
        /// </summary>
        /// <param name="transactionId"></param>
        public void CompleteDebit(Guid transactionId)
        {
            var preparation = _debitPreparations.SingleOrDefault(x => x.TransactionId == transactionId);
            if (preparation == null)
            {
                RaiseEvent(new DebitPreparationNotExist(Id, transactionId));
            }
            else
            {
                RaiseEvent(new DebitCompleted(Id, transactionId, preparation.Amount));
            }
        }
        /// <summary>完成转入
        /// </summary>
        /// <param name="transactionId"></param>
        public void CompleteCredit(Guid transactionId)
        {
            var preparation = _creditPreparations.SingleOrDefault(x => x.TransactionId == transactionId);
            if (preparation == null)
            {
                RaiseEvent(new CreditPreparationNotExist(Id, transactionId));
            }
            else
            {
                RaiseEvent(new CreditCompleted(Id, transactionId, preparation.Amount));
            }
        }

        #endregion

        #region Private Methods

        private void Handle(AccountOpened evnt)
        {
            Id = evnt.SourceId;
            Owner = evnt.Owner;
        }
        private void Handle(Deposited evnt)
        {
            Balance += evnt.Amount;
        }
        private void Handle(DebitPrepared evnt)
        {
            Balance -= evnt.Amount;
            _debitPreparations.Add(new DebitPreparation(evnt.TransactionId, evnt.Amount));
        }
        private void Handle(CreditPrepared evnt)
        {
            _creditPreparations.Add(new CreditPreparation(evnt.TransactionId, evnt.Amount));
        }
        private void Handle(DebitCompleted evnt)
        {
            var transaction = _debitPreparations.Single(x => x.TransactionId == evnt.TransactionId);
            _debitPreparations.Remove(transaction);
        }
        private void Handle(CreditCompleted evnt)
        {
            Balance += evnt.Amount;
            var transaction = _creditPreparations.Single(x => x.TransactionId == evnt.TransactionId);
            _creditPreparations.Remove(transaction);
        }

        #endregion
    }
}
