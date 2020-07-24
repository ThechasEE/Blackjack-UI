using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
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
    /// Interaction logic for MainMenu.xaml
    /// </summary>
    public partial class MainMenu : UserControl
    {
        private MainWindow parentWindow;
        private int deckCount;

        public MainMenu()
        {
            InitializeComponent();
        }

        // Find the main window of this control.
        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = (MainWindow)Window.GetWindow(this);

            // Set up slider.
            deckCount = parentWindow.deckCount;
            DeckSlider.Value = deckCount;
            DeckCounter.Text = deckCount.ToString();
        }

        // Handles the "How to play Blackjack" hyperlink.
        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            string url;

            try
            {
                System.Diagnostics.Process.Start(e.Uri.AbsoluteUri);
            }
            catch
            {
                url = e.Uri.AbsoluteUri;
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
        }

        // Updates the number to the right of the slider.
        private void DeckSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (DeckCounter != null)
            {
                deckCount = (int)e.NewValue;
                DeckCounter.Text = deckCount.ToString();
                parentWindow.deckCount = deckCount;
            }
        }

        // Change screen to freestyle gamemode.
        private void FreestyleButton_Click(object sender, RoutedEventArgs e)
        {
            parentWindow.MainScreen.Content = new Freestyle(deckCount);
        }

        // Change screen to challenge gamemode.
        private void ChallengeButton_Click(object sender, RoutedEventArgs e)
        {
            //parentWindow.MainScreen.Content = new Challenge();
        }
    }
}
