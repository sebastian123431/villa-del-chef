using UnityEngine;

namespace VillaDelChef.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewCrop", menuName = "VillaDelChef/Crop")]
    public class CropSO : ScriptableObject
    {
        [Header("General Info")]
        public string cropID;
        public string cropName;
        public Sprite seedIcon;
        public IngredientSO harvestIngredient;
        public int harvestAmount = 3;
        public int seedCost = 10;
        public int unlockLevel = 1;

        [Header("Growth Timers")]
        [Tooltip("Total growth time in seconds")]
        public float totalGrowthTimeSeconds = 120f;

        [Header("Visual Stages (Sprites)")]
        public Sprite seedStageSprite;
        public Sprite stage01Sprite;
        public Sprite stage02Sprite;
        public Sprite stage03Sprite;
        public Sprite readyStageSprite;
        public Sprite harvestedStageSprite;
    }
}
