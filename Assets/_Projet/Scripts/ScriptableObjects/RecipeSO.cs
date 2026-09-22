using System;
using System.Collections.Generic;
using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    [Serializable]
    public class IngredientRequirement
    {
        public IngredientSO ingredient;
        public int amount = 1;
    }

    public enum RecipeCategory
    {
        PlatoPrincipal,
        Acompanamiento,
        Bebida,
        Postre
    }

    public enum RecipeRarity
    {
        Comun,
        Raro,
        Especial,
        Gourmet
    }

    [CreateAssetMenu(fileName = "NewRecipe", menuName = "VillaDelChef/Recipe")]
    public class RecipeSO : ScriptableObject
    {
        [Header("Recipe Information")]
        public string recipeID;
        public string recipeName;
        public Sprite icon;
        public Sprite finishedDishSprite;
        public RecipeCategory category = RecipeCategory.PlatoPrincipal;
        public RecipeRarity rarity = RecipeRarity.Comun;

        [Header("Cooking Requirements")]
        public StationType requiredStation = StationType.Cocina;
        public List<IngredientRequirement> requiredIngredients = new List<IngredientRequirement>();
        public float cookTimeSeconds = 15f;

        [Header("Rewards & Progression")]
        public int sellPrice = 50;
        public int experienceReward = 15;
        public int unlockLevel = 1;

        [TextArea(2, 4)]
        public string description;
    }
}
