namespace ENode.Eventing
{
    public enum EventAppendResult
    {
        Success = 1,
        Failed = 2,
        DuplicateEvent = 3,
        DuplicateCommand = 4
    }
}
