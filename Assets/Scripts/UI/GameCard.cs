using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;


namespace CyberSpeed.UI
{
    public class GameCard : MonoBehaviour, IPointerClickHandler
    {
        [Space(10)]
        [Header("Card Images")]
        [SerializeField] private Image front;
        [SerializeField] private Image back;

        [Space(10)]
        [Header("Animation Settings")]
        [SerializeField] private float cardFlipTime = 0.25f;
        [SerializeField] private AnimationCurve flipCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Space(10)]
        [Header("Card Data")]
        [SerializeField] private int cardID;
        [SerializeField] private Sprite cardSprite;
        public Sprite CardSprite
        {
            get { return cardSprite; }
            private set
            {
                cardSprite = value;
                front.sprite = value;
            }
        }

        public static event Action<GameCard> OnCardFlipped;
        private Action<GameCard> onCardClickedCallback;

        private bool isFlipped = false;
        private bool isAnimating = false;
        private bool isInteractable = true;
        private bool isMatched = false;
        public int CardID { get { return cardID; } private set { cardID = value; } }
        public bool IsFlipped { get { return isFlipped; } private set { isFlipped = value; } }
        public bool IsAnimating { get { return isAnimating; } private set { isAnimating = value; } }
        public bool IsInteractable { get { return isInteractable; } private set { isInteractable = value; } }
        public bool IsMatched
        {
            get { return isMatched; }
            private set
            {
                isMatched = value;
                if (value) SetMatchedAlpha();
            }
        }

        public void InitCard(int cardID, Sprite frontSprite, Action<GameCard> cardClicked)
        {
            ResetCard();
            onCardClickedCallback = cardClicked;
            this.cardID = cardID;
            this.CardSprite = frontSprite;

            // Ensure card starts face down
            SetCardVisuals(false);
        }

        public void InitSavedCard(int cardID, Sprite frontSprite, bool isMatched, Action<GameCard> cardClicked)
        {
            ResetCard();
            onCardClickedCallback = cardClicked;
            this.cardID = cardID;
            this.CardSprite = frontSprite;
            this.IsFlipped = isMatched;
            this.IsMatched = isMatched;
            SetCardVisuals(isFlipped);
        }

        public void Matched()
        {
            IsMatched = true;
            IsInteractable = false;
        }

        private void SetMatchedAlpha()
        {
            front.color = new Color(1, 1, 1, 0.5f);
        }

        private void ResetMatchedAlpha()
        {
            front.color = new Color(1, 1, 1, 1);
        }

        public void SetInteractable(bool interactable)
        {
            IsInteractable = interactable;
        }

        public void EnableInteraction()
        {
            IsInteractable = true;
        }

        public void DisableInteraction()
        {
            IsInteractable = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!IsInteractable || IsAnimating || IsFlipped || IsMatched)
                return;

            FlipCard();
            onCardClickedCallback?.Invoke(this);
        }

        public void FlipCard()
        {
            if (IsAnimating)
                return;

            StartCoroutine(FlipCardAnimation());
        }

        public void FlipToFront(bool animate = true)
        {
            if (IsFlipped)
                return;

            if (animate)
            {
                StartCoroutine(FlipCardAnimation());
            }
            else
            {
                IsFlipped = true;
                SetCardVisuals(true);
            }
        }

        public void FlipToBack(bool animate = true)
        {
            if (!IsFlipped)
                return;

            if (animate)
            {
                StartCoroutine(FlipCardAnimation());
            }
            else
            {
                IsFlipped = false;
                SetCardVisuals(false);
            }
        }

        private IEnumerator FlipCardAnimation()
        {
            IsAnimating = true;

            // First half of flip - scale down to 0 on X axis
            float halfFlipTime = cardFlipTime * 0.5f;
            float elapsedTime = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 middleScale = new Vector3(0f, startScale.y, startScale.z);

            while (elapsedTime < halfFlipTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / halfFlipTime;
                float curveValue = flipCurve.Evaluate(progress);

                transform.localScale = Vector3.Lerp(startScale, middleScale, curveValue);
                yield return null;
            }

            // Switch card visuals at the middle of animation
            IsFlipped = !IsFlipped;
            SetCardVisuals(IsFlipped);

            // Second half of flip - scale back to original
            elapsedTime = 0f;
            Vector3 endScale = startScale;

            while (elapsedTime < halfFlipTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / halfFlipTime;
                float curveValue = flipCurve.Evaluate(progress);

                transform.localScale = Vector3.Lerp(middleScale, endScale, curveValue);
                yield return null;
            }

            // Ensure final scale is correct
            transform.localScale = endScale;

            IsAnimating = false;
            OnCardFlipped?.Invoke(this);
        }

        private void SetCardVisuals(bool showFront)
        {
            if (front != null)
                front.gameObject.SetActive(showFront);

            if (back != null)
                back.gameObject.SetActive(!showFront);
        }

        public void ResetCard()
        {
            StopAllCoroutines();
            ResetMatchedAlpha();
            IsFlipped = false;
            IsAnimating = false;
            IsInteractable = true; // Reset interactable state
            IsMatched = false; // Reset matched state
            transform.localScale = Vector3.one;
            SetCardVisuals(false);

            // Clear any previous card data
            cardID = 0;
            CardSprite = null;
            onCardClickedCallback = null;

            // Reset front image
            if (front != null)
            {
                front.sprite = null;
            }
        }

        // Method to match cards by ID (useful for memory game logic)
        public bool MatchesCard(GameCard otherCard)
        {
            return otherCard != null && cardID == otherCard.cardID;
        }
    }
}