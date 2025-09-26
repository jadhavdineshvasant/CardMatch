using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using CyberSpeed.SO;
using System;
using DifficultyLevelData = CyberSpeed.SO.DifficultyLevelSO.DifficultyLevelData;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

namespace CyberSpeed.UI
{
    public class LevelTile : MonoBehaviour, IPointerDownHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private Image levelImage;
        [SerializeField] private TextMeshProUGUI levelTitle;
        [SerializeField] private TextMeshProUGUI levelName;
        [SerializeField] private TextMeshProUGUI scoreMultiplier;
        [SerializeField] private Image saveIcon;
        DifficultyLevelData levelData;

        public void Init(DifficultyLevelData levelData, Action<DifficultyLevelData> onLevelClicked)
        {
            this.levelData = levelData;
            levelImage.sprite = levelData.levelSprite;
            levelTitle.text = $"{levelData.rowsCount} x {levelData.colsCount}";
            levelName.text = levelData.levelName;
            levelName.color = levelData.titleColor;
            saveIcon.gameObject.SetActive(levelData.IsSavedLevelExists());
            scoreMultiplier.text = $"( +{levelData.baseScore} per match )";

            Button btn = this.GetComponent<Button>();
            if (btn == null) this.AddComponent<Button>();
            btn.onClick.AddListener(() => onLevelClicked?.Invoke(levelData));
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            StartCoroutine(AnimateButton(true));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            StartCoroutine(AnimateButton(false));
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            StartCoroutine(AnimateButton(false));
        }

        private float scaleInOutTime = 0.15f;
        private IEnumerator AnimateButton(bool scaleIn)
        {
            float elapsedTime = 0f;
            Vector3 startScale = transform.localScale;
            Vector3 endScale = scaleIn ? new Vector3(1.1f, 1.1f, 1.1f) : Vector3.one;

            while (elapsedTime < scaleInOutTime)
            {
                elapsedTime += Time.deltaTime;
                float progress = elapsedTime / scaleInOutTime;
                transform.localScale = Vector3.Lerp(startScale, endScale, progress);
                yield return null;
            }

            transform.localScale = endScale;
        }
    }
}