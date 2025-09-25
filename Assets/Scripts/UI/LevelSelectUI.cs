using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI;
using CyberSpeed.SO;
using System;
using DifficultyLevelData = CyberSpeed.SO.DifficultyLevelSO.DifficultyLevelData;

namespace CyberSpeed.UI
{
    public class LevelSelectUI : MonoBehaviour
    {
        [SerializeField] private GameObject levelTilePrefab;
        [SerializeField] private Transform levelTileContainer;
        // [SerializeField] private DifficultyLevelSO difficultyLevelSO;

        public void InitLevels(DifficultyLevelSO difficultyLevelSO, Action<DifficultyLevelData> onLevelClicked)
        {
            foreach (var o in levelTileContainer.GetComponentsInChildren<LevelTile>())
            {
                Destroy(o.gameObject);
            }

            foreach (var levelData in difficultyLevelSO.levelDataList)
            {
                CreateLevelTile(levelData, onLevelClicked);
            }
        }

        private void CreateLevelTile(DifficultyLevelSO.DifficultyLevelData levelData, Action<DifficultyLevelData> onLevelClicked)
        {
            var levelTile = Instantiate(levelTilePrefab, levelTileContainer);
            levelTile.GetComponent<LevelTile>().Init(levelData, onLevelClicked);
        }
    }
}