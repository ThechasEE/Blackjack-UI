using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Blackjack
{
    public static class Constants
    {
        public const int CARDS_IN_A_DECK = 52;
        public const int AMOUNT_OF_STARTING_CARDS = 4;
        public const int DRAW_DELAY = 250;
        public const int POPUP_DELAY = 2000;
        public const int TIMEOUT_DELAY = 300;
        public const int DEALER = 0;
        public const int PLAYER = 1;
        public const int PLAY_BUTTON = 0;
        public const int BACK_BUTTON = 1;
        public const int HIT_BUTTON = 2;
        public const int STAND_BUTTON = 3;
        public const int DEALER_TURN_NOTIF = 0;
        public const int PUSH_NOTIF = 1;
        public const int BUST_NOTIF = 2;
        public const int LOST_NOTIF = 3;
        public const int DEALER_BLACKJACK_NOTIF = 4;
        public const int WIN_NOTIF = 5;
        public const int BLACKJACK_NOTIF = 6;
        public static readonly int[] CARD_VALUES = {11,2,3,4,5,6,7,8,9,10,10,10,10};
    }

    public partial class Card
    {
        private int cardValue;
        private string imageSource;

        public int CardValue { get => cardValue; set => cardValue = value; }
        public string ImageSource { get => imageSource; set => imageSource = value; }
    }

    public partial class Deck
    {
        private Card[] cards;
        private int totalCards;

        public Deck(int deckCount, int startingAmount)
        {
            // Allocates enough memory for all cards that could be in a deck.
            cards = new Card[deckCount * Constants.CARDS_IN_A_DECK];
            totalCards = startingAmount;
        }

        public Card[] Cards { get => cards; set => cards = value; }
        public int TotalCards { get => totalCards; set => totalCards = value; }
    }

    public partial class Hand
    {
        private LinkedList<Card> cards = new LinkedList<Card>();
        private int handValue = 0;

        public LinkedList<Card> Cards { get => cards; set => cards = value; }
        public int HandValue { get => handValue; set => handValue = value; }
    }

    // Defines methods and variables for how a game operates.
    public partial class Game
    {
        private int deckCount;
        private int overallCardCount;
        private int? startMoney;
        private int? betMin;
        private int? winMoney;
        private Deck playDeck;
        private Deck usedDeck;
        private Hand dealerHand;
        private Hand playerHand;
        private MainWindow parentWindow;
        private Random random = new Random();

        // Freestyle constructor.
        public Game(int deckCount, MainWindow parentWindow)
        {
            this.deckCount = deckCount;
            this.parentWindow = parentWindow;
            overallCardCount = deckCount * Constants.CARDS_IN_A_DECK;

            startMoney = null;
            betMin = null;
            winMoney = null;

            BlackjackSetup();
        }

        // Challenge constructor. WIP
        public Game(int deckCount, int startMoney, int betMin, int winMoney)
        {
            
        }

        // Set up the decks and hands.
        private void BlackjackSetup()
        {
            CreateDecks();

            dealerHand = new Hand();
            playerHand = new Hand();
        }

        // Player stands, do dealers turn.
        public async Task Stand(WrapPanel[] views, TextBlock[] totals, StackPanel[] notifications, Button[] buttons)
        {
            buttons[Constants.HIT_BUTTON].IsEnabled = false;
            buttons[Constants.STAND_BUTTON].IsEnabled = false;

            await DisplayNotification(notifications, Constants.DEALER_TURN_NOTIF);

            await Task.Delay(Constants.TIMEOUT_DELAY);

            RevealCard(views[Constants.DEALER], totals[Constants.DEALER]);

            // Draw dealers cards.
            while (dealerHand.HandValue < 17)
            {
                await Task.Delay(Constants.DRAW_DELAY);

                if (playDeck.TotalCards == 0)
                    RecombineDecks();

                // Draw the card.
                DrawCard(dealerHand, views[Constants.DEALER], false);
                totals[Constants.DEALER].Text = dealerHand.HandValue.ToString();
            }

            await Task.Delay(Constants.TIMEOUT_DELAY);

            // Dealer bust, player wins.
            if (dealerHand.HandValue > 21)
                await DisplayNotification(notifications, Constants.WIN_NOTIF);
            // Push.
            else if (dealerHand.HandValue == playerHand.HandValue)
                await DisplayNotification(notifications, Constants.PUSH_NOTIF);
            // Player win.
            else if (dealerHand.HandValue < playerHand.HandValue)
                await DisplayNotification(notifications, Constants.WIN_NOTIF);
            // Player lose.
            else
                await DisplayNotification(notifications, Constants.LOST_NOTIF);

            AllowReplay(buttons);
        }

        // Takes a card.
        public async Task Hit(WrapPanel[] views, TextBlock[] totals, StackPanel[] notifications, Button[] buttons)
        {
            if (playDeck.TotalCards == 0)
                RecombineDecks();

            // Draw the card.
            DrawCard(playerHand, views[Constants.PLAYER], false);
            totals[Constants.PLAYER].Text = playerHand.HandValue.ToString();

            // Player busted.
            if (playerHand.HandValue > 21)
            {
                buttons[Constants.HIT_BUTTON].IsEnabled = false;
                buttons[Constants.STAND_BUTTON].IsEnabled = false;

                await DisplayNotification(notifications, Constants.BUST_NOTIF);

                RevealCard(views[Constants.DEALER], totals[Constants.DEALER]);

                AllowReplay(buttons);
            }
        }

        // Takes a card from the play deck and puts it on the screen.
        private void DrawCard(Hand hand, WrapPanel panel, Boolean hideCard)
        {
            // Draw a card from the play deck.
            hand.Cards.AddLast(playDeck.Cards[playDeck.TotalCards - 1]);
            playDeck.Cards[playDeck.TotalCards - 1] = null;
            playDeck.TotalCards--;

            CalculateHandValue(hand);

            Image img = new Image();

            if (hideCard)
                img.Source = new BitmapImage(new Uri("/Resources/card53.png", UriKind.Relative));
            else
                img.Source = new BitmapImage(new Uri(hand.Cards.Last.Value.ImageSource, UriKind.Relative));

            img.Height = 194;
            img.Margin = new Thickness(5);
            panel.Children.Add(img);
        }

        // Does the initial draw of the game.
        public async Task Play(WrapPanel[] views, TextBlock[] totals, StackPanel[] notifications, Button[] buttons)
        {
            // Clean up last round.
            if (playerHand.Cards.Count > 0 || dealerHand.Cards.Count > 0)
            {
                DiscardHands();
                totals[Constants.DEALER].Text = "0";
                totals[Constants.PLAYER].Text = "0";
                views[Constants.DEALER].Children.RemoveRange(0, views[Constants.DEALER].Children.Count);
                views[Constants.PLAYER].Children.RemoveRange(0, views[Constants.PLAYER].Children.Count);
            }

            // Draw cards.
            for (int i = 0; i < Constants.AMOUNT_OF_STARTING_CARDS; i++)
            {
                await Task.Delay(Constants.DRAW_DELAY);

                if (playDeck.TotalCards == 0)
                    RecombineDecks();

                // Last draw. Hides last dealer card.
                if (i == Constants.AMOUNT_OF_STARTING_CARDS - 1)
                {
                    DrawCard(dealerHand, views[Constants.DEALER], true);
                    continue;
                }

                // Flop between drawing a player and dealer card.
                if (i % 2 == 0)
                {
                    DrawCard(playerHand, views[Constants.PLAYER], false);
                    totals[Constants.PLAYER].Text = playerHand.HandValue.ToString();
                }
                else
                {
                    DrawCard(dealerHand, views[Constants.DEALER], false);
                    totals[Constants.DEALER].Text = dealerHand.HandValue.ToString();
                }
            }

            // Handle blackjacks.
            if (dealerHand.HandValue == 21 || playerHand.HandValue == 21)
            {
                // Both dealer and player have blackjacks, push.
                if (dealerHand.HandValue == 21 && playerHand.HandValue == 21)
                {
                    await Task.Delay(Constants.TIMEOUT_DELAY);
                    RevealCard(views[Constants.DEALER], totals[Constants.DEALER]);
                    await DisplayNotification(notifications, Constants.PUSH_NOTIF);
                }
                // Dealer blackjack, player loses.
                else if (dealerHand.HandValue == 21 && playerHand.HandValue != 21)
                {
                    await Task.Delay(Constants.TIMEOUT_DELAY);
                    RevealCard(views[Constants.DEALER], totals[Constants.DEALER]);
                    await DisplayNotification(notifications, Constants.DEALER_BLACKJACK_NOTIF);
                }
                // Player blackjack, player wins.
                else if (dealerHand.HandValue != 21 && playerHand.HandValue == 21)
                {
                    await DisplayNotification(notifications, Constants.BLACKJACK_NOTIF);
                    RevealCard(views[Constants.DEALER], totals[Constants.DEALER]);
                }

                AllowReplay(buttons);
            }
            // Continue to hit or stand phase.
            else
            {
                buttons[Constants.HIT_BUTTON].IsEnabled = true;
                buttons[Constants.STAND_BUTTON].IsEnabled = true;
            }
        }

        // Discard the player and dealer hands into the used deck.
        private void DiscardHands()
        {
            // Discard player hand.
            while (playerHand.Cards.Count > 0)
            {
                usedDeck.Cards[usedDeck.TotalCards] = playerHand.Cards.Last.Value;
                usedDeck.TotalCards++;
                playerHand.Cards.RemoveLast();
            }

            // Discard dealer hand.
            while (dealerHand.Cards.Count > 0)
            {
                usedDeck.Cards[usedDeck.TotalCards] = dealerHand.Cards.Last.Value;
                usedDeck.TotalCards++;
                dealerHand.Cards.RemoveLast();
            }
        }

        // Flips over the dealers hidden card.
        private void RevealCard(WrapPanel dealerPanel, TextBlock delarTotal)
        {
            // Remove hidden card.
            dealerPanel.Children.RemoveAt(dealerPanel.Children.Count - 1);

            // Add non-hidden card.
            Image img = new Image();
            img.Source = new BitmapImage(new Uri(dealerHand.Cards.Last.Value.ImageSource, UriKind.Relative));
            img.Height = 194;
            img.Margin = new Thickness(5);
            dealerPanel.Children.Add(img);

            // Update total.
            delarTotal.Text = dealerHand.HandValue.ToString();
        }

        // Flash a specific notification onto the screen.
        private async Task DisplayNotification(StackPanel[] notifications, int n)
        {
            notifications[n].Visibility = Visibility.Visible;
            await Task.Delay(Constants.POPUP_DELAY);
            notifications[n].Visibility = Visibility.Hidden;
        }

        // Recombines the used deck into the play deck and then reshuffles.
        private void RecombineDecks()
        {
            while (usedDeck.TotalCards > 0)
            {
                playDeck.Cards[playDeck.TotalCards] = usedDeck.Cards[usedDeck.TotalCards - 1];
                usedDeck.Cards[usedDeck.TotalCards - 1] = null;
                usedDeck.TotalCards--;
                playDeck.TotalCards++;
            }

            ShuffleDeck(playDeck);
        }

        // Allow the game to be played again.
        private void AllowReplay(Button[] buttons)
        {
            buttons[Constants.PLAY_BUTTON].Content = "Again?";
            buttons[Constants.PLAY_BUTTON].IsEnabled = true;
        }

        // Calculates the value of all the cards in a hand. Stores the value in an internal varible in the hands class.
        private void CalculateHandValue(Hand hand)
        {
            int numOfAces = 0;
            int value = 0;

            // Loop through each card adding their value.
            // Aces are an exception because they count as 11 unless there is wiggle room to move their value downwards.
            foreach (Card card in hand.Cards)
            {
                if (card.CardValue == 11)
                    numOfAces++;

                value += card.CardValue;

                // Move aces value downward if their default value goes above 21.
                if (value > 21 && numOfAces > 0)
                {
                    numOfAces--;
                    value -= 10;
                }
            }

            hand.HandValue = value;
        }

        // Create and set up both the play deck and used deck.
        private void CreateDecks()
        {
            // Set up play deck.
            playDeck = new Deck(deckCount, overallCardCount);
            for (int i = 0; i < overallCardCount; i++)
            {
                playDeck.Cards[i] = new Card();
                playDeck.Cards[i].CardValue = Constants.CARD_VALUES[i % 13];
                playDeck.Cards[i].ImageSource = "/Resources/card" + ((i % 52) + 1) + ".png";
            }

            // Set up used deck.
            usedDeck = new Deck(deckCount, 0);

            ShuffleDeck(playDeck);
        }

        // Fisher-yates shuffle algorithm adjusted for deck shuffling.
        private void ShuffleDeck(Deck deck)
        {
            Card[] cards = deck.Cards;
            int n = deck.TotalCards;

            for (int i = 0; i < (n - 1); i++)
            {
                int r = i + random.Next(n - i);
                Card temp = cards[r];
                cards[r] = cards[i];
                cards[i] = temp;
            }
        }
    }
}
