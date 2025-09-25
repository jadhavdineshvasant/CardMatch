using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DifficultyLevelData = CyberSpeed.SO.DifficultyLevelSO.DifficultyLevelData;
using CyberSpeed.SO;
using CyberSpeed.Manager;

public static class Utilities
{
    /// <summary>
    /// Generate a random list of unique integers from 0 to maxValue-1.
    /// </summary>
    public static List<int> GetRandomIntList(int maxValue, int count)
    {
        if (count > maxValue) count = maxValue;

        // Fill list with [0..maxValue-1]
        List<int> numbers = new List<int>();
        for (int i = 0; i < maxValue; i++)
            numbers.Add(i);

        // Shuffle
        for (int i = 0; i < numbers.Count; i++)
        {
            int randIndex = Random.Range(i, numbers.Count);
            int temp = numbers[i];
            numbers[i] = numbers[randIndex];
            numbers[randIndex] = temp;
        }

        return numbers.GetRange(0, count);
    }

    /// <summary>
    /// Shuffle the provided int list in place using Fisher-Yates algorithm.
    /// </summary>
    public static void Shuffle(this List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            int temp = list[i];
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="levelData"></param>
    /// <returns></returns>
    public static bool ValidateLevelData(this DifficultyLevelData levelData)
    {
        int totalGridElements = levelData.rowsCount * levelData.colsCount;

        if (totalGridElements % 2 != 0)
        {
            Debug.LogError($"Grid size must be even for card matching! Current: {totalGridElements}");
            return false;
        }

        int requiredUniqueCards = totalGridElements / 2;
        CardSO cardData = GameManager.Instance.GetCardData();

        if (cardData.cardDataList.Count < requiredUniqueCards)
        {
            Debug.LogError($"Not enough card types! Required: {requiredUniqueCards}, Available: {cardData.cardDataList.Count}");
            return false;
        }

        return true;
    }
}
