using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyberSpeed.SerialisedClasses
{
    [System.Serializable]
    public class GameSaveData
    {
        public int rows;
        public int cols;
        public int turns;
        public int matches;
        public int streak;
        public int score;
        public float gameTimer;
        public List<int> cardID;
        public List<bool> cardMatched;
    }
}