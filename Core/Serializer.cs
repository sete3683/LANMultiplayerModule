using System;
using System.Text;

namespace Core
{
    public static class Serializer
    {
        public static bool SerializeUshort(Span<byte> span, ushort data, ref ushort cursor)
        {
            bool isSuccess = BitConverter.TryWriteBytes(span.Slice(cursor, span.Length - cursor), data);
            cursor += sizeof(ushort);

            return isSuccess;
        }

        public static bool SerializeString(ArraySegment<byte> segment, Span<byte> span, string data, ref ushort cursor)
        {
            ushort dataLen = (ushort)Encoding.UTF8.GetBytes(data, 0, data.Length, segment.Array, segment.Offset + cursor + sizeof(ushort));
            bool isSuccess = SerializeUshort(span, dataLen, ref cursor);
            cursor += dataLen;

            return isSuccess;
        }

        public static bool SerializeSize(Span<byte> span, ushort size)
        {
            return BitConverter.TryWriteBytes(span, size);
        }
    }

    public static class Deserializer
    {
        public static ushort DeserializeUshort(ReadOnlySpan<byte> span, ref ushort cursor)
        {
            ushort data = BitConverter.ToUInt16(span.Slice(cursor, span.Length - cursor));
            cursor += sizeof(ushort);

            return data;
        }
        
        public static ushort DeserializeSize(ArraySegment<byte> segment)
        {
            return BitConverter.ToUInt16(segment.Array, segment.Offset);
        }

        public static ushort DeserializeID(ArraySegment<byte> segment)
        {
            return BitConverter.ToUInt16(segment.Array, segment.Offset + sizeof(ushort));
        }

        public static string DeserializeString(ReadOnlySpan<byte> span, ushort dataLen, ref ushort cursor)
        {
            string data = Encoding.UTF8.GetString(span.Slice(cursor, dataLen));
            cursor += dataLen;

            return data;
        }
    }
}