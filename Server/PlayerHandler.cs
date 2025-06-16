using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using Shared;

namespace Server
{
    // Gère la connexion et la communication avec un joueur humain
    public class PlayerHandler
    {
        private readonly TcpClient _client;      // Connexion TCP du joueur
        private readonly GameServer _server;     // Référence au serveur principal
        private NetworkStream _stream;           // Flux réseau
        private StreamReader _reader;            // Pour lire les messages du client
        private StreamWriter _writer;            // Pour envoyer des messages au client

        public string Name { get; protected set; }      // Nom du joueur (peut être utilisé pour l'affichage)
        public GameSession Session { get; set; }        // Session de jeu associée à ce joueur

        public PlayerHandler(TcpClient client, GameServer server)
        {
            _client = client;
            _server = server;
        }

        // Gère la boucle de communication avec le client
        public void HandleClient()
        {
            try
            {
                _stream = _client.GetStream();
                _reader = new StreamReader(_stream);
                _writer = new StreamWriter(_stream) { AutoFlush = true };

                // Envoie un message d'information à la connexion
                SendMessage(new Message { Type = "INFO", Content = "Connected to Tic Tac Toe server!" });

                // Attendre que la session soit créée (pour le multijoueur)
                while (Session == null)
                {
                    Thread.Sleep(50);
                }

                // Boucle principale de réception des messages du client
                while (true)
                {
                    string? input = _reader.ReadLine();
                    if (input == null) break; // Fin de connexion

                    Message? msg = JsonSerializer.Deserialize<Message>(input);
                    if (msg == null) continue;

                    Console.WriteLine($"[SERVER] Received from player: {msg.Type} - {msg.Content}");

                    // Si le client demande une partie solo, on la lance immédiatement
                    if (msg.Type == "MODE" && msg.Content == "SOLO")
                    {
                        var ai = new AIPlayerHandler(_server);
                        var session = new GameSession(this, ai);
                        _server.OnNewSession?.Invoke(session);
                        session.Start();
                        break; // Sort de la boucle, ce joueur est en session solo
                    }

                    // Transmet le message à la session de jeu (pour traiter les coups, etc.)
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

        // Associe une session à ce joueur
        public virtual void SetSession(GameSession session)
        {
            Session = session;
        }

        // Envoie un message au client via le réseau
        public virtual void SendMessage(Message message)
        {
            string json = JsonSerializer.Serialize(message);
            _writer.WriteLine(json);
        }
    }
}
