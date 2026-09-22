using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Core;
using VillaDelChef.Inventory;
using VillaDelChef.Restaurant;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.UI;

namespace VillaDelChef.Cooking
{
    public enum StationState
    {
        Idle,
        Cooking,
        DishReady
    }

    public class CookingStation : GridObject
    {
        [Header("Station Configuration")]
        public StationType stationType = StationType.Cocina;
        public StationSO stationData;

        [Header("Runtime State")]
        public StationState currentState = StationState.Idle;
        public RecipeSO currentRecipe;
        public float currentCookTimer = 0f;
        public float totalCookTime = 0f;

        [Header("Visual Indicators")]
        public Transform dishSpawnPoint;
        public GameObject cookingFX;
        public SpriteRenderer readyIndicator;

        private DishInstance finishedDish;

        private void Update()
        {
            if (currentState == StationState.Cooking)
            {
                currentCookTimer += Time.deltaTime;
                if (currentCookTimer >= totalCookTime)
                {
                    OnCookingFinished();
                }
            }
        }

        public void OnInteract()
        {
            switch (currentState)
            {
                case StationState.Idle:
                    // Open recipe book filtered to this station
                    CookStationUI.Instance?.OpenForStation(this);
                    break;

                case StationState.Cooking:
                    Debug.Log($"[CookingStation] Cooking in progress... ({Mathf.RoundToInt(totalCookTime - currentCookTimer)}s remaining)");
                    break;

                case StationState.DishReady:
                    CollectFinishedDish();
                    break;
            }
        }

        public bool StartCooking(RecipeSO recipe)
        {
            if (currentState != StationState.Idle || recipe == null) return false;

            // Verify and consume ingredients
            if (InventoryManager.Instance != null && !InventoryManager.Instance.HasIngredients(recipe.requiredIngredients))
            {
                Debug.Log("[CookingStation] Missing ingredients in inventory!");
                return false;
            }

            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ConsumeIngredients(recipe.requiredIngredients);
            }

            currentRecipe = recipe;
            totalCookTime = recipe.cookTimeSeconds;
            currentCookTimer = 0f;
            currentState = StationState.Cooking;

            if (cookingFX != null) cookingFX.SetActive(true);
            if (readyIndicator != null) readyIndicator.gameObject.SetActive(false);

            GameEvents.TriggerCookingStarted(this, recipe);
            return true;
        }

        private void OnCookingFinished()
        {
            currentState = StationState.DishReady;
            if (cookingFX != null) cookingFX.SetActive(false);
            if (readyIndicator != null) readyIndicator.gameObject.SetActive(true);

            // Instantiate finished dish visual on station
            GameObject dishGO = new GameObject($"Dish_{currentRecipe.recipeName}");
            dishGO.transform.position = dishSpawnPoint != null ? dishSpawnPoint.position : transform.position + new Vector3(0f, 0.3f, 0f);
            finishedDish = dishGO.AddComponent<DishInstance>();
            finishedDish.Setup(currentRecipe);

            GameEvents.TriggerQuestProgressMade(QuestType.CookDishes, currentRecipe.recipeID, 1);
            Debug.Log($"[CookingStation] {currentRecipe.recipeName} is ready!");
        }

        public void CollectFinishedDish()
        {
            if (currentState != StationState.DishReady || finishedDish == null) return;

            if (DeliveryCounter.Instance != null && DeliveryCounter.Instance.HasSpace())
            {
                DeliveryCounter.Instance.AddDish(finishedDish);
                finishedDish = null;
                currentState = StationState.Idle;
                currentRecipe = null;
                if (readyIndicator != null) readyIndicator.gameObject.SetActive(false);
                Debug.Log("[CookingStation] Dish sent to Delivery Counter!");
            }
            else
            {
                Debug.Log("[CookingStation] Delivery counter is full! Wait for helper to serve dishes.");
            }
        }
    }
}
