using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Save;

namespace VillaDelChef.Progression
{
    public class ProgressionManager : MonoBehaviour
    {
        public static ProgressionManager Instance { get; private set; }

        [Header("Progression Values")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int currentExperience = 0;

        public int CurrentLevel => currentLevel;
        public int CurrentExperience => currentExperience;

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
                currentLevel = SaveManager.Instance.CurrentSave.level;
                currentExperience = SaveManager.Instance.CurrentSave.experience;
            }
            GameEvents.TriggerExperienceChanged(currentExperience, currentLevel);
        }

        public int GetExperienceRequiredForLevel(int level)
        {
            // Cozy linear-exponential curve
            return 50 + (level - 1) * 75;
        }

        public void AddExperience(int amount)
        {
            if (amount <= 0) return;

            currentExperience += amount;
            int xpRequired = GetExperienceRequiredForLevel(currentLevel);

            while (currentExperience >= xpRequired)
            {
                currentExperience -= xpRequired;
                currentLevel++;
                OnLevelUp();
                xpRequired = GetExperienceRequiredForLevel(currentLevel);
            }

            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.level = currentLevel;
                SaveManager.Instance.CurrentSave.experience = currentExperience;
            }

            GameEvents.TriggerExperienceChanged(currentExperience, currentLevel);
        }

        private void OnLevelUp()
        {
            Debug.Log($"[ProgressionManager] LEVEL UP! You reached Level {currentLevel}!");
            GameEvents.TriggerLevelUp(currentLevel);
        }
    }
}
