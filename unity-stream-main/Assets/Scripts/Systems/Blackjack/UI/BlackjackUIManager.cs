using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Blackjack
{
    public class BlackjackUIManager : MonoBehaviour
    {
        [Header("Game References")]
        [SerializeField] private BlackjackGameManager gameManager;
        [SerializeField] private BettingSystem bettingSystem;

        [Header("Score Displays")]
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private TextMeshProUGUI celesteScoreText;
        [SerializeField] private TextMeshProUGUI playerHandText;
        [SerializeField] private TextMeshProUGUI celesteHandText;

        [Header("Chip Display")]
        [SerializeField] private TextMeshProUGUI chipCountText;
        [SerializeField] private TextMeshProUGUI betAmountText;
        [SerializeField] private TextMeshProUGUI roundNumberText;

        [Header("Message Display")]
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private float messageDuration = 3f;

        [Header("Action Buttons")]
        [SerializeField] private Button hitButton;
        [SerializeField] private Button standButton;
        [SerializeField] private Button doubleButton;
        [SerializeField] private Button splitButton;
        [SerializeField] private Button surrenderButton;

        [Header("Betting Buttons")]
        [SerializeField] private Button[] chipButtons; // $5, $10, $25, $50, $100
        [SerializeField] private Button clearBetButton;
        [SerializeField] private Button dealButton;

        [Header("Insurance Buttons")]
        [SerializeField] private GameObject insurancePanel;
        [SerializeField] private Button insuranceYesButton;
        [SerializeField] private Button insuranceNoButton;

        [Header("New Round")]
        [SerializeField] private Button newRoundButton;

        [Header("Button Panels")]
        [SerializeField] private GameObject actionButtonPanel;
        [SerializeField] private GameObject bettingPanel;

        private Coroutine _messageCoroutine;

        private void Start()
        {
            SetupButtonListeners();
            SubscribeToEvents();
            UpdateUI();
            ShowBettingPhase();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SetupButtonListeners()
        {
            // Action buttons
            if (hitButton) hitButton.onClick.AddListener(OnHitClicked);
            if (standButton) standButton.onClick.AddListener(OnStandClicked);
            if (doubleButton) doubleButton.onClick.AddListener(OnDoubleClicked);
            if (splitButton) splitButton.onClick.AddListener(OnSplitClicked);
            if (surrenderButton) surrenderButton.onClick.AddListener(OnSurrenderClicked);

            // Betting buttons
            if (clearBetButton) clearBetButton.onClick.AddListener(OnClearBetClicked);
            if (dealButton) dealButton.onClick.AddListener(OnDealClicked);

            // Chip buttons
            if (chipButtons != null)
            {
                int[] values = bettingSystem?.ChipValues ?? new int[] { 5, 10, 25, 50, 100 };
                for (int i = 0; i < chipButtons.Length && i < values.Length; i++)
                {
                    int chipValue = values[i];
                    if (chipButtons[i] != null)
                    {
                        chipButtons[i].onClick.AddListener(() => OnChipClicked(chipValue));
                    }
                }
            }

            // Insurance buttons
            if (insuranceYesButton) insuranceYesButton.onClick.AddListener(OnInsuranceYesClicked);
            if (insuranceNoButton) insuranceNoButton.onClick.AddListener(OnInsuranceNoClicked);

            // New round button
            if (newRoundButton) newRoundButton.onClick.AddListener(OnNewRoundClicked);
        }

        private void SubscribeToEvents()
        {
            if (gameManager)
            {
                gameManager.OnStateChanged += OnGameStateChanged;
                gameManager.OnMessage += ShowMessage;
                gameManager.OnCardDealt += OnCardDealt;
            }

            if (bettingSystem)
            {
                bettingSystem.OnChipsChanged += OnChipsChanged;
                bettingSystem.OnBetChanged += OnBetChanged;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (gameManager)
            {
                gameManager.OnStateChanged -= OnGameStateChanged;
                gameManager.OnMessage -= ShowMessage;
                gameManager.OnCardDealt -= OnCardDealt;
            }

            if (bettingSystem)
            {
                bettingSystem.OnChipsChanged -= OnChipsChanged;
                bettingSystem.OnBetChanged -= OnBetChanged;
            }
        }

        #region Button Handlers

        private void OnHitClicked() => gameManager?.PlayerHit();
        private void OnStandClicked() => gameManager?.PlayerStand();
        private void OnDoubleClicked() => gameManager?.PlayerDoubleDown();
        private void OnSplitClicked() => gameManager?.PlayerSplit();
        private void OnSurrenderClicked() => gameManager?.PlayerSurrender();
        private void OnClearBetClicked() => bettingSystem?.ClearBet();
        private void OnDealClicked() => gameManager?.ConfirmBetAndDeal();
        private void OnChipClicked(int value) => bettingSystem?.AddToBet(value);
        private void OnInsuranceYesClicked() => gameManager?.TakeInsurance();
        private void OnInsuranceNoClicked() => gameManager?.DeclineInsurance();
        private void OnNewRoundClicked() => gameManager?.StartNewRound();

        #endregion

        #region Event Handlers

        private void OnGameStateChanged(BlackjackGameManager.GameState state)
        {
            Debug.Log($"[BlackjackUIManager] State changed to: {state}");

            switch (state)
            {
                case BlackjackGameManager.GameState.Idle:
                    ShowNewRoundButton();
                    break;

                case BlackjackGameManager.GameState.Betting:
                    ShowBettingPhase();
                    break;

                case BlackjackGameManager.GameState.Dealing:
                    HideAllPanels();
                    break;

                case BlackjackGameManager.GameState.PlayerTurn:
                    ShowActionButtons();
                    break;

                case BlackjackGameManager.GameState.InsuranceOffer:
                    ShowInsurancePanel();
                    break;

                case BlackjackGameManager.GameState.CelesteTurn:
                    HideAllPanels();
                    break;

                case BlackjackGameManager.GameState.Resolution:
                case BlackjackGameManager.GameState.RoundEnd:
                    HideAllPanels();
                    break;
            }

            UpdateUI();
        }

        private void OnCardDealt(Card card, string recipient)
        {
            UpdateHandDisplays();
        }

        private void OnChipsChanged(int chips)
        {
            UpdateChipDisplay();
        }

        private void OnBetChanged(int bet)
        {
            UpdateBetDisplay();
            UpdateDealButton();
        }

        #endregion

        #region UI Updates

        private void UpdateUI()
        {
            UpdateHandDisplays();
            UpdateChipDisplay();
            UpdateBetDisplay();
            UpdateActionButtons();
            UpdateRoundDisplay();
        }

        private void UpdateHandDisplays()
        {
            if (gameManager == null) return;

            // Player hand
            var playerHand = gameManager.PlayerHand;
            if (playerScoreText)
            {
                playerScoreText.text = playerHand.CardCount > 0 ? playerHand.ScoreDisplay : "-";
            }
            if (playerHandText)
            {
                playerHandText.text = playerHand.CardCount > 0 ? playerHand.ToString() : "";
            }

            // Celeste hand
            var celesteHand = gameManager.CelesteHand;
            bool showHoleCard = gameManager.CurrentState == BlackjackGameManager.GameState.CelesteTurn ||
                               gameManager.CurrentState == BlackjackGameManager.GameState.Resolution ||
                               gameManager.CurrentState == BlackjackGameManager.GameState.RoundEnd;

            if (celesteScoreText)
            {
                if (celesteHand.CardCount == 0)
                {
                    celesteScoreText.text = "-";
                }
                else if (showHoleCard)
                {
                    celesteScoreText.text = celesteHand.ScoreDisplay;
                }
                else
                {
                    celesteScoreText.text = gameManager.CelesteVisibleScore.ToString() + "?";
                }
            }

            if (celesteHandText)
            {
                if (celesteHand.CardCount == 0)
                {
                    celesteHandText.text = "";
                }
                else if (showHoleCard)
                {
                    celesteHandText.text = celesteHand.ToString();
                }
                else
                {
                    celesteHandText.text = gameManager.CelesteUpCard?.ShortName + " ??";
                }
            }
        }

        private void UpdateChipDisplay()
        {
            if (chipCountText && bettingSystem)
            {
                chipCountText.text = $"${bettingSystem.PlayerChips}";
            }
        }

        private void UpdateBetDisplay()
        {
            if (betAmountText && bettingSystem)
            {
                betAmountText.text = bettingSystem.CurrentBet > 0 ?
                    $"Bet: ${bettingSystem.CurrentBet}" : "Place Bet";
            }
        }

        private void UpdateRoundDisplay()
        {
            if (roundNumberText && gameManager)
            {
                roundNumberText.text = $"Round {gameManager.RoundNumber}";
            }
        }

        private void UpdateActionButtons()
        {
            if (gameManager == null) return;

            if (hitButton) hitButton.interactable = gameManager.CanHit;
            if (standButton) standButton.interactable = gameManager.CanStand;
            if (doubleButton) doubleButton.interactable = gameManager.CanDoubleDown;
            if (splitButton) splitButton.interactable = gameManager.CanSplit;
            if (surrenderButton) surrenderButton.interactable = gameManager.SurrenderAvailable;
        }

        private void UpdateDealButton()
        {
            if (dealButton && bettingSystem)
            {
                dealButton.interactable = bettingSystem.CurrentBet >= bettingSystem.MinimumBet;
            }
        }

        #endregion

        #region Panel Management

        private void ShowBettingPhase()
        {
            if (bettingPanel) bettingPanel.SetActive(true);
            if (actionButtonPanel) actionButtonPanel.SetActive(false);
            if (insurancePanel) insurancePanel.SetActive(false);
            if (newRoundButton) newRoundButton.gameObject.SetActive(false);
            UpdateDealButton();
        }

        private void ShowActionButtons()
        {
            if (bettingPanel) bettingPanel.SetActive(false);
            if (actionButtonPanel) actionButtonPanel.SetActive(true);
            if (insurancePanel) insurancePanel.SetActive(false);
            if (newRoundButton) newRoundButton.gameObject.SetActive(false);
            UpdateActionButtons();
        }

        private void ShowInsurancePanel()
        {
            if (bettingPanel) bettingPanel.SetActive(false);
            if (actionButtonPanel) actionButtonPanel.SetActive(false);
            if (insurancePanel) insurancePanel.SetActive(true);
            if (newRoundButton) newRoundButton.gameObject.SetActive(false);
        }

        private void ShowNewRoundButton()
        {
            if (bettingPanel) bettingPanel.SetActive(false);
            if (actionButtonPanel) actionButtonPanel.SetActive(false);
            if (insurancePanel) insurancePanel.SetActive(false);
            if (newRoundButton) newRoundButton.gameObject.SetActive(true);
        }

        private void HideAllPanels()
        {
            if (bettingPanel) bettingPanel.SetActive(false);
            if (actionButtonPanel) actionButtonPanel.SetActive(false);
            if (insurancePanel) insurancePanel.SetActive(false);
            if (newRoundButton) newRoundButton.gameObject.SetActive(false);
        }

        #endregion

        #region Messages

        public void ShowMessage(string message)
        {
            if (_messageCoroutine != null)
            {
                StopCoroutine(_messageCoroutine);
            }

            if (messageText)
            {
                messageText.text = message;
                _messageCoroutine = StartCoroutine(ClearMessageAfterDelay());
            }

            Debug.Log($"[BlackjackUI] {message}");
        }

        private IEnumerator ClearMessageAfterDelay()
        {
            yield return new WaitForSeconds(messageDuration);
            if (messageText) messageText.text = "";
        }

        #endregion

        #region Keyboard Shortcuts

        private void Update()
        {
            if (gameManager == null) return;

            // Only process shortcuts during player turn
            if (gameManager.CurrentState == BlackjackGameManager.GameState.PlayerTurn)
            {
                if (Input.GetKeyDown(KeyCode.H) && gameManager.CanHit)
                    gameManager.PlayerHit();
                else if (Input.GetKeyDown(KeyCode.S) && gameManager.CanStand)
                    gameManager.PlayerStand();
                else if (Input.GetKeyDown(KeyCode.D) && gameManager.CanDoubleDown)
                    gameManager.PlayerDoubleDown();
                else if (Input.GetKeyDown(KeyCode.P) && gameManager.CanSplit)
                    gameManager.PlayerSplit();
                else if (Input.GetKeyDown(KeyCode.R) && gameManager.SurrenderAvailable)
                    gameManager.PlayerSurrender();
            }
            else if (gameManager.CurrentState == BlackjackGameManager.GameState.Betting)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    gameManager.ConfirmBetAndDeal();
            }
            else if (gameManager.CurrentState == BlackjackGameManager.GameState.Idle)
            {
                if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space))
                    gameManager.StartNewRound();
            }
            else if (gameManager.CurrentState == BlackjackGameManager.GameState.InsuranceOffer)
            {
                if (Input.GetKeyDown(KeyCode.Y))
                    gameManager.TakeInsurance();
                else if (Input.GetKeyDown(KeyCode.N))
                    gameManager.DeclineInsurance();
            }
        }

        #endregion
    }
}
