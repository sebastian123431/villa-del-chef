using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Economy;
using VillaDelChef.Managers;
using VillaDelChef.Progression;

namespace VillaDelChef.UI
{
    public class ExpansionUI : MonoBehaviour
    {
        [Header("UI Roots")]
        public GameObject panelRoot;
        public Button closeButton;
        public Button unlockButton;

        [Header("Display Elements")]
        public Image expansionIcon;
        public Text titleText;
        public Text descriptionText;
        public Text levelRequirementText;
        public Text costText;
        public Text xpRewardText;
        public Text unlockButtonText;

        private ExpansionSO currentExpansion;

        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (unlockButton != null)
            {
                unlockButton.onClick.AddListener(OnUnlockClicked);
            }
        }

        public void Open(ExpansionSO expansion)
        {
            if (expansion == null) return;
            currentExpansion = expansion;

            if (panelRoot != null) panelRoot.SetActive(true);
            else gameObject.SetActive(true);

            RefreshUI();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            else gameObject.SetActive(false);

            currentExpansion = null;
        }

        private void RefreshUI()
        {
            if (currentExpansion == null) return;

            if (titleText != null)
            {
                titleText.text = currentExpansion.displayName;
            }

            if (descriptionText != null)
            {
                descriptionText.text = currentExpansion.description;
            }

            if (expansionIcon != null)
            {
                if (currentExpansion.icon != null)
                {
                    expansionIcon.sprite = currentExpansion.icon;
                    expansionIcon.gameObject.SetActive(true);
                }
                else
                {
                    expansionIcon.gameObject.SetActive(false);
                }
            }

            int currentLevel = (ProgressionManager.Instance != null) ? ProgressionManager.Instance.CurrentLevel : 1;
            bool levelMet = currentLevel >= currentExpansion.requiredRestaurantLevel;
            if (levelRequirementText != null)
            {
                string statusColor = levelMet ? "#44FF77" : "#FF5555";
                levelRequirementText.text = $"Nivel requerido: <color={statusColor}>{currentExpansion.requiredRestaurantLevel}</color> (Tu nivel: {currentLevel})";
            }

            int currentCoins = (EconomyManager.Instance != null) ? EconomyManager.Instance.Coins : 0;
            bool coinsMet = currentCoins >= currentExpansion.costGold;
            if (costText != null)
            {
                string statusColor = coinsMet ? "#FFDD44" : "#FF5555";
                costText.text = $"Costo: <color={statusColor}>${currentExpansion.costGold} Monedas</color>";
            }

            if (xpRewardText != null)
            {
                xpRewardText.text = $"+{currentExpansion.rewardXP} XP de Villa";
            }

            bool canUnlock = levelMet && coinsMet;
            if (unlockButton != null)
            {
                unlockButton.interactable = canUnlock;
            }

            if (unlockButtonText != null)
            {
                if (!levelMet)
                {
                    unlockButtonText.text = "🔒 Nivel Insuficiente";
                }
                else if (!coinsMet)
                {
                    unlockButtonText.text = "🪙 Monedas Insuficientes";
                }
                else
                {
                    unlockButtonText.text = "✨ ¡Desbloquear Zona!";
                }
            }
        }

        private void OnUnlockClicked()
        {
            if (currentExpansion == null) return;
            if (ExpansionManager.Instance == null) return;

            bool success = ExpansionManager.Instance.UnlockExpansion(currentExpansion);
            if (success)
            {
                Close();
            }
            else
            {
                RefreshUI();
            }
        }
    }
}
