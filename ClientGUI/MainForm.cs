using System;
using System.Windows.Forms;

namespace ClientGUI
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tic Tac Toe";

            var soloButton = new Button { Text = "Solo (contre IA)", Width = 200, Top = 30, Left = 50 };
            var multiButton = new Button { Text = "Multijoueur", Width = 200, Top = 80, Left = 50 };

            soloButton.Click += (s, e) => StartGame(true);
            multiButton.Click += (s, e) => StartGame(false);

            Controls.Add(soloButton);
            Controls.Add(multiButton);
            Width = 300;
            Height = 180;
        }

        private void StartGame(bool solo)
        {
            Hide();
            var gameForm = new GameForm(solo);
            gameForm.FormClosed += (s, e) => Close();
            gameForm.Show();
        }
    }
}
