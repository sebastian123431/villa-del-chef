using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    public enum FurnitureCategory
    {
        Mesa,
        Silla,
        Cocina,
        MesaEntrega,
        Sembradero,
        Decoracion,
        Pared,
        Piso
    }

    [CreateAssetMenu(fileName = "NewFurniture", menuName = "VillaDelChef/Furniture")]
    public class FurnitureSO : ScriptableObject
    {
        [Header("Identity")]
        public string furnitureID;
        public string furnitureName;
        public FurnitureCategory category;
        public Sprite shopIcon;
        public GameObject prefab;

        [Header("Grid Size")]
        [Tooltip("Width in 1x1 grid cells")]
        public int sizeX = 1;
        [Tooltip("Height in 1x1 grid cells")]
        public int sizeY = 1;

        [Header("Economy & Unlocks")]
        public int cost = 100;
        public int sellPrice = 50;
        public int unlockLevel = 1;
        public int comfortRating = 5;

        [TextArea(2, 3)]
        public string description;
    }
}
