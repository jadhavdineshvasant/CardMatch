using System.Collections;
using System.Collections.Generic;
using CyberSpeed.Manager;
using UnityEngine;
using TMPro;
using CyberSpeed.Utils;
using CyberSpeed.SerialisedClasses;
using UnityEngine.UI;

namespace CyberSpeed.UI
{
    public class ResultScreenUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI gameTimeTxt;
        [SerializeField] private TextMeshProUGUI totalTurnsTxt;
        [SerializeField] private TextMeshProUGUI matchCountTxt;
        [SerializeField] private TextMeshProUGUI comboStreaksTxt;
        [SerializeField] private TextMeshProUGUI totalScoreTxt;

        [Space(10)][SerializeField] Button playAgainBtn;
        [SerializeField] Button homeBtn;

        private void OnEnable()
        {
            EventDispatcher.Instance.Subscribe<ScoreData>(EventConstants.ON_GAME_RESULT, OnGameOver);
            playAgainBtn.onClick.AddListener(OnPlayAgainButtonClicked);
            homeBtn.onClick.AddListener(OnHomeButtonClicked);
        }

        private void OnDisable()
        {
            EventDispatcher.Instance.Unsubscribe<ScoreData>(EventConstants.ON_GAME_RESULT, OnGameOver);
            playAgainBtn.onClick.RemoveListener(OnPlayAgainButtonClicked);
            homeBtn.onClick.RemoveListener(OnHomeButtonClicked);
        }

        private void OnGameOver(ScoreData scoreData)
        {
            gameTimeTxt.text = scoreData.GameTime.ToString("F2") + "s";
            totalTurnsTxt.text = scoreData.TotalTurns.ToString();
            matchCountTxt.text = scoreData.TotalMatches.ToString();
            comboStreaksTxt.text = scoreData.TotalComboStreaks.ToString();
            totalScoreTxt.text = scoreData.TotalScore.ToString();
        }

        private void OnPlayAgainButtonClicked()
        {
            GameManager.Instance.OnPlayAgainClicked();
        }

        private void OnHomeButtonClicked()
        {
            GameManager.Instance.OnHomeClicked();
        }
    }
}