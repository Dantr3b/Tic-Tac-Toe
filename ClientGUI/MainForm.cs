using System;
using System.Windows.Forms;

namespace ClientGUI
{
    // Formulaire d'accueil pour choisir le mode de jeu
    public partial class MainForm : Form
    {
        public MainForm()
        {
            // Centre la fenêtre à l'écran
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Tic Tac Toe";

            // Bouton pour lancer une partie solo contre l'IA
            var soloButton = new Button { Text = "Solo (contre IA)", Width = 200, Top = 30, Left = 50 };
            // Bouton pour lancer une partie multijoueur
            var multiButton = new Button { Text = "Multijoueur", Width = 200, Top = 80, Left = 50 };

            // Associe le clic sur chaque bouton à la méthode StartGame avec le bon mode
            soloButton.Click += (s, e) => StartGame(true);
            multiButton.Click += (s, e) => StartGame(false);

            // Ajoute les boutons à la fenêtre
            Controls.Add(soloButton);
            Controls.Add(multiButton);

            // Définit la taille de la fenêtre
            Width = 300;
            Height = 180;
        }

        // Lance la fenêtre de jeu dans le mode choisi (solo ou multi)
        private void StartGame(bool solo)
        {
            Hide(); // Cache la fenêtre d'accueil
            var gameForm = new GameForm(solo); // Crée la fenêtre de jeu
            gameForm.FormClosed += (s, e) => Close(); // Ferme tout à la fermeture du jeu
            gameForm.Show(); // Affiche la fenêtre de jeu
        }
    }
}
