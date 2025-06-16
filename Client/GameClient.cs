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

        private bool _yourTurn = false;
        private bool _gameOver = false; 

        public GameClient(string serverIP, int port)
        {
            _serverIP = serverIP;
            _port = port;
        }

        public void Start()
        {
            try
            {
                _client = new TcpClient(_serverIP, _port);
                Console.WriteLine("[CLIENT] Connected to server");

                NetworkStream stream = _client.GetStream();
                _reader = new StreamReader(stream);
                _writer = new StreamWriter(stream) { AutoFlush = true };

                Thread listenThread = new Thread(Listen);
                listenThread.Start();

                while (true)
                {
                    if (_gameOver) break; // Arrête la boucle si la partie est finie

                    if (_yourTurn)
                    {
                        Console.Write("Enter your move (row,col): ");
                        string input = Console.ReadLine();

                        Message move = new Message { Type = "MOVE", Content = input };
                        string json = JsonSerializer.Serialize(move);
                        _writer.WriteLine(json);
                        _yourTurn = false;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[CLIENT] Error: {ex.Message}");
            }
        }

        private void Listen()
        {
            try
            {
                while (true)
                {
                    string? line = _reader.ReadLine();
                    if (line == null) break;

                    Message? msg = JsonSerializer.Deserialize<Message>(line);
                    if (msg == null) continue;

                    switch (msg.Type)
                    {
                        case "INFO":
                        case "START":
                        case "ERROR":
                            Console.WriteLine($"[SERVER] {msg.Content}");
                            break;

                        case "STATE":
                            _ui.DisplayBoard(msg.Content);
                            break;

                        case "YOUR_TURN":
                            if (!_gameOver) // Ne propose le tour que si la partie n'est pas finie
                            {
                                Console.WriteLine(">> It's your turn!");
                                _yourTurn = true;
                            }
                            break;

                        case "END":
                            Console.WriteLine($"[SERVER] {msg.Content}");
                            _gameOver = true; // Marque la partie comme terminée
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
