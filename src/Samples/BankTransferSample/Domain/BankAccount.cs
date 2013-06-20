using System;
using BankTransferSample.Events;
using ENode.Domain;
using ENode.Eventing;

namespace BankTransferSample.Domain
{
    [Serializable]
    public class BankAccount : AggregateRoot<Guid>,
        IEventHandler<AccountOpened>,
        IEventHandler<Deposited>,
        IEventHandler<TransferedOut>,
        IEventHandler<TransferedIn>
    {
        public string AccountNumber { get; private set; }
        public string Owner { get; private set; }
        public double Balance { get; private set; }

        public BankAccount() : base() { }
        public BankAccount(Guid accountId, string accountNumber, string owner) : base(accountId)
        {
            RaiseEvent(new AccountOpened(Id, accountNumber, owner));
        }

        public void Deposit(double amount)
        {
            RaiseEvent(new Deposited(Id, amount, string.Format("向账户{0}存入金额{1}", AccountNumber, amount)));
        }
        public void TransferOut(BankAccount targetAccount, double amount)
        {
            RaiseEvent(new TransferedOut(Id, targetAccount.Id, amount, string.Format("{0}向账户{1}转出金额{2}", AccountNumber, targetAccount.AccountNumber, amount)));
        }
        public void TransferIn(BankAccount sourceAccount, double amount)
        {
            RaiseEvent(new TransferedIn(sourceAccount.Id, Id, amount, string.Format("{0}从账户{1}转入金额{2}", AccountNumber, sourceAccount.AccountNumber, amount)));
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
            Balance -= evnt.Amount;
        }
        void IEventHandler<TransferedIn>.Handle(TransferedIn evnt)
        {
            Balance += evnt.Amount;
        }
    }
}
