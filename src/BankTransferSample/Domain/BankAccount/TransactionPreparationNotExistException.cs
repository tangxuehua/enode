using System;

namespace BankTransferSample.Domain
{
    public class TransactionPreparationNotExistException : Exception
    {
        public TransactionPreparationNotExistException(string accountId, string transactionId)
            : base(string.Format("TransactionPreparation[transactionId={0}] not exist in account[id={1}].", transactionId, accountId))
        {
        }
    }
}
