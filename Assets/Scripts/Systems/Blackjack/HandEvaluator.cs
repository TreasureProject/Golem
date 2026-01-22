using UnityEngine;

namespace Blackjack
{
    /// <summary>
    /// Result of comparing two hands
    /// </summary>
    public enum HandResult
    {
        PlayerWins,
        PlayerBlackjack,  // Natural 21, pays 3:2
        CelesteWins,
        Push,             // Tie
        PlayerBust,
        CelesteBust
    }

    public class HandEvaluator : MonoBehaviour
    {
        /// <summary>
        /// Compare player hand against Celeste's hand and determine the result
        /// </summary>
        public HandResult EvaluateHands(Hand playerHand, Hand celesteHand)
        {
            // Check for busts first
            if (playerHand.IsBusted)
            {
                return HandResult.PlayerBust;
            }

            if (celesteHand.IsBusted)
            {
                return HandResult.CelesteBust;
            }

            // Check for blackjacks
            bool playerBlackjack = playerHand.IsBlackjack;
            bool celesteBlackjack = celesteHand.IsBlackjack;

            if (playerBlackjack && celesteBlackjack)
            {
                return HandResult.Push; // Both have blackjack = push
            }

            if (playerBlackjack)
            {
                return HandResult.PlayerBlackjack; // Player natural 21
            }

            if (celesteBlackjack)
            {
                return HandResult.CelesteWins; // Celeste natural 21
            }

            // Compare scores
            int playerScore = playerHand.Score;
            int celesteScore = celesteHand.Score;

            if (playerScore > celesteScore)
            {
                return HandResult.PlayerWins;
            }
            else if (celesteScore > playerScore)
            {
                return HandResult.CelesteWins;
            }
            else
            {
                return HandResult.Push;
            }
        }

        /// <summary>
        /// Get the payout multiplier for a result
        /// </summary>
        public float GetPayoutMultiplier(HandResult result)
        {
            return result switch
            {
                HandResult.PlayerWins => 1.0f,       // 1:1
                HandResult.PlayerBlackjack => 1.5f, // 3:2
                HandResult.CelesteBust => 1.0f,     // 1:1
                HandResult.Push => 0f,               // Bet returned
                HandResult.CelesteWins => -1.0f,    // Lose bet
                HandResult.PlayerBust => -1.0f,     // Lose bet
                _ => 0f
            };
        }

        /// <summary>
        /// Get display message for a result
        /// </summary>
        public string GetResultMessage(HandResult result)
        {
            return result switch
            {
                HandResult.PlayerWins => "You Win!",
                HandResult.PlayerBlackjack => "Blackjack! You Win!",
                HandResult.CelesteWins => "Celeste Wins",
                HandResult.CelesteBust => "Celeste Busts! You Win!",
                HandResult.Push => "Push - It's a Tie",
                HandResult.PlayerBust => "Bust! You Lose",
                _ => "Unknown Result"
            };
        }

        /// <summary>
        /// Check if the result is a win for the player
        /// </summary>
        public bool IsPlayerWin(HandResult result)
        {
            return result == HandResult.PlayerWins ||
                   result == HandResult.PlayerBlackjack ||
                   result == HandResult.CelesteBust;
        }

        /// <summary>
        /// Check if the result is a loss for the player
        /// </summary>
        public bool IsPlayerLoss(HandResult result)
        {
            return result == HandResult.CelesteWins ||
                   result == HandResult.PlayerBust;
        }

        /// <summary>
        /// Calculate insurance payout (2:1 if dealer has blackjack)
        /// </summary>
        public float CalculateInsurancePayout(Hand celesteHand, float insuranceBet)
        {
            if (celesteHand.IsBlackjack)
            {
                return insuranceBet * 2f; // Insurance pays 2:1
            }
            return -insuranceBet; // Lose insurance bet
        }
    }
}
