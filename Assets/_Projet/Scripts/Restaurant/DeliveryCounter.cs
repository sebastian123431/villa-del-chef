using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.Core;
using VillaDelChef.Interaction;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Restaurant
{
    public class DeliveryCounter : GridObject, IInteractable
    {
        public static DeliveryCounter Instance { get; private set; }

        public string InteractionPrompt => $"Mostrador de Entrega ({readyDishes.Count}/{GetEffectiveCapacity()})";
        public bool CanInteract => true;
        public void Interact() { }

        [Header("Counter Settings")]
        [Range(1, 3)]
        public int counterLevel = 2; // Level 1 = 2, Level 2 = 4, Level 3 = 6
        public int maxCapacity = 4;
        public List<Transform> dishSlots = new List<Transform>();

        [Header("Dishes on Counter")]
        public List<DishInstance> readyDishes = new List<DishInstance>();

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

        public int GetEffectiveCapacity()
        {
            switch (counterLevel)
            {
                case 1: return 2;
                case 2: return 4;
                case 3: return 6;
                default: return Mathf.Max(2, maxCapacity);
            }
        }

        public bool HasSpace()
        {
            return readyDishes.Count < GetEffectiveCapacity();
        }

        public bool AddDish(DishInstance dish)
        {
            if (!HasSpace() || dish == null) return false;

            readyDishes.Add(dish);
            RealignDishes();

            GameEvents.TriggerDishReady(dish);
            return true;
        }

        public DishInstance FindMatchingDish(RecipeSO recipe)
        {
            if (recipe == null || readyDishes.Count == 0) return null;

            for (int i = 0; i < readyDishes.Count; i++)
            {
                if (readyDishes[i] != null && readyDishes[i].recipeData != null && readyDishes[i].recipeData.recipeID == recipe.recipeID)
                {
                    return readyDishes[i];
                }
            }
            return null;
        }

        public DishInstance TakeSpecificDish(DishInstance targetDish)
        {
            if (targetDish == null || !readyDishes.Contains(targetDish)) return null;

            readyDishes.Remove(targetDish);
            RealignDishes();
            return targetDish;
        }

        public DishInstance TakeNextDish()
        {
            if (readyDishes.Count == 0) return null;

            DishInstance dish = readyDishes[0];
            readyDishes.RemoveAt(0);
            RealignDishes();
            return dish;
        }

        private void RealignDishes()
        {
            for (int i = 0; i < readyDishes.Count; i++)
            {
                if (readyDishes[i] == null) continue;

                if (dishSlots != null && i < dishSlots.Count && dishSlots[i] != null)
                {
                    readyDishes[i].transform.SetParent(dishSlots[i]);
                    readyDishes[i].transform.localPosition = Vector3.zero;
                }
                else
                {
                    readyDishes[i].transform.SetParent(transform);
                    readyDishes[i].transform.localPosition = new Vector3((i - (GetEffectiveCapacity() / 2f) + 0.5f) * 0.35f, 0.2f, 0f);
                }
            }
        }
    }
}

