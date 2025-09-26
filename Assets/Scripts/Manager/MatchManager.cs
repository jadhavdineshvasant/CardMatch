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

        private GameCard openCard = null;
        private List<GameCard> activeCards = new List<GameCard>();
        private List<GameCard> matchedCards = new List<GameCard>();
        private bool isMatchingInProgress = false;

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

            Debug.Log($"Game initialized with {activeCards.Count} cards");
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

            float waitTime = isMatch ? 0.3f : 0.8f;

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

            int baseScore = 100;
            int streakBonus = (streak - 1) * 50;
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
                AudioManager.Instance.PlayResultScreenSFX();
                StartCoroutine(HandleGameComplete());
            }
        }

        private IEnumerator HandleGameComplete()
        {
            isMatchingInProgress = true;
            yield return new WaitForSeconds(1f);

            var finalScoreData = new ScoreData
            {
                GameTime = Time.time - gameStartTime,
                TotalTurns = totalTurns,
                TotalMatches = totalMatches,
                TotalComboStreaks = (streak - 1) < 0 ? 0 : streak - 1,
                TotalScore = totalScore
            };

            GameManager.Instance.ShowResultScreenUI();
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

        public List<GameCard> GetActiveCards()
        {
            return activeCards;
        }
    }
}