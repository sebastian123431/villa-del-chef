using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    public enum QuestType
    {
        CookDishes,
        HarvestCrops,
        ServeCustomers,
        EarnCoins,
        BuyFurniture,
        CollectIngredients
    }

    [CreateAssetMenu(fileName = "NewQuest", menuName = "VillaDelChef/Quest")]
    public class QuestSO : ScriptableObject
    {
        [Header("Quest Identity")]
        public string questID;
        public string title;
        [TextArea(2, 3)]
        public string description;
        public QuestType questType;
        public Sprite icon;

        [Header("Goal")]
        [Tooltip("Specific target ID, e.g. recipeID, cropID, or empty for any")]
        public string targetID;
        public int requiredAmount = 5;

        [Header("Rewards")]
        public int rewardCoins = 100;
        public int rewardXP = 30;
        public RecipeSO unlockedRecipeReward;
        public FurnitureSO unlockedFurnitureReward;
    }
}
