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


        private void CreateDefaultSave()
        {
            currentSaveData = new SaveData
            {
                coins = 200,
                experience = 0,
                level = 1,
                reputation = 10,
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
            CreateDefaultSave();
        }
    }
}
