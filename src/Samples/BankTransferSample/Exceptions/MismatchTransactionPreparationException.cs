using System;
using BankTransferSample.Domain;

namespace BankTransferSample.Exceptions
{
    public class MismatchTransactionPreparationException : Exception
    {
        public MismatchTransactionPreparationException(TransactionType transactionType, PreparationType preparationType)
            : base(string.Format("Mismatch transaction type [{0}] and preparation type [{1}].", transactionType, preparationType))
        {

        }
    }
}
