using System.Collections;
using System.Collections.Generic;
using CyberSpeed.Utils;
using UnityEngine;
using CyberSpeed.UI;
using CyberSpeed.SO;
using DifficultyLevelData = CyberSpeed.SO.DifficultyLevelSO.DifficultyLevelData;

namespace CyberSpeed.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scriptable Objects References")]
        [SerializeField] private DifficultyLevelSO difficultyLevelSO;

        [Header("UI Screens")]
        [SerializeField] private LevelSelectUI levelSelectUIHandler;
        [SerializeField] private IntroScreenUI introUIHandler;

        DifficultyLevelData selectedLevel;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            InitGame();
        }

        void InitGame()
        {
            ShowIntroUI();
            HideLevelSelectUI();
        }

        public void OnIntroPlayClicked()
        {
            HideIntroUI();
            ShowLevelSelectUI();
        }

        #region Intro Screen Show/Hide
        public void ShowIntroUI()
        {
            introUIHandler.gameObject.SetActive(true);
        }

        public void HideIntroUI()
        {
            introUIHandler.gameObject.SetActive(false);
        }
        #endregion

        #region Level Select Screen Show/Hide
        public void ShowLevelSelectUI()
        {
            levelSelectUIHandler.InitLevels(difficultyLevelSO, OnLevelClicked);
            levelSelectUIHandler.gameObject.SetActive(true);
        }

        public void HideLevelSelectUI()
        {
            levelSelectUIHandler.gameObject.SetActive(false);
        }
        #endregion

        private void OnLevelClicked(DifficultyLevelData levelData)
        {
            Debug.Log($"level clicked {levelData.levelName}");
            selectedLevel = levelData;
        }
    }
}