using System;
using UnityEngine;

namespace Blackjack
{
    public class BettingSystem : MonoBehaviour
    {
        [Header("Starting Values")]
        [SerializeField] private int startingChips = 1000;
        [SerializeField] private int minimumBet = 5;
        [SerializeField] private int maximumBet = 500;

        [Header("Chip Denominations")]
        [SerializeField] private int[] chipValues = { 5, 10, 25, 50, 100 };

        private int _playerChips;
        private int _currentBet;
        private int _insuranceBet;

        public event Action<int> OnChipsChanged;
        public event Action<int> OnBetChanged;
        public event Action<int, string> OnPayout; // amount, reason

        /// <summary>
        /// Player's current chip count
        /// </summary>
        public int PlayerChips => _playerChips;

        /// <summary>
        /// Current bet amount
        /// </summary>
        public int CurrentBet => _currentBet;

        /// <summary>
        /// Insurance bet amount (if any)
        /// </summary>
        public int InsuranceBet => _insuranceBet;

        /// <summary>
        /// Available chip denominations
        /// </summary>
        public int[] ChipValues => chipValues;

        /// <summary>
        /// Minimum allowed bet
        /// </summary>
        public int MinimumBet => minimumBet;

        /// <summary>
        /// Maximum allowed bet
        /// </summary>
        public int MaximumBet => maximumBet;

        /// <summary>
        /// Whether the player can afford to bet
        /// </summary>
        public bool CanBet => _playerChips >= minimumBet;

        /// <summary>
        /// Whether the player can double their current bet
        /// </summary>
        public bool CanDoubleDown => _playerChips >= _currentBet;

        /// <summary>
        /// Whether the player can afford insurance (half the bet)
        /// </summary>
        public bool CanAffordInsurance => _playerChips >= _currentBet / 2;

        private void Awake()
        {
            ResetChips();
        }

        /// <summary>
        /// Reset chips to starting amount
        /// </summary>
        public void ResetChips()
        {
            _playerChips = startingChips;
            _currentBet = 0;
            _insuranceBet = 0;
            OnChipsChanged?.Invoke(_playerChips);
            OnBetChanged?.Invoke(_currentBet);
        }

        /// <summary>
        /// Add chips to the bet
        /// </summary>
        public bool AddToBet(int amount)
        {
            if (amount <= 0) return false;
            if (_currentBet + amount > maximumBet) return false;
            if (_playerChips < amount) return false;

            _playerChips -= amount;
            _currentBet += amount;

            OnChipsChanged?.Invoke(_playerChips);
            OnBetChanged?.Invoke(_currentBet);

            Debug.Log($"[BettingSystem] Added {amount} to bet. Bet: {_currentBet}, Chips: {_playerChips}");
            return true;
        }

        /// <summary>
        /// Remove chips from the bet
        /// </summary>
        public bool RemoveFromBet(int amount)
        {
            if (amount <= 0) return false;
            if (_currentBet < amount) return false;

            _currentBet -= amount;
            _playerChips += amount;

            OnChipsChanged?.Invoke(_playerChips);
            OnBetChanged?.Invoke(_currentBet);

            Debug.Log($"[BettingSystem] Removed {amount} from bet. Bet: {_currentBet}, Chips: {_playerChips}");
            return true;
        }

        /// <summary>
        /// Clear the current bet (return to chips)
        /// </summary>
        public void ClearBet()
        {
            if (_currentBet > 0)
            {
                _playerChips += _currentBet;
                _currentBet = 0;
                OnChipsChanged?.Invoke(_playerChips);
                OnBetChanged?.Invoke(_currentBet);
            }
        }

        /// <summary>
        /// Confirm the bet (move to pot, cannot be undone)
        /// </summary>
        public bool ConfirmBet()
        {
            if (_currentBet < minimumBet)
            {
                Debug.LogWarning($"[BettingSystem] Bet {_currentBet} is below minimum {minimumBet}");
                return false;
            }

            Debug.Log($"[BettingSystem] Bet confirmed: {_currentBet}");
            return true;
        }

