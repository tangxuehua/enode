using System;
using System.Collections.Generic;
using System.Linq;
using BankTransferSample.Commands;
using ENode.Commanding;
using ENode.EQueue;

namespace BankTransferSample.EQueueIntegrations
{
    public class CommandTypeCodeManager : ICommandTypeCodeProvider
    {
        private IDictionary<int, Type> _typeCodeDict = new Dictionary<int, Type>();

        public CommandTypeCodeManager()
        {
            _typeCodeDict.Add(100, typeof(CreateAccount));
            _typeCodeDict.Add(101, typeof(Deposit));
            _typeCodeDict.Add(102, typeof(Withdraw));
            _typeCodeDict.Add(103, typeof(PrepareDebit));
            _typeCodeDict.Add(104, typeof(PrepareCredit));
            _typeCodeDict.Add(105, typeof(CommitDebit));
            _typeCodeDict.Add(106, typeof(CommitCredit));
            _typeCodeDict.Add(107, typeof(AbortDebit));
            _typeCodeDict.Add(108, typeof(AbortCredit));

            _typeCodeDict.Add(201, typeof(CreateTransaction));
            _typeCodeDict.Add(202, typeof(StartTransaction));
            _typeCodeDict.Add(203, typeof(ConfirmDebitPreparation));
            _typeCodeDict.Add(204, typeof(ConfirmCreditPreparation));
            _typeCodeDict.Add(205, typeof(ConfirmDebit));
            _typeCodeDict.Add(206, typeof(ConfirmCredit));
            _typeCodeDict.Add(207, typeof(AbortTransaction));
        }

        public int GetTypeCode(ICommand command)
        {
            return _typeCodeDict.Single(x => x.Value == command.GetType()).Key;
        }
        public Type GetType(int typeCode)
        {
            return _typeCodeDict[typeCode];
        }
    }
}
