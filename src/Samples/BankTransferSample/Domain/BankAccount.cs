using System;
using BankTransferSample.Events;
using ENode.Domain;
using ENode.Eventing;

namespace BankTransferSample.Domain
{
    /// <summary>银行账号聚合根
    /// </summary>
    [Serializable]
    public class BankAccount : AggregateRoot<string>,
        IEventHandler<AccountOpened>,         //银行账户已开
        IEventHandler<Deposited>,             //钱已存入
        IEventHandler<TransferedOut>,         //钱已转出
        IEventHandler<TransferedIn>,          //钱已转入
        IEventHandler<TransferOutRolledback>  //转出已回滚
    {
        /// <summary>拥有者
        /// </summary>
        public string Owner { get; private set; }
        /// <summary>当前余额
        /// </summary>
        public double Balance { get; private set; }

        public BankAccount() { }
        public BankAccount(string accountNumber, string owner) : base(accountNumber)
        {
            RaiseEvent(new AccountOpened(Id, owner));
        }

        /// <summary>存款
        /// </summary>
        /// <param name="amount"></param>
        public void Deposit(double amount)
        {
            RaiseEvent(new Deposited(Id, amount, string.Format("向账户{0}存入金额{1}", Id, amount)));
        }
        /// <summary>转出
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="transferInfo"></param>
        public void TransferOut(Guid processId, TransferInfo transferInfo)
        {
            if (Id != transferInfo.SourceAccountNumber)
            {
                throw new Exception(string.Format("源账户{0}与当前账户{1}不匹配，不能转出！", transferInfo.SourceAccountNumber, Id));
            }
            if (Balance < transferInfo.Amount)
            {
                throw new Exception(string.Format("账户{0}余额不足，不能转出！", Id));
            }
            RaiseEvent(new TransferedOut(
                processId,
                transferInfo,
                string.Format("{0}向账户{1}转出金额{2}", transferInfo.SourceAccountNumber, transferInfo.TargetAccountNumber, transferInfo.Amount)));
        }
        /// <summary>转入
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="transferInfo"></param>
        public void TransferIn(Guid processId, TransferInfo transferInfo)
        {
            if (Id != transferInfo.TargetAccountNumber)
            {
                throw new Exception(string.Format("目标账户{0}与当前账户{1}不匹配，不能转入！", transferInfo.TargetAccountNumber, Id));
            }
            RaiseEvent(
                new TransferedIn(
                    processId,
                    transferInfo,
                    string.Format("{0}从账户{1}转入金额{2}", transferInfo.TargetAccountNumber, transferInfo.SourceAccountNumber, transferInfo.Amount)));
        }
        /// <summary>回滚转出
        /// </summary>
        /// <param name="processId"></param>
        /// <param name="transferInfo"></param>
        public void RollbackTransferOut(Guid processId, TransferInfo transferInfo)
        {
            if (Id != transferInfo.SourceAccountNumber)
            {
                throw new Exception(string.Format("源账户{0}与当前账户{1}不匹配，不能回滚转出！", transferInfo.SourceAccountNumber, Id));
            }
            RaiseEvent(new TransferOutRolledback(processId, transferInfo, string.Format("账户{0}回滚转出金额{1}", Id, transferInfo.Amount)));
        }

        void IEventHandler<AccountOpened>.Handle(AccountOpened evnt)
        {
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
