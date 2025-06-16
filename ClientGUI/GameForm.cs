using System;
using System.Drawing;
using System.Windows.Forms;
using Shared;

namespace ClientGUI
{
    // Formulaire principal pour la grille de jeu Tic Tac Toe
    public partial class GameForm : Form
    {
        private Button[,] buttons = new Button[3, 3]; // Tableau des boutons de la grille
        private GameClient _client;                   // Client réseau associé à ce formulaire

        // Constructeur : initialise la fenêtre et la grille
        public GameForm(bool soloMode)
        {
            // Définition du titre et des dimensions de la fenêtre
            Text = soloMode ? "Tic Tac Toe - Solo" : "Tic Tac Toe - Multijoueur";
            Width = 250;
            Height = 300;
            StartPosition = FormStartPosition.CenterScreen;

            // Création des 9 boutons pour la grille
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
                        Tag = $"{r},{c}" // Stocke la position dans le Tag
                    };
                    btn.Click += OnGridClick; // Gère le clic sur la case
                    buttons[r, c] = btn;
                    Controls.Add(btn);        // Ajoute le bouton à la fenêtre
                }
            }

            // Crée et démarre le client réseau (solo ou multi)
            _client = new GameClient(this, soloMode);
            _client.Start();
        }

        // Gestionnaire d'événement pour le clic sur une case
        private void OnGridClick(object sender, EventArgs e)
        {
            var btn = sender as Button;
            string pos = btn.Tag.ToString(); // Récupère la position (ex: "1,2")
            _client.SendMove(pos);           // Envoie le coup au serveur
        }

        // Met à jour l'affichage de la grille selon l'état reçu du serveur
        public void UpdateBoard(string state)
        {
            Invoke(new Action(() =>
            {
                for (int i = 0; i < 9; i++)
                {
                    int r = i / 3;
                    int c = i % 3;
                    char symbol = state[i] == '.' ? ' ' : state[i]; // Vide si '.'
                    buttons[r, c].Text = symbol.ToString();
                    buttons[r, c].Enabled = symbol == ' '; // Active si vide
                }
            }));
        }

        // Affiche un message à l'utilisateur (popup)
        public void ShowMessage(string msg)
        {
            MessageBox.Show(msg);
        }
    }
}
