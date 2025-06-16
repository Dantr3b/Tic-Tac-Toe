using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using Shared;

namespace Server
{
    public class PlayerHandler
    {
        private readonly TcpClient _client;
        private readonly GameServer _server;
        private NetworkStream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;

        public string Name { get; protected set; } 
        public GameSession Session { get; set; }

        public PlayerHandler(TcpClient client, GameServer server)
        {
            _client = client;
            _server = server;
        }

        public void HandleClient()
        {
            try
            {
                _stream = _client.GetStream();
                _reader = new StreamReader(_stream);
                _writer = new StreamWriter(_stream) { AutoFlush = true };

                SendMessage(new Message { Type = "INFO", Content = "Connected to Tic Tac Toe server!" });

                // Attendre que la session soit créée (pour le multijoueur)
                while (Session == null)
                {
                    Thread.Sleep(50);
                }

                while (true)
                {
                    string? input = _reader.ReadLine();
                    if (input == null) break;

                    Message? msg = JsonSerializer.Deserialize<Message>(input);
                    if (msg == null) continue;

                    Console.WriteLine($"[SERVER] Received from player: {msg.Type} - {msg.Content}");

                    if (msg.Type == "MODE" && msg.Content == "SOLO")
                    {
                        var ai = new AIPlayerHandler(_server);
                        var session = new GameSession(this, ai);
                        _server.OnNewSession?.Invoke(session);
                        session.Start();
                        break;
                    }

                    // Transmettre à la session de jeu
                    Session?.ReceiveMessage(this, msg);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Player disconnected or error: {ex.Message}");
            }
            finally
            {
                _client.Close();
            }
        }

        public virtual void SetSession(GameSession session)
        {
            Session = session;
        }

        public virtual void SendMessage(Message message)
        {
            string json = JsonSerializer.Serialize(message);
            _writer.WriteLine(json);
        }
    }
}
