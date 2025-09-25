using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSaveData
{
    public int rows;
    public int cols;
    public int turns;
    public int matches;
    public int streak;
    public int score;
    public int gameTimer;
    public List<int> cardValues;
    public List<bool> cardMatched;
}