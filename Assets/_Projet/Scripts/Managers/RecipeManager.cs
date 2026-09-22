using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Managers
{
    public class RecipeManager : MonoBehaviour
    {
        public static RecipeManager Instance { get; private set; }

        [Header("Recipe Database")]
        public List<RecipeSO> allRecipes = new List<RecipeSO>();

        private HashSet<string> unlockedRecipeIDs = new HashSet<string>();

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
            if (allRecipes == null || allRecipes.Count == 0)
            {
                allRecipes = new List<RecipeSO>(Resources.LoadAll<RecipeSO>("Recipes"));
            }
            LoadUnlockedRecipes();
            GameEvents.OnLevelUp += OnPlayerLevelUp;
        }

        private void OnDestroy()
        {
            GameEvents.OnLevelUp -= OnPlayerLevelUp;
        }

        private void LoadUnlockedRecipes()
        {
            unlockedRecipeIDs.Clear();

            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null && SaveManager.Instance.CurrentSave.unlockedRecipes.Count > 0)
            {
                foreach (var id in SaveManager.Instance.CurrentSave.unlockedRecipes)
                {
                    unlockedRecipeIDs.Add(id);
                }
            }
            else
            {
                // Unlock level 1 recipes initially
                foreach (var recipe in allRecipes)
                {
                    if (recipe != null && recipe.unlockLevel <= 1)
                    {
                        unlockedRecipeIDs.Add(recipe.recipeID);
                    }
                }
                SyncToSave();
            }
        }

        private void OnPlayerLevelUp(int newLevel)
        {
            foreach (var recipe in allRecipes)
            {
                if (recipe != null && recipe.unlockLevel <= newLevel && !unlockedRecipeIDs.Contains(recipe.recipeID))
                {
                    unlockedRecipeIDs.Add(recipe.recipeID);
                    Debug.Log($"[RecipeManager] New recipe unlocked: {recipe.recipeName}!");
                }
            }
            SyncToSave();
        }

        public bool IsRecipeUnlocked(string recipeID)
        {
            return unlockedRecipeIDs.Contains(recipeID);
        }

        public List<RecipeSO> GetRecipesForStation(StationType stationType)
        {
            List<RecipeSO> list = new List<RecipeSO>();
            foreach (var recipe in allRecipes)
            {
                if (recipe != null && recipe.requiredStation == stationType && IsRecipeUnlocked(recipe.recipeID))
                {
                    list.Add(recipe);
                }
            }
            return list;
        }

        public RecipeSO GetRandomUnlockedRecipe()
        {
            List<RecipeSO> unlocked = new List<RecipeSO>();
            foreach (var r in allRecipes)
            {
                if (r != null && IsRecipeUnlocked(r.recipeID))
                {
                    unlocked.Add(r);
                }
            }

            if (unlocked.Count > 0)
            {
                return unlocked[Random.Range(0, unlocked.Count)];
            }
            return allRecipes.Count > 0 ? allRecipes[0] : null;
        }

        private void SyncToSave()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.unlockedRecipes = new List<string>(unlockedRecipeIDs);
            }
        }
    }
}
