using System;

namespace BankTransferSample.Domain
{
    public class MismatchTransactionPreparationException : Exception
    {
        public MismatchTransactionPreparationException(TransactionType transactionType, PreparationType preparationType)
            : base(string.Format("Mismatch transaction type [{0}] and preparation type [{1}].", transactionType, preparationType))
        {

        }
    }
}
