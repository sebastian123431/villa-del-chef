using System;
using System.IO;
using UnityEngine;
using VillaDelChef.Core;

namespace VillaDelChef.Save
{
    public class SaveManager : MonoBehaviour
    {
        public static SaveManager Instance { get; private set; }

        private const string SAVE_FILE_NAME = "villadelchef_save.json";
        private string saveFilePath;
        private SaveData currentSaveData;

        public SaveData CurrentSave => currentSaveData;
        public long OfflineSecondsElapsed { get; private set; }

        public static bool HasSaveFile()
        {
            string path = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            return File.Exists(path);
        }

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                saveFilePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
                LoadOrCreateData();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            Application.targetFrameRate = currentSaveData.targetFrameRate;
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                SaveGame();
            }
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        public void LoadOrCreateData()
        {
            string backupPath = saveFilePath + ".bak";

            if (File.Exists(saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    currentSaveData = JsonUtility.FromJson<SaveData>(json);
                    
                    // Calculate offline time
                    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    OfflineSecondsElapsed = Math.Max(0, now - currentSaveData.lastSaveTimestampSeconds);

                    // Centralized migration for older saves (e.g. v1 -> v2)
                    MigrateSaveIfNeeded(currentSaveData);

                    Debug.Log($"[SaveManager] Loaded save game (v{currentSaveData.saveVersion}). Offline time: {OfflineSecondsElapsed} seconds.");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SaveManager] Error loading primary save data: {ex.Message}. Attempting backup restore.");
                }
            }

            // Attempt backup restore
            if (File.Exists(backupPath))
            {
                try
                {
                    string json = File.ReadAllText(backupPath);
                    currentSaveData = JsonUtility.FromJson<SaveData>(json);
                    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    OfflineSecondsElapsed = Math.Max(0, now - currentSaveData.lastSaveTimestampSeconds);

                    // Centralized migration for older saves (e.g. v1 -> v2)
                    MigrateSaveIfNeeded(currentSaveData);

                    Debug.Log($"[SaveManager] Restored save game from backup. Offline time: {OfflineSecondsElapsed} seconds.");
                    return;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SaveManager] Error loading backup save data: {ex.Message}.");
                }
            }

            CreateDefaultSave();
        }

        private void MigrateSaveIfNeeded(SaveData data)
        {
            if (data == null) return;

            if (data.saveVersion < 2)
            {
                Debug.Log($"[SaveManager] Migrando partida guardada de v{data.saveVersion} a v2...");

                bool hasAnyProgress = 
                    data.level > 1 ||
                    data.experience > 0 ||
                    (data.inventory != null && data.inventory.Count > 0) ||
                    (data.placedFurniture != null && data.placedFurniture.Count > 0) ||
                    (data.cropPlots != null && data.cropPlots.Count > 0) ||
                    (data.quests != null && data.quests.Count > 0) ||
                    (data.vendors != null && data.vendors.Count > 0) ||
                    (data.craftingStations != null && data.craftingStations.Count > 0) ||
                    (data.unlockedExpansions != null && data.unlockedExpansions.Count > 0) ||
                    (data.unlockedRecipes != null && data.unlockedRecipes.Count > 0) ||
                    data.tutorialCompleted ||
                    data.tutorialStep > 0 ||
                    (data.coins != 200 && data.coins != 250) ||
                    data.reputation != 10;

                if (hasAnyProgress)
                {
                    data.hasStartedGame = true;
                    data.starterItemsGranted = true;
                }

                data.saveVersion = 2;
                SaveGame();
                Debug.Log($"[SaveManager] Migración a v2 completada. hasStartedGame={data.hasStartedGame}, starterItemsGranted={data.starterItemsGranted}");
            }
        }

        public void SaveGame()
        {
            if (currentSaveData == null) return;

            // Sync dynamic subsystems before serialization
            if (VillaDelChef.Managers.CraftingManager.Instance != null)
            {
                VillaDelChef.Managers.CraftingManager.Instance.PopulateSaveData(currentSaveData);
            }
            if (VillaDelChef.Managers.ExpansionManager.Instance != null)
            {
                VillaDelChef.Managers.ExpansionManager.Instance.PopulateSaveData(currentSaveData);
            }

            currentSaveData.lastSaveTimestampSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string tempPath = saveFilePath + ".tmp";
            string backupPath = saveFilePath + ".bak";

            try
            {
                string json = JsonUtility.ToJson(currentSaveData, true);

                // 1. Write to temporary file
                File.WriteAllText(tempPath, json);

                // 2. Backup existing save if present
                if (File.Exists(saveFilePath))
                {
                    File.Copy(saveFilePath, backupPath, true);
                }

                // 3. Atomically replace main file
                File.Copy(tempPath, saveFilePath, true);
                File.Delete(tempPath);

                Debug.Log($"[SaveManager] Game saved atomically at {saveFilePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveManager] Failed to save game: {ex.Message}");
            }
        }

        public bool CanContinueGame()
        {
            return currentSaveData != null && currentSaveData.hasStartedGame;
        }

        public void StartNewGame()
        {
            ResetSaveData();
            if (currentSaveData != null)
            {
                currentSaveData.hasStartedGame = true;
                SaveGame();
            }
        }

        private void CreateDefaultSave()
        {
            currentSaveData = new SaveData
            {
                saveVersion = 2,
                coins = 200,
                experience = 0,
                level = 1,
                reputation = 10,
                hasStartedGame = false,
                starterItemsGranted = false,
                lastSaveTimestampSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            OfflineSecondsElapsed = 0;
            SaveGame();
        }

        public void ResetSaveData()
        {
            if (File.Exists(saveFilePath))
            {
                File.Delete(saveFilePath);
            }
            string backupPath = saveFilePath + ".bak";
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
            CreateDefaultSave();
        }
    }
}
