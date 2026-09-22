using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.Managers;

namespace VillaDelChef.UI
{
    public class LevelUpModalUI : MonoBehaviour
    {
        public static LevelUpModalUI Instance { get; private set; }

        [Header("UI Root")]
        public GameObject panelRoot;
        public Text levelNumberText;
        public Text rewardText;
        public Text descriptionText;
        public Button claimButton;

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
            if (panelRoot != null) panelRoot.SetActive(false);
            if (claimButton != null) claimButton.onClick.AddListener(Close);

            GameEvents.OnLevelUp += HandleLevelUp;
        }

        private void OnDestroy()
        {
            GameEvents.OnLevelUp -= HandleLevelUp;
        }

        public void HandleLevelUp(int newLevel)
        {
            int rewardCoins = newLevel * 50;
            EconomyManager.Instance?.AddCoins(rewardCoins);
            AudioManager.Instance?.PlayLevelUp();

            if (levelNumberText != null) levelNumberText.text = $"¡NIVEL {newLevel}!";
            if (rewardText != null) rewardText.text = $"+${rewardCoins} Monedas de Bonificación";
            if (descriptionText != null) descriptionText.text = "¡Tu restaurante es cada vez más popular! Sigue cocinando para desbloquear nuevas recetas e ingredientes.";

            if (panelRoot != null) panelRoot.SetActive(true);
            else gameObject.SetActive(true);
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            else gameObject.SetActive(false);
        }
    }
}
