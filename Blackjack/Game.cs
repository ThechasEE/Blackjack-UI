using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Blackjack
{
    // All constants for the project.
    public static class Constants
    {
        public const int CARDS_IN_A_DECK = 52;
        public const int AMOUNT_OF_STARTING_CARDS = 4;
        public const int DRAW_DELAY = 1000;
    }

    // Card structure.
    public partial class Card
    {
        private int cardNumber;
        private string imageSource;

        public int CardNumber { get => cardNumber; set => cardNumber = value; }
        public string ImageSource { get => imageSource; set => imageSource = value; }
    }

    // Deck structure.
    public partial class Deck
    {
        private Card[] cards;
        private int totalCards;

        public Deck(int deckCount, int startingAmount)
        {
            // Allocate enough memory for all cards that can be in the deck.
            cards = new Card[deckCount * Constants.CARDS_IN_A_DECK];
            // Hold a count of the total number of cards in the deck currently.
            totalCards = startingAmount;
        }

        public Card[] Cards { get => cards; set => cards = value; }
        public int TotalCards { get => totalCards; set => totalCards = value; }
    }

    // Hand structure.
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
        private Random random = new Random();

        // Freestyle constructor.
        public Game(int deckCount)
        {
            // Store deck count and overall card count.
            this.deckCount = deckCount;
            overallCardCount = deckCount * Constants.CARDS_IN_A_DECK;

            // Initialized to null as there is no money factor in freestyle.
            startMoney = null;
            betMin = null;
            winMoney = null;

            BlackjackSetup();
        }

        // Challenge constructor.
        public Game(int deckCount, int startMoney, int betMin, int winMoney)
        {
            // Store deck count and overall card count.
            this.deckCount = deckCount;
            overallCardCount = deckCount * Constants.CARDS_IN_A_DECK;

            // Set challenge values.
            this.startMoney = startMoney;
            this.betMin = betMin;
            this.winMoney = winMoney;

            BlackjackSetup();
        }

        // Set up the decks and hands.
        private void BlackjackSetup()
        {
            CreateDecks();

            dealerHand = new Hand();
            playerHand = new Hand();
        }

        private void DrawCard(Hand hand, WrapPanel panel, TextBlock total)
        {
            hand.Cards.AddLast(playDeck.Cards[playDeck.TotalCards - 1]);
            playDeck.Cards[playDeck.TotalCards - 1] = null;
            playDeck.TotalCards--;

            Image img = new Image();
            img.Source = new BitmapImage(new Uri(hand.Cards.Last.Value.ImageSource, UriKind.Relative));
            img.Height = 194;
            panel.Children.Add(img);
        }

        // Does the initial draw of the game.
        public async Task DrawInitialCards(WrapPanel dealerView, WrapPanel playerView, TextBlock dealerTotal, TextBlock playerTotal)
        {
            // Flip-flop drawing cards between the player and dealer.
            for (int i = 0; i < Constants.AMOUNT_OF_STARTING_CARDS; i++)
            {
                await Task.Delay(Constants.DRAW_DELAY);

                // Recombine decks if needed.
                if (playDeck.Cards.Length == 0)
                    // Recombine decks.

                Console.WriteLine(i % 2);

                if (i % 2 == 0)
                    DrawCard(playerHand, playerView, playerTotal);
                else
                    DrawCard(dealerHand, dealerView, dealerTotal);
            }
        }

        // Create and set up both the play deck and used deck.
        private void CreateDecks()
        {
            // Set up play deck.
            playDeck = new Deck(deckCount, overallCardCount);
            for (int i = 0; i < overallCardCount; i++)
            {
                playDeck.Cards[i] = new Card();
                playDeck.Cards[i].CardNumber = i;
                playDeck.Cards[i].ImageSource = "/Resources/card" + ((i % 52) + 1) + ".png";
            }

            // Set up used deck.
            usedDeck = new Deck(deckCount, 0);

            // Shuffle play deck.
            ShuffleDeck(playDeck);
        }

        // Fisher-yates shuffle algorithm adjusted for deck shuffling.
        private void ShuffleDeck(Deck deck)
        {
            Card[] cards = deck.Cards;
            int n = cards.Length;

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
