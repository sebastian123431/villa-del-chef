using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Inventory
{
    public class InventoryManager : MonoBehaviour
    {
        public static InventoryManager Instance { get; private set; }

        [Header("Starting Items")]
        public List<IngredientRequirement> defaultStartingItems = new List<IngredientRequirement>();

        private Dictionary<string, int> inventory = new Dictionary<string, int>();

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

        private void Start()
        {
            LoadFromSaveOrDefaults();
        }

        private void LoadFromSaveOrDefaults()
        {
            inventory.Clear();
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null && SaveManager.Instance.CurrentSave.inventory.Count > 0)
            {
                foreach (var entry in SaveManager.Instance.CurrentSave.inventory)
                {
                    inventory[entry.itemID] = entry.amount;
                    GameEvents.TriggerInventoryUpdated(entry.itemID, entry.amount);
                }
            }
            else
            {
                // Give basic starting items
                foreach (var req in defaultStartingItems)
                {
                    if (req.ingredient != null)
                    {
                        AddItem(req.ingredient.ingredientID, req.amount);
                    }
                }
            }
        }

        public int GetItemCount(string itemID)
        {
            return inventory.TryGetValue(itemID, out int count) ? count : 0;
        }

        public IReadOnlyDictionary<string, int> GetAllItems()
        {
            return inventory;
        }

        public void AddItem(string itemID, int amount)
        {
            if (string.IsNullOrEmpty(itemID) || amount <= 0) return;

            if (inventory.ContainsKey(itemID))
            {
                inventory[itemID] += amount;
            }
            else
            {
                inventory[itemID] = amount;
            }

            SyncToSave();
            GameEvents.TriggerInventoryUpdated(itemID, inventory[itemID]);
        }

        public bool RemoveItem(string itemID, int amount)
        {
            if (string.IsNullOrEmpty(itemID) || amount <= 0) return true;
            if (!inventory.ContainsKey(itemID) || inventory[itemID] < amount) return false;

            inventory[itemID] -= amount;
            if (inventory[itemID] <= 0)
            {
                inventory.Remove(itemID);
            }

            SyncToSave();
            GameEvents.TriggerInventoryUpdated(itemID, inventory.ContainsKey(itemID) ? inventory[itemID] : 0);
            return true;
        }

        public bool HasIngredients(List<IngredientRequirement> requirements)
        {
            if (requirements == null || requirements.Count == 0) return true;

            foreach (var req in requirements)
            {
                if (req.ingredient == null) continue;
                if (GetItemCount(req.ingredient.ingredientID) < req.amount)
                {
                    return false;
                }
            }
            return true;
        }

        public bool ConsumeIngredients(List<IngredientRequirement> requirements)
        {
            if (!HasIngredients(requirements)) return false;

            foreach (var req in requirements)
            {
                if (req.ingredient == null) continue;
                RemoveItem(req.ingredient.ingredientID, req.amount);
            }
            return true;
        }

        private void SyncToSave()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.inventory.Clear();
                foreach (var pair in inventory)
                {
                    SaveManager.Instance.CurrentSave.inventory.Add(new InventoryItemEntry
                    {
                        itemID = pair.Key,
                        amount = pair.Value
                    });
                }
            }
        }
    }
}
