namespace Client
{
    public class GameUI
    {
        public void DisplayBoard(string board)
        {
            Console.WriteLine("Current Board:");
            for (int i = 0; i < 9; i++)
            {
                char c = board[i] == '.' ? ' ' : board[i];
                Console.Write($" {c} ");
                if ((i + 1) % 3 == 0) Console.WriteLine();
                else Console.Write("|");
            }
            Console.WriteLine();
        }
    }
}
