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

        private bool isFlipped = false;
        private bool isAnimating = false;
        private bool isInteractable = true;

        public static event Action<GameCard> OnCardFlipped;
        private Action<GameCard> onCardClickedCallback;

        public int CardID { get { return cardID; } private set { cardID = value; } }
        public bool IsFlipped { get { return isFlipped; } private set { isFlipped = value; } }
        public bool IsAnimating { get { return isAnimating; } private set { isAnimating = value; } }
        public bool IsInteractable { get { return isInteractable; } private set { isInteractable = value; } }
        public Sprite CardSprite { get { return cardSprite; } private set { cardSprite = value; } }

        public void InitCard(int cardID, Sprite frontSprite, Action<GameCard> cardClicked)
        {
            ResetCard();
            onCardClickedCallback = cardClicked;
            this.cardID = cardID;
            this.cardSprite = frontSprite;

            // Ensure card starts face down
            SetCardVisuals(false);

            // Set front image if sprite is assigned
            if (frontSprite != null && front != null)
            {
                front.sprite = frontSprite;
            }
        }

        public void Matched()
        {
            IsInteractable = false;
            SetMatchedAlpha();
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
            isInteractable = interactable;
        }

        public void EnableInteraction()
        {
            isInteractable = true;
        }

        public void DisableInteraction()
        {
            isInteractable = false;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!isInteractable || isAnimating || isFlipped)
                return;

            FlipCard();
            onCardClickedCallback?.Invoke(this);
        }

        public void FlipCard()
        {
            if (isAnimating)
                return;

            StartCoroutine(FlipCardAnimation());
        }

        public void FlipToFront(bool animate = true)
        {
            Debug.Log(isFlipped);

            if (isFlipped)
                return;

            if (animate)
            {
                StartCoroutine(FlipCardAnimation());
            }
            else
            {
                isFlipped = true;
                SetCardVisuals(true);
            }
        }

        public void FlipToBack(bool animate = true)
        {
            if (!isFlipped)
                return;

            if (animate)
            {
                StartCoroutine(FlipCardAnimation());
            }
            else
            {
                isFlipped = false;
                SetCardVisuals(false);
            }
        }

        private IEnumerator FlipCardAnimation()
        {
            isAnimating = true;

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
            isFlipped = !isFlipped;
            SetCardVisuals(isFlipped);

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

            isAnimating = false;
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
            isFlipped = false;
            isAnimating = false;
            isInteractable = true; // Reset interactable state
            transform.localScale = Vector3.one;
            SetCardVisuals(false);

            // Clear any previous card data
            cardID = 0;
            cardSprite = null;
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