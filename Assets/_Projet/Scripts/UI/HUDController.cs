using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Building;
using VillaDelChef.Core;

namespace VillaDelChef.UI
{
    public class HUDController : MonoBehaviour
    {
        public static HUDController Instance { get; private set; }

        [Header("Top Bar Display")]
        public Text coinsText;
        public Text levelText;
        public Text reputationText;
        public Slider xpSlider;
        public Text xpText;

        [Header("Action Buttons")]
        public Button buildModeButton;
        public Button inventoryButton;
        public Button questButton;
        public Button marketButton;
        public Button openCloseButton;
        public Text openCloseText;
        public Button staffButton;

        [Header("Panels")]
        public GameObject buildPanel;
        public GameObject inventoryPanel;
        public GameObject questPanel;
        public GameObject marketPanel;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            RegisterEvents();

            if (buildModeButton != null)
            {
                buildModeButton.onClick.AddListener(OnBuildButtonClicked);
            }
            if (inventoryButton != null)
            {
                inventoryButton.onClick.AddListener(() => InventoryUI.Instance?.Toggle());
            }
            if (questButton != null)
            {
                questButton.onClick.AddListener(() => QuestUI.Instance?.Toggle());
            }
            if (marketButton != null)
            {
                marketButton.onClick.AddListener(() => MarketUI.Instance?.Toggle());
            }

            if (openCloseButton != null)
            {
                openCloseButton.onClick.AddListener(OnOpenCloseClicked);
            }
            else
            {
                CreateFallbackOpenCloseButton();
            }

            if (staffButton != null)
            {
                staffButton.onClick.AddListener(() =>
                {
                    if (StaffMenuUI.Instance != null) StaffMenuUI.Instance.Toggle();
                    else StaffMenuUI.CreateFallbackModal();
                });
            }
            else
            {
                CreateFallbackStaffButton();
            }

            if (Managers.RestaurantOperatingManager.Instance != null)
            {
                Managers.RestaurantOperatingManager.Instance.OnOperatingStateChanged += UpdateOperatingStateDisplay;
                UpdateOperatingStateDisplay(Managers.RestaurantOperatingManager.Instance.IsOpen);
            }

            // Sync initial states if managers are already up
            if (Economy.EconomyManager.Instance != null)
            {
                UpdateCoins(Economy.EconomyManager.Instance.Coins);
                UpdateReputation(Economy.EconomyManager.Instance.Reputation);
            }
            if (Progression.ProgressionManager.Instance != null)
            {
                UpdateXP(Progression.ProgressionManager.Instance.CurrentExperience, Progression.ProgressionManager.Instance.CurrentLevel);
            }
        }

        private void RegisterEvents()
        {
            GameEvents.OnCoinsChanged += UpdateCoins;
            GameEvents.OnReputationChanged += UpdateReputation;
            GameEvents.OnExperienceChanged += UpdateXP;
            GameEvents.OnBuildModeToggled += HandleBuildModeToggled;
        }

        private void OnDestroy()
        {
            GameEvents.OnCoinsChanged -= UpdateCoins;
            GameEvents.OnReputationChanged -= UpdateReputation;
            GameEvents.OnExperienceChanged -= UpdateXP;
            GameEvents.OnBuildModeToggled -= HandleBuildModeToggled;

            if (Managers.RestaurantOperatingManager.Instance != null)
            {
                Managers.RestaurantOperatingManager.Instance.OnOperatingStateChanged -= UpdateOperatingStateDisplay;
            }
        }

        private void OnOpenCloseClicked()
        {
            Managers.RestaurantOperatingManager.Instance?.ToggleOperatingState();
        }

        private void UpdateOperatingStateDisplay(bool isOpen)
        {
            if (openCloseText != null)
            {
                openCloseText.text = isOpen ? "ABIERTO" : "CERRADO";
                openCloseText.color = Color.white;
            }

            if (openCloseButton != null)
            {
                var img = openCloseButton.GetComponent<Image>();
                if (img != null)
                {
                    img.color = isOpen ? new Color(0.2f, 0.65f, 0.3f) : new Color(0.85f, 0.28f, 0.25f);
                }
            }
        }

        private void CreateFallbackOpenCloseButton()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            GameObject btnGO = new GameObject("Btn_OpenCloseRestaurant");
            btnGO.transform.SetParent(canvas.transform, false);

            RectTransform rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(20f, -80f);
            rt.sizeDelta = new Vector2(130, 42);

            Image img = btnGO.AddComponent<Image>();
            img.color = new Color(0.85f, 0.28f, 0.25f);

            openCloseButton = btnGO.AddComponent<Button>();
            openCloseButton.onClick.AddListener(OnOpenCloseClicked);

            GameObject textGO = new GameObject("Label");
            textGO.transform.SetParent(btnGO.transform, false);
            openCloseText = textGO.AddComponent<Text>();
            openCloseText.text = "CERRADO";
            openCloseText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            openCloseText.fontSize = 16;
            openCloseText.fontStyle = FontStyle.Bold;
            openCloseText.alignment = TextAnchor.MiddleCenter;
            openCloseText.color = Color.white;

            RectTransform trt = textGO.GetComponent<RectTransform>();
            trt.sizeDelta = rt.sizeDelta;

            if (Managers.RestaurantOperatingManager.Instance != null)
            {
                UpdateOperatingStateDisplay(Managers.RestaurantOperatingManager.Instance.IsOpen);
            }
        }

        private void CreateFallbackStaffButton()
        {
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null) return;

            GameObject btnGO = new GameObject("Btn_StaffMenu");
            btnGO.transform.SetParent(canvas.transform, false);

            RectTransform rt = btnGO.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(160f, -80f);
            rt.sizeDelta = new Vector2(120, 42);

            Image img = btnGO.AddComponent<Image>();
            img.color = new Color(0.24f, 0.45f, 0.72f);

            staffButton = btnGO.AddComponent<Button>();
            staffButton.onClick.AddListener(() =>
            {
                if (StaffMenuUI.Instance != null) StaffMenuUI.Instance.Toggle();
                else StaffMenuUI.CreateFallbackModal();
            });

            GameObject textGO = new GameObject("Label");
            textGO.transform.SetParent(btnGO.transform, false);
            Text t = textGO.AddComponent<Text>();
            t.text = "PERSONAL";
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.fontSize = 15;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;

            RectTransform trt = textGO.GetComponent<RectTransform>();
            trt.sizeDelta = rt.sizeDelta;
        }

        private void UpdateCoins(int coins)
        {
            if (coinsText != null) coinsText.text = coins.ToString("N0");
        }

        private void UpdateReputation(int rep)
        {
            if (reputationText != null) reputationText.text = rep.ToString();
        }

        private void UpdateXP(int currentXP, int currentLevel)
        {
            if (levelText != null) levelText.text = $"Nivel {currentLevel}";

            int requiredXP = Progression.ProgressionManager.Instance != null ?
                Progression.ProgressionManager.Instance.GetExperienceRequiredForLevel(currentLevel) : 100;

            if (xpSlider != null)
            {
                xpSlider.maxValue = requiredXP;
                xpSlider.value = currentXP;
            }
            if (xpText != null)
            {
                xpText.text = $"{currentXP}/{requiredXP}";
            }
        }

        private void OnBuildButtonClicked()
        {
            if (BuildManager.Instance != null)
            {
                BuildManager.Instance.ToggleBuildMode();
            }
        }

        private void HandleBuildModeToggled(bool isBuildMode)
        {
            if (buildPanel != null)
            {
                buildPanel.SetActive(isBuildMode);
            }
        }
    }
}
