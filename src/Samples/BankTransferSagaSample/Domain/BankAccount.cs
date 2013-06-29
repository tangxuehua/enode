using System;
using BankTransferSagaSample.Events;
using ENode.Domain;
using ENode.Eventing;

namespace BankTransferSagaSample.Domain
{
    /// <summary>银行账号聚合根
    /// </summary>
    [Serializable]
    public class BankAccount : AggregateRoot<Guid>,
        IEventHandler<AccountOpened>,         //银行账户已开
        IEventHandler<Deposited>,             //钱已存入
        IEventHandler<TransferedOut>,         //钱已转出
        IEventHandler<TransferedIn>,          //钱已转入
        IEventHandler<TransferOutRolledback>  //转出已回滚
    {
        /// <summary>账号（卡号）
        /// </summary>
        public string AccountNumber { get; private set; }
        /// <summary>拥有者
        /// </summary>
        public string Owner { get; private set; }
        /// <summary>当前余额
        /// </summary>
        public double Balance { get; private set; }

        public BankAccount() : base() { }
        public BankAccount(Guid accountId, string accountNumber, string owner) : base(accountId)
        {
            RaiseEvent(new AccountOpened(Id, accountNumber, owner));
        }

        /// <summary>存款
        /// </summary>
        /// <param name="amount"></param>
        public void Deposit(double amount)
        {
            RaiseEvent(new Deposited(Id, amount, string.Format("向账户{0}存入金额{1}", AccountNumber, amount)));
        }
        /// <summary>转出
        /// </summary>
        /// <param name="targetAccount"></param>
        /// <param name="processId"></param>
        /// <param name="transferInfo"></param>
        public void TransferOut(BankAccount targetAccount, Guid processId, TransferInfo transferInfo)
        {
            //这里判断当前余额是否足够
            if (Balance < transferInfo.Amount)
            {
                throw new Exception(string.Format("账户{0}余额不足，不能转账！", AccountNumber));
            }
            RaiseEvent(new TransferedOut(processId, transferInfo, string.Format("{0}向账户{1}转出金额{2}", AccountNumber, targetAccount.AccountNumber, transferInfo.Amount)));
        }
        /// <summary>转入
        /// </summary>
        /// <param name="sourceAccount"></param>
        /// <param name="processId"></param>
        /// <param name="transferInfo"></param>
        public void TransferIn(BankAccount sourceAccount, Guid processId, TransferInfo transferInfo)
        {
            RaiseEvent(new TransferedIn(processId, transferInfo, string.Format("{0}从账户{1}转入金额{2}", AccountNumber, sourceAccount.AccountNumber, transferInfo.Amount)));
        }
        /// <summary>回滚转出
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="transferInfo"></param>
        public void RollbackTransferOut(Guid processId, TransferInfo transferInfo)
        {
            RaiseEvent(new TransferOutRolledback(processId, transferInfo, string.Format("账户{0}回滚转出金额{1}", AccountNumber, transferInfo.Amount)));
        }

        void IEventHandler<AccountOpened>.Handle(AccountOpened evnt)
        {
            AccountNumber = evnt.AccountNumber;
            Owner = evnt.Owner;
        }
        void IEventHandler<Deposited>.Handle(Deposited evnt)
        {
            Balance += evnt.Amount;
        }
        void IEventHandler<TransferedOut>.Handle(TransferedOut evnt)
        {
            Balance -= evnt.TransferInfo.Amount;
        }
        void IEventHandler<TransferedIn>.Handle(TransferedIn evnt)
        {
            Balance += evnt.TransferInfo.Amount;
        }
        void IEventHandler<TransferOutRolledback>.Handle(TransferOutRolledback evnt)
        {
            Balance += evnt.TransferInfo.Amount;
        }
    }
}
