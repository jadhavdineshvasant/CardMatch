using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyberSpeed.SO
{
    [CreateAssetMenu(fileName = "DifficultyLevelData", menuName = "ScriptableObjects/DifficultyLevelData", order = 1)]
    public class DifficultyLevelSO : ScriptableObject
    {
        [System.Serializable]
        public class DifficultyLevelData
        {
            public string levelName;
            public Sprite levelSprite;
            public float previewDuration;
            public int rowsCount;
            public int colsCount;
            public int scoreMultiplier;
            public Color titleColor;

        }

        public List<DifficultyLevelData> levelDataList;
    }
}