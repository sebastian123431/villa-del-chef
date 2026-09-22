using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    public enum IngredientCategory
    {
        Cultivable,
        Comprable,
        Especial
    }

    [CreateAssetMenu(fileName = "NewIngredient", menuName = "VillaDelChef/Ingredient")]
    public class IngredientSO : ScriptableObject
    {
        [Header("General Info")]
        public string ingredientID;
        public string ingredientName;
        [TextArea(2, 4)]
        public string description;
        public IngredientCategory category;
        public Sprite icon;

        [Header("Economy")]
        public int buyPrice = 5;
        public int sellPrice = 2;
        public int unlockLevel = 1;
    }
}
