using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace CyberSpeed.SerialisedClasses
{
    [Serializable]
    public class ScoreData
    {
        public float GameTime;
        public int TotalTurns;
        public int TotalMatches;
        public int TotalComboStreaks;
        // public int TotalScore;
    }
}