namespace BankTransferSample.Domain
{
    /// <summary>转账流程状态
    /// </summary>
    public enum TransferProcessState
    {
        NotStarted,
        Started,
        TransferOutRequested,
        TransferInRequested,
        RollbackTransferOutRequested,
        Completed
    }
}
