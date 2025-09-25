using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CyberSpeed.Utils;
using CyberSpeed.Manager;
using CyberSpeed.SerialisedClasses;
using CyberSpeed.SO;
using DifficultyLevelData = CyberSpeed.SO.DifficultyLevelSO.DifficultyLevelData;
using UnityEngine.UI;

namespace CyberSpeed.UI
{
    public class GameController : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private GameGrid cardGrid;

        // Cached components for performance
        private GridLayoutGroup gridLayoutGroup;

        void Awake()
        {
            root.gameObject.SetActive(false);
            // Cache expensive component reference
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
            Debug.Log($"Starting level: {levelData.levelName}");
            root.gameObject.SetActive(true);

            if (!levelData.ValidateLevelData()) return;

            SetupGridLayout(levelData.colsCount);
            int totalGridElements = levelData.rowsCount * levelData.colsCount;

            List<int> shuffledCardIDs = GenerateShuffledCardPairs(totalGridElements);
            SpawnCards(shuffledCardIDs);
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

            // Get random unique card indices
            List<int> cardIDs = Utilities.GetRandomIntList(cardData.cardDataList.Count, totalUniqueItems);

            // Create pairs by duplicating the list
            List<int> cardPairs = new List<int>(totalGridElements);
            cardPairs.AddRange(cardIDs);
            cardPairs.AddRange(cardIDs);

            // Shuffle the pairs
            cardPairs.Shuffle();

            Debug.Log($"Generated {totalUniqueItems} unique card pairs for grid");
            return cardPairs;
        }

        private void SpawnCards(List<int> cardIDs)
        {
            GameManager gameManager = GameManager.Instance;
            CardSO cardData = gameManager.GetCardData();
            var objPool = gameManager.GetObjectPool();
            Transform gridTransform = cardGrid.transform;

            for (int i = 0; i < cardIDs.Count; i++)
            {
                var cardInfo = cardData.cardDataList[cardIDs[i]];
                var cardObj = objPool.Get();

                // Set parent to grid and reset transform
                cardObj.transform.SetParent(gridTransform, false);

                // Initialize card with cached component reference
                var gameCard = cardObj.GetComponent<GameCard>();
                gameCard.InitCard(cardInfo.cardID, cardInfo.cardSprite, CardClicked);
            }
        }

        List<int> openCardIds = new List<int>();
        private void CardClicked(GameCard gameCard)
        {
            int clickedCardID = gameCard.CardID;

            bool isMatch = openCardIds.Contains(clickedCardID);

            if (isMatch)
            {
                //this is double card opened
                openCardIds.Remove(clickedCardID);
            }
            else
            {
                //this is single card opened
                openCardIds.Add(clickedCardID);
            }

            // if (openCardIds.Count == 2)
            // {
            //     Debug.Log("Match found");
            //     gameCard.Matched();
            //     openCardIds.Clear();
            // }
        }
    }
}