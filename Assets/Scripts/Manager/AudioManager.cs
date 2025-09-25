using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CyberSpeed.Manager
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioSource bgMusicSource;
        private bool isBGMusicPlaying = false;

        [Header("Audio Clip BG Music")]
        [SerializeField] private AudioClip bgMusic;

        [Header("SFX")]
        [SerializeField] private AudioClip gameStartSFX;
        [SerializeField] private AudioClip cardFlipSFX;
        [SerializeField] private AudioClip cardMatchSuccessSFX;
        [SerializeField] private AudioClip cardMatchFailSFX;
        [SerializeField] private AudioClip gameFinishSFX;
        [SerializeField] private AudioClip buttonClickSFX;


        void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public void PlayBGMusic()
        {
            if (isBGMusicPlaying || bgMusicSource != null) return;
            bgMusicSource.loop = true;
            bgMusicSource.Play();
            isBGMusicPlaying = true;
        }

        public void StopBGMusic()
        {
            bgMusicSource.Stop();
            isBGMusicPlaying = false;
        }

        public void PlaySFX(AudioClip sfx)
        {
            if (audioSource != null)
                audioSource.PlayOneShot(sfx);
        }

        public void StopSFX()
        {
            if (audioSource != null)
                audioSource.Stop();
        }

        public void PlayGameStartSFX()
        {
            PlaySFX(gameStartSFX);
        }

        public void PlayCardFlipSFX()
        {
            PlaySFX(cardFlipSFX);
        }

        public void PlayCardMatchSuccessSFX()
        {
            PlaySFX(cardMatchSuccessSFX);
        }

        public void PlayCardMatchFailSFX()
        {
            PlaySFX(cardMatchFailSFX);
        }

        public void PlayButtonClickSFX()
        {
            PlaySFX(buttonClickSFX);
        }

        public void PlayResultScreenSFX()
        {
            PlaySFX(gameFinishSFX);
        }
    }
}