using System;
using System.Net.Sockets;
using System.Net;

namespace Core
{
    public class Connector
    {
        Socket _searchSocket;
        Socket _connectSocket;

        byte[] _searchBuffer = new byte[64];

        Func<Session> _sessionFactory;

        public void Init(int searchPort, Func<Session> mySession)
        {
            //Init Search Socket
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, searchPort);
            _searchSocket = new Socket(ep.AddressFamily, SocketType.Dgram, ProtocolType.Udp);
            _searchSocket.Bind(ep);

            //Init Search Async Event
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnSearchComplete);

            //Init Session Factory
            _sessionFactory += mySession;

            Search(args);
        }

        void Search(SocketAsyncEventArgs args)
        {
            args.SetBuffer(_searchBuffer);

            try
            {
                bool pending = _searchSocket.ReceiveFromAsync(args);

                if (!pending)
                    OnSearchComplete(null, args);
            }
            catch (Exception e)
            {
                //Need log
            }
        }

        void OnSearchComplete(Object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                try
                {
                    if ((PacketID)Deserializer.DeserializeSize(_searchBuffer) == PacketID.Search)
                    {
                        SearchPacket searchPacket = new SearchPacket();
                        searchPacket.Deserialize(new ArraySegment<byte>(_searchBuffer, 0, args.BytesTransferred));

                        IPEndPoint ep = new IPEndPoint(IPAddress.Parse(searchPacket.ip), searchPacket.port);
                        Connect(ep);
                    }
                    else
                    {
                        Search(args);
                    }
                }
                catch (Exception e)
                {
                    //Need Log
                }
            }
            else
            {
                _searchSocket.Close();
            }
        }

        void Connect(IPEndPoint ep)
        {
            //Init Connect Socket
            _connectSocket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            //Init Connect Async Event
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.RemoteEndPoint = ep;
            args.Completed += OnConnectComplete;

            try
            {
                bool pending = _connectSocket.ConnectAsync(args);

                if (!pending)
                    OnConnectComplete(null, args);
            }
            catch (Exception e)
            {
                //Need log
            }
        }

        void OnConnectComplete(Object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Init(args.ConnectSocket);
                session.MyConnectMethod(args.RemoteEndPoint);
            }
            else
            {
                //Need Log
            }
        }
    }
}