using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CyberSpeed.Utils;
using CyberSpeed.SerialisedClasses;
using CyberSpeed.UI;
using DifficultyLevelData = CyberSpeed.SO.DifficultyLevelSO.DifficultyLevelData;
using System.Linq;
using System.IO;

namespace CyberSpeed.Manager
{
    public class MatchManager : MonoBehaviour
    {
        public static MatchManager Instance { get; private set; }

        // Configuration constants
        private const float MATCH_SUCCESS_DELAY = 0.3f;
        private const float MATCH_FAIL_DELAY = 0.8f;
        private const float GAME_COMPLETE_DELAY = 1f;

        private GameCard openCard = null;
        private List<GameCard> activeCards = new List<GameCard>();
        private HashSet<GameCard> matchedCards = new HashSet<GameCard>();
        private bool isMatchingInProgress = false;

        private GameManager gameManager;
        private DifficultyLevelData levelData;

        public int streak { get; private set; }
        public int totalScore { get; private set; }
        public int totalTurns { get; private set; }
        public int totalMatches { get; private set; }
        public int bestComboStreak { get; private set; }
        public float gameStartTime { get; private set; }

        public bool IsMatchingInProgress => isMatchingInProgress;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                gameManager = GameManager.Instance;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void OnEnable()
        {
            EventDispatcher.Instance.Subscribe(EventConstants.ON_GAME_CLEANUP, OnGameCleanup);
        }

        private void OnDisable()
        {
            EventDispatcher.Instance.Unsubscribe(EventConstants.ON_GAME_CLEANUP, OnGameCleanup);
        }

        private void OnGameCleanup()
        {
            // Reset all game state when cleanup is triggered
            FullReset();
        }

        public void InitializeGame(List<GameCard> cards)
        {
            // Reset stats first, then set up the cards
            ResetGameStats();

            activeCards = new List<GameCard>(cards);
            matchedCards.Clear();

            // Cache level data for performance
            levelData = gameManager.GetSlectedLevelData();

            Debug.Log($"Game initialized with {activeCards.Count} cards");
        }

        public void InitializeSavedGame(List<GameCard> cards, GameSaveData savedLevelData)
        {
            InitializeGame(cards);

            // Restore saved game stats
            streak = savedLevelData.streak;
            totalScore = savedLevelData.score;
            totalTurns = savedLevelData.turns;
            totalMatches = savedLevelData.matches;
            bestComboStreak = savedLevelData.streak;

            // Add already matched cards to matchedCards list
            for (int i = 0; i < cards.Count && i < savedLevelData.cardMatched.Count; i++)
            {
                if (savedLevelData.cardMatched[i] && cards[i] != null)
                {
                    matchedCards.Add(cards[i]);
                }
            }

            Debug.Log($"Saved game loaded: {matchedCards.Count} cards already matched");
        }

        public void StartGame()
        {
            gameStartTime = Time.time;
            DispatchScoreUpdate();
        }

        public void HandleCardClick(GameCard gameCard)
        {
            if (isMatchingInProgress) return;
            if (gameCard.IsFlipped && gameCard != openCard) return;
            if (gameCard == openCard) return;

            AudioManager.Instance.PlayCardFlipSFX();

            if (openCard == null)
            {
                openCard = gameCard;
                Debug.Log($"First card selected: ID {gameCard.CardID}");
                return;
            }

            Debug.Log($"Second card selected: ID {gameCard.CardID}");
            StartCoroutine(ProcessMatch(gameCard));
        }

        private IEnumerator ProcessMatch(GameCard gameCard)
        {
            isMatchingInProgress = true;
            SetAllCardsInteractable(false);
            totalTurns++;

            int clickedCardID = gameCard.CardID;
            bool isMatch = openCard.CardID == clickedCardID;

            Debug.Log($"Checking match: Card1 ID={openCard.CardID}, Card2 ID={clickedCardID}");

            float waitTime = isMatch ? MATCH_SUCCESS_DELAY : MATCH_FAIL_DELAY;

            yield return new WaitForSeconds(waitTime);

            if (isMatch)
            {
                ProcessSuccessfulMatch(gameCard);
            }
            else
            {
                ProcessFailedMatch(gameCard);
            }

            openCard = null;
            isMatchingInProgress = false;
            SetAllCardsInteractable(true);
        }

