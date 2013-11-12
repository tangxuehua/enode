namespace BankTransferSample.Domain
{
    /// <summary>转账交易状态
    /// </summary>
    public enum TransactionStatus
    {
        Started,
        DebitPreparationConfirmed,
        CreditPreparationConfirmed,
        Completed,
        Aborted
    }
}
