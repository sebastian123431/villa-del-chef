using System;
using UnityEngine;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Core
{
    public static class GameEvents
    {
        // Economy & Progression
        public static event Action<int> OnCoinsChanged;
        public static void TriggerCoinsChanged(int currentCoins) => OnCoinsChanged?.Invoke(currentCoins);

        public static event Action<int, int> OnExperienceChanged;
        public static void TriggerExperienceChanged(int currentXP, int currentLevel) => OnExperienceChanged?.Invoke(currentXP, currentLevel);

        public static event Action<int> OnLevelUp;
        public static void TriggerLevelUp(int newLevel) => OnLevelUp?.Invoke(newLevel);

        public static event Action<int> OnReputationChanged;
        public static void TriggerReputationChanged(int reputation) => OnReputationChanged?.Invoke(reputation);

        // Inventory
        public static event Action<string, int> OnInventoryUpdated;
        public static void TriggerInventoryUpdated(string itemID, int count) => OnInventoryUpdated?.Invoke(itemID, count);

        // Cooking & Kitchen
        public static event Action<object, RecipeSO> OnCookingStarted;
        public static void TriggerCookingStarted(object station, RecipeSO recipe) => OnCookingStarted?.Invoke(station, recipe);

        public static event Action<object> OnDishReady;
        public static void TriggerDishReady(object dishInstance) => OnDishReady?.Invoke(dishInstance);

        public static event Action<object, object> OnDishDelivered;
        public static void TriggerDishDelivered(object dishInstance, object table) => OnDishDelivered?.Invoke(dishInstance, table);

        // Farming
        public static event Action<object, CropSO> OnCropPlanted;
        public static void TriggerCropPlanted(object plot, CropSO crop) => OnCropPlanted?.Invoke(plot, crop);

        public static event Action<object, CropSO, int> OnCropHarvested;
        public static void TriggerCropHarvested(object plot, CropSO crop, int amount) => OnCropHarvested?.Invoke(plot, crop, amount);

        // Customers
        public static event Action<object> OnCustomerArrived;
        public static void TriggerCustomerArrived(object customer) => OnCustomerArrived?.Invoke(customer);

        public static event Action<object> OnCustomerOrdered;
        public static void TriggerCustomerOrdered(object customer) => OnCustomerOrdered?.Invoke(customer);

        public static event Action<object, bool> OnCustomerServed;
        public static void TriggerCustomerServed(object customer, bool satisfied) => OnCustomerServed?.Invoke(customer, satisfied);

        public static event Action<object> OnCustomerLeft;
        public static void TriggerCustomerLeft(object customer) => OnCustomerLeft?.Invoke(customer);

        // Build Mode & Furniture
        public static event Action<bool> OnBuildModeToggled;
        public static void TriggerBuildModeToggled(bool isBuildMode) => OnBuildModeToggled?.Invoke(isBuildMode);

        public static event Action<FurnitureSO, Vector2Int> OnFurniturePlaced;
        public static void TriggerFurniturePlaced(FurnitureSO furniture, Vector2Int position) => OnFurniturePlaced?.Invoke(furniture, position);

        public static event Action<FurnitureSO> OnFurnitureSold;
        public static void TriggerFurnitureSold(FurnitureSO furniture) => OnFurnitureSold?.Invoke(furniture);

        // Quests
        public static event Action<QuestSO> OnQuestCompleted;
        public static void TriggerQuestCompleted(QuestSO quest) => OnQuestCompleted?.Invoke(quest);

        public static event Action<QuestType, string, int> OnQuestProgressMade;
        public static void TriggerQuestProgressMade(QuestType type, string targetID, int amount) => OnQuestProgressMade?.Invoke(type, targetID, amount);
    }
}
