using System;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            string serverIP = "127.0.0.1";
            int port = 5000;

            GameClient client = new GameClient(serverIP, port);
            client.Start();
        }
    }
}
