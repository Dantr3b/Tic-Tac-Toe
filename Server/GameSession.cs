using System;
using Shared;

namespace Server
{
    public class GameSession
    {
        private readonly PlayerHandler _playerX;
        private readonly PlayerHandler _playerO;
        private char[,] _board = new char[3, 3]; // 'X', 'O', or '\0'
        private PlayerHandler _currentPlayer;
        private bool _gameOver = false;

        public GameSession(PlayerHandler player1, PlayerHandler player2)
        {
            _playerX = player1;
            _playerO = player2;
            _currentPlayer = _playerX;

            _playerX.Session = this;
            _playerO.Session = this;
        }

        public void Start()
        {
            Console.WriteLine("[GAME] Starting a new session");
            SendToBoth(new Message { Type = "START", Content = "Game started! X begins." });
            UpdateClients();
        }

        public void ReceiveMessage(PlayerHandler sender, Message msg)
        {
            if (_gameOver) return;

            if (msg.Type == "MOVE")
            {
                var parts = msg.Content.Split(',');
                if (parts.Length != 2) return;

                if (!int.TryParse(parts[0], out int row) || !int.TryParse(parts[1], out int col))
                    return;

                if (sender != _currentPlayer)
                {
                    sender.SendMessage(new Message { Type = "ERROR", Content = "Not your turn." });
                    return;
                }

                if (_board[row, col] != '\0')
                {
                    sender.SendMessage(new Message { Type = "ERROR", Content = "Cell already taken." });
                    return;
                }

                char symbol = sender == _playerX ? 'X' : 'O';
                _board[row, col] = symbol;

                if (CheckWin(symbol))
                {
                    _gameOver = true;
                    SendToBoth(new Message { Type = "END", Content = $"Player {symbol} wins!" });
                    UpdateClients();
                    return;
                }

                if (CheckDraw())
                {
                    _gameOver = true;
                    SendToBoth(new Message { Type = "END", Content = "Draw!" });
                    UpdateClients();
                    return;
                }

                // Passer au joueur suivant
                _currentPlayer = (_currentPlayer == _playerX) ? _playerO : _playerX;
                UpdateClients();
            }
        }

        public void Restart()
        {
            _board = new char[3, 3];
            _gameOver = false;
            _currentPlayer = _playerX;
            SendToBoth(new Message { Type = "INFO", Content = "Nouvelle partie ! X commence." });
            UpdateClients();
        }

        private void UpdateClients()
        {
            string state = SerializeBoard();
            _playerX.SendMessage(new Message { Type = "STATE", Content = state });
            _playerO.SendMessage(new Message { Type = "STATE", Content = state });

            _currentPlayer.SendMessage(new Message { Type = "YOUR_TURN", Content = "" });
        }

        private string SerializeBoard()
        {
            string result = "";
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    result += _board[r, c] == '\0' ? "." : _board[r, c].ToString();
            return result; // Exemple : "X.OXO...O"
        }

        private bool CheckWin(char symbol)
        {
            for (int i = 0; i < 3; i++)
            {
                if (_board[i, 0] == symbol && _board[i, 1] == symbol && _board[i, 2] == symbol) return true;
                if (_board[0, i] == symbol && _board[1, i] == symbol && _board[2, i] == symbol) return true;
            }

            if (_board[0, 0] == symbol && _board[1, 1] == symbol && _board[2, 2] == symbol) return true;
            if (_board[0, 2] == symbol && _board[1, 1] == symbol && _board[2, 0] == symbol) return true;

            return false;
        }

        private bool CheckDraw()
        {
            foreach (char c in _board)
                if (c == '\0') return false;
            return true;
        }

        private void SendToBoth(Message msg)
        {
            _playerX.SendMessage(msg);
            _playerO.SendMessage(msg);
        }

        public char[,] GetBoard()
        {
            return (char[,])_board.Clone();
        }

        public static bool CheckWinStatic(char[,] board, char symbol)
        {
            for (int i = 0; i < 3; i++)
            {
                if (board[i, 0] == symbol && board[i, 1] == symbol && board[i, 2] == symbol) return true;
                if (board[0, i] == symbol && board[1, i] == symbol && board[2, i] == symbol) return true;
            }

            if (board[0, 0] == symbol && board[1, 1] == symbol && board[2, 2] == symbol) return true;
            if (board[0, 2] == symbol && board[1, 1] == symbol && board[2, 0] == symbol) return true;

            return false;
        }
    }
}
