namespace ENode.Eventing
{
    public interface IEventStreamConvertService
    {
        EventByteStream ConvertTo(EventStream source);
        EventStream ConvertFrom(EventByteStream source);
    }
}
