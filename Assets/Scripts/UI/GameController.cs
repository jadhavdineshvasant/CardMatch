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
        [SerializeField] private GameObject memoriseMSG;
        [SerializeField] private Image memoriseMsgBar;

        // Cached components for performance
        private GridLayoutGroup gridLayoutGroup;

        // Game state
        private List<GameCard> activeCards = new List<GameCard>();
        private List<GameCard> matchedCards = new List<GameCard>();
        private bool isPreviewMode = false;
        private bool isMatchingInProgress = false;

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

            // Reset game state
            ResetGameState();

            SetupGridLayout(levelData.colsCount);
            int totalGridElements = levelData.rowsCount * levelData.colsCount;

            List<int> shuffledCardIDs = GenerateShuffledCardPairs(totalGridElements);
            SpawnCards(shuffledCardIDs);

            // Start preview mode
            StartCoroutine(PreviewGrid(levelData.previewDuration));
        }

        private void ResetGameState()
        {
            // Stop any ongoing coroutines
            StopAllCoroutines();

            // Reset game state variables
            openCard = null;
            isPreviewMode = false;
            isMatchingInProgress = false;
            matchedCards.Clear();

            // Hide memorize message if it's showing
            memoriseMSG.SetActive(false);

            Debug.Log("Game state reset for new level");
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
            // Clear previous cards
            activeCards.Clear();

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

                // Add to active cards list for preview control
                activeCards.Add(gameCard);
            }
        }

        private IEnumerator PreviewGrid(float previewDuration)
        {
            // Show memorize message and initialize fillbar
            memoriseMSG.SetActive(true);
            memoriseMsgBar.fillAmount = 1f;

            // Brief delay before starting preview
            yield return new WaitForSeconds(0.25f);

            isPreviewMode = true;
            Debug.Log($"Starting card preview for {previewDuration} seconds");

            // Show all cards face up with animation and disable interaction
            foreach (var card in activeCards)
            {
                card.FlipToFront(animate: true);
                card.DisableInteraction();
            }

            // Wait for card flip animations to complete
            yield return new WaitForSeconds(0.5f);

            // Preview countdown with fillbar
            float timer = previewDuration;
            while (timer > 0)
            {
                timer -= Time.deltaTime;
                // Update fillbar (inverted so it empties as time runs out)
                memoriseMsgBar.fillAmount = timer / previewDuration;
                yield return null;
            }

            // Ensure fillbar is empty at the end
            memoriseMsgBar.fillAmount = 0f;

            Debug.Log("Preview time up - hiding cards");

            // Flip all cards back face down with animation
            foreach (var card in activeCards)
            {
                card.FlipToBack(animate: true);
            }

            // Wait for flip animations to complete before enabling interaction
            yield return new WaitForSeconds(0.5f);

            // Enable interaction for gameplay
            foreach (var card in activeCards)
            {
                card.EnableInteraction();
            }

            // Hide memorize message
            memoriseMSG.SetActive(false);

            isPreviewMode = false;
            Debug.Log("Preview complete - gameplay started");
        }

        private GameCard openCard = null;

        private void CardClicked(GameCard gameCard)
        {
            // Prevent interaction during preview mode or while matching is in progress
            if (isPreviewMode || isMatchingInProgress) return;

            // Prevent clicking already flipped cards (matched cards or currently selected card)
            if (gameCard.IsFlipped && gameCard != openCard) return;

            // Prevent clicking the same card twice
            if (gameCard == openCard) return;

            if (openCard == null)
            {
                // First card selected
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
            // Set matching in progress and disable all card interactions
            isMatchingInProgress = true;
            SetAllCardsInteractable(false);

            int clickedCardID = gameCard.CardID;
            bool isMatch = openCard.CardID == clickedCardID;

            Debug.Log($"Checking match: Card1 ID={openCard.CardID}, Card2 ID={clickedCardID}");

            // Give player time to see both cards
            yield return new WaitForSeconds(0.75f);

            if (isMatch)
            {
                Debug.Log("Match found!");
                openCard.Matched();
                gameCard.Matched();

                // Track matched cards
                matchedCards.Add(openCard);
                matchedCards.Add(gameCard);

                // TODO: Add score, check for win condition, etc.
            }
            else
            {
                Debug.Log("No match - flipping cards back");
                openCard.FlipToBack();
                gameCard.FlipToBack();
            }

            // Reset state for next turn
            openCard = null;
            isMatchingInProgress = false;

            // Re-enable interaction for all non-matched cards
            SetAllCardsInteractable(true);
        }

        private void SetAllCardsInteractable(bool interactable)
        {
            foreach (var card in activeCards)
            {
                if (interactable)
                {
                    // When enabling, only enable non-matched cards
                    if (!matchedCards.Contains(card))
                    {
                        card.SetInteractable(true);
                    }
                }
                else
                {
                    // When disabling, disable ALL cards regardless of match status
                    card.SetInteractable(false);
                }
            }
        }
    }
}