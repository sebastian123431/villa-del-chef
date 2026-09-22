using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Crafting;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Managers
{
    public class CraftingManager : MonoBehaviour
    {
        private static CraftingManager _instance;
        public static CraftingManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<CraftingManager>();
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Recipes Database")]
        public List<CraftingRecipeSO> allCraftRecipes = new List<CraftingRecipeSO>();

        [Header("Active Stations")]
        public List<CraftingStation> activeStations = new List<CraftingStation>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
                return;
            }

            LoadRecipesDatabase();
        }

        private void Start()
        {
            LoadFromSave();
            StartCoroutine(CentralizedCraftingTickRoutine());
        }

        private System.Collections.IEnumerator CentralizedCraftingTickRoutine()
        {
            var wait = new WaitForSeconds(0.5f);
            while (true)
            {
                yield return wait;
                long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                for (int i = 0; i < activeStations.Count; i++)
                {
                    if (activeStations[i] != null && activeStations[i].currentState == CraftingState.Crafting)
                    {
                        activeStations[i].TickCrafting(0.5f, now);
                    }
                }
            }
        }

        public void LoadRecipesDatabase()
        {
            var loaded = Resources.LoadAll<CraftingRecipeSO>("CraftingRecipes");
            if (loaded != null && loaded.Length > 0)
            {
                allCraftRecipes = new List<CraftingRecipeSO>(loaded);
            }
        }

        public void RegisterStation(CraftingStation station)
        {
            if (station != null && !activeStations.Contains(station))
            {
                activeStations.Add(station);
                RestoreStationFromSave(station);
            }
        }

        public void UnregisterStation(CraftingStation station)
        {
            if (station != null && activeStations.Contains(station))
            {
                activeStations.Remove(station);
            }
        }

        public List<CraftingRecipeSO> GetRecipesForStation(CraftingStationType stationType)
        {
            List<CraftingRecipeSO> matching = new List<CraftingRecipeSO>();
            foreach (var recipe in allCraftRecipes)
            {
                if (recipe != null && recipe.requiredStation == stationType)
                {
                    matching.Add(recipe);
                }
            }
            return matching;
        }

        public CraftingRecipeSO GetRecipeByID(string craftID)
        {
            if (string.IsNullOrEmpty(craftID)) return null;
            return allCraftRecipes.Find(r => r != null && r.craftID == craftID);
        }

        public void PopulateSaveData(SaveData data)
        {
            if (data == null) return;
            if (data.craftingStations == null)
            {
                data.craftingStations = new List<CraftingStationSaveEntry>();
            }
            data.craftingStations.Clear();

            for (int i = 0; i < activeStations.Count; i++)
            {
                var s = activeStations[i];
                if (s == null) continue;

                var entry = new CraftingStationSaveEntry
                {
                    stationID = $"craft_station_{i}",
                    stationType = s.stationType.ToString(),
                    gridX = s.gridPosition.x,
                    gridY = s.gridPosition.y,
                    currentCraftID = s.currentRecipe != null ? s.currentRecipe.craftID : "",
                    remainingTime = s.GetRemainingSeconds(),
                    isReadyToCollect = (s.currentState == CraftingState.ReadyToCollect),
                    craftStartTimestampSeconds = s.craftStartTimestampUTC,
                    craftFinishTimestampSeconds = s.craftFinishTimestampUTC
                };
                data.craftingStations.Add(entry);
            }
        }

        public void SaveToCurrentSave()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSave == null) return;
            PopulateSaveData(SaveManager.Instance.CurrentSave);
        }

        public void LoadFromSave()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSave == null) return;

            var list = SaveManager.Instance.CurrentSave.craftingStations;
            if (list == null || list.Count == 0) return;

            foreach (var station in activeStations)
            {
                RestoreStationFromSave(station);
            }
        }

        private void RestoreStationFromSave(CraftingStation station)
        {
            if (station == null || SaveManager.Instance == null || SaveManager.Instance.CurrentSave == null) return;
            var list = SaveManager.Instance.CurrentSave.craftingStations;
            if (list == null || list.Count == 0) return;

            var entry = list.Find(e => e != null && e.gridX == station.gridPosition.x && e.gridY == station.gridPosition.y);
            if (entry == null || string.IsNullOrEmpty(entry.currentCraftID)) return;

            var recipe = GetRecipeByID(entry.currentCraftID);
            if (recipe == null) return;

            station.currentRecipe = recipe;
            station.totalCraftTime = recipe.craftTimeSeconds;

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            float remaining;
            if (entry.craftFinishTimestampSeconds > 0)
            {
                remaining = (float)(entry.craftFinishTimestampSeconds - now);
            }
            else
            {
                long offlineSecs = SaveManager.Instance.OfflineSecondsElapsed;
                remaining = entry.remainingTime - offlineSecs;
            }

            if (entry.isReadyToCollect || remaining <= 0f)
            {
                station.currentState = CraftingState.ReadyToCollect;
                station.currentCraftTimer = recipe.craftTimeSeconds;
                station.craftStartTimestampUTC = 0;
                station.craftFinishTimestampUTC = 0;
            }
            else
            {
                station.currentState = CraftingState.Crafting;
                station.currentCraftTimer = Mathf.Max(0f, recipe.craftTimeSeconds - remaining);
                station.craftFinishTimestampUTC = now + (long)Mathf.Ceil(remaining);
                station.craftStartTimestampUTC = now - (long)Mathf.Floor(station.currentCraftTimer);
            }
            station.UpdateVisuals();
        }
    }
}
