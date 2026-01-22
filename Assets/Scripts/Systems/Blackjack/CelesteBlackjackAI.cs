using UnityEngine;

namespace Blackjack
{
    /// <summary>
    /// Celeste's blackjack AI using basic strategy.
    /// In the current implementation, Celeste follows standard dealer rules (hit until 17).
    /// This class provides the framework for more sophisticated AI behavior.
    /// </summary>
    public class CelesteBlackjackAI : MonoBehaviour
    {
        public enum Decision { Hit, Stand, DoubleDown, Split, Surrender }

        /// <summary>
        /// Get Celeste's decision based on her hand and the visible dealer card.
        /// For now, Celeste plays as a standard dealer (hit until 17).
        /// </summary>
        public Decision GetDecision(Hand celesteHand, Card dealerUpCard)
        {
            int score = celesteHand.Score;
            bool isSoft = celesteHand.IsSoft;

            // Standard dealer rules: hit on 16 or less, stand on 17+
            // Some casinos hit soft 17, but we'll stand on all 17s
            if (score >= 17)
            {
                return Decision.Stand;
            }

            return Decision.Hit;
        }

        /// <summary>
        /// Get decision using basic strategy (for a more competitive AI).
        /// This follows mathematically optimal play.
        /// </summary>
        public Decision GetBasicStrategyDecision(Hand hand, Card dealerUpCard)
        {
            int score = hand.Score;
            int dealerValue = dealerUpCard.Value;
            bool isSoft = hand.IsSoft;
            bool canSplit = hand.CanSplit;

            // Handle pairs (splitting)
            if (canSplit)
            {
                var splitDecision = EvaluateSplit(hand.Cards[0].Value, dealerValue);
                if (splitDecision == Decision.Split)
                    return Decision.Split;
            }

            // Handle soft hands (Ace counted as 11)
            if (isSoft)
            {
                return EvaluateSoftHand(score, dealerValue);
            }

            // Handle hard hands
            return EvaluateHardHand(score, dealerValue);
        }

        private Decision EvaluateHardHand(int score, int dealerValue)
        {
            // Hard 17+: Always stand
            if (score >= 17)
                return Decision.Stand;

            // Hard 13-16: Stand vs dealer 2-6, hit vs 7+
            if (score >= 13 && score <= 16)
            {
                return dealerValue >= 2 && dealerValue <= 6 ? Decision.Stand : Decision.Hit;
            }

            // Hard 12: Stand vs dealer 4-6, hit otherwise
            if (score == 12)
            {
                return dealerValue >= 4 && dealerValue <= 6 ? Decision.Stand : Decision.Hit;
            }

            // Hard 11: Always double (or hit if can't double)
            if (score == 11)
            {
                return Decision.DoubleDown;
            }

            // Hard 10: Double vs dealer 2-9, hit vs 10/A
            if (score == 10)
            {
                return dealerValue >= 2 && dealerValue <= 9 ? Decision.DoubleDown : Decision.Hit;
            }

            // Hard 9: Double vs dealer 3-6, hit otherwise
            if (score == 9)
            {
                return dealerValue >= 3 && dealerValue <= 6 ? Decision.DoubleDown : Decision.Hit;
            }

            // Hard 8 or less: Always hit
            return Decision.Hit;
        }

        private Decision EvaluateSoftHand(int score, int dealerValue)
        {
            // Soft 19-21: Stand
            if (score >= 19)
                return Decision.Stand;

            // Soft 18: Stand vs 2,7,8; double vs 3-6; hit vs 9,10,A
            if (score == 18)
            {
                if (dealerValue == 2 || dealerValue == 7 || dealerValue == 8)
                    return Decision.Stand;
                if (dealerValue >= 3 && dealerValue <= 6)
                    return Decision.DoubleDown;
                return Decision.Hit;
            }

            // Soft 17: Double vs 3-6, hit otherwise
            if (score == 17)
            {
                return dealerValue >= 3 && dealerValue <= 6 ? Decision.DoubleDown : Decision.Hit;
            }

            // Soft 15-16: Double vs 4-6, hit otherwise
            if (score == 15 || score == 16)
            {
                return dealerValue >= 4 && dealerValue <= 6 ? Decision.DoubleDown : Decision.Hit;
            }

            // Soft 13-14: Double vs 5-6, hit otherwise
            if (score == 13 || score == 14)
            {
                return dealerValue >= 5 && dealerValue <= 6 ? Decision.DoubleDown : Decision.Hit;
            }

            return Decision.Hit;
        }

        private Decision EvaluateSplit(int pairValue, int dealerValue)
        {
            // Always split Aces and 8s
            if (pairValue == 1 || pairValue == 8) // Ace = 1, 8 = 8
                return Decision.Split;

            // Never split 10s or 5s
            if (pairValue == 10 || pairValue == 5)
                return Decision.Hit; // Don't split, continue to hard hand logic

            // 9s: Split vs 2-9 except 7
            if (pairValue == 9)
            {
                return (dealerValue >= 2 && dealerValue <= 9 && dealerValue != 7) ?
                       Decision.Split : Decision.Stand;
            }

            // 7s: Split vs 2-7
            if (pairValue == 7)
            {
                return dealerValue >= 2 && dealerValue <= 7 ? Decision.Split : Decision.Hit;
            }

            // 6s: Split vs 2-6
            if (pairValue == 6)
            {
                return dealerValue >= 2 && dealerValue <= 6 ? Decision.Split : Decision.Hit;
            }

            // 4s: Split vs 5-6 only
            if (pairValue == 4)
            {
                return dealerValue >= 5 && dealerValue <= 6 ? Decision.Split : Decision.Hit;
            }

            // 3s and 2s: Split vs 2-7
            if (pairValue == 3 || pairValue == 2)
            {
                return dealerValue >= 2 && dealerValue <= 7 ? Decision.Split : Decision.Hit;
            }

            return Decision.Hit;
        }

        /// <summary>
        /// Get a commentary/taunt based on the game situation
        /// </summary>
        public string GetCommentary(Hand playerHand, Hand celesteHand, BlackjackGameManager.GameState state)
        {
            switch (state)
            {
                case BlackjackGameManager.GameState.Dealing:
                    return "Let's see what the cards have in store...";

                case BlackjackGameManager.GameState.PlayerTurn:
                    if (playerHand.Score >= 19)
                        return "Nice hand! Playing it safe?";
                    if (playerHand.Score <= 11)
                        return "Room to grow there...";
                    if (playerHand.Score >= 17)
                        return "Tricky spot. What's it gonna be?";
                    return "The tension builds...";

                case BlackjackGameManager.GameState.CelesteTurn:
                    if (celesteHand.Score >= 20)
                        return "Looking good for me!";
                    if (celesteHand.Score <= 16)
                        return "I need to draw...";
                    return "Let's see how this plays out.";

                case BlackjackGameManager.GameState.Resolution:
                    if (playerHand.IsBusted)
                        return "Ooh, that's rough. Better luck next time!";
                    if (celesteHand.IsBusted)
                        return "Ugh, I went over. You got me this time.";
                    if (playerHand.IsBlackjack)
                        return "Blackjack! Impressive!";
                    return "";

                default:
                    return "";
            }
        }
    }
}
