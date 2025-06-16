using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Server
{
    public class GameServer
    {
        private readonly int _port;
        private TcpListener _listener;
        private bool _isRunning;
        private List<PlayerHandler> _connectedPlayers = new();

        public Action<GameSession>? OnNewSession; 

        public GameServer(int port)
        {
            _port = port;
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"[SERVER] Started on port {_port}");

            while (_isRunning)
            {
                TcpClient client = _listener.AcceptTcpClient();
                Console.WriteLine("[SERVER] New client connected");

                PlayerHandler playerHandler = new PlayerHandler(client, this);
                _connectedPlayers.Add(playerHandler);

                Thread playerThread = new Thread(playerHandler.HandleClient);
                playerThread.Start();



                              
                if (_connectedPlayers.Count % 2 == 0)
                {
                    var player1 = _connectedPlayers[^2];
                    var player2 = _connectedPlayers[^1];

                    GameSession session = new GameSession(player1, player2);
                    OnNewSession?.Invoke(session);
                    session.Start();
                }
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }
    }
}
