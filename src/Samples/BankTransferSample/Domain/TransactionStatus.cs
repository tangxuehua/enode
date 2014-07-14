namespace BankTransferSample.Domain
{
    /// <summary>交易状态
    /// </summary>
    public enum TransactionStatus
    {
        Started,
        PreparationCompleted,
        Completed,
        CancelStarted,
        Canceled
    }
}
