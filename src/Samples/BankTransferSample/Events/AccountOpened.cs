using System;
using ENode.Eventing;

namespace BankTransferSample.Events {
    [Serializable]
    public class AccountOpened : Event {
        public Guid AccountId { get; private set; }
        public string AccountNumber { get; private set; }
        public string Owner { get; private set; }

        public AccountOpened(Guid accountId, string accountNumber, string owner) {
            AccountId = accountId;
            AccountNumber = accountNumber;
            Owner = owner;
        }
    }

}
