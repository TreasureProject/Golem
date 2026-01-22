using System;
using UnityEngine;

namespace Blackjack
{
    [Serializable]
    public class Card
    {
        public enum Suit { Hearts, Diamonds, Clubs, Spades }
        public enum Rank {
            Ace = 1, Two = 2, Three = 3, Four = 4, Five = 5,
            Six = 6, Seven = 7, Eight = 8, Nine = 9, Ten = 10,
            Jack = 11, Queen = 12, King = 13
        }

        [SerializeField] private Suit _suit;
        [SerializeField] private Rank _rank;

        public Suit CardSuit => _suit;
        public Rank CardRank => _rank;

        /// <summary>
        /// The blackjack value of this card (Ace = 1 or 11, Face cards = 10)
        /// </summary>
        public int Value
        {
            get
            {
                if (_rank >= Rank.Ten) return 10;
                return (int)_rank;
            }
        }

        /// <summary>
        /// Whether this card is an Ace (can be 1 or 11)
        /// </summary>
        public bool IsAce => _rank == Rank.Ace;

        /// <summary>
        /// Whether this card is a face card (Jack, Queen, King)
        /// </summary>
        public bool IsFaceCard => _rank >= Rank.Jack;

        /// <summary>
        /// Whether this card is a 10-value card (10, J, Q, K)
        /// </summary>
        public bool IsTenValue => Value == 10;

        public Card(Suit suit, Rank rank)
        {
            _suit = suit;
            _rank = rank;
        }

        /// <summary>
        /// Get sprite/asset name for this card (e.g., "Hearts_Ace", "Spades_King")
        /// </summary>
        public string AssetName => $"{_suit}_{_rank}";

        /// <summary>
        /// Short display string (e.g., "A♥", "K♠", "10♦")
        /// </summary>
        public string ShortName
        {
            get
            {
                string rankStr = _rank switch
                {
                    Rank.Ace => "A",
                    Rank.Jack => "J",
                    Rank.Queen => "Q",
                    Rank.King => "K",
                    _ => ((int)_rank).ToString()
                };

                string suitStr = _suit switch
                {
                    Suit.Hearts => "♥",
                    Suit.Diamonds => "♦",
                    Suit.Clubs => "♣",
                    Suit.Spades => "♠",
                    _ => "?"
                };

                return $"{rankStr}{suitStr}";
            }
        }

        public override string ToString() => ShortName;

        /// <summary>
        /// Serialize card to dictionary for AI communication
        /// </summary>
        public System.Collections.Generic.Dictionary<string, string> ToDict()
        {
            return new System.Collections.Generic.Dictionary<string, string>
            {
                { "rank", _rank.ToString() },
                { "suit", _suit.ToString() }
            };
        }
    }
}
