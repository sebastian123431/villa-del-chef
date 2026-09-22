using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Save;

namespace VillaDelChef.Managers
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }

        [Header("Audio Sources")]
        public AudioSource musicSource;
        public AudioSource sfxSource;

        [Header("Audio Clips")]
        public AudioClip backgroundMusic;
        public AudioClip coinSFX;
        public AudioClip cookingSFX;
        public AudioClip harvestSFX;
        public AudioClip dishReadySFX;
        public AudioClip levelUpSFX;
        public AudioClip buttonClickSFX;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SetMusicVolume(SaveManager.Instance.CurrentSave.musicVolume);
                SetSFXVolume(SaveManager.Instance.CurrentSave.sfxVolume);
            }

            if (musicSource != null && backgroundMusic != null && !musicSource.isPlaying)
            {
                musicSource.clip = backgroundMusic;
                musicSource.loop = true;
                musicSource.Play();
            }

            RegisterEvents();
        }

        private void RegisterEvents()
        {
            GameEvents.OnCoinsChanged += (coins) => PlaySFX(coinSFX);
            GameEvents.OnCropHarvested += (plot, crop, amount) => PlaySFX(harvestSFX);
            GameEvents.OnDishReady += (dish) => PlaySFX(dishReadySFX);
            GameEvents.OnLevelUp += (lvl) => PlaySFX(levelUpSFX);
            GameEvents.OnCookingStarted += (st, rec) => PlaySFX(cookingSFX);
            GameEvents.OnBuildModeToggled += (bm) => PlaySFX(buttonClickSFX);
        }

        public void PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip, volume);
            }
        }

        public void PlayButtonClick()
        {
            PlaySFX(buttonClickSFX);
        }

        public void PlayCoin()
        {
            PlaySFX(coinSFX);
        }

        public void PlayLevelUp()
        {
            PlaySFX(levelUpSFX);
        }

        public void PlayHarvest()
        {
            PlaySFX(harvestSFX);
        }

        public void PlayDishReady()
        {
            PlaySFX(dishReadySFX);
        }

        public void PlayCooking()
        {
            PlaySFX(cookingSFX);
        }

        public void SetMusicVolume(float volume)
        {
            if (musicSource != null) musicSource.volume = volume;
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.musicVolume = volume;
            }
        }

        public void SetSFXVolume(float volume)
        {
            if (sfxSource != null) sfxSource.volume = volume;
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.sfxVolume = volume;
            }
        }
    }
}
