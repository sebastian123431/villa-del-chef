using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Managers
{
    [System.Serializable]
    public class ActiveQuest
    {
        public QuestSO questData;
        public int currentProgress;
        public bool isCompleted;
    }

    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        [Header("Quest Database")]
        public List<QuestSO> allQuests = new List<QuestSO>();
        public List<ActiveQuest> activeQuests = new List<ActiveQuest>();

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
            if (allQuests == null || allQuests.Count == 0)
            {
                allQuests = new List<QuestSO>(Resources.LoadAll<QuestSO>("Quests"));
            }
            LoadQuests();
            GameEvents.OnQuestProgressMade += HandleQuestProgress;
        }

        private void OnDestroy()
        {
            GameEvents.OnQuestProgressMade -= HandleQuestProgress;
        }

        private void LoadQuests()
        {
            activeQuests.Clear();

            // Check save
            Dictionary<string, QuestSaveData> savedDict = new Dictionary<string, QuestSaveData>();
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                foreach (var q in SaveManager.Instance.CurrentSave.quests)
                {
                    savedDict[q.questID] = q;
                }
            }

            foreach (var qSO in allQuests)
            {
                if (qSO == null) continue;

                ActiveQuest aq = new ActiveQuest
                {
                    questData = qSO,
                    currentProgress = 0,
                    isCompleted = false
                };

                if (savedDict.TryGetValue(qSO.questID, out var sData))
                {
                    aq.currentProgress = sData.currentProgress;
                    aq.isCompleted = sData.isCompleted;
                }

                activeQuests.Add(aq);
            }
        }

        private void HandleQuestProgress(QuestType type, string targetID, int amount)
        {
            foreach (var q in activeQuests)
            {
                if (q.isCompleted || q.questData == null) continue;

                if (q.questData.questType == type)
                {
                    if (string.IsNullOrEmpty(q.questData.targetID) || q.questData.targetID == targetID)
                    {
                        q.currentProgress += amount;
                        if (q.currentProgress >= q.questData.requiredAmount)
                        {
                            CompleteQuest(q);
                        }
                    }
                }
            }
            SyncToSave();
        }

        private void CompleteQuest(ActiveQuest q)
        {
            q.isCompleted = true;
            q.currentProgress = q.questData.requiredAmount;

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddCoins(q.questData.rewardCoins);
                EconomyManager.Instance.AddExperience(q.questData.rewardXP);

                if (q.questData.rewardReputation > 0)
                {
                    EconomyManager.Instance.ModifyReputation(q.questData.rewardReputation);
                }
            }

            if (q.questData.unlockedRecipeReward != null && RecipeManager.Instance != null)
            {
                RecipeManager.Instance.UnlockRecipe(q.questData.unlockedRecipeReward.recipeID);
            }

            Debug.Log($"[QuestManager] ¡Misión completada: {q.questData.title}!");
            GameEvents.TriggerQuestCompleted(q.questData);
        }

        private void SyncToSave()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.quests.Clear();
                foreach (var q in activeQuests)
                {
                    SaveManager.Instance.CurrentSave.quests.Add(new QuestSaveData
                    {
                        questID = q.questData.questID,
                        currentProgress = q.currentProgress,
                        isCompleted = q.isCompleted
                    });
                }
            }
        }
    }
}
