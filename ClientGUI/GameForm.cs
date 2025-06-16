using System;
using System.Drawing;
using System.Windows.Forms;
using Shared;

namespace ClientGUI
{
    public partial class GameForm : Form
    {
        private Button[,] buttons = new Button[3, 3];
        private GameClient _client;

        public GameForm(bool soloMode)
        {
            Text = soloMode ? "Tic Tac Toe - Solo" : "Tic Tac Toe - Multijoueur";
            Width = 250;
            Height = 300;
            StartPosition = FormStartPosition.CenterScreen;

            for (int r = 0; r < 3; r++)
            {
                for (int c = 0; c < 3; c++)
                {
                    var btn = new Button
                    {
                        Width = 60,
                        Height = 60,
                        Left = 20 + c * 65,
                        Top = 20 + r * 65,
                        Font = new Font(FontFamily.GenericSansSerif, 20),
                        Tag = $"{r},{c}"
                    };
                    btn.Click += OnGridClick;
                    buttons[r, c] = btn;
                    Controls.Add(btn);
                }
            }

            _client = new GameClient(this, soloMode);
            _client.Start();
        }

        private void OnGridClick(object sender, EventArgs e)
        {
            var btn = sender as Button;
            string pos = btn.Tag.ToString();
            _client.SendMove(pos);
        }

        public void UpdateBoard(string state)
        {
            Invoke(new Action(() =>
            {
                for (int i = 0; i < 9; i++)
                {
                    int r = i / 3;
                    int c = i % 3;
                    char symbol = state[i] == '.' ? ' ' : state[i];
                    buttons[r, c].Text = symbol.ToString();
                    buttons[r, c].Enabled = symbol == ' ';
                }
            }));
        }

        public void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
        }
    }
}
