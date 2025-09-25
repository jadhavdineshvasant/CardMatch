using System.Collections;
using System.Collections.Generic;
using CyberSpeed.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace CyberSpeed.UI
{
    public class ExitPopupUI : MonoBehaviour
    {
        [SerializeField] Button closeBtn;
        [SerializeField] Button exitSaveYesBtn;
        [SerializeField] Button exitSaveNoBtn;

        private void OnEnable()
        {
            closeBtn.onClick.AddListener(OnCloseButtonClicked);
            exitSaveYesBtn.onClick.AddListener(OnExitSaveYesButtonClicked);
            exitSaveNoBtn.onClick.AddListener(OnExitSaveNoButtonClicked);
        }

        private void OnDisable()
        {
            closeBtn.onClick.RemoveListener(OnCloseButtonClicked);
            exitSaveYesBtn.onClick.RemoveListener(OnExitSaveYesButtonClicked);
            exitSaveNoBtn.onClick.RemoveListener(OnExitSaveNoButtonClicked);
        }

        private void OnCloseButtonClicked()
        {
            GameManager.Instance.HideExitPopupUI();
        }

        private void OnExitSaveYesButtonClicked()
        {
            GameManager.Instance.ExitPopupSaveYes();
            GameManager.Instance.HideExitPopupUI();
        }

        private void OnExitSaveNoButtonClicked()
        {
            GameManager.Instance.ExitPopupSaveNo();
            GameManager.Instance.HideExitPopupUI();
        }
    }
}