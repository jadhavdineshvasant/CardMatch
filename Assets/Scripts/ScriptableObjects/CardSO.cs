using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyberSpeed.SO
{
    [CreateAssetMenu(fileName = "CardData", menuName = "ScriptableObjects/CardData", order = 2)]
    public class CardSO : ScriptableObject
    {
        [System.Serializable]
        public class CardData
        {
            public int cardID;
            public Sprite cardSprite;
        }

        public List<CardData> cardDataList;

        public CardData GetCard(int index)
        {
            var card = cardDataList.Find(X => X.cardID == index);
            if (card == null)
            {
                Debug.LogWarning($"Card with ID {index} not found");
                return null;
            }
            return card;
        }

        public Sprite GetCardSprite(int index)
        {
            var card = GetCard(index);
            if (card == null)
            {
                Debug.LogWarning($"Card with ID {index} not found");
                return null;
            }
            return card.cardSprite;
        }
    }
}