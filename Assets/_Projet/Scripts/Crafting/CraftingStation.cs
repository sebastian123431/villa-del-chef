using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Core;
using VillaDelChef.Interaction;
using VillaDelChef.Inventory;
using VillaDelChef.Managers;
using VillaDelChef.Progression;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.UI;

namespace VillaDelChef.Crafting
{
    public enum CraftingState
    {
        Idle,
        Crafting,
        ReadyToCollect
    }

    public class CraftingStation : GridObject, IInteractable
    {
        public string InteractionPrompt => (currentState == CraftingState.ReadyToCollect) 
            ? "Recoger Producto" 
            : (currentState == CraftingState.Crafting ? "Ver Elaboración" : "Elaborar Insumo");

        public bool CanInteract => true;
        public void Interact() => OnInteract();

        [Header("Station Configuration")]
        public CraftingStationType stationType = CraftingStationType.Molino;
        public string stationName = "Estación de Crafting";

        [Header("Runtime State")]
        public CraftingState currentState = CraftingState.Idle;
        public CraftingRecipeSO currentRecipe;
        public float currentCraftTimer = 0f;
        public float totalCraftTime = 0f;
        public long craftStartTimestampUTC = 0;
        public long craftFinishTimestampUTC = 0;

        [Header("Visual Feedback")]
        public SpriteRenderer stationRenderer;
        public SpriteRenderer readyIndicator;
        public GameObject activeFX;
        public Transform outputSpawnPoint;

        private void Start()
        {
            if (CraftingManager.Instance != null)
            {
                CraftingManager.Instance.RegisterStation(this);
            }
            UpdateVisuals();
        }

        public void TickCrafting(float deltaSeconds, long nowUtc)
        {
            if (currentState == CraftingState.Crafting)
            {
                if (craftFinishTimestampUTC > 0 && nowUtc >= craftFinishTimestampUTC)
                {
                    OnCraftFinished();
                }
                else
                {
                    currentCraftTimer += deltaSeconds;
                    if (currentCraftTimer >= totalCraftTime)
                    {
                        OnCraftFinished();
                    }
                }
            }
        }

        public void OnInteract()
        {
            switch (currentState)
            {
                case CraftingState.Idle:
                case CraftingState.Crafting:
                    if (CraftingUI.Instance != null)
                    {
                        CraftingUI.Instance.OpenForStation(this);
                    }
                    else
                    {
                        Debug.Log($"[CraftingStation] {stationName} en estado {currentState}. Tiempo: {Mathf.RoundToInt(totalCraftTime - currentCraftTimer)}s");
                    }
                    break;

                case CraftingState.ReadyToCollect:
                    CollectCraftedItem();
                    break;
            }
        }

        public bool StartCrafting(CraftingRecipeSO recipe)
        {
            if (currentState != CraftingState.Idle || recipe == null) return false;

            if (InventoryManager.Instance != null && !InventoryManager.Instance.HasIngredients(recipe.requiredIngredients))
            {
                Debug.LogWarning($"[CraftingStation] No tienes los ingredientes requeridos para {recipe.recipeName}.");
                return false;
            }

            // Consume ingredients
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.ConsumeIngredients(recipe.requiredIngredients);
            }

            currentRecipe = recipe;
            totalCraftTime = recipe.craftTimeSeconds;
            currentCraftTimer = 0f;
            currentState = CraftingState.Crafting;

            long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            craftStartTimestampUTC = now;
            craftFinishTimestampUTC = now + (long)Mathf.Ceil(recipe.craftTimeSeconds);

            GameEvents.TriggerCraftStarted(this, recipe);
            UpdateVisuals();
            CraftingManager.Instance?.SaveToCurrentSave();
            Debug.Log($"[CraftingStation] Iniciada elaboración de {recipe.recipeName} ({recipe.craftTimeSeconds}s).");
            return true;
        }

        public void OnCraftFinished()
        {
            currentState = CraftingState.ReadyToCollect;
            currentCraftTimer = totalCraftTime;

            if (currentRecipe != null)
            {
                GameEvents.TriggerCraftCompleted(this, currentRecipe);
            }

            UpdateVisuals();
            CraftingManager.Instance?.SaveToCurrentSave();
            Debug.Log($"[CraftingStation] ¡{currentRecipe?.recipeName} completado! Listo para recolectar.");
        }

        public bool CollectCraftedItem()
        {
            if (currentState != CraftingState.ReadyToCollect || currentRecipe == null) return false;

            int amount = currentRecipe.resultAmount;
            string itemID = currentRecipe.resultIngredient != null ? currentRecipe.resultIngredient.ingredientID : currentRecipe.craftID;

            if (InventoryManager.Instance != null && currentRecipe.resultIngredient != null)
            {
                InventoryManager.Instance.AddItem(currentRecipe.resultIngredient.ingredientID, amount);
            }

            if (ProgressionManager.Instance != null && currentRecipe.experienceReward > 0)
            {
                ProgressionManager.Instance.AddExperience(currentRecipe.experienceReward);
            }

            GameEvents.TriggerCraftCollected(this, currentRecipe, amount);
            GameEvents.TriggerQuestProgressMade(QuestType.CraftItems, currentRecipe.craftID, amount);

            VillaDelChef.UI.FloatingTextManager.Instance?.ShowSuccess($"+{amount} {currentRecipe.recipeName}", transform.position + Vector3.up * 0.8f);
            if (currentRecipe.experienceReward > 0)
            {
                VillaDelChef.UI.FloatingTextManager.Instance?.ShowXP(currentRecipe.experienceReward, transform.position + Vector3.up * 1.1f);
            }

            Debug.Log($"[CraftingStation] Recolectado {amount}x {currentRecipe.recipeName}. Recompensa: +{currentRecipe.experienceReward} XP");

            currentRecipe = null;
            currentState = CraftingState.Idle;
            currentCraftTimer = 0f;
            totalCraftTime = 0f;
            craftStartTimestampUTC = 0;
            craftFinishTimestampUTC = 0;

            UpdateVisuals();
            CraftingManager.Instance?.SaveToCurrentSave();
            return true;
        }

        public void SpeedUpInstantly()
        {
            if (currentState == CraftingState.Crafting)
            {
                currentCraftTimer = totalCraftTime;
                craftFinishTimestampUTC = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                OnCraftFinished();
            }
        }

        public float GetProgress()
        {
            if (totalCraftTime <= 0f) return 0f;
            return Mathf.Clamp01(currentCraftTimer / totalCraftTime);
        }

        public float GetRemainingSeconds()
        {
            return Mathf.Max(0f, totalCraftTime - currentCraftTimer);
        }

        public void UpdateVisuals()
        {
            if (readyIndicator != null)
            {
                bool isReady = (currentState == CraftingState.ReadyToCollect);
                readyIndicator.gameObject.SetActive(isReady);
                if (isReady && currentRecipe != null && currentRecipe.icon != null)
                {
                    readyIndicator.sprite = currentRecipe.icon;
                }
            }

            if (activeFX != null)
            {
                activeFX.SetActive(currentState == CraftingState.Crafting);
            }
        }

        private void OnMouseDown()
        {
            if (!UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                OnInteract();
            }
        }

        private void OnDestroy()
        {
            if (CraftingManager.Instance != null)
            {
                CraftingManager.Instance.UnregisterStation(this);
            }
        }
    }
}
