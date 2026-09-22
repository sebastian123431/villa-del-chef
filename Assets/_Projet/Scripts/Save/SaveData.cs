using System;
using System.Collections.Generic;

namespace VillaDelChef.Save
{
    [Serializable]
    public class InventoryItemEntry
    {
        public string itemID;
        public int amount;
    }

    [Serializable]
    public class PlacedFurnitureData
    {
        public string furnitureID;
        public int gridX;
        public int gridY;
        public int rotationDegrees;
    }

    [Serializable]
    public class CropPlotData
    {
        public int plotIndex;
        public string cropID;
        public long plantTimestampSeconds;
        public bool isPlanted;
    }

    [Serializable]
    public class QuestSaveData
    {
        public string questID;
        public int currentProgress;
        public bool isCompleted;
    }

    [Serializable]
    public class SaveData
    {
        // Player stats
        public int coins = 250;
        public int experience = 0;
        public int level = 1;
        public int reputation = 10;

        // Inventory
        public List<InventoryItemEntry> inventory = new List<InventoryItemEntry>();

        // Recipes unlocked
        public List<string> unlockedRecipes = new List<string>();

        // Placed Furniture
        public List<PlacedFurnitureData> placedFurniture = new List<PlacedFurnitureData>();

        // Crop plots
        public List<CropPlotData> cropPlots = new List<CropPlotData>();

        // Quests
        public List<QuestSaveData> quests = new List<QuestSaveData>();

        // Tutorial
        public bool tutorialCompleted = false;
        public int tutorialStep = 0;

        // Meta & Timestamps
        public long lastSaveTimestampSeconds;
        public int targetFrameRate = 60;
        public float musicVolume = 1f;
        public float sfxVolume = 1f;
    }
}