        private void ProcessSuccessfulMatch(GameCard gameCard)
        {
            AudioManager.Instance.PlayCardMatchSuccessSFX();

            streak++;
            totalMatches++;

            if (streak > bestComboStreak)
            {
                bestComboStreak = streak;
            }

            int baseScore = levelData.baseScore;
            int streakBonus = (streak - 1) * (baseScore / 2);
            int matchScore = baseScore + streakBonus;
            totalScore += matchScore;

            Debug.Log($"Match found! Streak: {streak} | Match Score: {matchScore} | Total Score: {totalScore}");

            openCard.Matched();
            gameCard.Matched();
            matchedCards.Add(openCard);
            matchedCards.Add(gameCard);

            Debug.Log($"Cards matched! Now have {matchedCards.Count} matched cards out of {activeCards.Count} total");

            DispatchScoreUpdate();
            CheckWinCondition();
        }

        private void ProcessFailedMatch(GameCard gameCard)
        {
            AudioManager.Instance.PlayCardMatchFailSFX();

            streak = 0;
            openCard.FlipToBack();
            gameCard.FlipToBack();
            DispatchScoreUpdate();
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

        private void CheckWinCondition()
        {
            Debug.Log($"Win Check: Matched Cards = {matchedCards.Count}, Active Cards = {activeCards.Count}");

            if (matchedCards.Count == activeCards.Count)
            {
                Debug.Log("WIN CONDITION MET! Starting game completion...");
                gameManager.ClearCurrentLevelData();
                AudioManager.Instance.PlayResultScreenSFX();
                StartCoroutine(HandleGameComplete());
            }
        }

        private IEnumerator HandleGameComplete()
        {
            isMatchingInProgress = true;
            yield return new WaitForSeconds(GAME_COMPLETE_DELAY);

            float gameTime = Time.time - gameStartTime;
            var finalScoreData = new ScoreData
            {
                GameTime = gameTime,
                TotalTurns = totalTurns,
                TotalMatches = totalMatches,
                TotalComboStreaks = (streak - 1) < 0 ? 0 : streak - 1,
                TotalScore = totalScore
            };

            gameManager.ShowResultScreenUI();
            EventDispatcher.Instance.Dispatch(EventConstants.ON_GAME_RESULT, finalScoreData);
            Debug.Log($"Game Complete! Final Score: {totalScore}, Time: {finalScoreData.GameTime:F2}s");
        }

        private void ResetGameStats()
        {
            openCard = null;
            isMatchingInProgress = false;

            streak = 0;
            totalScore = 0;
            totalTurns = 0;
            totalMatches = 0;
            bestComboStreak = 0;
        }

        private void FullReset()
        {
            ResetGameStats();

            // Clear card tracking lists completely
            activeCards.Clear();
            matchedCards.Clear();
        }

        private void DispatchScoreUpdate()
        {
            float gameTime = Time.time - gameStartTime;
            var scoreData = new ScoreData
            {
                GameTime = gameTime,
                TotalTurns = totalTurns,
                TotalMatches = totalMatches,
                TotalComboStreaks = (streak - 1) < 0 ? 0 : streak - 1,
                TotalScore = totalScore
            };

            EventDispatcher.Instance.Dispatch(EventConstants.ON_SCORE_UPDATED, scoreData);

            Debug.Log($"Score updated: Turns: {totalTurns}, Matches: {totalMatches}, Current Streak: {streak}, Best Streak: {bestComboStreak}, Score: {totalScore}");
        }

        public List<GameCard> GetActiveCards() => activeCards;
    }
}