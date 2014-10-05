namespace BankTransferSample.Domain
{
    /// <summary>交易状态
    /// </summary>
    public enum TransactionStatus
    {
        Started = 1,
        AccountValidateCompleted,
        PreparationCompleted,
        Completed,
        Canceled
    }
}
