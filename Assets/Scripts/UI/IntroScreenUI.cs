using System.Collections;
using System.Collections.Generic;
using CyberSpeed.Manager;
using UnityEngine;
using UnityEngine.UI;

public class IntroScreenUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button playButton;

    private void OnEnable()
    {
        playButton.onClick.AddListener(OnPlayButtonClicked);
    }

    private void OnDisable()
    {
        playButton.onClick.RemoveListener(OnPlayButtonClicked);
    }

    public void OnPlayButtonClicked()
    {
        GameManager.Instance.OnIntroPlayClicked();
    }
}
