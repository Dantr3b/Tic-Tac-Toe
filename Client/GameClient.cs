using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using Shared;

namespace Client
{
    public class GameClient
    {
        private readonly string _serverIP;
        private readonly int _port;
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private GameUI _ui = new();

        private bool _yourTurn = false;   // Indique si c'est au tour du joueur
        private bool _gameOver = false;   // Indique si la partie est terminée

        public GameClient(string serverIP, int port)
        {
            _serverIP = serverIP;
            _port = port;
        }

        // Démarre la connexion au serveur et la boucle de jeu principale
        public void Start()
        {
            try
            {
                _client = new TcpClient(_serverIP, _port);
                Console.WriteLine("[CLIENT] Connected to server");

                NetworkStream stream = _client.GetStream();
                _reader = new StreamReader(stream);
                _writer = new StreamWriter(stream) { AutoFlush = true };

                // Lance un thread pour écouter les messages du serveur
                Thread listenThread = new Thread(Listen);
                listenThread.Start();

                // Boucle principale du client
                while (true)
                {
                    if (_gameOver) break; // Arrête la boucle si la partie est finie

                    if (_yourTurn)
                    {
                        Console.Write("Enter your move (row,col): ");
                        string input = Console.ReadLine();

                        // Envoie le coup au serveur
                        Message move = new Message { Type = "MOVE", Content = input };
                        string json = JsonSerializer.Serialize(move);
                        _writer.WriteLine(json);
                        _yourTurn = false; // Attend la réponse du serveur
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT] Error: {ex.Message}");
            }
        }

        // Écoute les messages du serveur dans un thread séparé
        private void Listen()
        {
            try
            {
                while (true)
                {
                    string? line = _reader.ReadLine();
                    if (line == null) break; // Fin de connexion

                    Message? msg = JsonSerializer.Deserialize<Message>(line);
                    if (msg == null) continue;

                    switch (msg.Type)
                    {
                        case "INFO":
                        case "START":
                        case "ERROR":
                            // Affiche les messages d'information ou d'erreur
                            Console.WriteLine($"[SERVER] {msg.Content}");
                            break;

                        case "STATE":
                            // Affiche le plateau de jeu
                            _ui.DisplayBoard(msg.Content);
                            break;

                        case "YOUR_TURN":
                            // Active le tour du joueur si la partie n'est pas finie
                            if (!_gameOver) 
                            {
                                Console.WriteLine(">> It's your turn!");
                                _yourTurn = true;
                            }
                            break;

                        case "END":
                            // Affiche le message de fin de partie
                            Console.WriteLine($"[SERVER] {msg.Content}");
                            _gameOver = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT] Connection closed: {ex.Message}");
            }
        }
    }
}
