using Core;
using System;
using System.Net;
using System.Threading;

namespace Client
{
    public class MyClientSession : Session
    {
        public override void MyConnectMethod(EndPoint ep)
        {
            Console.WriteLine("It Works!");
            Console.WriteLine($"Connected with {ep}!");

            SendBuffer sendBuffer = new SendBuffer(256);

            ReadyPacket readyPacket = new ReadyPacket();
            Send(readyPacket.Serialize(sendBuffer));

            Thread.Sleep(1000);

            StartPacket startPacket = new StartPacket();
            Send(startPacket.Serialize(sendBuffer));

            Thread.Sleep(1000);

            ProcessPacket processPacket = new ProcessPacket() { userID = 1, data = "Hello Server!" };
            Send(processPacket.Serialize(sendBuffer));

            Thread.Sleep(1000);

            EndPacket endPacket = new EndPacket() { userID = 1, endID = EndID.Win };
            Send(endPacket.Serialize(sendBuffer));

            Thread.Sleep(1000);

            Disconnect();
        }

        public override void MySendMethod(int count)
        {
            Console.WriteLine($"Sent {count} bytes!");
        }

        public override void MyRecvMethod(ArraySegment<byte> segment) { }
        public override void MyDisconnectMethod(EndPoint ep)
        {
            Console.WriteLine($"Disconnected with {ep}!");
        }
    }
}