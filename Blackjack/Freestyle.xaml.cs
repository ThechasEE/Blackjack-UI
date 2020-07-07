using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Blackjack
{
    /// <summary>
    /// Interaction logic for Freestyle.xaml
    /// </summary>
    public partial class Freestyle : UserControl
    {
        private MainWindow parentWindow;
        private Game blackjackInstance;
        private int deckCount;

        public Freestyle(int deckCount)
        {
            InitializeComponent();

            this.deckCount = deckCount;
            blackjackInstance = new Game(this.deckCount);

            // Set button states.
            HitButton.IsEnabled = false;
            StandButton.IsEnabled = false;
        }

        // Find the main window of this control.
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow(this);
        }

        // Take the player back to the main menu.
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.MainScreen.Content = new MainMenu();
        }

        // Play a round of the game. Does the initial draw.
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButton.IsEnabled = false;

            await blackjackInstance.DrawInitialCards(DealerView, PlayerView, DealerTotal, PlayerTotal);

            HitButton.IsEnabled = true;
            StandButton.IsEnabled = true;
        }
    }
}
