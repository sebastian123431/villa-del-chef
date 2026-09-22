using System;
using System.Collections.Generic;
using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    [Serializable]
    public class VendorItemEntry
    {
        public string itemID;
        public string displayName;
        public Sprite icon;
        public int buyPrice = 15;
        public int maxStock = 10;
        public int requiredPlayerLevel = 1;
        [Tooltip("Si es verdadero, añade este ítem directamente al inventario como ingrediente/producto.")]
        public bool isInventoryItem = true;
    }

    [CreateAssetMenu(fileName = "NewVendor", menuName = "VillaDelChef/Vendor")]
    public class VendorSO : ScriptableObject
    {
        [Header("Vendor Identity")]
        public string vendorID;
        public string vendorTitle;

        [Header("Restock Configuration")]
        [Tooltip("Tiempo en segundos para reponer automáticamente el stock completo (ej. 180s = 3 min).")]
        public int restockIntervalSeconds = 180;

        [Header("Items Catalog")]
        public List<VendorItemEntry> catalog = new List<VendorItemEntry>();
    }
}
