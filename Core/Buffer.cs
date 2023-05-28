using System;

namespace Core
{
    public class RecvBuffer : Buffer
    {
        int _readPos = 0;

        public int DataSize
        {
            get
            {
                return _writePos - _readPos;
            }
        }

        public ArraySegment<byte> ReadSegment
        {
            get
            {
                return _buffer.Slice(_buffer.Offset + _readPos, DataSize);
            }
        }

        public ArraySegment<byte> WriteSegment
        {
            get
            {
                return _buffer.Slice(_buffer.Offset + _writePos, FreeSize);
            }
        }

        public RecvBuffer(int size) : base(size) { }

        public bool OnReadComplete(int count)
        {
            if (count > DataSize)
            {
                return false;
            }
            else
            {
                _readPos += count;
                return true;
            }
        }

        public bool OnWriteComplete(int count)
        {
            if (count > FreeSize)
            {
                return false;
            }
            else
            {
                _writePos += count;
                return true;
            }
        }

        public override void Clear()
        {
            if (DataSize == 0)
            {
                _readPos = _writePos = 0;
            }
            else
            {
                Array.Copy(_buffer.Array, _buffer.Offset + _readPos, _buffer.Array, _buffer.Offset, DataSize);

                int temp = DataSize;
                _readPos = 0;
                _writePos = temp;
            }
        }
    }

    public class SendBuffer : Buffer
    {
        public SendBuffer(int size) : base(size) { }

        public ArraySegment<byte> Open(int count)
        {
            if (count > FreeSize)
                Clear();
            
            return _buffer.Slice(_buffer.Offset + _writePos, count);
        }

        public ArraySegment<byte> Close(int count)
        {
            ArraySegment<byte> segment = _buffer.Slice(_buffer.Offset + _writePos, count);
            _writePos += count;

            return segment;
        }

        public override void Clear()
        {
            _buffer = new ArraySegment<byte>(new byte[_buffer.Count], 0, _buffer.Count);
            _writePos = 0;
        }
    }

    public abstract class Buffer
    {
        protected ArraySegment<byte> _buffer;
        protected int _writePos = 0;

        public int FreeSize 
        { 
            get 
            { 
                return _buffer.Count - _writePos; 
            } 
        }

        public Buffer(int size)
        {
            _buffer = new ArraySegment<byte>(new byte[size], 0, size);
        }

        public abstract void Clear();
    }
}