using System;
using System.IO;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading;
using Shared;

namespace ClientGUI
{
    public class GameClient
    {
        private readonly GameForm _form;
        private readonly bool _soloMode;
        private TcpClient _client;
        private StreamReader _reader;
        private StreamWriter _writer;
        private Thread _listenThread;
        private bool _yourTurn;

        public GameClient(GameForm form, bool soloMode)
        {
            _form = form;
            _soloMode = soloMode;
        }

        public void Start()
        {
            if (_soloMode)
            {
                // Démarrer le serveur local avec IA automatiquement (optionnel)
                ConnectToServer();
            }
            else
            {
                ConnectToServer();
            }
        }

        private void ConnectToServer()
        {
            try
            {
                _client = new TcpClient("127.0.0.1", 5000);
                NetworkStream stream = _client.GetStream();
                _reader = new StreamReader(stream);
                _writer = new StreamWriter(stream) { AutoFlush = true };

                _listenThread = new Thread(Listen);
                _listenThread.Start();
            }
            catch (Exception ex)
            {
                _form.ShowMessage($"Erreur de connexion : {ex.Message}");
            }
        }

        public void SendMove(string pos)
        {
            if (!_yourTurn) return;

            var msg = new Shared.Message
            {
                Type = "MOVE",
                Content = pos
            };

            string json = JsonSerializer.Serialize(msg);
            _writer.WriteLine(json);
            _yourTurn = false;
        }

        private void Listen()
        {
            try
            {
                while (true)
                {
                    string line = _reader.ReadLine();
                    if (line == null) break;

                    var msg = JsonSerializer.Deserialize<Shared.Message>(line);
                    if (msg == null) continue;

                    switch (msg.Type)
                    {
                        case "INFO":
                        case "START":
                        case "END":
                        case "ERROR":
                            _form.ShowMessage(msg.Content);
                            break;

                        case "STATE":
                            _form.UpdateBoard(msg.Content);
                            break;

                        case "YOUR_TURN":
                            _yourTurn = true;
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _form.ShowMessage($"Connexion perdue : {ex.Message}");
            }
        }
    }
}
