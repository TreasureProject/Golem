using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Blackjack
{
    public class BlackjackGameManager : MonoBehaviour
    {
        public enum GameState
        {
            Idle,           // Waiting to start
            Betting,        // Player placing bet
            Dealing,        // Cards being dealt
            PlayerTurn,     // Player's turn to act
            InsuranceOffer, // Offering insurance (dealer shows Ace)
            CelesteTurn,    // Celeste's turn
            Resolution,     // Determining winner, payouts
            RoundEnd        // Brief pause before next round
        }

        [Header("References")]
        [SerializeField] private DeckManager deckManager;
        [SerializeField] private BettingSystem bettingSystem;
        [SerializeField] private HandEvaluator handEvaluator;

        [Header("Settings")]
        [SerializeField] private float dealDelay = 0.5f;
        [SerializeField] private float turnDelay = 1.0f;
        [SerializeField] private float resultDelay = 2.0f;

        [Header("Debug")]
        [SerializeField] private GameState _currentState = GameState.Idle;

        // Hands
        private Hand _playerHand = new Hand();
        private Hand _celesteHand = new Hand();
        private List<Hand> _splitHands = new List<Hand>();
        private int _currentSplitHandIndex = 0;

        // Round tracking
        private int _roundNumber = 0;
        private bool _insuranceOffered = false;
        private bool _insuranceTaken = false;
        private bool _surrenderAvailable = true;
        private bool _hasActed = false; // Player has taken at least one action

        // Events
        public event Action<GameState> OnStateChanged;
        public event Action<Card, string> OnCardDealt; // card, recipient ("player" or "celeste")
        public event Action<HandResult> OnRoundResult;
        public event Action<string> OnMessage;
        public event Action OnDealStarted;
        public event Action OnPlayerTurnStarted;
        public event Action OnCelesteTurnStarted;

        // Public accessors
        public GameState CurrentState => _currentState;
        public Hand PlayerHand => _playerHand;
        public Hand CelesteHand => _celesteHand;
        public IReadOnlyList<Hand> SplitHands => _splitHands.AsReadOnly();
        public int RoundNumber => _roundNumber;
        public bool InsuranceAvailable => _insuranceOffered && !_insuranceTaken && !_hasActed;
        public bool SurrenderAvailable => _surrenderAvailable && !_hasActed && _splitHands.Count == 0;
        public bool CanHit => _currentState == GameState.PlayerTurn && !CurrentPlayerHand.IsBusted;
        public bool CanStand => _currentState == GameState.PlayerTurn;
        public bool CanDoubleDown => _currentState == GameState.PlayerTurn &&
                                     CurrentPlayerHand.CanDoubleDown &&
                                     bettingSystem.CanDoubleDown &&
                                     !_hasActed;
        public bool CanSplit => _currentState == GameState.PlayerTurn &&
                                CurrentPlayerHand.CanSplit &&
                                _splitHands.Count < 3 && // Max 4 hands
                                bettingSystem.CanDoubleDown &&
                                !_hasActed;

        /// <summary>
        /// Get the current hand being played (handles splits)
        /// </summary>
        public Hand CurrentPlayerHand
        {
            get
            {
                if (_splitHands.Count > 0 && _currentSplitHandIndex < _splitHands.Count)
                    return _splitHands[_currentSplitHandIndex];
                return _playerHand;
            }
        }

        /// <summary>
        /// Celeste's visible card (first card, hole card hidden)
        /// </summary>
        public Card CelesteUpCard => _celesteHand.CardCount > 0 ? _celesteHand.Cards[0] : null;

        /// <summary>
        /// Celeste's visible score (only showing up card)
        /// </summary>
        public int CelesteVisibleScore => CelesteUpCard?.Value ?? 0;

        private void Awake()
        {
            if (deckManager == null) deckManager = GetComponent<DeckManager>();
            if (bettingSystem == null) bettingSystem = GetComponent<BettingSystem>();
            if (handEvaluator == null) handEvaluator = GetComponent<HandEvaluator>();
        }

        private void Start()
        {
            SetState(GameState.Idle);
        }

        private void SetState(GameState newState)
        {
            if (_currentState == newState) return;

            Debug.Log($"[BlackjackGameManager] State: {_currentState} -> {newState}");
            _currentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        #region Public Actions

        /// <summary>
        /// Start a new round (begin betting phase)
        /// </summary>
        public void StartNewRound()
        {
            if (_currentState != GameState.Idle && _currentState != GameState.RoundEnd)
            {
                Debug.LogWarning("[BlackjackGameManager] Cannot start new round - game in progress");
                return;
            }

            // Check if reshuffle needed
            if (deckManager.ShouldReshuffle)
            {
                deckManager.Shuffle();
                OnMessage?.Invoke("Shuffling the deck...");
            }

            _roundNumber++;
            _playerHand.Clear();
            _celesteHand.Clear();
            _splitHands.Clear();
            _currentSplitHandIndex = 0;
            _insuranceOffered = false;
            _insuranceTaken = false;
            _surrenderAvailable = true;
            _hasActed = false;

            SetState(GameState.Betting);
            OnMessage?.Invoke($"Round {_roundNumber} - Place your bet!");
        }

        /// <summary>
        /// Confirm bet and start dealing
        /// </summary>
        public void ConfirmBetAndDeal()
        {
            if (_currentState != GameState.Betting)
            {
                Debug.LogWarning("[BlackjackGameManager] Cannot deal - not in betting state");
                return;
            }

            if (!bettingSystem.ConfirmBet())
            {
                OnMessage?.Invoke("Please place a valid bet");
                return;
            }

            StartCoroutine(DealInitialCards());
        }

        /// <summary>
        /// Player hits (takes another card)
        /// </summary>
        public void PlayerHit()
        {
            if (!CanHit) return;

            _hasActed = true;
            _surrenderAvailable = false;

            var card = deckManager.DealCard();
            CurrentPlayerHand.AddCard(card);
            OnCardDealt?.Invoke(card, "player");

            Debug.Log($"[BlackjackGameManager] Player hits: {card.ShortName}. Hand: {CurrentPlayerHand}");

            if (CurrentPlayerHand.IsBusted)
            {
                OnMessage?.Invoke("Bust!");
                StartCoroutine(HandlePlayerBust());
            }
            else if (CurrentPlayerHand.Score == 21)
            {
                OnMessage?.Invoke("21!");
                PlayerStand(); // Auto-stand on 21
            }
        }

        /// <summary>
        /// Player stands (ends their turn)
        /// </summary>
        public void PlayerStand()
        {
            if (!CanStand) return;

            _hasActed = true;
            Debug.Log($"[BlackjackGameManager] Player stands with {CurrentPlayerHand.Score}");

            // Check if more split hands to play
            if (_splitHands.Count > 0 && _currentSplitHandIndex < _splitHands.Count - 1)
            {
                _currentSplitHandIndex++;
                _hasActed = false;
                OnMessage?.Invoke($"Playing hand {_currentSplitHandIndex + 1}");
                return;
            }

            // Move to Celeste's turn
            StartCoroutine(PlayCelesteTurn());
        }

        /// <summary>
        /// Player doubles down
        /// </summary>
        public void PlayerDoubleDown()
        {
            if (!CanDoubleDown) return;

            if (!bettingSystem.DoubleBet())
            {
                OnMessage?.Invoke("Cannot afford to double down");
                return;
            }

            _hasActed = true;
            _surrenderAvailable = false;

            var card = deckManager.DealCard();
            CurrentPlayerHand.AddCard(card);
            OnCardDealt?.Invoke(card, "player");

            Debug.Log($"[BlackjackGameManager] Player doubles down: {card.ShortName}. Hand: {CurrentPlayerHand}");
            OnMessage?.Invoke($"Double down! Drew {card.ShortName}");

            if (CurrentPlayerHand.IsBusted)
            {
                StartCoroutine(HandlePlayerBust());
            }
            else
            {
                // Auto-stand after double
                PlayerStand();
            }
        }

        /// <summary>
        /// Player splits their pair
        /// </summary>
        public void PlayerSplit()
        {
            if (!CanSplit) return;

            if (!bettingSystem.DoubleBet())
            {
                OnMessage?.Invoke("Cannot afford to split");
                return;
            }

            _hasActed = true;
            _surrenderAvailable = false;

            // Create split hands
            var card1 = _playerHand.RemoveCardAt(1);
            var hand1 = new Hand();
            hand1.AddCard(_playerHand.Cards[0]);
            hand1.AddCard(deckManager.DealCard());

            var hand2 = new Hand();
            hand2.AddCard(card1);
            hand2.AddCard(deckManager.DealCard());

            _splitHands.Add(hand1);
            _splitHands.Add(hand2);
            _currentSplitHandIndex = 0;
            _hasActed = false; // Reset for first split hand

            _playerHand.Clear();

            Debug.Log($"[BlackjackGameManager] Player splits. Hand 1: {hand1}, Hand 2: {hand2}");
            OnMessage?.Invoke("Split! Playing hand 1");

            OnCardDealt?.Invoke(hand1.Cards[1], "player");
            OnCardDealt?.Invoke(hand2.Cards[1], "player");
        }

        /// <summary>
        /// Player surrenders (forfeit half bet)
        /// </summary>
        public void PlayerSurrender()
        {
            if (!SurrenderAvailable) return;

            bettingSystem.ProcessSurrender();
            OnMessage?.Invoke("Surrendered - half bet returned");

            SetState(GameState.RoundEnd);
            StartCoroutine(EndRoundDelay());
        }

        /// <summary>
        /// Player takes insurance
        /// </summary>
        public void TakeInsurance()
        {
            if (!InsuranceAvailable) return;

            if (bettingSystem.PlaceInsurance())
            {
                _insuranceTaken = true;
                OnMessage?.Invoke("Insurance taken");
            }
        }

        /// <summary>
        /// Player declines insurance
        /// </summary>
        public void DeclineInsurance()
        {
            _insuranceOffered = false;
            OnMessage?.Invoke("Insurance declined");

            // Check for dealer blackjack
            if (_celesteHand.IsBlackjack)
            {
                StartCoroutine(RevealDealerBlackjack());
            }
            else
            {
                SetState(GameState.PlayerTurn);
                OnPlayerTurnStarted?.Invoke();
            }
        }

        #endregion

        #region Coroutines

        private IEnumerator DealInitialCards()
        {
            SetState(GameState.Dealing);
            OnDealStarted?.Invoke();
            OnMessage?.Invoke("Dealing...");

            yield return new WaitForSeconds(dealDelay);

            // Deal: Player, Celeste, Player, Celeste (hole card)
            var p1 = deckManager.DealCard();
            _playerHand.AddCard(p1);
            OnCardDealt?.Invoke(p1, "player");
            yield return new WaitForSeconds(dealDelay);

            var c1 = deckManager.DealCard();
            _celesteHand.AddCard(c1);
            OnCardDealt?.Invoke(c1, "celeste");
            yield return new WaitForSeconds(dealDelay);

            var p2 = deckManager.DealCard();
            _playerHand.AddCard(p2);
            OnCardDealt?.Invoke(p2, "player");
            yield return new WaitForSeconds(dealDelay);

            var c2 = deckManager.DealCard();
            _celesteHand.AddCard(c2);
            OnCardDealt?.Invoke(c2, "celeste_hidden"); // Hole card
            yield return new WaitForSeconds(dealDelay);

            Debug.Log($"[BlackjackGameManager] Deal complete. Player: {_playerHand}, Celeste: {_celesteHand}");

            // Check for insurance offer (dealer shows Ace)
            if (CelesteUpCard.IsAce)
            {
                _insuranceOffered = true;
                SetState(GameState.InsuranceOffer);
                OnMessage?.Invoke("Insurance?");
                yield break;
            }

            // Check for player blackjack
            if (_playerHand.IsBlackjack)
            {
                yield return new WaitForSeconds(turnDelay);

                if (_celesteHand.IsBlackjack)
                {
                    OnMessage?.Invoke("Both have Blackjack - Push!");
                    ResolveRound(HandResult.Push);
                }
                else
                {
                    OnMessage?.Invoke("Blackjack!");
                    ResolveRound(HandResult.PlayerBlackjack);
                }
                yield break;
            }

            // Check for dealer blackjack (when showing 10-value)
            if (CelesteUpCard.IsTenValue && _celesteHand.IsBlackjack)
            {
                yield return StartCoroutine(RevealDealerBlackjack());
                yield break;
            }

            SetState(GameState.PlayerTurn);
            OnPlayerTurnStarted?.Invoke();
            OnMessage?.Invoke("Your turn - Hit or Stand?");
        }

        private IEnumerator RevealDealerBlackjack()
        {
            OnMessage?.Invoke("Checking for Blackjack...");
            yield return new WaitForSeconds(turnDelay);

            if (_insuranceTaken)
            {
                bettingSystem.ProcessInsurance(true);
                OnMessage?.Invoke("Dealer Blackjack! Insurance pays!");
            }
            else
            {
                OnMessage?.Invoke("Dealer Blackjack!");
            }

            yield return new WaitForSeconds(turnDelay);
            ResolveRound(HandResult.CelesteWins);
        }

        private IEnumerator HandlePlayerBust()
        {
            yield return new WaitForSeconds(turnDelay);

            // Check if more split hands
            if (_splitHands.Count > 0 && _currentSplitHandIndex < _splitHands.Count - 1)
            {
                _currentSplitHandIndex++;
                _hasActed = false;
                OnMessage?.Invoke($"Playing hand {_currentSplitHandIndex + 1}");
                SetState(GameState.PlayerTurn);
                yield break;
            }

            // All hands bust - resolve immediately
            ResolveRound(HandResult.PlayerBust);
        }

        private IEnumerator PlayCelesteTurn()
        {
            SetState(GameState.CelesteTurn);
            OnCelesteTurnStarted?.Invoke();
            OnMessage?.Invoke("Celeste's turn...");

            yield return new WaitForSeconds(turnDelay);

            // Process insurance if dealer doesn't have blackjack
            if (_insuranceTaken)
            {
                bettingSystem.ProcessInsurance(false);
            }

            // Celeste hits until 17 or higher (standard dealer rules)
            while (_celesteHand.Score < 17)
            {
                var card = deckManager.DealCard();
                _celesteHand.AddCard(card);
                OnCardDealt?.Invoke(card, "celeste");

                Debug.Log($"[BlackjackGameManager] Celeste hits: {card.ShortName}. Hand: {_celesteHand}");
                OnMessage?.Invoke($"Celeste draws {card.ShortName}");

                yield return new WaitForSeconds(dealDelay);
            }

            if (_celesteHand.IsBusted)
            {
                OnMessage?.Invoke("Celeste busts!");
            }
            else
            {
                OnMessage?.Invoke($"Celeste stands with {_celesteHand.Score}");
            }

            yield return new WaitForSeconds(turnDelay);

            // Evaluate all hands
            EvaluateAllHands();
        }

        private IEnumerator EndRoundDelay()
        {
            yield return new WaitForSeconds(resultDelay);
            SetState(GameState.Idle);
        }

        #endregion

        #region Resolution

        private void EvaluateAllHands()
        {
            if (_splitHands.Count > 0)
            {
                // Evaluate each split hand
                for (int i = 0; i < _splitHands.Count; i++)
                {
                    var result = handEvaluator.EvaluateHands(_splitHands[i], _celesteHand);
                    ProcessHandResult(result, $"Hand {i + 1}");
                }
            }
            else
            {
                var result = handEvaluator.EvaluateHands(_playerHand, _celesteHand);
                ResolveRound(result);
            }
        }

        private void ProcessHandResult(HandResult result, string handName)
        {
            float multiplier = handEvaluator.GetPayoutMultiplier(result);
            string message = handEvaluator.GetResultMessage(result);

            Debug.Log($"[BlackjackGameManager] {handName}: {message}");

            // Note: For splits, we'd need per-hand bet tracking
            // Simplified: just report the result
            OnMessage?.Invoke($"{handName}: {message}");
        }

        private void ResolveRound(HandResult result)
        {
            SetState(GameState.Resolution);

            float multiplier = handEvaluator.GetPayoutMultiplier(result);
            string message = handEvaluator.GetResultMessage(result);

            OnMessage?.Invoke(message);
            OnRoundResult?.Invoke(result);

            if (handEvaluator.IsPlayerWin(result))
            {
                bettingSystem.ProcessWin(multiplier, message);
            }
            else if (handEvaluator.IsPlayerLoss(result))
            {
                bettingSystem.ProcessLoss(message);
            }
            else
            {
                bettingSystem.ProcessPush();
            }

            SetState(GameState.RoundEnd);
            StartCoroutine(EndRoundDelay());
        }

        #endregion

        #region Debug

#if UNITY_EDITOR
        [ContextMenu("Debug: Start Round")]
        private void DebugStartRound() => StartNewRound();

        [ContextMenu("Debug: Print State")]
        private void DebugPrintState()
        {
            Debug.Log($"State: {_currentState}");
            Debug.Log($"Player: {_playerHand}");
            Debug.Log($"Celeste: {_celesteHand}");
            Debug.Log($"Bet: {bettingSystem.CurrentBet}, Chips: {bettingSystem.PlayerChips}");
        }
#endif

        #endregion
    }
}
