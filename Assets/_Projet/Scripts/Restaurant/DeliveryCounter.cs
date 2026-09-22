using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.Core;

namespace VillaDelChef.Restaurant
{
    public class DeliveryCounter : GridObject
    {
        public static DeliveryCounter Instance { get; private set; }

        [Header("Counter Settings")]
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

        public bool HasSpace()
        {
            return readyDishes.Count < maxCapacity;
        }

        public bool AddDish(DishInstance dish)
        {
            if (!HasSpace() || dish == null) return false;

            readyDishes.Add(dish);
            int slotIndex = readyDishes.Count - 1;

            if (dishSlots != null && slotIndex < dishSlots.Count && dishSlots[slotIndex] != null)
            {
                dish.transform.SetParent(dishSlots[slotIndex]);
                dish.transform.localPosition = Vector3.zero;
            }
            else
            {
                dish.transform.SetParent(transform);
                dish.transform.localPosition = new Vector3((slotIndex - 1.5f) * 0.4f, 0.2f, 0f);
            }

            GameEvents.TriggerDishReady(dish);
            return true;
        }

        public DishInstance TakeNextDish()
        {
            if (readyDishes.Count == 0) return null;

            DishInstance dish = readyDishes[0];
            readyDishes.RemoveAt(0);

            // Re-align remaining dishes
            for (int i = 0; i < readyDishes.Count; i++)
            {
                if (dishSlots != null && i < dishSlots.Count && dishSlots[i] != null)
                {
                    readyDishes[i].transform.SetParent(dishSlots[i]);
                    readyDishes[i].transform.localPosition = Vector3.zero;
                }
                else
                {
                    readyDishes[i].transform.localPosition = new Vector3((i - 1.5f) * 0.4f, 0.2f, 0f);
                }
            }

            return dish;
        }
    }
}