        /// <summary>
        /// Double the current bet (for double down)
        /// </summary>
        public bool DoubleBet()
        {
            if (!CanDoubleDown)
            {
                Debug.LogWarning("[BettingSystem] Cannot afford to double down");
                return false;
            }

            _playerChips -= _currentBet;
            _currentBet *= 2;

            OnChipsChanged?.Invoke(_playerChips);
            OnBetChanged?.Invoke(_currentBet);

            Debug.Log($"[BettingSystem] Doubled bet to {_currentBet}");
            return true;
        }

        /// <summary>
        /// Place insurance bet (half of current bet)
        /// </summary>
        public bool PlaceInsurance()
        {
            int insuranceAmount = _currentBet / 2;

            if (_playerChips < insuranceAmount)
            {
                Debug.LogWarning("[BettingSystem] Cannot afford insurance");
                return false;
            }

            _playerChips -= insuranceAmount;
            _insuranceBet = insuranceAmount;

            OnChipsChanged?.Invoke(_playerChips);

            Debug.Log($"[BettingSystem] Insurance placed: {_insuranceBet}");
            return true;
        }

        /// <summary>
        /// Process payout for a win
        /// </summary>
        public void ProcessWin(float multiplier, string reason = "Win")
        {
            int payout = Mathf.RoundToInt(_currentBet * multiplier);
            int total = _currentBet + payout; // Return bet + winnings

            _playerChips += total;
            OnChipsChanged?.Invoke(_playerChips);
            OnPayout?.Invoke(total, reason);

            Debug.Log($"[BettingSystem] Win! Payout: {total} ({reason}). Chips: {_playerChips}");
            _currentBet = 0;
            OnBetChanged?.Invoke(_currentBet);
        }

        /// <summary>
        /// Process loss (bet is already deducted)
        /// </summary>
        public void ProcessLoss(string reason = "Loss")
        {
            OnPayout?.Invoke(-_currentBet, reason);
            Debug.Log($"[BettingSystem] Loss: -{_currentBet} ({reason}). Chips: {_playerChips}");

            _currentBet = 0;
            OnBetChanged?.Invoke(_currentBet);
        }

        /// <summary>
        /// Process push (return bet)
        /// </summary>
        public void ProcessPush()
        {
            _playerChips += _currentBet;
            OnChipsChanged?.Invoke(_playerChips);
            OnPayout?.Invoke(0, "Push");

            Debug.Log($"[BettingSystem] Push. Bet returned. Chips: {_playerChips}");

            _currentBet = 0;
            OnBetChanged?.Invoke(_currentBet);
        }

        /// <summary>
        /// Process surrender (return half bet)
        /// </summary>
        public void ProcessSurrender()
        {
            int halfBet = _currentBet / 2;
            _playerChips += halfBet;

            OnChipsChanged?.Invoke(_playerChips);
            OnPayout?.Invoke(-halfBet, "Surrender");

            Debug.Log($"[BettingSystem] Surrender. Half bet returned: {halfBet}. Chips: {_playerChips}");

            _currentBet = 0;
            OnBetChanged?.Invoke(_currentBet);
        }

        /// <summary>
        /// Process insurance payout
        /// </summary>
        public void ProcessInsurance(bool dealerHasBlackjack)
        {
            if (_insuranceBet <= 0) return;

            if (dealerHasBlackjack)
            {
                int payout = _insuranceBet * 3; // Return insurance + 2:1 winnings
                _playerChips += payout;
                OnPayout?.Invoke(payout, "Insurance Win");
                Debug.Log($"[BettingSystem] Insurance pays! +{payout}");
            }
            else
            {
                OnPayout?.Invoke(-_insuranceBet, "Insurance Loss");
                Debug.Log($"[BettingSystem] Insurance lost: -{_insuranceBet}");
            }

            OnChipsChanged?.Invoke(_playerChips);
            _insuranceBet = 0;
        }

        /// <summary>
        /// Add chips (for testing or bonuses)
        /// </summary>
        public void AddChips(int amount)
        {
            _playerChips += amount;
            OnChipsChanged?.Invoke(_playerChips);
        }
    }
}
