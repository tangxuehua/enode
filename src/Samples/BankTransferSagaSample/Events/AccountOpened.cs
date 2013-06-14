using System;
using ENode.Eventing;

namespace BankTransferSagaSample.Events
{
    [Serializable]
    public class AccountOpened : Event
    {
        public Guid AccountId { get; private set; }
        public string AccountNumber { get; private set; }
        public string Customer { get; private set; }

        public AccountOpened(Guid accountId, string accountNumber, string customer)
        {
            AccountId = accountId;
            AccountNumber = accountNumber;
            Customer = customer;
        }
    }

}
