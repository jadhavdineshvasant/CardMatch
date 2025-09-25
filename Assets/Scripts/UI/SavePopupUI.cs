using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CyberSpeed.Manager;

public class SavePopupUI : MonoBehaviour
{
    [SerializeField] Button closeBtn;
    [SerializeField] Button saveYesBtn;

    private void OnEnable()
    {
        closeBtn.onClick.AddListener(OnCloseButtonClicked);
        saveYesBtn.onClick.AddListener(OnSaveYesButtonClicked);
    }

    private void OnDisable()
    {
        closeBtn.onClick.RemoveListener(OnCloseButtonClicked);
        saveYesBtn.onClick.RemoveListener(OnSaveYesButtonClicked);
    }

    private void OnCloseButtonClicked()
    {
        GameManager.Instance.HideSavePopupUI();
    }

    private void OnSaveYesButtonClicked()
    {
        GameManager.Instance.SaveGameYes();
        GameManager.Instance.HideSavePopupUI();
    }
}
