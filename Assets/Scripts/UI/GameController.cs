using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CyberSpeed.Utils;
using CyberSpeed.Manager;
using CyberSpeed.SerialisedClasses;
using CyberSpeed.SO;
using DifficultyLevelData = CyberSpeed.SO.DifficultyLevelSO.DifficultyLevelData;
using UnityEngine.UI;
using TMPro;
using System.Text;

namespace CyberSpeed.UI
{
    public class GameController : MonoBehaviour
    {
        [Header("Object Pool References")]
        [SerializeField] private ObjectPool cardPool;

        [SerializeField] private GameObject cardPrefab;

        [SerializeField] private GameObject root;
        [SerializeField] private GridLayoutGroup gridLayoutGroup;

        [SerializeField] private GameObject memoriseMSG;
        [SerializeField] private Image memoriseMsgBar;
        [SerializeField] private TextMeshProUGUI gameTimer;

        [SerializeField] Button homeBtn, saveBtn;

        // Configuration constants
        private const float PREVIEW_INITIAL_DELAY = 0.25f;
        private const float PREVIEW_FLIP_DELAY = 0.5f;
        private const float TIMER_UPDATE_INTERVAL = 1f;

        private List<GameCard> activeCards = new List<GameCard>();
        private bool isPreviewMode = false;
        private float gameStartTime = 0f;

        // Cached references for performance
        private GameManager gameManager;
        private MatchManager matchManager;
        private StringBuilder timerStringBuilder = new StringBuilder(8);
        private Transform gridTransform;
        private CardFactory cardFactory;

        void Awake()
        {
            root.gameObject.SetActive(false);
            cardPool.InitializePool(cardPrefab);

            // Cache frequently used references
            gameManager = GameManager.Instance;
            matchManager = MatchManager.Instance;
            gridTransform = gridLayoutGroup.transform;

            // Initialize the simple card factory
            cardFactory = new CardFactory(cardPool, gameManager.GetCardData());
        }

        private void OnEnable()
        {
            EventDispatcher.Instance.Subscribe<ScoreData>(EventConstants.ON_GAME_RESULT, OnGameOver);
            MatchManager.OnGameProgressChanged += OnGameProgressed;
            GameManager.OnExitYes += HandleExitYes;
            homeBtn.onClick.AddListener(OnHomeButtonClicked);
            saveBtn.onClick.AddListener(OnSaveButtonClicked);
        }

        private void OnDisable()
        {
            EventDispatcher.Instance.Unsubscribe<ScoreData>(EventConstants.ON_GAME_RESULT, OnGameOver);
            MatchManager.OnGameProgressChanged -= OnGameProgressed;
            GameManager.OnExitYes -= HandleExitYes;
            homeBtn.onClick.RemoveListener(OnHomeButtonClicked);
            saveBtn.onClick.RemoveListener(OnSaveButtonClicked);
        }

        private void OnGameProgressed(bool isProgressed)
        {
            saveBtn.interactable = isProgressed;
        }

        private void OnGameOver(ScoreData scoreData)
        {
            CleanupGameplayScreen();
        }

        public void OnLevelStarted(DifficultyLevelData levelData, GameSaveData savedLevelData = null)
        {
            // Common setup
            AudioManager.Instance.StopBGMusic();
            AudioManager.Instance.PlayGameStartSFX();
            root.gameObject.SetActive(true);

            if (!levelData.ValidateLevelData()) return;

            ResetUIState();
            SetupGridLayout(levelData.colsCount);

            bool isResumedGame = savedLevelData != null;

            saveBtn.interactable = isResumedGame;

            if (isResumedGame)
            {
                // Resumed game logic
                DispatchScoreUpdate(savedLevelData);
                SpawnCards(savedLevelData);
                matchManager.InitializeSavedGame(activeCards, savedLevelData);

                // Start timer for resumed games from saved time
                gameStartTime = Time.time - savedLevelData.gameTimer;
                StartCoroutine(UpdateGameTimer());
            }
            else
            {
                // New game logic
                int totalGridElements = levelData.rowsCount * levelData.colsCount;
                List<int> shuffledCardIDs = GenerateShuffledCardPairs(totalGridElements);
                SpawnCards(shuffledCardIDs);
                matchManager.InitializeGame(activeCards);


            }

            // Start preview only for new games
            StartCoroutine(PreviewGrid(levelData.previewDuration));
        }

        public void OnLevelResumed(DifficultyLevelData levelData, GameSaveData savedLevelData)
        {
            OnLevelStarted(levelData, savedLevelData);
        }

        private void ResetUIState()
        {
            StopAllCoroutines();
            isPreviewMode = false;
            memoriseMSG.SetActive(false);
        }

        private void SetupGridLayout(int columnCount)
        {
            gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayoutGroup.constraintCount = columnCount;
        }

