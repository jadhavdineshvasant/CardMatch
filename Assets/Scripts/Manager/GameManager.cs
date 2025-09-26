using System.Collections;
using System.Collections.Generic;
using CyberSpeed.Utils;
using UnityEngine;
using CyberSpeed.UI;
using CyberSpeed.SO;
using DifficultyLevelData = CyberSpeed.SO.DifficultyLevelSO.DifficultyLevelData;
using System;

namespace CyberSpeed.Manager
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Scriptable Objects References")]
        [SerializeField] private DifficultyLevelSO difficultyLevelSO;
        [SerializeField] private CardSO cardSO;

        [Header("UI Screens")]
        [SerializeField] private LevelSelectUI levelSelectUIHandler;
        [SerializeField] private IntroScreenUI introUIHandler;
        [SerializeField] private ResultScreenUI resultScreenUIHandler;
        [SerializeField] private ExitPopupUI exitUIHandler;
        [SerializeField] private SavePopupUI saveUIHandler;
        [SerializeField] private GameController gameHandler;

        public static event Action OnExitYes;

        private DifficultyLevelData selectedLevel;

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
            HideAllScreens();
            ShowIntroUI();
        }

        public void OnIntroPlayClicked()
        {
            HideIntroUI();
            ShowLevelSelectUI();
        }

        #region Result Screen Show/Hide
        public void ShowResultScreenUI()
        {
            resultScreenUIHandler.gameObject.SetActive(true);
        }

        public void HideResultScreenUI()
        {
            resultScreenUIHandler.gameObject.SetActive(false);
        }
        #endregion

        #region Intro Screen Show/Hide
        public void ShowIntroUI()
        {
            AudioManager.Instance.PlayBGMusic();
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

        private void OnLevelClicked(DifficultyLevelData levelData)
        {
            selectedLevel = levelData;
            HideAllScreens();
            gameHandler.OnLevelStarted(levelData);
        }
        #endregion

        #region Exit Popup Show/Hide
        public void ShowExitPopupUI()
        {
            exitUIHandler.gameObject.SetActive(true);
        }

        public void ExitPopupSaveYes()
        {
            // call save functionality from here
            SaveGameProgress();
            OnExitYes?.Invoke();
            ShowIntroUI();
        }

        public void ExitPopupSaveNo()
        {
            OnExitYes?.Invoke();
            ShowIntroUI();
            // dont save anything : do exit
        }

        public void HideExitPopupUI()
        {
            exitUIHandler.gameObject.SetActive(false);
        }
        #endregion

        #region Game Save Popup Show/Hide
        public void ShowSavePopupUI()
        {
            saveUIHandler.gameObject.SetActive(true);
        }

        public void SaveGameYes()
        {
            // call save functionality from here
            SaveGameProgress();
        }

        public void HideSavePopupUI()
        {
            saveUIHandler.gameObject.SetActive(false);
        }
        #endregion
        public void OnPlayAgainClicked()
        {
            HideAllScreens();
            ShowLevelSelectUI();
        }

        public void OnHomeClicked()
        {
            HideAllScreens();
            ShowIntroUI();
        }

        private void HideAllScreens()
        {
            levelSelectUIHandler.gameObject.SetActive(false);
            introUIHandler.gameObject.SetActive(false);
            resultScreenUIHandler.gameObject.SetActive(false);
            exitUIHandler.gameObject.SetActive(false);
        }

        private void SaveGameProgress()
        {
            SaveManager.Instance.SaveGameProgress();
        }

        public CardSO GetCardData()
        {
            return cardSO;
        }

        public DifficultyLevelSO GetLevelData()
        {
            return difficultyLevelSO;
        }

        public DifficultyLevelData GetSlectedLevelData()
        {
            return selectedLevel;
        }
    }
}