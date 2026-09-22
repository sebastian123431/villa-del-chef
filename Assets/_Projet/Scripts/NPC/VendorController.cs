using System;
using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.Inventory;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.NPC
{
    public class VendorController : MonoBehaviour
    {
        [Header("Vendor Definition")]
        public VendorSO vendorData;

        [Header("Runtime State")]
        public long nextRestockTimestampSeconds = 0;

        private Dictionary<string, int> stockMap = new Dictionary<string, int>();
        private bool isInitialized = false;

        private void Start()
        {
            InitializeStockIfNeeded();
        }

        public void InitializeStockIfNeeded()
        {
            if (isInitialized || vendorData == null) return;

            // 1. Try loading from save data
            LoadFromSave();

            if (!isInitialized)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                if (nextRestockTimestampSeconds <= 0)
                {
                    nextRestockTimestampSeconds = now + vendorData.restockIntervalSeconds;
                }

                foreach (var item in vendorData.catalog)
                {
                    if (item != null && !stockMap.ContainsKey(item.itemID))
                    {
                        stockMap[item.itemID] = item.maxStock;
                    }
                }

                isInitialized = true;
                SaveToCurrentSave();
            }
        }


        public void CheckRestock()
        {
            if (vendorData == null) return;
            InitializeStockIfNeeded();

            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            if (now >= nextRestockTimestampSeconds)
            {
                // Full restock
                foreach (var item in vendorData.catalog)
                {
                    if (item != null)
                    {
                        stockMap[item.itemID] = item.maxStock;
                    }
                }
                nextRestockTimestampSeconds = now + vendorData.restockIntervalSeconds;
                Debug.Log($"[VendorController] Puesto {vendorData.vendorID} ha sido reabastecido por completo.");
            }
        }

        public int GetStock(string itemID)
        {
            InitializeStockIfNeeded();
            CheckRestock();

            if (stockMap.TryGetValue(itemID, out int current))
            {
                return current;
            }
            return 0;
        }

        public long GetRemainingRestockSeconds()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return Math.Max(0, nextRestockTimestampSeconds - now);
        }

        public bool TryPurchaseItem(VendorItemEntry item, int amount = 1)
        {
            if (item == null || amount <= 0) return false;
            InitializeStockIfNeeded();
            CheckRestock();

            int currentStock = GetStock(item.itemID);
            if (currentStock < amount)
            {
                Debug.LogWarning($"[VendorController] Stock insuficiente para {item.displayName}.");
                return false;
            }

            int totalCost = item.buyPrice * amount;
            if (EconomyManager.Instance != null && !EconomyManager.Instance.HasCoins(totalCost))
            {
                Debug.LogWarning($"[VendorController] Monedas insuficientes para comprar {item.displayName}.");
                return false;
            }

            // Deduct economy & stock
            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.SpendCoins(totalCost);
            }

            stockMap[item.itemID] = currentStock - amount;

            // Add to inventory
            if (item.isInventoryItem && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem(item.itemID, amount);
            }

            SaveToCurrentSave();
            GameEvents.TriggerQuestProgressMade(QuestType.CollectIngredients, item.itemID, amount);
            Debug.Log($"[VendorController] Comprado {amount}x {item.displayName} por {totalCost} monedas. Stock restante: {stockMap[item.itemID]}");
            return true;
        }

        public void LoadFromSave()
        {
            if (vendorData == null || Save.SaveManager.Instance == null || Save.SaveManager.Instance.CurrentSave == null) return;

            var list = Save.SaveManager.Instance.CurrentSave.vendors;
            if (list == null) return;

            var entry = list.Find(v => v.vendorID == vendorData.vendorID);
            if (entry != null && entry.stock != null && entry.stock.Count > 0)
            {
                stockMap.Clear();
                foreach (var s in entry.stock)
                {
                    stockMap[s.itemID] = s.currentStock;
                }
                nextRestockTimestampSeconds = entry.nextRestockTimestampSeconds;
                isInitialized = true;
                CheckRestock();
            }
        }

        public void SaveToCurrentSave()
        {
            if (vendorData == null || Save.SaveManager.Instance == null || Save.SaveManager.Instance.CurrentSave == null) return;

            var list = Save.SaveManager.Instance.CurrentSave.vendors;
            if (list == null) return;

            var entry = list.Find(v => v.vendorID == vendorData.vendorID);
            if (entry == null)
            {
                entry = new Save.VendorSaveData { vendorID = vendorData.vendorID };
                list.Add(entry);
            }

            entry.nextRestockTimestampSeconds = nextRestockTimestampSeconds;
            entry.stock.Clear();
            foreach (var kvp in stockMap)
            {
                entry.stock.Add(new Save.VendorStockSaveEntry { itemID = kvp.Key, currentStock = kvp.Value });
            }
        }

        public Dictionary<string, int> ExportStock()
        {
            InitializeStockIfNeeded();
            return new Dictionary<string, int>(stockMap);
        }

        public void ImportStock(Dictionary<string, int> savedStock, long savedRestockTime)
        {
            if (savedStock != null)
            {
                stockMap = new Dictionary<string, int>(savedStock);
            }
            nextRestockTimestampSeconds = savedRestockTime;
            isInitialized = true;
            CheckRestock();
        }
    }
}

