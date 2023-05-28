using Core;
using System;
using System.Collections.Generic;
using System.Net;

namespace Server
{
    public class MyServerSession : Session
    {
        public static List<MyServerSession> sessionList = new List<MyServerSession>();

        public override void MyConnectMethod(EndPoint ep)
        {
            Console.WriteLine("It Works!");
            Console.WriteLine($"Connected with {ep}!");

            sessionList.Add(this);
        }

        public override void MySendMethod(int count) { }
        public override void MyRecvMethod(ArraySegment<byte> segment)
        {
            ushort data = Deserializer.DeserializeID(segment);
            PacketID packetID = (PacketID)data;

            switch (packetID)
            {
                case PacketID.Ready:
                    Console.WriteLine("[From Client] I'm Ready!");
                    break;

                case PacketID.Start:
                    Console.WriteLine("[From Client] Let's go!");
                    break;

                case PacketID.Process:
                    Console.WriteLine("[From Client] I made this!");
                    ProcessPacket processPacket = new ProcessPacket();
                    processPacket.Deserialize(segment);
                    Console.WriteLine("[From Client] " + processPacket.data);
                    break;

                case PacketID.End:
                    Console.WriteLine("[From Client] I'm Done!");
                    break;
            }
        }
        public override void MyDisconnectMethod(EndPoint ep)
        {
            Console.WriteLine($"Disconnected with {ep}!");
        }
    }
}