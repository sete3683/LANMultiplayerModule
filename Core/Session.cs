using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Core
{
    public abstract class Session
    {
        Socket _socket;

        RecvBuffer _recvBuffer = new RecvBuffer(1024);
        protected SendBuffer _sendBuffer = new SendBuffer(1024);

        Queue<ArraySegment<byte>> _sendQueue = new Queue<ArraySegment<byte>>();
        List<ArraySegment<byte>> _sendList = new List<ArraySegment<byte>>();

        SocketAsyncEventArgs _sendArgs = new SocketAsyncEventArgs();
        SocketAsyncEventArgs _recvArgs = new SocketAsyncEventArgs();

        object _sendLock = new object();
        int _disconnected = 0;

        public abstract void MyConnectMethod(EndPoint ep);
        public abstract void MySendMethod(int count);
        public abstract void MyRecvMethod(ArraySegment<byte> segment);
        public abstract void MyDisconnectMethod(EndPoint ep);

        public void Init(Socket socket)
        {
            _socket = socket;
            _sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnSendComplete);
            _recvArgs.Completed += new EventHandler<SocketAsyncEventArgs>(OnReceiveComplete);

            Receive();
        }

        public void Disconnect()
        {
            try
            {
                if (Interlocked.CompareExchange(ref _disconnected, 1, 0) == 0)
                {
                    MyDisconnectMethod(_socket.RemoteEndPoint);
                    _socket.Shutdown(SocketShutdown.Both);
                    _socket.Close();

                    _sendQueue.Clear();
                    _sendList.Clear();
                }
                else
                {
                    //Need log
                }
            }
            catch (Exception e)
            {
                //Need log
            }
        }

        public void Send(ArraySegment<byte> segment)
        {
            lock (_sendLock)
            {
                _sendQueue.Enqueue(segment);

                if (_sendList.Count == 0)
                    Send();
            }
        }

        void Send()
        {
            while (_sendQueue.Count > 0)
            {
                ArraySegment<byte> segment = _sendQueue.Dequeue();
                _sendList.Add(segment);
            }

            _sendArgs.BufferList = _sendList;

            try
            {
                bool pending = _socket.SendAsync(_sendArgs);

                if (!pending)
                    OnSendComplete(null, _sendArgs);
            }
            catch (Exception e)
            {
                //Need log
            }
        }

        void OnSendComplete(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                try
                {
                    _sendArgs.BufferList = null;
                    _sendList.Clear();

                    MySendMethod(args.BytesTransferred);

                    if (_sendQueue.Count > 0)
                        Send();
                }
                catch (Exception e)
                {
                    //Need log
                }
            }
            else
            {
                Disconnect();
            }
        }

        void Receive()
        {
            _recvBuffer.Clear();

            ArraySegment<byte> segment = _recvBuffer.WriteSegment;
            _recvArgs.SetBuffer(segment.Array, segment.Offset, segment.Count);

            try
            {
                bool pending = _socket.ReceiveAsync(_recvArgs);

                if (pending == false)
                    OnReceiveComplete(null, _recvArgs);
            }
            catch (Exception e)
            {
                //Need log
            }
        }

        void OnReceiveComplete(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                try
                {
                    if (!_recvBuffer.OnWriteComplete(args.BytesTransferred))
                    {
                        Disconnect();
                        return;
                    }

                    int recvCount = CheckPacket(_recvBuffer.ReadSegment);

                    if (!_recvBuffer.OnReadComplete(recvCount))
                    {
                        Disconnect();
                        return;
                    }

                    Receive();
                }
                catch (Exception e)
                {
                    //Need log
                }
            }
            else
            {
                Disconnect();
            }
        }

        int CheckPacket(ArraySegment<byte> segment)
        {
            int recvCount = 0;

            while (true)
            {
                if (segment.Count < 2)
                    break;

                ushort packetSize = BitConverter.ToUInt16(segment.Array, segment.Offset);

                if (segment.Count < packetSize)
                    break;

                MyRecvMethod(segment.Slice(segment.Offset, packetSize));

                recvCount += packetSize;
                segment = segment.Slice(segment.Offset + packetSize, segment.Count - packetSize);
            }

            return recvCount;
        }
    }
}