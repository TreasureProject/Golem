using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Blackjack
{
    [Serializable]
    public class Hand
    {
        [SerializeField] private List<Card> _cards = new List<Card>();

        public IReadOnlyList<Card> Cards => _cards.AsReadOnly();
        public int CardCount => _cards.Count;

        /// <summary>
        /// Calculate the best score for this hand (handles Aces as 1 or 11)
        /// </summary>
        public int Score
        {
            get
            {
                int score = 0;
                int aces = 0;

                foreach (var card in _cards)
                {
                    if (card.IsAce)
                    {
                        aces++;
                        score += 11; // Initially count Ace as 11
                    }
                    else
                    {
                        score += card.Value;
                    }
                }

                // Convert Aces from 11 to 1 if busting
                while (score > 21 && aces > 0)
                {
                    score -= 10;
                    aces--;
                }

                return score;
            }
        }

        /// <summary>
        /// Whether this hand is "soft" (has an Ace counted as 11)
        /// </summary>
        public bool IsSoft
        {
            get
            {
                if (!_cards.Any(c => c.IsAce)) return false;

                // Calculate score counting all Aces as 1
                int hardScore = _cards.Sum(c => c.IsAce ? 1 : c.Value);

                // If adding 10 (Ace as 11 instead of 1) doesn't bust, it's soft
                return hardScore + 10 <= 21;
            }
        }

        /// <summary>
        /// Whether this hand has busted (score > 21)
        /// </summary>
        public bool IsBusted => Score > 21;

        /// <summary>
        /// Whether this is a natural blackjack (2 cards totaling 21)
        /// </summary>
        public bool IsBlackjack => _cards.Count == 2 && Score == 21;

        /// <summary>
        /// Whether this hand can be split (2 cards of same value)
        /// </summary>
        public bool CanSplit => _cards.Count == 2 && _cards[0].Value == _cards[1].Value;

        /// <summary>
        /// Whether this hand can double down (typically first 2 cards only)
        /// </summary>
        public bool CanDoubleDown => _cards.Count == 2;

        /// <summary>
        /// Add a card to the hand
        /// </summary>
        public void AddCard(Card card)
        {
            if (card == null) throw new ArgumentNullException(nameof(card));
            _cards.Add(card);
        }

        /// <summary>
        /// Remove and return a card at the specified index (for splitting)
        /// </summary>
        public Card RemoveCardAt(int index)
        {
            if (index < 0 || index >= _cards.Count)
                throw new ArgumentOutOfRangeException(nameof(index));

            var card = _cards[index];
            _cards.RemoveAt(index);
            return card;
        }

        /// <summary>
        /// Clear all cards from the hand
        /// </summary>
        public void Clear()
        {
            _cards.Clear();
        }

        /// <summary>
        /// Get display string for the hand
        /// </summary>
        public override string ToString()
        {
            if (_cards.Count == 0) return "Empty";
            return string.Join(" ", _cards.Select(c => c.ShortName)) + $" ({Score})";
        }

        /// <summary>
        /// Get score display string (e.g., "17", "Soft 17", "Blackjack!", "BUST")
        /// </summary>
        public string ScoreDisplay
        {
            get
            {
                if (IsBusted) return "BUST";
                if (IsBlackjack) return "Blackjack!";
                if (IsSoft) return $"Soft {Score}";
                return Score.ToString();
            }
        }

        /// <summary>
        /// Serialize hand to list of card dictionaries for AI communication
        /// </summary>
        public List<Dictionary<string, string>> ToCardList()
        {
            return _cards.Select(c => c.ToDict()).ToList();
        }

        /// <summary>
        /// Create a hidden representation (for dealer's hole card)
        /// </summary>
        public List<Dictionary<string, string>> ToCardListWithHidden(int hiddenIndex = 1)
        {
            var result = new List<Dictionary<string, string>>();
            for (int i = 0; i < _cards.Count; i++)
            {
                if (i == hiddenIndex)
                {
                    result.Add(new Dictionary<string, string>
                    {
                        { "rank", "hidden" },
                        { "suit", "hidden" }
                    });
                }
                else
                {
                    result.Add(_cards[i].ToDict());
                }
            }
            return result;
        }
    }
}
