using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CyberSpeed.Manager;

namespace CyberSpeed.UI
{
    public class LoadSavedGamePopupUI : MonoBehaviour
    {
        [SerializeField] Button loadSavedGameYesBtn;
        [SerializeField] Button loadSavedGameNoBtn;

        void OnEnable()
        {
            loadSavedGameYesBtn.onClick.AddListener(OnLoadSavedGameYesButtonClicked);
            loadSavedGameNoBtn.onClick.AddListener(OnLoadSavedGameNoButtonClicked);
        }

        void OnDisable()
        {
            loadSavedGameYesBtn.onClick.RemoveListener(OnLoadSavedGameYesButtonClicked);
            loadSavedGameNoBtn.onClick.RemoveListener(OnLoadSavedGameNoButtonClicked);
        }

        void OnLoadSavedGameYesButtonClicked()
        {
            GameManager.Instance.LoadSavedGameYes();
            GameManager.Instance.HideLoadSavedGamePopupUI();
        }

        void OnLoadSavedGameNoButtonClicked()
        {
            GameManager.Instance.LoadNewGame();
            GameManager.Instance.HideLoadSavedGamePopupUI();
        }
    }
}