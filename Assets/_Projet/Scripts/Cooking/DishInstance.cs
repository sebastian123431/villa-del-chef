using UnityEngine;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Cooking
{
    public class DishInstance : MonoBehaviour
    {
        [Header("Dish Data")]
        public RecipeSO recipeData;
        [System.NonSerialized] public object targetCustomer;
        [System.NonSerialized] public object targetTable;
        [System.NonSerialized] public bool isReserved = false;

        [Header("Visuals")]
        public SpriteRenderer dishRenderer;

        public void Setup(RecipeSO recipe, object customer = null, object table = null)
        {
            this.recipeData = recipe;
            this.targetCustomer = customer;
            this.targetTable = table;

            if (dishRenderer == null)
            {
                dishRenderer = GetComponent<SpriteRenderer>();
                if (dishRenderer == null)
                {
                    dishRenderer = gameObject.AddComponent<SpriteRenderer>();
                }
            }

            if (dishRenderer != null && recipe != null)
            {
                dishRenderer.sprite = recipe.finishedDishSprite != null ? recipe.finishedDishSprite : recipe.icon;
                dishRenderer.sortingOrder = 10;
            }
        }
    }
}
