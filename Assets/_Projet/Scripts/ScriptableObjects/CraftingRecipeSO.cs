using System.Collections.Generic;
using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    public enum CraftingStationType
    {
        Molino,             // Molienda de granos/trigo a harina
        MesaAmasado,        // Amasado de pan, pizza, pastas
        Procesador,         // Triturado de salsas, purés, dips
        PrensaLactea,       // Maduración de quesos, mantequillas
        MarmitaDulce        // Mermeladas, jaleas, jarabes dulces
    }

    [CreateAssetMenu(fileName = "NewCraftRecipe", menuName = "VillaDelChef/Crafting Recipe")]
    public class CraftingRecipeSO : ScriptableObject
    {
        [Header("Recipe Identity")]
        public string craftID;
        public string recipeName;
        [TextArea(2, 3)]
        public string description;
        public CraftingStationType requiredStation;
        public Sprite icon;

        [Header("Production Timing")]
        public float craftTimeSeconds = 8f;

        [Header("Ingredients Required")]
        public List<IngredientRequirement> requiredIngredients = new List<IngredientRequirement>();

        [Header("Result Output")]
        public IngredientSO resultIngredient;
        public int resultAmount = 1;

        [Header("Progression Rewards")]
        public int experienceReward = 15;
        public int requiredPlayerLevel = 1;
    }
}
