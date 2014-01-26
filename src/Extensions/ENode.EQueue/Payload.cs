namespace ENode.EQueue
{
    public class Payload
    {
        public byte[] Data { get; private set; }
        public int TypeCode { get; private set; }

        public Payload(byte[] data, int typeCode)
        {
            Data = data;
            TypeCode = typeCode;
        }
    }
}
