using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;

namespace Server
{
    public class GameServer
    {
        private readonly int _port;                  // Port d'écoute du serveur
        private TcpListener _listener;               // Listener TCP pour accepter les connexions
        private bool _isRunning;                     // Indique si le serveur tourne
        private List<PlayerHandler> _connectedPlayers = new(); // Liste des joueurs connectés

        public Action<GameSession>? OnNewSession;    // Callback pour notifier la création d'une nouvelle session

        public GameServer(int port)
        {
            _port = port;
        }

        // Démarre le serveur et accepte les connexions entrantes
        public void Start()
        {
            _listener = new TcpListener(IPAddress.Any, _port);
            _listener.Start();
            _isRunning = true;

            Console.WriteLine($"[SERVER] Started on port {_port}");

            while (_isRunning)
            {
                // Attend et accepte une nouvelle connexion client
                TcpClient client = _listener.AcceptTcpClient();
                Console.WriteLine("[SERVER] New client connected");

                // Crée un gestionnaire pour ce joueur
                PlayerHandler playerHandler = new PlayerHandler(client, this);
                _connectedPlayers.Add(playerHandler);

                // Lance un thread pour gérer la communication avec ce joueur
                Thread playerThread = new Thread(playerHandler.HandleClient);
                playerThread.Start();

                // Si deux joueurs sont connectés, crée une nouvelle session de jeu
                if (_connectedPlayers.Count % 2 == 0)
                {
                    var player1 = _connectedPlayers[^2]; // Avant-dernier joueur connecté
                    var player2 = _connectedPlayers[^1]; // Dernier joueur connecté

                    GameSession session = new GameSession(player1, player2);
                    OnNewSession?.Invoke(session); // Notifie la création de la session
                    session.Start();               // Démarre la partie
                }
            }
        }

        // Arrête le serveur proprement
        public void Stop()
        {
            _isRunning = false;
            _listener.Stop();
        }
    }
}
