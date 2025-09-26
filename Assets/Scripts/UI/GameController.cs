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
        [SerializeField] private GameGrid cardGrid;
        [SerializeField] private GameObject memoriseMSG;
        [SerializeField] private Image memoriseMsgBar;
        [SerializeField] private TextMeshProUGUI gameTimer;

        [SerializeField] Button homeBtn, saveBtn;

        [SerializeField] private GridLayoutGroup gridLayoutGroup;

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

        void Awake()
        {
            root.gameObject.SetActive(false);
            cardPool.InitializePool(cardPrefab);

            // Cache frequently used references
            gameManager = GameManager.Instance;
            matchManager = MatchManager.Instance;
            gridTransform = cardGrid.transform;
        }

        private void OnEnable()
        {
            EventDispatcher.Instance.Subscribe<ScoreData>(EventConstants.ON_GAME_RESULT, OnGameOver);
            GameManager.OnExitYes += HandleExitYes;
            homeBtn.onClick.AddListener(OnHomeButtonClicked);
            saveBtn.onClick.AddListener(OnSaveButtonClicked);
        }

        private void OnDisable()
        {
            EventDispatcher.Instance.Unsubscribe<ScoreData>(EventConstants.ON_GAME_RESULT, OnGameOver);
            GameManager.OnExitYes -= HandleExitYes;
            homeBtn.onClick.RemoveListener(OnHomeButtonClicked);
            saveBtn.onClick.RemoveListener(OnSaveButtonClicked);
        }

        private void OnGameOver(ScoreData scoreData)
        {
            CleanupGameplayScreen();
        }

        public void OnLevelResumed(DifficultyLevelData levelData, GameSaveData savedLevelData)
        {
            AudioManager.Instance.StopBGMusic();
            AudioManager.Instance.PlayGameStartSFX();

            root.gameObject.SetActive(true);

            if (!levelData.ValidateLevelData()) return;

            ResetUIState();
            SetupGridLayout(levelData.colsCount);

            DispatchScoreUpdate(savedLevelData);

            SpawnCards(savedLevelData);

            matchManager.InitializeSavedGame(activeCards, savedLevelData);
        }

        public void OnLevelStarted(DifficultyLevelData levelData)
        {
            AudioManager.Instance.StopBGMusic();
            AudioManager.Instance.PlayGameStartSFX();

            root.gameObject.SetActive(true);

            if (!levelData.ValidateLevelData()) return;

            ResetUIState();
            SetupGridLayout(levelData.colsCount);
            int totalGridElements = levelData.rowsCount * levelData.colsCount;

            List<int> shuffledCardIDs = GenerateShuffledCardPairs(totalGridElements);
            SpawnCards(shuffledCardIDs);

            // Initialize the match manager with the spawned cards
            matchManager.InitializeGame(activeCards);

            StartCoroutine(PreviewGrid(levelData.previewDuration));
        }

        private void ResetUIState()
        {
            StopAllCoroutines();
            isPreviewMode = false;
            gameStartTime = Time.time;
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

            CardSO cardData = gameManager.GetCardData();

            for (int i = 0; i < cardIDs.Count; i++)
            {
                var cardInfo = cardData.cardDataList[cardIDs[i]];
                var cardObj = cardPool.GetObjectFromPool();

                cardObj.transform.SetParent(gridTransform, false);
                cardObj.gameObject.SetActive(true);

                var gameCard = cardObj.GetComponent<GameCard>();
                gameCard.InitCard(cardInfo.cardID, cardInfo.cardSprite, matchManager.HandleCardClick);
                activeCards.Add(gameCard);
            }
        }

        private void SpawnCards(GameSaveData savedLevelData)
        {
            activeCards.Clear();

            List<int> cardIDs = savedLevelData.cardID;
            CardSO cardData = gameManager.GetCardData();

            for (int i = 0; i < cardIDs.Count; i++)
            {
                var cardInfo = cardData.cardDataList[cardIDs[i]];
                var cardObj = cardPool.GetObjectFromPool();

                cardObj.transform.SetParent(gridTransform, false);
                cardObj.gameObject.SetActive(true);

                var gameCard = cardObj.GetComponent<GameCard>();
                gameCard.InitSavedCard(cardInfo.cardID, cardInfo.cardSprite, savedLevelData.isFlipped[i], savedLevelData.cardMatched[i], matchManager.HandleCardClick);
                activeCards.Add(gameCard);
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
                card.FlipToBack(animate: true);
            }

            yield return new WaitForSeconds(PREVIEW_FLIP_DELAY);

            foreach (var card in activeCards)
            {
                card.EnableInteraction();
            }

            memoriseMSG.SetActive(false);
            isPreviewMode = false;

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
                int minutes = Mathf.FloorToInt(currentTime * (1f / 60f));
                int seconds = Mathf.FloorToInt(currentTime % 60);

                timerStringBuilder.Clear();
                timerStringBuilder.Append(minutes.ToString("00"));
                timerStringBuilder.Append(':');
                timerStringBuilder.Append(seconds.ToString("00"));
                gameTimer.text = timerStringBuilder.ToString();

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
            // Return all cards to object pool and clear grid - optimized to call ReleaseAll only once
            cardPool.ReleaseAll();
        }

        private void ResetUIElements()
        {
            // Reset timer display
            gameTimer.text = "00:00";

            // Hide memorize message if showing
            memoriseMSG.SetActive(false);
            memoriseMsgBar.fillAmount = 0f;

            // Reset preview mode
            isPreviewMode = false;
        }

        private void OnHomeButtonClicked()
        {
            gameManager.ShowExitPopupUI();
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