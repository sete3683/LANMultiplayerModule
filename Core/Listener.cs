using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Core
{
    public class Listener
    {
        Socket _searchSocket;
        Socket _listenSocket;

        byte[] _searchBuffer = new byte[64];
        ArraySegment<byte> _searchSegment;

        Thread _searchThread;
        bool _isSearching;

        Func<Session> _sessionFactory;

        public void Init(int searchPort, int listenPort, Func<Session> mySession)
        {
            //Init Search Socket
            _searchSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            _searchSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            //Init Listen Socket
            IPEndPoint ep = new IPEndPoint(Dns.GetHostAddresses(Dns.GetHostName(), AddressFamily.InterNetwork)[0], listenPort);
            _listenSocket = new Socket(ep.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.LingerState = new LingerOption(true, 0);
            _listenSocket.Bind(ep);
            _listenSocket.Listen(4);

            //Init Search Buffer
            SearchPacket searchPacket = new SearchPacket() { ip = ep.Address.ToString(), port = (ushort)ep.Port };
            _searchSegment = searchPacket.Serialize(_searchBuffer);

            //Init Accept Async Event
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptComplete);

            //Init Search Thread
            _searchThread = new Thread(() => Search(searchPort));
            _searchThread.Start();

            //Init Session Factory
            _sessionFactory += mySession;

            Accept(args);
        }
        
        void Search(int searchPort)
        {
            _isSearching = true;

            IPEndPoint ep = new IPEndPoint(IPAddress.Broadcast, searchPort);

            while (_isSearching)
            {
                _searchSocket.SendTo(_searchSegment, ep);
                Thread.Sleep(2000);
            }
        }

        public void StopSearch()
        {
            if (_isSearching)
            {
                _isSearching = false;
                _searchThread.Join();
            }
        }

        void Accept(SocketAsyncEventArgs args)
        {
            try
            {
                bool pending = _listenSocket.AcceptAsync(args);

                if (!pending)
                    OnAcceptComplete(null, args);
            }
            catch (Exception e)
            {
                //Need log
            }
        }

        void OnAcceptComplete(object sender, SocketAsyncEventArgs args)
        {
            Session session = _sessionFactory.Invoke();
            session.Init(args.AcceptSocket);
            session.MyConnectMethod(args.AcceptSocket.RemoteEndPoint);

            args.AcceptSocket = null;
            Accept(args);
        }
    }
}