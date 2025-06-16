using System;
using Shared;

namespace Server
{
    // Représente une session de jeu entre deux joueurs (humain ou IA)
    public class GameSession
    {
        private readonly PlayerHandler _playerX;      // Joueur X
        private readonly PlayerHandler _playerO;      // Joueur O
        private char[,] _board = new char[3, 3];      // Plateau de jeu ('X', 'O', ou '\0')
        private PlayerHandler _currentPlayer;         // Joueur dont c'est le tour
        private bool _gameOver = false;               // Indique si la partie est terminée

        // Constructeur : initialise la session avec deux joueurs
        public GameSession(PlayerHandler player1, PlayerHandler player2)
        {
            _playerX = player1;
            _playerO = player2;
            _currentPlayer = _playerX; // X commence

            _playerX.Session = this;
            _playerO.Session = this;
        }

        // Démarre la session de jeu
        public void Start()
        {
            Console.WriteLine("[GAME] Starting a new session");
            SendToBoth(new Message { Type = "START", Content = "Game started! X begins." });
            UpdateClients();
        }

        // Traite un message reçu d'un joueur (ex: un coup joué)
        public void ReceiveMessage(PlayerHandler sender, Message msg)
        {
            if (_gameOver) return;

            if (msg.Type == "MOVE")
            {
                var parts = msg.Content.Split(',');
                if (parts.Length != 2) return;

                if (!int.TryParse(parts[0], out int row) || !int.TryParse(parts[1], out int col))
                    return;

                // Vérifie que c'est bien au tour du joueur
                if (sender != _currentPlayer)
                {
                    sender.SendMessage(new Message { Type = "ERROR", Content = "Not your turn." });
                    return;
                }

                // Vérifie que la case est libre
                if (_board[row, col] != '\0')
                {
                    sender.SendMessage(new Message { Type = "ERROR", Content = "Cell already taken." });
                    return;
                }

                // Place le symbole du joueur sur la grille
                char symbol = sender == _playerX ? 'X' : 'O';
                _board[row, col] = symbol;

                // Vérifie si le joueur a gagné
                if (CheckWin(symbol))
                {
                    _gameOver = true;
                    SendToBoth(new Message { Type = "END", Content = $"Player {symbol} wins!" });
                    UpdateClients();
                    return;
                }

                // Vérifie s'il y a match nul
                if (CheckDraw())
                {
                    _gameOver = true;
                    SendToBoth(new Message { Type = "END", Content = "Draw!" });
                    UpdateClients();
                    return;
                }

                // Passe au joueur suivant et met à jour les clients
                _currentPlayer = (_currentPlayer == _playerX) ? _playerO : _playerX;
                UpdateClients();
            }
        }

        // Relance une nouvelle partie avec les mêmes joueurs
        public void Restart()
        {
            _board = new char[3, 3];
            _gameOver = false;
            _currentPlayer = _playerX;
            SendToBoth(new Message { Type = "INFO", Content = "Nouvelle partie ! X commence." });
            UpdateClients();
        }

        // Envoie l'état du plateau à chaque joueur et indique à qui c'est le tour
        private void UpdateClients()
        {
            string state = SerializeBoard();
            _playerX.SendMessage(new Message { Type = "STATE", Content = state });
            _playerO.SendMessage(new Message { Type = "STATE", Content = state });

            _currentPlayer.SendMessage(new Message { Type = "YOUR_TURN", Content = "" });
        }

        // Sérialise le plateau sous forme de chaîne (ex: "X.OXO...O")
        private string SerializeBoard()
        {
            string result = "";
            for (int r = 0; r < 3; r++)
                for (int c = 0; c < 3; c++)
                    result += _board[r, c] == '\0' ? "." : _board[r, c].ToString();
            return result;
        }

        // Vérifie si le joueur avec le symbole donné a gagné
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

        // Vérifie s'il y a match nul (aucune case vide)
        private bool CheckDraw()
        {
            foreach (char c in _board)
                if (c == '\0') return false;
            return true;
        }

        // Envoie un message aux deux joueurs
        private void SendToBoth(Message msg)
        {
            _playerX.SendMessage(msg);
            _playerO.SendMessage(msg);
        }

        // Retourne une copie du plateau (pour l'IA)
        public char[,] GetBoard()
        {
            return (char[,])_board.Clone();
        }

        // Méthode statique pour vérifier une victoire sur un plateau donné (utilisé par l'IA)
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
