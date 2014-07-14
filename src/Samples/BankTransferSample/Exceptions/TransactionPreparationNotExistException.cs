using System;
using BankTransferSample.Domain;

namespace BankTransferSample.Exceptions
{
    public class TransactionPreparationNotExistException : Exception
    {
        public TransactionPreparationNotExistException(string accountId, string transactionId)
            : base(string.Format("TransactionPreparation[transactionId={0}] not exist in account[id={1}].", transactionId, accountId))
        {
        }
    }
}
