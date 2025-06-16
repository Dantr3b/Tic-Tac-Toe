using System;
using Shared;

namespace Server
{
    public class AIPlayerHandler : PlayerHandler
    {
        private GameSession _session;

        public AIPlayerHandler(GameServer server) : base(null, server)
        {
            Name = "AI";
        }

        public override void SetSession(GameSession session)
        {
            _session = session;
            base.SetSession(session);
        }

        public override void SendMessage(Message message)
        {
            if (message.Type == "YOUR_TURN")
            {
                // Simuler une pause
                Thread.Sleep(1000);

                // Choisir un coup
                string move = ComputeBestMove(_session.GetBoard(), 'O'); // 'O' = IA
                Message msg = new Message { Type = "MOVE", Content = move };

                Console.WriteLine($"[AI] Playing move: {move}");
                _session.ReceiveMessage(this, msg);
            }
        }

        private string ComputeBestMove(char[,] board, char symbol)
        {
            // Essayer de gagner
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (board[r, c] == '\0' && WouldWin(board, r, c, symbol))
                        return $"{r},{c}";

            // Bloquer l'autre joueur
            char opponent = symbol == 'X' ? 'O' : 'X';
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (board[r, c] == '\0' && WouldWin(board, r, c, opponent))
                        return $"{r},{c}";

            // Jouer première case libre
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    if (board[r, c] == '\0')
                        return $"{r},{c}";

            return "0,0"; // Par défaut
        }

        private bool WouldWin(char[,] board, int row, int col, char symbol)
        {
            board[row, col] = symbol;
            bool win = GameSession.CheckWinStatic(board, symbol);
            board[row, col] = '\0';
            return win;
        }
    }
}
