using System.Collections.Generic;
using ENode.Exceptions;

namespace BankTransferSample.Exceptions
{
    public class InvalidAccountException : PublishableException
    {
        public string AccountId { get; set; }
        public string TransactionId { get; set; }
        public string Description { get; set; }

        public InvalidAccountException(string accountId, string transactionId, string description)
        {
            AccountId = accountId;
            TransactionId = transactionId;
            Description = description;
        }

        public override void SerializeTo(IDictionary<string, string> serializableInfo)
        {
            serializableInfo.Add("AccountId", AccountId);
            serializableInfo.Add("TransactionId", TransactionId);
            serializableInfo.Add("Description", Description);
        }
        public override void RestoreFrom(IDictionary<string, string> serializableInfo)
        {
            AccountId = serializableInfo["AccountId"];
            TransactionId = serializableInfo["TransactionId"];
            Description = serializableInfo["Description"];
        }
    }
}
