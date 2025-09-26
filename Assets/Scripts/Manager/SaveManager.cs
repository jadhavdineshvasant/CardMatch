using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CyberSpeed.UI;
using CyberSpeed.Utils;
using System.IO;
using System.Linq;

namespace CyberSpeed.Manager
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void SaveGameProgress()
        {
            var matchManager = MatchManager.Instance;
            var gameManager = GameManager.Instance;

            List<GameCard> activeCards = new List<GameCard>();
            activeCards = matchManager.GetActiveCards();

            var levelData = gameManager.GetSlectedLevelData();

            string fileName = $"{levelData.rowsCount}_{levelData.colsCount}.json";

            string savePath = Path.Combine(Application.persistentDataPath, fileName);

            var saveData = new GameSaveData
            {
                rows = levelData.rowsCount,
                cols = levelData.colsCount,
                turns = matchManager.totalTurns,
                matches = matchManager.totalMatches,
                streak = matchManager.streak,
                score = matchManager.totalScore,
                gameTimer = Time.time - matchManager.gameStartTime,
                cardID = activeCards.Select(card => card.CardID).ToList(),
                cardMatched = activeCards.Select(card => card.IsMatched).ToList()
            };

            // string json = JsonUtility.ToJson(saveData, true);
            string json = JsonUtility.ToJson(saveData, false);

            Debug.Log(savePath);
            Debug.Log(json);

            File.WriteAllText(savePath, json);
        }
    }
}