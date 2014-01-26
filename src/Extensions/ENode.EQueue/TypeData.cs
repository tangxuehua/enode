namespace ENode.EQueue
{
    public class TypeData<TData>
    {
        public int TypeCode { get; private set; }
        public TData Data { get; private set; }

        public TypeData(int typeCode, TData data)
        {
            TypeCode = typeCode;
            Data = data;
        }
    }
}
