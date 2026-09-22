using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Restaurant
{
    public class Table : GridObject
    {
        [Header("Chairs")]
        public List<Chair> chairs = new List<Chair>();

        [Header("Plates Spot")]
        public Transform dishSpot;

        [Header("State")]
        public bool isReserved = false;
        public RecipeSO currentOrder;
        public DishInstance servedDish;
        public bool needsCleaning = false;

        public bool HasAvailableChair()
        {
            if (isReserved) return false;
            foreach (var chair in chairs)
            {
                if (chair != null && !chair.isOccupied)
                {
                    return true;
                }
            }
            return false;
        }

        public Chair GetFirstAvailableChair()
        {
            foreach (var chair in chairs)
            {
                if (chair != null && !chair.isOccupied)
                {
                    return chair;
                }
            }
            return null;
        }

        public void PlaceDish(DishInstance dish)
        {
            servedDish = dish;
            if (dish != null)
            {
                dish.transform.SetParent(dishSpot != null ? dishSpot : transform);
                dish.transform.localPosition = dishSpot != null ? Vector3.zero : new Vector3(0f, 0.2f, 0f);
            }
        }

        public void ClearTable()
        {
            if (servedDish != null)
            {
                Destroy(servedDish.gameObject);
                servedDish = null;
            }
            currentOrder = null;
            isReserved = false;
            needsCleaning = false;
            foreach (var chair in chairs)
            {
                if (chair != null) chair.SetOccupied(false);
            }
        }
    }
}
