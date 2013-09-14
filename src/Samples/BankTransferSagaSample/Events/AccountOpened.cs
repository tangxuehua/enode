using System;
using ENode.Eventing;

namespace BankTransferSagaSample.Events
{
    /// <summary>银行账户已开
    /// </summary>
    [Serializable]
    public class AccountOpened : Event
    {
        public string AccountNumber { get; private set; }
        public string Owner { get; private set; }

        public AccountOpened(string accountNumber, string owner)
        {
            AccountNumber = accountNumber;
            Owner = owner;
        }
    }
}
