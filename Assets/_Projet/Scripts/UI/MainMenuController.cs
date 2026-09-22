using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VillaDelChef.Managers;

namespace VillaDelChef.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Menu Buttons")]
        public Button playButton;
        public Button optionsButton;
        public Button quitButton;

        [Header("Options Panel")]
        public GameObject optionsPanel;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Button closeOptionsButton;
        public Button fpsToggle30Button;
        public Button fpsToggle60Button;

        [Header("Scene Navigation")]
        public string restaurantSceneName = "02_Restaurant";

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);
            if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
            if (quitButton != null) quitButton.onClick.AddListener(OnQuitClicked);
            if (closeOptionsButton != null) closeOptionsButton.onClick.AddListener(OnCloseOptionsClicked);

            if (optionsPanel != null) optionsPanel.SetActive(false);

            if (musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(val => AudioManager.Instance?.SetMusicVolume(val));
            }
            if (sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(val => AudioManager.Instance?.SetSFXVolume(val));
            }

            if (fpsToggle30Button != null) fpsToggle30Button.onClick.AddListener(() => Application.targetFrameRate = 30);
            if (fpsToggle60Button != null) fpsToggle60Button.onClick.AddListener(() => Application.targetFrameRate = 60);
        }

        private void OnPlayClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            SceneManager.LoadScene(restaurantSceneName);
        }

        private void OnOptionsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (optionsPanel != null) optionsPanel.SetActive(true);
        }

        private void OnCloseOptionsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        private void OnQuitClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            Application.Quit();
        }
    }
}
