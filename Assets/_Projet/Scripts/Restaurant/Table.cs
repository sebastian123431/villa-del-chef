using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.Customers;
using VillaDelChef.Interaction;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Restaurant
{
    public enum TableState
    {
        Available,
        Reserved,
        Occupied,
        WaitingFood,
        Eating,
        Dirty,
        Cleaning
    }

    public class Table : GridObject, IInteractable
    {
        public string InteractionPrompt => tableState == TableState.Dirty ? "Mesa Sucia (esperando limpieza)" : "Mesa del Restaurante";
        public bool CanInteract => true;
        public void Interact() { }

        [Header("Chairs")]
        public List<Chair> chairs = new List<Chair>();

        [Header("Plates Spot")]
        public Transform dishSpot;

        [Header("Visual Feedback")]
        public GameObject dirtyIndicator;

        [Header("State")]
        public TableState tableState = TableState.Available;
        public CustomerController currentCustomer;
        public bool isReserved = false;
        public bool isCleaningReserved = false;
        public RecipeSO currentOrder;
        public DishInstance servedDish;
        public bool needsCleaning = false;

        public bool HasAvailableChair()
        {
            if (isReserved || needsCleaning || isCleaningReserved || tableState != TableState.Available) return false;

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

        public void ReserveForCustomer(CustomerController customer)
        {
            currentCustomer = customer;
            isReserved = true;
            tableState = TableState.Reserved;
        }

        public void CustomerSeated(CustomerController customer)
        {
            currentCustomer = customer;
            isReserved = false;
            tableState = TableState.Occupied;
        }

        public void SetWaitingOrder(RecipeSO order)
        {
            currentOrder = order;
            tableState = TableState.WaitingFood;
        }

        public void PlaceDish(DishInstance dish)
        {
            servedDish = dish;
            tableState = TableState.Eating;

            if (dish != null)
            {
                dish.transform.SetParent(dishSpot != null ? dishSpot : transform);
                dish.transform.localPosition = dishSpot != null ? Vector3.zero : new Vector3(0f, 0.2f, 0f);
            }
        }

        public void MarkDirty()
        {
            tableState = TableState.Dirty;
            needsCleaning = true;
            isCleaningReserved = false;
            currentCustomer = null;

            if (dirtyIndicator != null)
            {
                dirtyIndicator.SetActive(true);
            }
            else
            {
                // Ensure dirty visual exists if no explicit prefab child
                CreateDefaultDirtyVisual();
            }

            foreach (var chair in chairs)
            {
                if (chair != null) chair.SetOccupied(false);
            }
        }

        public void StartCleaning()
        {
            tableState = TableState.Cleaning;
            isCleaningReserved = true;
        }

        public void FinishCleaning()
        {
            if (servedDish != null)
            {
                Destroy(servedDish.gameObject);
                servedDish = null;
            }

            if (dirtyIndicator != null)
            {
                dirtyIndicator.SetActive(false);
            }

            currentOrder = null;
            currentCustomer = null;
            isReserved = false;
            isCleaningReserved = false;
            needsCleaning = false;
            tableState = TableState.Available;
        }

        public void ClearTable()
        {
            // If the table is dirty or being cleaned, do NOT reset it to Available!
            if (tableState == TableState.Dirty || tableState == TableState.Cleaning || needsCleaning)
            {
                currentCustomer = null;
                return;
            }
            FinishCleaning();
        }

        private void CreateDefaultDirtyVisual()
        {
            if (dirtyIndicator == null)
            {
                dirtyIndicator = new GameObject("DirtyVisual");
                dirtyIndicator.transform.SetParent(transform);
                dirtyIndicator.transform.localPosition = new Vector3(0f, 0.25f, 0f);

                SpriteRenderer sr = dirtyIndicator.AddComponent<SpriteRenderer>();
                sr.color = new Color(0.6f, 0.45f, 0.25f, 0.85f);
                SpriteRenderer baseSR = GetComponent<SpriteRenderer>();
                sr.sortingOrder = (baseSR != null ? baseSR.sortingOrder : 10) + 2;

                // Simple 8x8 dirty crumbs texture
                Texture2D tex = new Texture2D(8, 8);
                Color c = new Color(0.45f, 0.3f, 0.15f, 1f);
                Color clear = Color.clear;
                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        tex.SetPixel(x, y, (x % 3 == 0 && y % 2 == 0) ? c : clear);
                    }
                }
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                sr.sprite = Sprite.Create(tex, new Rect(0, 0, 8, 8), new Vector2(0.5f, 0.5f), 16);
            }
            dirtyIndicator.SetActive(true);
        }
    }
}

