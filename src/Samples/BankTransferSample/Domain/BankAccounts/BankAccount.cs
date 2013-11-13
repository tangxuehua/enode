using System;
using System.Collections.Generic;
using System.Linq;
using BankTransferSample.DomainEvents.BankAccount;
using ENode.Domain;

namespace BankTransferSample.Domain.BankAccounts
{
    /// <summary>银行账号聚合根，封装银行账户余额变动的数据一致性
    /// </summary>
    [Serializable]
    public class BankAccount : AggregateRoot<string>
    {
        #region Private Variables

        private IList<DebitPreparation> _debitPreparations;
        private IList<CreditPreparation> _creditPreparations;
        private IList<Guid> _completedTransactions;

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
            RaiseEvent(new AccountCreated(accountId, owner, DateTime.Now));
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
            _completedTransactions = new List<Guid>();
        }

        #endregion

        #region Public Methods

        /// <summary>存款
        /// </summary>
        /// <param name="amount"></param>
        public void Deposit(double amount)
        {
            RaiseEvent(new Deposited(Id, amount, Balance + amount, DateTime.Now));
        }
        /// <summary>取款
        /// </summary>
        /// <param name="amount"></param>
        public void Withdraw(double amount)
        {
            var availableBalance = GetAvailableBalance();
            if (availableBalance < amount)
            {
                RaiseEvent(new WithdrawInsufficientBalance(Id, amount, Balance, availableBalance));
                return;
            }
            RaiseEvent(new Withdrawn(Id, amount, Balance - amount, DateTime.Now));
        }
        /// <summary>预转出
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="amount"></param>
        public void PrepareDebit(Guid transactionId, double amount)
        {
            if (_completedTransactions.Any(x => x== transactionId))
            {
                RaiseEvent(new InvalidTransactionOperation(Id, transactionId, TransactionOperationType.PrepareDebit));
                return;
            }
            if (_debitPreparations.Any(x => x.TransactionId == transactionId))
            {
                RaiseEvent(new DuplicatedDebitPreparation(Id, transactionId));
                return;
            }
            var availableBalance = GetAvailableBalance();
            if (availableBalance < amount)
            {
                RaiseEvent(new DebitInsufficientBalance(Id, transactionId, amount, Balance, availableBalance));
                return;
            }

            RaiseEvent(new DebitPrepared(Id, transactionId, amount));
        }
        /// <summary>预转入
        /// </summary>
        /// <param name="transactionId"></param>
        /// <param name="amount"></param>
        public void PrepareCredit(Guid transactionId, double amount)
        {
            if (_completedTransactions.Any(x => x == transactionId))
            {
                RaiseEvent(new InvalidTransactionOperation(Id, transactionId, TransactionOperationType.PrepareCredit));
                return;
            }
            if (_creditPreparations.Any(x => x.TransactionId == transactionId))
            {
                RaiseEvent(new DuplicatedCreditPreparation(Id, transactionId));
                return;
            }

            RaiseEvent(new CreditPrepared(Id, transactionId, amount));
        }
        /// <summary>执行转出
        /// </summary>
        /// <param name="transactionId"></param>
        public void CommitDebit(Guid transactionId)
        {
            if (_completedTransactions.Any(x => x == transactionId))
            {
                RaiseEvent(new InvalidTransactionOperation(Id, transactionId, TransactionOperationType.CompleteDebit));
                return;
            }
            var preparation = _debitPreparations.SingleOrDefault(x => x.TransactionId == transactionId);
            if (preparation == null)
            {
                RaiseEvent(new DebitPreparationNotExist(Id, transactionId));
                return;
            }

            RaiseEvent(new DebitCommitted(Id, transactionId, preparation.Amount, Balance - preparation.Amount, DateTime.Now));
        }
        /// <summary>执行转入
        /// </summary>
        /// <param name="transactionId"></param>
        public void CommitCredit(Guid transactionId)
        {
            if (_completedTransactions.Any(x => x == transactionId))
            {
                RaiseEvent(new InvalidTransactionOperation(Id, transactionId, TransactionOperationType.CompleteCredit));
                return;
            }

            var preparation = _creditPreparations.SingleOrDefault(x => x.TransactionId == transactionId);
            if (preparation == null)
            {
                RaiseEvent(new CreditPreparationNotExist(Id, transactionId));
                return;
            }

            RaiseEvent(new CreditCommitted(Id, transactionId, preparation.Amount, Balance + preparation.Amount, DateTime.Now));
        }

        #endregion

        #region Private Methods

        /// <summary>获取当前真正可用的余额，需要将已冻结的余额计算在内
        /// </summary>
        /// <returns></returns>
        private double GetAvailableBalance()
        {
            if (_debitPreparations.Count == 0)
            {
                return Balance;
            }

            var totalDebitPreparationAmount = 0D;
            foreach (var preparation in _debitPreparations)
            {
                totalDebitPreparationAmount += preparation.Amount;
            }

            return Balance - totalDebitPreparationAmount;
        }

        #endregion

        #region Handler Methods

        private void Handle(AccountCreated evnt)
        {
            Id = evnt.SourceId;
            Owner = evnt.Owner;
        }
        private void Handle(Deposited evnt)
        {
            Balance = evnt.CurrentBalance;
        }
        private void Handle(Withdrawn evnt)
        {
            Balance = evnt.CurrentBalance;
        }
        private void Handle(DebitPrepared evnt)
        {
            _debitPreparations.Add(new DebitPreparation(evnt.TransactionId, evnt.Amount));
        }
        private void Handle(CreditPrepared evnt)
        {
            _creditPreparations.Add(new CreditPreparation(evnt.TransactionId, evnt.Amount));
        }
        private void Handle(DebitCommitted evnt)
        {
            Balance = evnt.CurrentBalance;
            _debitPreparations.Remove(_debitPreparations.Single(x => x.TransactionId == evnt.TransactionId));
            _completedTransactions.Add(evnt.TransactionId);
        }
        private void Handle(CreditCommitted evnt)
        {
            Balance = evnt.CurrentBalance;
            _creditPreparations.Remove(_creditPreparations.Single(x => x.TransactionId == evnt.TransactionId));
            _completedTransactions.Add(evnt.TransactionId);
        }

        #endregion
    }
}
