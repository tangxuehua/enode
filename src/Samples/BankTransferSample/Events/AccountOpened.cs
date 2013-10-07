using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>银行账户已开
    /// </summary>
    [Serializable]
    public class AccountOpened : Event
    {
        public string Owner { get; private set; }

        public AccountOpened(string accountId, string owner) : base(accountId)
        {
            Owner = owner;
        }
    }
}
