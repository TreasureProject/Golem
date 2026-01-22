using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Blackjack
{
    public class DeckManager : MonoBehaviour
    {
        [Header("Deck Configuration")]
        [Tooltip("Number of decks in the shoe (casino standard is 6-8)")]
        [SerializeField] private int numberOfDecks = 6;

        [Tooltip("Reshuffle when this percentage of cards remain")]
        [SerializeField] [Range(0.1f, 0.5f)] private float reshuffleThreshold = 0.25f;

        [Header("Visual References")]
        [Tooltip("Position where the deck sits on the table")]
        [SerializeField] private Transform deckPosition;

        private List<Card> _shoe = new List<Card>();
        private List<Card> _dealtCards = new List<Card>();

        public event Action OnShuffled;
        public event Action<Card> OnCardDealt;

        /// <summary>
        /// Number of cards remaining in the shoe
        /// </summary>
        public int CardsRemaining => _shoe.Count;

        /// <summary>
        /// Total cards in a fresh shoe
        /// </summary>
        public int TotalCards => numberOfDecks * 52;

        /// <summary>
        /// Percentage of deck that has been dealt (0.0 to 1.0)
        /// </summary>
        public float DeckPenetration => 1f - (float)CardsRemaining / TotalCards;

        /// <summary>
        /// Whether the shoe should be reshuffled
        /// </summary>
        public bool ShouldReshuffle => (float)CardsRemaining / TotalCards <= reshuffleThreshold;

        /// <summary>
        /// Position of the deck on the table
        /// </summary>
        public Transform DeckPosition => deckPosition;

        private void Awake()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize and shuffle a fresh shoe
        /// </summary>
        public void Initialize()
        {
            CreateShoe();
            Shuffle();
        }

        /// <summary>
        /// Create all cards for the shoe (multiple decks)
        /// </summary>
        private void CreateShoe()
        {
            _shoe.Clear();
            _dealtCards.Clear();

            for (int deck = 0; deck < numberOfDecks; deck++)
            {
                foreach (Card.Suit suit in Enum.GetValues(typeof(Card.Suit)))
                {
                    foreach (Card.Rank rank in Enum.GetValues(typeof(Card.Rank)))
                    {
                        _shoe.Add(new Card(suit, rank));
                    }
                }
            }

            Debug.Log($"[DeckManager] Created shoe with {_shoe.Count} cards ({numberOfDecks} decks)");
        }

        /// <summary>
        /// Shuffle the shoe using Fisher-Yates algorithm
        /// </summary>
        public void Shuffle()
        {
            // Return all dealt cards to the shoe
            _shoe.AddRange(_dealtCards);
            _dealtCards.Clear();

            // Fisher-Yates shuffle
            for (int i = _shoe.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (_shoe[i], _shoe[j]) = (_shoe[j], _shoe[i]);
            }

            Debug.Log($"[DeckManager] Shuffled {_shoe.Count} cards");
            OnShuffled?.Invoke();
        }

        /// <summary>
        /// Deal one card from the shoe
        /// </summary>
        public Card DealCard()
        {
            if (_shoe.Count == 0)
            {
                Debug.LogWarning("[DeckManager] Shoe is empty! Reshuffling...");
                Shuffle();
            }

            var card = _shoe[_shoe.Count - 1];
            _shoe.RemoveAt(_shoe.Count - 1);
            _dealtCards.Add(card);

            Debug.Log($"[DeckManager] Dealt {card.ShortName} ({CardsRemaining} remaining)");
            OnCardDealt?.Invoke(card);

            return card;
        }

        /// <summary>
        /// Deal multiple cards
        /// </summary>
        public List<Card> DealCards(int count)
        {
            var cards = new List<Card>();
            for (int i = 0; i < count; i++)
            {
                cards.Add(DealCard());
            }
            return cards;
        }

        /// <summary>
        /// Peek at the top card without removing it (for testing)
        /// </summary>
        public Card PeekTopCard()
        {
            if (_shoe.Count == 0) return null;
            return _shoe[_shoe.Count - 1];
        }

        /// <summary>
        /// Get deck status for debugging
        /// </summary>
        public string GetStatus()
        {
            return $"Shoe: {CardsRemaining}/{TotalCards} cards ({DeckPenetration:P0} dealt)";
        }

#if UNITY_EDITOR
        [ContextMenu("Debug: Print Deck Status")]
        private void DebugPrintStatus()
        {
            Debug.Log(GetStatus());
        }

        [ContextMenu("Debug: Shuffle Now")]
        private void DebugShuffle()
        {
            Shuffle();
        }
#endif
    }
}
