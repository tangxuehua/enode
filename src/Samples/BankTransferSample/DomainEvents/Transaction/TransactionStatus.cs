namespace BankTransferSample.DomainEvents.Transaction
{
    /// <summary>转账交易状态
    /// </summary>
    public enum TransactionStatus
    {
        Started,
        Committed,
        Completed
    }
}
