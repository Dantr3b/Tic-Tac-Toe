using System;
using System.Threading;

namespace Server
{
    class Program
    {
        static GameSession? lastSession = null;

        static void Main(string[] args)
        {
            int port = 5000;
            GameServer server = new GameServer(port);

            // Thread pour écouter la console
            new Thread(() =>
            {
                while (true)
                {
                    string? cmd = Console.ReadLine();
                    if (cmd != null && cmd.Trim().ToLower() == "restart" && lastSession != null)
                    {
                        lastSession.Restart();
                        Console.WriteLine("[SERVER] Partie relancée !");
                    }
                }
            }).Start();

            // Passe une référence pour garder la dernière session
            server.OnNewSession = session => lastSession = session;

            server.Start();
        }
    }
}
