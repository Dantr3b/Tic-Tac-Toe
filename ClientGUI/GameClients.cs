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
        private readonly GameForm _form;      // Référence au formulaire principal (UI)
        private readonly bool _soloMode;      // Indique si on joue en solo contre l'IA
        private TcpClient _client;            // Connexion TCP au serveur
        private StreamReader _reader;         // Pour lire les messages du serveur
        private StreamWriter _writer;         // Pour envoyer des messages au serveur
        private Thread _listenThread;         // Thread d'écoute des messages serveur
        private bool _yourTurn;               // Indique si c'est au tour du joueur

        public GameClient(GameForm form, bool soloMode)
        {
            _form = form;
            _soloMode = soloMode;
        }

        // Démarre la connexion au serveur (solo ou multi)
        public void Start()
        {
            if (_soloMode)
            {
                ConnectToServer();
            }
            else
            {
                ConnectToServer();
            }
        }

        // Établit la connexion TCP et démarre le thread d'écoute
        private void ConnectToServer()
        {
            try
            {
                _client = new TcpClient("127.0.0.1", 5000);
                NetworkStream stream = _client.GetStream();
                _reader = new StreamReader(stream);
                _writer = new StreamWriter(stream) { AutoFlush = true };

                // Lance le thread qui écoute les messages du serveur
                _listenThread = new Thread(Listen);
                _listenThread.Start();
            }
            catch (Exception ex)
            {
                _form.ShowMessage($"Erreur de connexion : {ex.Message}");
            }
        }

        // Envoie un coup au serveur si c'est le tour du joueur
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

        // Thread d'écoute des messages du serveur
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
                            // Affiche les messages d'information ou d'erreur dans l'UI
                            _form.ShowMessage(msg.Content);
                            break;

                        case "STATE":
                            // Met à jour l'affichage du plateau
                            _form.UpdateBoard(msg.Content);
                            break;

                        case "YOUR_TURN":
                            // Active le tour du joueur
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
