using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VillaDelChef.Managers;

namespace VillaDelChef.UI
{
    public class MainMenuController : MonoBehaviour
    {
        [Header("Background Graphic")]
        [Tooltip("Assign mainmenu_background.png when available in Assets/_Projet/Art/UI/MainMenu/")]
        public Image backgroundImage;

        [Header("Menu Buttons")]
        public Button playButton;
        public Button continueButton;
        public Button optionsButton;
        public Button creditsButton;
        public Button quitButton;

        [Header("Options Panel")]
        public GameObject optionsPanel;
        public Slider musicVolumeSlider;
        public Slider sfxVolumeSlider;
        public Button closeOptionsButton;
        public Button fpsToggle30Button;
        public Button fpsToggle60Button;

        [Header("Credits Panel")]
        public GameObject creditsPanel;
        public Button closeCreditsButton;

        [Header("Scene Navigation")]
        public string restaurantSceneName = "02_Restaurant";

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayClicked);

            if (continueButton != null)
            {
                bool hasSave = VillaDelChef.Save.SaveManager.HasSaveFile();
                continueButton.interactable = hasSave;
                continueButton.onClick.AddListener(OnPlayClicked);
            }

            if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
            if (closeOptionsButton != null) closeOptionsButton.onClick.AddListener(OnCloseOptionsClicked);

            if (creditsButton != null) creditsButton.onClick.AddListener(OnCreditsClicked);
            if (closeCreditsButton != null) closeCreditsButton.onClick.AddListener(OnCloseCreditsClicked);

            if (quitButton != null)
            {
                // Quit button only visible on desktop / PC standalone
                bool isMobile = Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer;
                quitButton.gameObject.SetActive(!isMobile);
                quitButton.onClick.AddListener(OnQuitClicked);
            }

            if (optionsPanel != null) optionsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);

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
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        private void OnCloseOptionsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        private void OnCreditsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (creditsPanel != null) creditsPanel.SetActive(true);
            if (optionsPanel != null) optionsPanel.SetActive(false);
        }

        private void OnCloseCreditsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (creditsPanel != null) creditsPanel.SetActive(false);
        }

        private void OnQuitClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            Application.Quit();
        }
    }
}
