using System.Collections;
using System.Collections.Generic;
using CyberSpeed.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace CyberSpeed.UI
{
    public class ExitPopupUI : MonoBehaviour
    {
        [SerializeField] Button exitSaveYesBtn;
        [SerializeField] Button exitSaveNoBtn;

        private void OnEnable()
        {
            exitSaveYesBtn.onClick.AddListener(OnExitSaveYesButtonClicked);
            exitSaveNoBtn.onClick.AddListener(OnExitSaveNoButtonClicked);
        }

        private void OnDisable()
        {
            exitSaveYesBtn.onClick.RemoveListener(OnExitSaveYesButtonClicked);
            exitSaveNoBtn.onClick.RemoveListener(OnExitSaveNoButtonClicked);
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