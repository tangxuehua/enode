namespace ENode.Distribute.EventStore.Protocols
{
    public enum RequestCode
    {
        DetectAlive = 10,
        StoreEvent = 11,
        GetEventStream = 12,
        QueryAggregateEventStreams = 13,
    }
}
