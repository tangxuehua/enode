using System;
using System.Collections.Generic;
using System.Linq;
using BankTransferSample.DomainEvents.BankAccount;
using BankTransferSample.DomainEvents.Transaction;
using ENode.EQueue;
using ENode.Eventing;

namespace BankTransferSample.EQueueIntegrations
{
    public class EventTypeCodeManager : IEventTypeCodeProvider
    {
        private IDictionary<int, Type> _typeCodeDict = new Dictionary<int, Type>();

        public EventTypeCodeManager()
        {
            _typeCodeDict.Add(100, typeof(AccountCreated));
            _typeCodeDict.Add(101, typeof(CreditAborted));
            _typeCodeDict.Add(102, typeof(CreditCommitted));
            _typeCodeDict.Add(103, typeof(CreditPreparationNotExist));
            _typeCodeDict.Add(104, typeof(CreditPrepared));
            _typeCodeDict.Add(105, typeof(DebitAborted));
            _typeCodeDict.Add(106, typeof(DebitCommitted));
            _typeCodeDict.Add(107, typeof(DebitInsufficientBalance));
            _typeCodeDict.Add(108, typeof(DebitPreparationNotExist));
            _typeCodeDict.Add(109, typeof(DebitPrepared));
            _typeCodeDict.Add(110, typeof(Deposited));
            _typeCodeDict.Add(111, typeof(DuplicatedCreditPreparation));
            _typeCodeDict.Add(112, typeof(DuplicatedDebitPreparation));
            _typeCodeDict.Add(113, typeof(InvalidTransactionOperation));
            _typeCodeDict.Add(114, typeof(WithdrawInsufficientBalance));
            _typeCodeDict.Add(115, typeof(Withdrawn));

            _typeCodeDict.Add(201, typeof(CreditConfirmed));
            _typeCodeDict.Add(202, typeof(CreditPreparationConfirmed));
            _typeCodeDict.Add(203, typeof(DebitConfirmed));
            _typeCodeDict.Add(204, typeof(DebitPreparationConfirmed));
            _typeCodeDict.Add(205, typeof(TransactionAborted));
            _typeCodeDict.Add(206, typeof(TransactionCommitted));
            _typeCodeDict.Add(207, typeof(TransactionCompleted));
            _typeCodeDict.Add(208, typeof(TransactionCreated));
            _typeCodeDict.Add(209, typeof(TransactionStarted));
        }

        public int GetTypeCode(IDomainEvent domainEvent)
        {
            return _typeCodeDict.Single(x => x.Value == domainEvent.GetType()).Key;
        }
        public Type GetType(int typeCode)
        {
            return _typeCodeDict[typeCode];
        }
    }
}
