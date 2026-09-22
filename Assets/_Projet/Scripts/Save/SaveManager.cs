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
            if (File.Exists(saveFilePath))
            {
                try
                {
                    string json = File.ReadAllText(saveFilePath);
                    currentSaveData = JsonUtility.FromJson<SaveData>(json);
                    
                    // Calculate offline time
                    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    OfflineSecondsElapsed = Math.Max(0, now - currentSaveData.lastSaveTimestampSeconds);
                    Debug.Log($"[SaveManager] Loaded save game. Offline time: {OfflineSecondsElapsed} seconds.");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[SaveManager] Error loading save data: {ex.Message}. Creating fresh save.");
                    CreateDefaultSave();
                }
            }
            else
            {
                CreateDefaultSave();
            }
        }

        public void SaveGame()
        {
            if (currentSaveData == null) return;

            currentSaveData.lastSaveTimestampSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            try
            {
                string json = JsonUtility.ToJson(currentSaveData, true);
                File.WriteAllText(saveFilePath, json);
                Debug.Log($"[SaveManager] Game saved successfully at {saveFilePath}");
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
