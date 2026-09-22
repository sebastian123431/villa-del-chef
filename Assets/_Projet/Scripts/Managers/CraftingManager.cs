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

        public void SaveToCurrentSave()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSave == null) return;

            var list = SaveManager.Instance.CurrentSave.craftingStations;
            if (list == null)
            {
                list = new List<CraftingStationSaveEntry>();
                SaveManager.Instance.CurrentSave.craftingStations = list;
            }
            list.Clear();

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
                    isReadyToCollect = (s.currentState == CraftingState.ReadyToCollect)
                };
                list.Add(entry);
            }
        }

        public void LoadFromSave()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSave == null) return;

            var list = SaveManager.Instance.CurrentSave.craftingStations;
            if (list == null || list.Count == 0) return;

            foreach (var entry in list)
            {
                if (entry == null) continue;

                var station = activeStations.Find(s => s != null && s.gridPosition.x == entry.gridX && s.gridPosition.y == entry.gridY);
                if (station != null && !string.IsNullOrEmpty(entry.currentCraftID))
                {
                    var recipe = GetRecipeByID(entry.currentCraftID);
                    if (recipe != null)
                    {
                        station.currentRecipe = recipe;
                        station.totalCraftTime = recipe.craftTimeSeconds;

                        if (entry.isReadyToCollect || entry.remainingTime <= 0f)
                        {
                            station.currentState = CraftingState.ReadyToCollect;
                            station.currentCraftTimer = recipe.craftTimeSeconds;
                        }
                        else
                        {
                            station.currentState = CraftingState.Crafting;
                            station.currentCraftTimer = Mathf.Max(0f, recipe.craftTimeSeconds - entry.remainingTime);
                        }
                        station.UpdateVisuals();
                    }
                }
            }
        }
    }
}
