using Core;

namespace Client
{
    public class Client
    {
        public static int searchPort = 2345;

        static void Main()
        {
            Connector connector = new Connector();
            connector.Init(searchPort, () => { return new MyClientSession(); });

            while (true) { };
        }
    }
}