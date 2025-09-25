using System.Collections;
using System.Collections.Generic;
using CyberSpeed.Utils;
using CyberSpeed.Manager;
using UnityEngine;
using TMPro;
using CyberSpeed.SerialisedClasses;

namespace CyberSpeed.UI
{
    public class GameScoreUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI turnTxt;
        [SerializeField] private TextMeshProUGUI matchTxt;
        [SerializeField] private TextMeshProUGUI streakTxt;
        [SerializeField] private TextMeshProUGUI scoreTxt;

        private void OnEnable()
        {
            EventDispatcher.Instance.Subscribe<ScoreData>(EventConstants.ON_SCORE_UPDATED, OnScoreUpdated);
        }

        private void OnDisable()
        {
            EventDispatcher.Instance.Unsubscribe<ScoreData>(EventConstants.ON_SCORE_UPDATED, OnScoreUpdated);
        }

        private void OnScoreUpdated(ScoreData scoreData)
        {
            turnTxt.text = $"Turns: {scoreData.TotalTurns}";
            matchTxt.text = $"Matches: {scoreData.TotalMatches}";
            streakTxt.text = $"Streaks: {scoreData.TotalComboStreaks}";
            scoreTxt.text = $"Score: {scoreData.TotalScore}";
        }
    }
}