        private List<int> GenerateShuffledCardPairs(int totalGridElements)
        {
            int totalUniqueItems = totalGridElements / 2;
            CardSO cardData = gameManager.GetCardData();

            List<int> cardIDs = Utilities.GetRandomIntList(cardData.cardDataList.Count, totalUniqueItems);
            List<int> cardPairs = new List<int>(totalGridElements);
            cardPairs.AddRange(cardIDs);
            cardPairs.AddRange(cardIDs);
            cardPairs.Shuffle();

            return cardPairs;
        }

        private void SpawnCards(List<int> cardIDs)
        {
            activeCards.Clear();

            // Use factory to create cards
            activeCards = cardFactory.CreateCards(cardIDs, gridTransform, matchManager.HandleCardClick);
        }

        private void SpawnCards(GameSaveData savedLevelData)
        {
            activeCards.Clear();

            List<int> cardIDs = savedLevelData.cardID;

            // Use factory to create saved cards
            for (int i = 0; i < cardIDs.Count; i++)
            {
                bool isMatched = i < savedLevelData.cardMatched.Count ? savedLevelData.cardMatched[i] : false;

                var card = cardFactory.CreateSavedCard(cardIDs[i], gridTransform, isMatched, matchManager.HandleCardClick);
                activeCards.Add(card);
            }
        }

        private IEnumerator PreviewGrid(float previewDuration)
        {
            memoriseMSG.SetActive(true);
            memoriseMsgBar.fillAmount = 1f;
            yield return new WaitForSeconds(PREVIEW_INITIAL_DELAY);

            isPreviewMode = true;

            foreach (var card in activeCards)
            {
                card.FlipToFront(animate: true);
                card.DisableInteraction();
            }

            yield return new WaitForSeconds(PREVIEW_FLIP_DELAY);

            float timer = previewDuration;
            float invPreviewDuration = 1f / previewDuration;

            while (timer > 0)
            {
                timer -= Time.deltaTime;
                memoriseMsgBar.fillAmount = timer * invPreviewDuration;
                yield return null;
            }

            memoriseMsgBar.fillAmount = 0f;

            foreach (var card in activeCards)
            {
                if (!card.IsMatched)
                    card.FlipToBack(animate: true);
            }

            yield return new WaitForSeconds(PREVIEW_FLIP_DELAY);

            foreach (var card in activeCards)
            {
                card.EnableInteraction();
            }

            memoriseMSG.SetActive(false);
            isPreviewMode = false;

            // Set game start time when actual gameplay begins (after preview)
            gameStartTime = Time.time;

            // Start the match manager game and timer
            matchManager.StartGame();
            StartCoroutine(UpdateGameTimer());
        }

        private IEnumerator UpdateGameTimer()
        {
            var waitForSecond = new WaitForSeconds(TIMER_UPDATE_INTERVAL);

            while (!matchManager.IsMatchingInProgress && !isPreviewMode)
            {
                float currentTime = Time.time - gameStartTime;
                gameTimer.text = currentTime.GetGameTime();


                // gameTimer.text = gameStartTime.GetGameTime().ToString();

                yield return waitForSecond;
            }
        }

        public void CleanupGameplayScreen()
        {
            // Stop all running coroutines
            StopAllCoroutines();

            // Hide the main gameplay screen
            root.gameObject.SetActive(false);

            // Clear grid content
            ClearGrid();

            // Reset UI elements
            ResetUIElements();

            // Clear active cards list
            activeCards.Clear();

            // Notify other UI components to cleanup
            EventDispatcher.Instance.Dispatch(EventConstants.ON_GAME_CLEANUP);
        }

        private void ClearGrid()
        {
            // Use factory to release all cards and clear grid
            cardFactory.ReleaseAllCards();
        }

        private void ResetUIElements()
        {
            // Reset timer display
            gameTimer.text = "";

            // Hide memorize message if showing
            memoriseMSG.SetActive(false);
            memoriseMsgBar.fillAmount = 0f;

            // Reset preview mode
            isPreviewMode = false;
        }

        private void OnHomeButtonClicked()
        {
            bool isGameProgressed = matchManager.HasGameProgress();
            if (isGameProgressed)
            {
                gameManager.ShowExitPopupUI();
            }
            else
            {
                HandleExitYes();
                gameManager.ShowIntroUI();
            }
        }

        private void HandleExitYes()
        {
            CleanupGameplayScreen();
            gameManager.HideExitPopupUI();
        }

        private void OnSaveButtonClicked()
        {
            gameManager.ShowSavePopupUI();
        }

        private void DispatchScoreUpdate(GameSaveData savedLevelData)
        {
            var scoreData = new ScoreData
            {
                GameTime = Time.time - gameStartTime,
                TotalTurns = savedLevelData.turns,
                TotalMatches = savedLevelData.matches,
                TotalComboStreaks = (savedLevelData.streak - 1) < 0 ? 0 : savedLevelData.streak - 1,
                TotalScore = savedLevelData.score
            };

            EventDispatcher.Instance.Dispatch(EventConstants.ON_SCORE_UPDATED, scoreData);
        }
    }
}