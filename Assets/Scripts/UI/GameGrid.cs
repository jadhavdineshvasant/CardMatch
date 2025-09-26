using System.Collections.Generic;
using UnityEngine;
using CyberSpeed.Utils;

namespace CyberSpeed.UI
{
    /// <summary>
    /// Simple GameGrid with basic card access methods
    /// </summary>
    public class GameGrid : MonoBehaviour
    {
        private List<GameCard> cards = new List<GameCard>();

        /// <summary>
        /// Gets all cards in the grid
        /// </summary>
        public List<GameCard> GetAllCards()
        {
            return new List<GameCard>(cards);
        }

        /// <summary>
        /// Gets a card by index
        /// </summary>
        public GameCard GetCard(int index)
        {
            if (index >= 0 && index < cards.Count)
                return cards[index];
            return null;
        }

        /// <summary>
        /// Gets cards by their ID
        /// </summary>
        public List<GameCard> GetCardsByID(int cardID)
        {
            return cards.FindAll(card => card.CardID == cardID);
        }

        /// <summary>
        /// Gets total number of cards
        /// </summary>
        public int GetCardCount()
        {
            return cards.Count;
        }

        /// <summary>
        /// Adds cards to the grid
        /// </summary>
        public void SetCards(List<GameCard> newCards)
        {
            cards = new List<GameCard>(newCards);
        }

        /// <summary>
        /// Clears all cards from the grid
        /// </summary>
        public void ClearCards()
        {
            cards.Clear();
        }

        /// <summary>
        /// Checks if all cards are matched
        /// </summary>
        public bool AreAllCardsMatched()
        {
            foreach (var card in cards)
            {
                if (!card.IsMatched)
                    return false;
            }
            return cards.Count > 0;
        }

        /// <summary>
        /// Gets matched cards count
        /// </summary>
        public int GetMatchedCardsCount()
        {
            int count = 0;
            foreach (var card in cards)
            {
                if (card.IsMatched)
                    count++;
            }
            return count;
        }
    }
}