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

        [Header("New Game Confirmation Modal")]
        public GameObject confirmNewGamePanel;
        public Button confirmNewGameBtn;
        public Button cancelNewGameBtn;

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
                bool canContinue = VillaDelChef.Save.SaveManager.Instance != null 
                    ? VillaDelChef.Save.SaveManager.Instance.CanContinueGame() 
                    : VillaDelChef.Save.SaveManager.HasSaveFile();
                continueButton.interactable = canContinue;
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            if (optionsButton != null) optionsButton.onClick.AddListener(OnOptionsClicked);
            if (closeOptionsButton != null) closeOptionsButton.onClick.AddListener(OnCloseOptionsClicked);

            if (creditsButton != null) creditsButton.onClick.AddListener(OnCreditsClicked);
            if (closeCreditsButton != null) closeCreditsButton.onClick.AddListener(OnCloseCreditsClicked);

            if (confirmNewGameBtn != null) confirmNewGameBtn.onClick.AddListener(ConfirmNewGame);
            if (cancelNewGameBtn != null) cancelNewGameBtn.onClick.AddListener(CancelNewGame);
            if (confirmNewGamePanel != null) confirmNewGamePanel.SetActive(false);

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
            bool canContinue = VillaDelChef.Save.SaveManager.Instance != null && VillaDelChef.Save.SaveManager.Instance.CanContinueGame();

            if (canContinue)
            {
                // Active game exists: prompt for confirmation before wiping progress
                if (confirmNewGamePanel != null)
                {
                    confirmNewGamePanel.SetActive(true);
                }
                else
                {
                    ShowFallbackConfirmationModal();
                }
            }
            else
            {
                StartNewGameAndLoad();
            }
        }

        private void OnContinueClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            string targetScene = VillaDelChef.Save.SaveManager.Instance != null 
                ? VillaDelChef.Save.SaveManager.Instance.GetTargetGameplayScene() 
                : restaurantSceneName;
            SceneManager.LoadScene(targetScene);
        }

        public void ConfirmNewGame()
        {
            AudioManager.Instance?.PlayButtonClick();
            StartNewGameAndLoad();
        }

        public void CancelNewGame()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (confirmNewGamePanel != null) confirmNewGamePanel.SetActive(false);
        }

        private void StartNewGameAndLoad()
        {
            VillaDelChef.Save.SaveManager.Instance?.StartNewGame();
            SceneManager.LoadScene("03_Prologue");
        }

        private void ShowFallbackConfirmationModal()
        {
            GameObject modalGO = new GameObject("Fallback_ConfirmModal");
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null) modalGO.transform.SetParent(canvas.transform, false);

            RectTransform rt = modalGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(620, 320);
            rt.anchoredPosition = Vector2.zero;

            Image bg = modalGO.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.12f, 0.16f, 0.98f);

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(modalGO.transform, false);
            Text t = textGO.AddComponent<Text>();
            t.text = "¿Comenzar una nueva partida?\nSe perderá el progreso guardado actual.";
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 24;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            RectTransform trt = textGO.GetComponent<RectTransform>();
            trt.anchoredPosition = new Vector2(0f, 40f);
            trt.sizeDelta = new Vector2(560, 120);

            // Confirm Button
            GameObject cfmGO = new GameObject("ConfirmBtn");
            cfmGO.transform.SetParent(modalGO.transform, false);
            Image cfmBg = cfmGO.AddComponent<Image>();
            cfmBg.color = new Color(0.8f, 0.25f, 0.25f);
            Button cfmBtn = cfmGO.AddComponent<Button>();
            RectTransform cfmRT = cfmGO.GetComponent<RectTransform>();
            cfmRT.anchoredPosition = new Vector2(130f, -80f);
            cfmRT.sizeDelta = new Vector2(180, 55);
            GameObject cfmTextGO = new GameObject("Text");
            cfmTextGO.transform.SetParent(cfmGO.transform, false);
            Text cfmT = cfmTextGO.AddComponent<Text>();
            cfmT.text = "CONFIRMAR";
            cfmT.font = t.font;
            cfmT.alignment = TextAnchor.MiddleCenter;
            cfmT.color = Color.white;
            cfmBtn.onClick.AddListener(() =>
            {
                Destroy(modalGO);
                StartNewGameAndLoad();
            });

            // Cancel Button
            GameObject cclGO = new GameObject("CancelBtn");
            cclGO.transform.SetParent(modalGO.transform, false);
            Image cclBg = cclGO.AddComponent<Image>();
            cclBg.color = new Color(0.3f, 0.5f, 0.8f);
            Button cclBtn = cclGO.AddComponent<Button>();
            RectTransform cclRT = cclGO.GetComponent<RectTransform>();
            cclRT.anchoredPosition = new Vector2(-130f, -80f);
            cclRT.sizeDelta = new Vector2(180, 55);
            GameObject cclTextGO = new GameObject("Text");
            cclTextGO.transform.SetParent(cclGO.transform, false);
            Text cclT = cclTextGO.AddComponent<Text>();
            cclT.text = "CANCELAR";
            cclT.font = t.font;
            cclT.alignment = TextAnchor.MiddleCenter;
            cclT.color = Color.white;
            cclBtn.onClick.AddListener(() => Destroy(modalGO));
        }

        private void OnOptionsClicked()
        {
            AudioManager.Instance?.PlayButtonClick();
            if (optionsPanel != null) optionsPanel.SetActive(true);
            if (creditsPanel != null) creditsPanel.SetActive(false);
            if (confirmNewGamePanel != null) confirmNewGamePanel.SetActive(false);
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
            if (confirmNewGamePanel != null) confirmNewGamePanel.SetActive(false);
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
