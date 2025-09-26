using System.Collections.Generic;
using UnityEngine;

namespace CyberSpeed.UI
{
    /// <summary>
    /// Simple GameGrid for card management
    /// </summary>
    public class GameGrid : MonoBehaviour
    {
        private List<GameCard> cards = new List<GameCard>();

        /// <summary>
        /// Sets cards to the grid
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
    }
}