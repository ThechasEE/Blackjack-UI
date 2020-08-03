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
        private StackPanel[] notifications;
        private WrapPanel[] views;
        private TextBlock[] totals;
        private Button[] buttons;
        private int deckCount;

        public Freestyle(int deckCount)
        {
            InitializeComponent();

            this.deckCount = deckCount;

            notifications = new StackPanel[] { DealersTurnNotification, PushNotification, BustNotification, LostNotification, DealerBlackjackNotification, WinNotification, BlackjackNotification };
            views = new WrapPanel[] { DealerView, PlayerView };
            totals = new TextBlock[] { DealerTotal, PlayerTotal, FreestyleWin, FreestyleLost, FreestylePush };
            buttons = new Button[] { PlayButton, BackButton, HitButton, StandButton };
        }

        // Find the main window of this control.
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow(this);

            FreestyleWin.Text = parentWindow.freestyleWinCount.ToString();
            FreestyleLost.Text = parentWindow.freestyleLostCount.ToString();
            FreestylePush.Text = parentWindow.freestylePushCount.ToString();

            blackjackInstance = new Game(deckCount, parentWindow);

            BackButton.IsEnabled = true;
            PlayButton.IsEnabled = true;
        }

        // Play a round of the game. Does the initial draw.
        private async void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            PlayButton.IsEnabled = false;
            PlayButton.Content = "Play";

            await blackjackInstance.Play(views, totals, notifications, buttons);
        }

        // Ask the player to go back to menu.
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            ExitPopup.IsOpen = true;
        }

        // Draw a card.
        private async void HitButton_Click(object sender, RoutedEventArgs e)
        {
            await blackjackInstance.Hit(views, totals, notifications, buttons);
        }

        // Signal for dealer to do his turn.
        private async void StandButton_Click(object sender, RoutedEventArgs e)
        {
            await blackjackInstance.Stand(views, totals, notifications, buttons);
        }

        // Hide the back menu.
        private void ConfirmCancel_Click(object sender, RoutedEventArgs e)
        {
            ExitPopup.IsOpen = false;
        }

        // Go back to main menu.
        private void ConfirmBack_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.MainScreen.Content = new MainMenu();
        }
    }
}
