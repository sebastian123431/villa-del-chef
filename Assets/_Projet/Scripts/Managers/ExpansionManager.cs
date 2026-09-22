using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.Progression;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Managers
{
    public class ExpansionManager : MonoBehaviour
    {
        public static ExpansionManager Instance { get; private set; }

        [Header("Expansions Data")]
        [SerializeField] private List<ExpansionSO> allExpansions = new List<ExpansionSO>();

        private HashSet<string> unlockedIDs = new HashSet<string>();
        private Dictionary<string, ExpansionSign> activeSigns = new Dictionary<string, ExpansionSign>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                LoadAllExpansionDefinitions();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            ApplyInitialLockState();
            SyncWithSaveData();
        }

        private void LoadAllExpansionDefinitions()
        {
            if (allExpansions == null || allExpansions.Count == 0)
            {
                allExpansions = new List<ExpansionSO>(Resources.LoadAll<ExpansionSO>("Expansions"));
            }
            Debug.Log($"[ExpansionManager] {allExpansions.Count} definiciones de expansión cargadas.");
        }

        private void ApplyInitialLockState()
        {
            if (GridManager.Instance == null) return;

            // Lock areas defined in all expansions by default
            foreach (var exp in allExpansions)
            {
                if (exp != null && exp.gridBounds.width > 0 && exp.gridBounds.height > 0)
                {
                    GridManager.Instance.LockZoneArea(exp.gridBounds, exp.targetZone);
                }
            }
        }

        public void RegisterSign(ExpansionSign sign)
        {
            if (sign == null || sign.expansionData == null) return;
            string id = sign.expansionData.expansionID;

            if (!activeSigns.ContainsKey(id))
            {
                activeSigns.Add(id, sign);
            }

            // If already unlocked, hide or disable sign
            if (unlockedIDs.Contains(id))
            {
                sign.gameObject.SetActive(false);
            }
        }

        public bool IsExpansionUnlocked(string id)
        {
            return unlockedIDs.Contains(id);
        }

        public bool CanUnlockExpansion(ExpansionSO exp, out string reason)
        {
            reason = string.Empty;
            if (exp == null)
            {
                reason = "Expansión inválida.";
                return false;
            }

            if (unlockedIDs.Contains(exp.expansionID))
            {
                reason = "Esta zona ya está desbloqueada.";
                return false;
            }

            int currentLevel = (ProgressionManager.Instance != null) ? ProgressionManager.Instance.CurrentLevel : 1;
            if (currentLevel < exp.requiredRestaurantLevel)
            {
                reason = $"Requiere nivel {exp.requiredRestaurantLevel} de restaurante (Actual: {currentLevel}).";
                return false;
            }

            int currentCoins = (EconomyManager.Instance != null) ? EconomyManager.Instance.Coins : 0;
            if (currentCoins < exp.costGold)
            {
                reason = $"Monedas insuficientes. Requiere {exp.costGold} (Tienes {currentCoins}).";
                return false;
            }

            return true;
        }

        public bool UnlockExpansion(ExpansionSO exp)
        {
            if (!CanUnlockExpansion(exp, out string reason))
            {
                Debug.LogWarning($"[ExpansionManager] No se puede desbloquear '{exp?.displayName}': {reason}");
                return false;
            }

            // Spend coins
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.SpendCoins(exp.costGold);
            }

            // Add XP
            if (ProgressionManager.Instance != null && exp.rewardXP > 0)
            {
                ProgressionManager.Instance.AddExperience(exp.rewardXP);
            }

            // Unlock grid zone
            if (GridManager.Instance != null)
            {
                GridManager.Instance.UnlockZoneArea(exp.gridBounds, exp.targetZone);
            }

            unlockedIDs.Add(exp.expansionID);

            // Remove sign with visual celebration
            if (activeSigns.TryGetValue(exp.expansionID, out ExpansionSign sign) && sign != null)
            {
                sign.OnExpansionPurchased();
            }

            // Audio & events
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayLevelUp();
            }

            GameEvents.TriggerExpansionUnlocked(exp);
            GameEvents.TriggerQuestProgressMade(QuestType.UnlockExpansion, exp.expansionID, 1);

            // Save state
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();
            }

            Debug.Log($"[ExpansionManager] ¡Expansión '{exp.displayName}' desbloqueada con éxito!");
            return true;
        }

        public void PopulateSaveData(SaveData data)
        {
            if (data == null) return;
            data.unlockedExpansions = new List<string>(unlockedIDs);
        }

        public void LoadFromSaveData(SaveData data)
        {
            if (data == null || data.unlockedExpansions == null) return;

            unlockedIDs.Clear();
            foreach (var id in data.unlockedExpansions)
            {
                unlockedIDs.Add(id);
                ExpansionSO exp = allExpansions.Find(e => e != null && e.expansionID == id);
                if (exp != null && GridManager.Instance != null)
                {
                    GridManager.Instance.UnlockZoneArea(exp.gridBounds, exp.targetZone);
                }

                if (activeSigns.TryGetValue(id, out ExpansionSign sign) && sign != null)
                {
                    sign.gameObject.SetActive(false);
                }
            }
        }

        private void SyncWithSaveData()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                LoadFromSaveData(SaveManager.Instance.CurrentSave);
            }
        }

        public List<ExpansionSO> GetAllExpansions() => allExpansions;
    }
}
