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
        [SerializeField] private CardSO cardSO;

        [Header("Object Pool References")]
        [SerializeField] private ObjectPool cardPool;

        [Header("UI Screens")]
        [SerializeField] private LevelSelectUI levelSelectUIHandler;
        [SerializeField] private IntroScreenUI introUIHandler;
        [SerializeField] private ResultScreenUI resultScreenUIHandler;

        // [SerializeField] private CardsGrid cardGrid;

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
            selectedLevel = levelData;
            HideAllScreens();
            EventDispatcher.Instance.Dispatch(EventConstants.ON_LEVEL_STARTED, levelData);
        }

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
        }

        // public void OnGameOver(ScoreData scoreData)
        // {
        //     ShowResultScreenUI();
        // }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                SpawnCard(new Vector3(0, 0, 0));
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                // HideCard();
                for (int i = 0; i < 20; i++)
                {
                    SpawnCard(new Vector3(0, 0, 0));
                }
            }
        }

        void SpawnCard(Vector3 position)
        {
            GameObject card = cardPool.Get();
            card.transform.position = position;
        }

        void HideCard(GameObject card)
        {
            cardPool.Release(card);
        }

        public CardSO GetCardData()
        {
            return cardSO;
        }

        public DifficultyLevelSO GetLevelData()
        {
            return difficultyLevelSO;
        }

        public ObjectPool GetObjectPool()
        {
            return cardPool;
        }
    }
}