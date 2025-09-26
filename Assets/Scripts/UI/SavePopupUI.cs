using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CyberSpeed.Manager;

namespace CyberSpeed.UI
{
    public class SavePopupUI : MonoBehaviour
    {
        [SerializeField] Button saveYesBtn;

        private void OnEnable()
        {
            saveYesBtn.onClick.AddListener(OnSaveYesButtonClicked);
        }

        private void OnDisable()
        {
            saveYesBtn.onClick.RemoveListener(OnSaveYesButtonClicked);
        }

        private void OnSaveYesButtonClicked()
        {
            GameManager.Instance.SaveGameYes();
            GameManager.Instance.HideSavePopupUI();
        }
    }
}