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
        private bool isPreviewMode = false;
        private float gameStartTime = 0f;

        void Awake()
        {
            root.gameObject.SetActive(false);
            gridLayoutGroup = cardGrid.GetComponent<GridLayoutGroup>();
        }

        public void OnLevelStarted(DifficultyLevelData levelData)
        {
            root.gameObject.SetActive(true);

            if (!levelData.ValidateLevelData()) return;

            ResetUIState();
            SetupGridLayout(levelData.colsCount);
            int totalGridElements = levelData.rowsCount * levelData.colsCount;

            List<int> shuffledCardIDs = GenerateShuffledCardPairs(totalGridElements);
            SpawnCards(shuffledCardIDs);

            // Initialize the match manager with the spawned cards
            MatchManager.Instance.InitializeGame(activeCards);

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
                gameCard.InitCard(cardInfo.cardID, cardInfo.cardSprite, MatchManager.Instance.HandleCardClick);
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

            // Start the match manager game and timer
            MatchManager.Instance.StartGame();
            StartCoroutine(UpdateGameTimer());
        }

        private IEnumerator UpdateGameTimer()
        {
            while (!MatchManager.Instance.IsMatchingInProgress && !isPreviewMode)
            {
                float currentTime = Time.time - gameStartTime;
                int minutes = Mathf.FloorToInt(currentTime / 60);
                int seconds = Mathf.FloorToInt(currentTime % 60);
                gameTimer.text = $"{minutes:00}:{seconds:00}";
                yield return new WaitForSeconds(1f);
            }
        }
    }
}