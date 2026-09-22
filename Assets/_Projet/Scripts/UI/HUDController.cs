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
