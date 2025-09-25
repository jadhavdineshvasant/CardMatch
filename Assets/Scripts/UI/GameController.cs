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

namespace CyberSpeed.UI
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameGrid cardGrid;
        [SerializeField] private GameObject memoriseMSG;
        [SerializeField] private Image memoriseMsgBar;
        [SerializeField] private TextMeshProUGUI gameTimer;

        private GridLayoutGroup gridLayoutGroup;
        private List<GameCard> activeCards = new List<GameCard>();
        private List<GameCard> matchedCards = new List<GameCard>();
        private bool isPreviewMode = false;
        private bool isMatchingInProgress = false;

        void Awake()
        {
            root.gameObject.SetActive(false);
            gridLayoutGroup = cardGrid.GetComponent<GridLayoutGroup>();
        }

        private void OnEnable()
        {
            EventDispatcher.Instance.Subscribe<DifficultyLevelData>(EventConstants.ON_LEVEL_STARTED, OnLevelStarted);
        }

        private void OnDisable()
        {
            EventDispatcher.Instance.Unsubscribe<DifficultyLevelData>(EventConstants.ON_LEVEL_STARTED, OnLevelStarted);
        }

        private void OnLevelStarted(DifficultyLevelData levelData)
        {
            root.gameObject.SetActive(true);

            if (!levelData.ValidateLevelData()) return;

            ResetGameState();
            SetupGridLayout(levelData.colsCount);
            int totalGridElements = levelData.rowsCount * levelData.colsCount;

            List<int> shuffledCardIDs = GenerateShuffledCardPairs(totalGridElements);
            SpawnCards(shuffledCardIDs);
            StartCoroutine(PreviewGrid(levelData.previewDuration));
        }

        private void ResetGameState()
        {
            StopAllCoroutines();

            openCard = null;
            isPreviewMode = false;
            isMatchingInProgress = false;
            matchedCards.Clear();

            streak = 0;
            totalScore = 0;
            totalTurns = 0;
            totalMatches = 0;
            bestComboStreak = 0;
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
            CardSO cardData = GameManager.Instance.GetCardData();

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

            GameManager gameManager = GameManager.Instance;
            CardSO cardData = gameManager.GetCardData();
            var objPool = gameManager.GetObjectPool();
            Transform gridTransform = cardGrid.transform;

            for (int i = 0; i < cardIDs.Count; i++)
            {
                var cardInfo = cardData.cardDataList[cardIDs[i]];
                var cardObj = objPool.Get();

                cardObj.transform.SetParent(gridTransform, false);

                var gameCard = cardObj.GetComponent<GameCard>();
                gameCard.InitCard(cardInfo.cardID, cardInfo.cardSprite, CardClicked);
                activeCards.Add(gameCard);
            }
        }

        private IEnumerator PreviewGrid(float previewDuration)
        {
            memoriseMSG.SetActive(true);
            memoriseMsgBar.fillAmount = 1f;
            yield return new WaitForSeconds(0.25f);

            isPreviewMode = true;

            foreach (var card in activeCards)
            {
                card.FlipToFront(animate: true);
                card.DisableInteraction();
            }

            yield return new WaitForSeconds(0.5f);

            float timer = previewDuration;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                memoriseMsgBar.fillAmount = timer / previewDuration;
                yield return null;
            }

            memoriseMsgBar.fillAmount = 0f;

            foreach (var card in activeCards)
            {
                card.FlipToBack(animate: true);
            }

            yield return new WaitForSeconds(0.5f);

            foreach (var card in activeCards)
            {
                card.EnableInteraction();
            }

            memoriseMSG.SetActive(false);
            isPreviewMode = false;
            DispatchScoreUpdate();
        }

        private GameCard openCard = null;
        private int streak = 0;
        private int totalScore = 0;
        private int totalTurns = 0;
        private int totalMatches = 0;
        private int bestComboStreak = 0;
        private float gameStartTime = 0f;

        public int CurrentStreak => streak;
        public int TotalScore => totalScore;

        private void CardClicked(GameCard gameCard)
        {
            if (isPreviewMode || isMatchingInProgress) return;
            if (gameCard.IsFlipped && gameCard != openCard) return;
            if (gameCard == openCard) return;

            if (openCard == null)
            {
                openCard = gameCard;
                Debug.Log($"First card selected: ID {gameCard.CardID}");
                return;
            }

            // Second card selected - start match checking
            Debug.Log($"Second card selected: ID {gameCard.CardID}");
            StartCoroutine(CheckForMatch(gameCard));
        }

        IEnumerator CheckForMatch(GameCard gameCard)
        {
            isMatchingInProgress = true;
            SetAllCardsInteractable(false);
            totalTurns++;

            int clickedCardID = gameCard.CardID;
            bool isMatch = openCard.CardID == clickedCardID;

            Debug.Log($"Checking match: Card1 ID={openCard.CardID}, Card2 ID={clickedCardID}");

            yield return new WaitForSeconds(0.75f);

            if (isMatch)
            {
                streak++;
                totalMatches++;

                if (streak > bestComboStreak)
                {
                    bestComboStreak = streak;
                }

                int baseScore = 100;
                int streakBonus = (streak - 1) * 50; // +50 for each additional match in streak
                int matchScore = baseScore + streakBonus;
                totalScore += matchScore;

                Debug.Log($"Match found! Streak: {streak} | Match Score: {matchScore} | Total Score: {totalScore}");

                openCard.Matched();
                gameCard.Matched();
                matchedCards.Add(openCard);
                matchedCards.Add(gameCard);
                DispatchScoreUpdate();

                // TODO: Check for win condition
            }
            else
            {
                streak = 0;
                openCard.FlipToBack();
                gameCard.FlipToBack();
                DispatchScoreUpdate();
            }

            openCard = null;
            isMatchingInProgress = false;
            SetAllCardsInteractable(true);
        }

        private void SetAllCardsInteractable(bool interactable)
        {
            foreach (var card in activeCards)
            {
                if (interactable)
                {
                    if (!matchedCards.Contains(card))
                    {
                        card.SetInteractable(true);
                    }
                }
                else
                {
                    card.SetInteractable(false);
                }
            }
        }

        // this method can be utilised for displaying the next match value on streak
        // public int GetNextMatchValue()
        // {
        //     int baseScore = 100;
        //     int nextStreakBonus = streak * 50; // What the bonus would be for the NEXT match
        //     return baseScore + nextStreakBonus;
        // }

        private void DispatchScoreUpdate()
        {
            var scoreData = new ScoreData
            {
                GameTime = Time.time - gameStartTime,
                TotalTurns = totalTurns,
                TotalMatches = totalMatches,
                TotalComboStreaks = (streak - 1) < 0 ? 0 : streak - 1,
                TotalScore = totalScore
            };

            EventDispatcher.Instance.Dispatch(EventConstants.ON_SCORE_UPDATED, scoreData);
            Debug.Log($"Score updated: Turns: {totalTurns}, Matches: {totalMatches}, Current Streak: {streak}, Best Streak: {bestComboStreak}, Score: {totalScore}");
        }
    }
}