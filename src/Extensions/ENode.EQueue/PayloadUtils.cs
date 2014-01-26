using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ENode.Commanding;

namespace ENode.EQueue
{
    public class PayloadUtils
    {
        public static byte[] EncodePayload(Payload payload)
        {
            var typeCodeBytes = BitConverter.GetBytes(payload.TypeCode);
            var data = new byte[typeCodeBytes.Length + payload.Data.Length];

            typeCodeBytes.CopyTo(data, 0);
            payload.Data.CopyTo(data, typeCodeBytes.Length);

            return data;
        }
        public static Payload DecodePayload(byte[] buffer)
        {
            var typeCodeBytes = new byte[4];
            var dataBytes = new byte[buffer.Length - 4];
            Array.Copy(buffer, 0, typeCodeBytes, 0, 4);
            Array.Copy(buffer, 4, dataBytes, 0, dataBytes.Length);

            var typeCode = BitConverter.ToInt32(typeCodeBytes, 0);

            return new Payload(dataBytes, typeCode);
        }
    }
}
