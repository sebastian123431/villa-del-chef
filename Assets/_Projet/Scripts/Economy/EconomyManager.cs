using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Save;

namespace VillaDelChef.Economy
{
    public class EconomyManager : MonoBehaviour
    {
        public static EconomyManager Instance { get; private set; }

        [Header("Currencies")]
        [SerializeField] private int coins = 200;
        [SerializeField] private int reputation = 10;

        public int Coins => coins;
        public int Reputation => reputation;

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
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                coins = SaveManager.Instance.CurrentSave.coins;
                reputation = SaveManager.Instance.CurrentSave.reputation;
            }
            GameEvents.TriggerCoinsChanged(coins);
            GameEvents.TriggerReputationChanged(reputation);
        }

        public bool HasCoins(int amount)
        {
            return coins >= amount;
        }

        public void AddCoins(int amount)
        {
            if (amount <= 0) return;
            coins += amount;
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.coins = coins;
            }
            GameEvents.TriggerCoinsChanged(coins);
            GameEvents.TriggerQuestProgressMade(ScriptableObjects.QuestType.EarnCoins, "", amount);
        }

        public bool SpendCoins(int amount)
        {
            if (amount <= 0) return true;
            if (coins < amount) return false;

            coins -= amount;
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.coins = coins;
            }
            GameEvents.TriggerCoinsChanged(coins);
            return true;
        }

        public void AddExperience(int amount)
        {
            Progression.ProgressionManager.Instance?.AddExperience(amount);
        }

        public void ModifyReputation(int delta)
        {
            reputation = Mathf.Max(0, reputation + delta);
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.reputation = reputation;
            }
            GameEvents.TriggerReputationChanged(reputation);
        }
    }
}
