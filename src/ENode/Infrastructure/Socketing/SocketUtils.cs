using System;
using System.Text;

namespace ENode.Infrastructure.Socketing
{
    public class SocketUtils
    {
        public static int ParseMessageLength(byte[] buffer)
        {
            var data = new byte[4];
            for (var i = 0; i < 4; i++)
            {
                data[i] = buffer[i];
            }
            return BitConverter.ToInt32(data, 0);
        }
        public static byte[] BuildMessage(string content, Encoding encoding)
        {
            var data = encoding.GetBytes(content);
            var header = BitConverter.GetBytes(data.Length);
            var message = new byte[header.Length + data.Length];
            header.CopyTo(message, 0);
            data.CopyTo(message, header.Length);
            return message;
        }
    }
}
