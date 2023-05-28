using System;

namespace Core
{
    public enum PacketID : ushort
    {
        Search = 1,
        Ready,
        Start,
        Process,
        End
    }

    public enum EndID : ushort
    {
        Win = 1,
        Lose
    }

    public class SearchPacket : Packet
    {
        public string ip;
        public ushort port;

        public SearchPacket() : base(PacketID.Search) { }

        public ArraySegment<byte> Serialize(byte[] sendBuffer)
        {
            ArraySegment<byte> segment = new ArraySegment<byte>(sendBuffer);
            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            ushort cursor = 0;
            bool isSuccess = true;

            isSuccess &= Serializer.SerializeUshort(span, (ushort)packetID, ref cursor);
            isSuccess &= Serializer.SerializeString(segment, span, ip, ref cursor);
            isSuccess &= Serializer.SerializeUshort(span, port, ref cursor);

            return (isSuccess ? segment.Slice(segment.Offset, cursor) : null);
        }

        public override ArraySegment<byte> Serialize(SendBuffer sendBuffer)
        {
            ArraySegment<byte> segment = sendBuffer.Open(128);
            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            ushort cursor = 0;
            bool isSuccess = true;

            isSuccess &= Serializer.SerializeUshort(span, (ushort)packetID, ref cursor);
            isSuccess &= Serializer.SerializeString(segment, span, ip, ref cursor);
            isSuccess &= Serializer.SerializeUshort(span, port, ref cursor);

            return (isSuccess ? sendBuffer.Close(cursor) : null);
        }

        public override void Deserialize(ArraySegment<byte> segment)
        {
            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            ushort cursor = sizeof(ushort);

            ushort ipLen = Deserializer.DeserializeUshort(span, ref cursor);
            ip = Deserializer.DeserializeString(span, ipLen, ref cursor);
            port = Deserializer.DeserializeUshort(span, ref cursor);
        }
    }

    public class ReadyPacket : Packet
    {
        public ReadyPacket() : base(PacketID.Ready) { }

        public override ArraySegment<byte> Serialize(SendBuffer sendBuffer)
        {
            ArraySegment<byte> segment = sendBuffer.Open(32);
            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            ushort cursor = sizeof(ushort);
            bool isSuccess = true;

            isSuccess &= Serializer.SerializeUshort(span, (ushort)packetID, ref cursor);
            isSuccess &= Serializer.SerializeSize(span, cursor);

            return (isSuccess ? sendBuffer.Close(cursor) : null);
        }

        public override void Deserialize(ArraySegment<byte> segment) { }
    }

    public class StartPacket : Packet
    {
        public StartPacket() : base(PacketID.Start) { }

        public override ArraySegment<byte> Serialize(SendBuffer sendBuffer)
        {
            ArraySegment<byte> segment = sendBuffer.Open(32);
            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            ushort cursor = sizeof(ushort);
            bool isSuccess = true;

            isSuccess &= Serializer.SerializeUshort(span, (ushort)packetID, ref cursor);
            isSuccess &= Serializer.SerializeSize(span, cursor);

            return (isSuccess ? sendBuffer.Close(cursor) : null);
        }

        public override void Deserialize(ArraySegment<byte> segment) { }
    }

    public class ProcessPacket : Packet
    {
        public ushort userID;
        public string data;

        public ProcessPacket() : base(PacketID.Process) { }

        public override ArraySegment<byte> Serialize(SendBuffer sendBuffer)
        {
            ArraySegment<byte> segment = sendBuffer.Open(128);
            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            ushort cursor = sizeof(ushort);
            bool isSuccess = true;

            isSuccess &= Serializer.SerializeUshort(span, (ushort)packetID, ref cursor);
            isSuccess &= Serializer.SerializeUshort(span, userID, ref cursor);
            isSuccess &= Serializer.SerializeString(segment, span, data, ref cursor);
            isSuccess &= Serializer.SerializeSize(span, cursor);

            return (isSuccess ? sendBuffer.Close(cursor) : null);
        }

        public override void Deserialize(ArraySegment<byte> segment)
        {
            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            ushort cursor = sizeof(ushort) * 2;

            userID = Deserializer.DeserializeUshort(span, ref cursor);
            ushort dataLen = Deserializer.DeserializeUshort(span, ref cursor);
            data = Deserializer.DeserializeString(span, dataLen, ref cursor);
        }
    }

    public class EndPacket : Packet
    {
        public ushort userID;
        public EndID endID;

        public EndPacket() : base(PacketID.End) { }


        public override ArraySegment<byte> Serialize(SendBuffer sendBuffer)
        {
            ArraySegment<byte> segment = sendBuffer.Open(64);
            Span<byte> span = new Span<byte>(segment.Array, segment.Offset, segment.Count);
            ushort cursor = sizeof(ushort);
            bool isSuccess = true;

            isSuccess &= Serializer.SerializeUshort(span, (ushort)packetID, ref cursor);
            isSuccess &= Serializer.SerializeUshort(span, userID, ref cursor);
            isSuccess &= Serializer.SerializeUshort(segment, (ushort)endID, ref cursor);
            isSuccess &= Serializer.SerializeSize(span, cursor);

            return (isSuccess ? sendBuffer.Close(cursor) : null);
        }

        public override void Deserialize(ArraySegment<byte> segment)
        {
            ReadOnlySpan<byte> span = new ReadOnlySpan<byte>(segment.Array, segment.Offset, segment.Count);
            ushort cursor = sizeof(ushort) * 2;

            userID = Deserializer.DeserializeUshort(span, ref cursor);
            endID = (EndID)Deserializer.DeserializeUshort(span, ref cursor);
        }
    }

    public abstract class Packet
    {
        public PacketID packetID;

        public Packet(PacketID packetID)
        {
            this.packetID = packetID;
        }

        public abstract ArraySegment<byte> Serialize(SendBuffer sendBuffer);
        public abstract void Deserialize(ArraySegment<byte> segment);
    }
}