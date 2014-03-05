namespace ENode.Eventing
{
    public interface IEventStreamConvertService
    {
        EventCommitRecord ConvertTo(EventStream source);
        EventStream ConvertFrom(EventCommitRecord source);
    }
}
