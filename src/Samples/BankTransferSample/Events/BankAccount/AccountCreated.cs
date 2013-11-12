using System;
using ENode.Eventing;

namespace BankTransferSample.Events
{
    /// <summary>已开户
    /// </summary>
    [Serializable]
    public class AccountCreated : DomainEvent<string>, ISourcingEvent
    {
        /// <summary>账号拥有者
        /// </summary>
        public string Owner { get; private set; }
        /// <summary>开户时间
        /// </summary>
        public DateTime CreatedTime { get; private set; }

        public AccountCreated(string accountId, string owner, DateTime createdTime) : base(accountId)
        {
            Owner = owner;
            CreatedTime = createdTime;
        }
    }
}
