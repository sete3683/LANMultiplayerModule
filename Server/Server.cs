using Core;
using System.Collections.Generic;

namespace Server
{
    public class Server
    {
        public static int searchPort = 2345;
        public static int listenPort = 3456;

        static void Main()
        {
            Listener listener = new Listener();
            listener.Init(searchPort, listenPort, () => { return new MyServerSession(); });

            while (true) { };
        }
    }
}