using UnityEngine;
using System.Collections.Generic;
using CyberSpeed.UI;
using CyberSpeed.SO;
using CyberSpeed.SerialisedClasses;
using System;

namespace CyberSpeed.Utils
{
    /// <summary>
    /// Simple factory for creating game cards
    /// </summary>
    public class CardFactory
    {
        private ObjectPool cardPool;
        private CardSO cardData;

        public CardFactory(ObjectPool pool, CardSO data)
        {
            cardPool = pool;
            cardData = data;
        }

        /// <summary>
        /// Creates a new game card
        /// </summary>
        public GameCard CreateCard(int cardID, Transform parent, Action<GameCard> onCardClicked)
        {
            var cardObj = cardPool.GetObjectFromPool();
            cardObj.transform.SetParent(parent, false);
            cardObj.gameObject.SetActive(true);

            var gameCard = cardObj.GetComponent<GameCard>();
            var cardInfo = cardData.GetCard(cardID);
            gameCard.InitCard(cardInfo.cardID, cardInfo.cardSprite, onCardClicked);

            return gameCard;
        }

        /// <summary>
        /// Creates a saved game card with state
        /// </summary>
        public GameCard CreateSavedCard(int cardID, Transform parent, bool isFlipped, bool isMatched, Action<GameCard> onCardClicked)
        {
            var cardObj = cardPool.GetObjectFromPool();
            cardObj.transform.SetParent(parent, false);
            cardObj.gameObject.SetActive(true);

            var gameCard = cardObj.GetComponent<GameCard>();
            var cardInfo = cardData.GetCard(cardID);
            gameCard.InitSavedCard(cardInfo.cardID, cardInfo.cardSprite, isFlipped, isMatched, onCardClicked);

            return gameCard;
        }

        /// <summary>
        /// Creates multiple cards at once
        /// </summary>
        public List<GameCard> CreateCards(List<int> cardIDs, Transform parent, Action<GameCard> onCardClicked)
        {
            var cards = new List<GameCard>();

            foreach (var cardID in cardIDs)
            {
                var card = CreateCard(cardID, parent, onCardClicked);
                cards.Add(card);
            }

            return cards;
        }

        /// <summary>
        /// Releases all cards in the pool
        /// </summary>
        public void ReleaseAllCards()
        {
            cardPool.ReleaseAll();
        }
    }
}
