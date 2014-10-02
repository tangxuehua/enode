using System;
using System.Collections.Generic;
using System.Linq;
using BankTransferSample.DomainEvents;
using BankTransferSample.Exceptions;
using ENode.Domain;

namespace BankTransferSample.Domain
{
    /// <summary>银行账户聚合根，封装银行账户余额变动的数据一致性
    /// </summary>
    [Serializable]
    public class BankAccount : AggregateRoot<string>
    {
        #region Private Variables

        private IDictionary<string, TransactionPreparation> _transactionPreparations;

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
        public BankAccount(string accountId, string owner) : base(accountId)
        {
            ApplyEvent(new AccountCreatedEvent(accountId, owner));
        }

        #endregion

        #region Public Methods

        /// <summary>添加一笔预操作
        /// </summary>
        public void AddTransactionPreparation(string transactionId, TransactionType transactionType, PreparationType preparationType, double amount)
        {
            var availableBalance = GetAvailableBalance();
            if (preparationType == PreparationType.DebitPreparation && availableBalance < amount)
            {
                throw new InsufficientBalanceException(Id, transactionId, transactionType, amount, Balance, availableBalance);
            }

            ApplyEvent(new TransactionPreparationAddedEvent(new TransactionPreparation(Id, transactionId, transactionType, preparationType, amount)));
        }
        /// <summary>提交一笔预操作
        /// </summary>
        public void CommitTransactionPreparation(string transactionId)
        {
            var transactionPreparation = GetTransactionPreparation(transactionId);
            var currentBalance = Balance;
            if (transactionPreparation.PreparationType == PreparationType.DebitPreparation)
            {
                currentBalance -= transactionPreparation.Amount;
            }
            else if (transactionPreparation.PreparationType == PreparationType.CreditPreparation)
            {
                currentBalance += transactionPreparation.Amount;
            }
            ApplyEvent(new TransactionPreparationCommittedEvent(currentBalance, transactionPreparation));
        }
        /// <summary>取消一笔预操作
        /// </summary>
        public void CancelTransactionPreparation(string transactionId)
        {
            ApplyEvent(new TransactionPreparationCanceledEvent(GetTransactionPreparation(transactionId)));
        }

        #endregion

        #region Private Methods

        /// <summary>获取当前账户内的一笔预操作，如果预操作不存在，则抛出异常
        /// </summary>
        private TransactionPreparation GetTransactionPreparation(string transactionId)
        {
            if (_transactionPreparations == null || _transactionPreparations.Count == 0)
            {
                throw new TransactionPreparationNotExistException(Id, transactionId);
            }
            TransactionPreparation transactionPreparation;
            if (!_transactionPreparations.TryGetValue(transactionId, out transactionPreparation))
            {
                throw new TransactionPreparationNotExistException(Id, transactionId);
            }
            return transactionPreparation;
        }
        /// <summary>获取当前账户的可用余额，需要将已冻结的余额计算在内
        /// </summary>
        private double GetAvailableBalance()
        {
            if (_transactionPreparations == null || _transactionPreparations.Count == 0)
            {
                return Balance;
            }

            var totalDebitTransactionPreparationAmount = 0D;
            foreach (var debitTransactionPreparation in _transactionPreparations.Values.Where(x => x.PreparationType == PreparationType.DebitPreparation))
            {
                totalDebitTransactionPreparationAmount += debitTransactionPreparation.Amount;
            }

            return Balance - totalDebitTransactionPreparationAmount;
        }

        #endregion

        #region Handler Methods

        private void Handle(AccountCreatedEvent evnt)
        {
            _transactionPreparations = new Dictionary<string, TransactionPreparation>();
            Id = evnt.AggregateRootId;
            Owner = evnt.Owner;
        }
        private void Handle(TransactionPreparationAddedEvent evnt)
        {
            _transactionPreparations.Add(evnt.TransactionPreparation.TransactionId, evnt.TransactionPreparation);
        }
        private void Handle(TransactionPreparationCommittedEvent evnt)
        {
            _transactionPreparations.Remove(evnt.TransactionPreparation.TransactionId);
            Balance = evnt.CurrentBalance;
        }
        private void Handle(TransactionPreparationCanceledEvent evnt)
        {
            _transactionPreparations.Remove(evnt.TransactionPreparation.TransactionId);
        }

        #endregion
    }
}
