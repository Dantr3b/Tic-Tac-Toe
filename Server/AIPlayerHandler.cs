using System;
using Shared;

namespace Server
{
    // Gestionnaire de joueur IA (Intelligence Artificielle)
    public class AIPlayerHandler : PlayerHandler
    {
        private GameSession _session; // Référence à la session de jeu courante

        // Constructeur : l'IA n'a pas de TcpClient, juste une référence serveur
        public AIPlayerHandler(GameServer server) : base(null, server)
        {
            Name = "AI"; // Nom affiché pour l'IA
        }

        // Associe la session de jeu à l'IA
        public override void SetSession(GameSession session)
        {
            _session = session;
            base.SetSession(session);
        }

        // Réception d'un message (appelé par la session)
        public override void SendMessage(Message message)
        {
            if (message.Type == "YOUR_TURN")
            {
                // Simule une pause pour rendre l'IA plus humaine
                Thread.Sleep(1000);

                // Calcule le meilleur coup à jouer
                string move = ComputeBestMove(_session.GetBoard(), 'O'); // 'O' = symbole de l'IA
                Message msg = new Message { Type = "MOVE", Content = move };

                Console.WriteLine($"[AI] Playing move: {move}");
                // Envoie le coup directement à la session (pas via le réseau)
                _session.ReceiveMessage(this, msg);
            }
            // Les autres types de messages sont ignorés par l'IA
        }

        // Calcule le meilleur coup à jouer pour l'IA
        private string ComputeBestMove(char[,] board, char symbol)
        {
            // 1. Essayer de gagner si possible
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (board[r, c] == '\0' && WouldWin(board, r, c, symbol))
                        return $"{r},{c}";

            // 2. Bloquer l'adversaire s'il peut gagner au prochain coup
            char opponent = symbol == 'X' ? 'O' : 'X';
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (board[r, c] == '\0' && WouldWin(board, r, c, opponent))
                        return $"{r},{c}";

            // 3. Sinon, jouer la première case libre trouvée
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (board[r, c] == '\0')
                        return $"{r},{c}";

            // 4. Par défaut (ne devrait jamais arriver)
            return "0,0";
        }

        // Vérifie si jouer à (row, col) avec le symbole donné permet de gagner
        private bool WouldWin(char[,] board, int row, int col, char symbol)
        {
            board[row, col] = symbol;
            bool win = GameSession.CheckWinStatic(board, symbol);
            board[row, col] = '\0'; // Annule le coup simulé
            return win;
        }
    }
}
