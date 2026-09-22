using System;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Core;
using VillaDelChef.Interaction;
using VillaDelChef.Inventory;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.Managers;

namespace VillaDelChef.Farming
{
    public enum PlotState
    {
        Empty,
        Growing,
        ReadyToHarvest
    }

    public class CropPlot : GridObject, IInteractable
    {
        public string InteractionPrompt => currentState == PlotState.ReadyToHarvest ? "Cosechar" : (currentState == PlotState.Empty ? "Sembrar" : "Cultivando");
        public bool CanInteract => true;
        public void Interact() => OnInteract();

        [Header("Plot Identification")]
        public int plotIndex = 0;

        [Header("Crop State")]
        public PlotState currentState = PlotState.Empty;
        public CropSO plantedCrop;
        public long plantTimestampSeconds = 0;

        [Header("Visual Components")]
        public SpriteRenderer plantRenderer;
        public SpriteRenderer plotBaseRenderer;
        public GameObject readyVFX;

        private void Start()
        {
            UpdateVisuals();
            if (FarmingManager.Instance != null)
            {
                FarmingManager.Instance.RegisterPlot(this);
            }
        }

        public void TickGrowth(long currentNow)
        {
            if (currentState == PlotState.Growing && plantedCrop != null)
            {
                long elapsed = currentNow - plantTimestampSeconds;
                if (elapsed >= plantedCrop.totalGrowthTimeSeconds)
                {
                    currentState = PlotState.ReadyToHarvest;
                    UpdateVisuals();
                }
                else
                {
                    UpdateVisuals();
                }
            }
        }


        public void OnInteract()
        {
            switch (currentState)
            {
                case PlotState.Empty:
                    // Plant default or selected crop
                    if (FarmingManager.Instance != null)
                    {
                        CropSO cropToPlant = FarmingManager.Instance.GetSelectedOrAvailableCrop();
                        if (cropToPlant != null)
                        {
                            Plant(cropToPlant);
                        }
                    }
                    break;

                case PlotState.Growing:
                    long remaining = (long)plantedCrop.totalGrowthTimeSeconds - (DateTimeOffset.UtcNow.ToUnixTimeSeconds() - plantTimestampSeconds);
                    Debug.Log($"[CropPlot] {plantedCrop.cropName} growing... ({Math.Max(0, remaining)}s remaining)");
                    break;

                case PlotState.ReadyToHarvest:
                    Harvest();
                    break;
            }
        }

        public bool Plant(CropSO crop)
        {
            if (currentState != PlotState.Empty || crop == null) return false;

            plantedCrop = crop;
            plantTimestampSeconds = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            currentState = PlotState.Growing;

            UpdateVisuals();
            GameEvents.TriggerCropPlanted(this, crop);
            return true;
        }

        public void RestoreState(CropSO crop, long timestamp)
        {
            plantedCrop = crop;
            plantTimestampSeconds = timestamp;
            if (crop != null)
            {
                long elapsed = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - timestamp;
                currentState = (elapsed >= crop.totalGrowthTimeSeconds) ? PlotState.ReadyToHarvest : PlotState.Growing;
            }
            else
            {
                currentState = PlotState.Empty;
            }
            UpdateVisuals();
        }

        public void Harvest()
        {
            if (currentState != PlotState.ReadyToHarvest || plantedCrop == null) return;

            // Add harvested ingredients to inventory
            if (InventoryManager.Instance != null && plantedCrop.harvestIngredient != null)
            {
                InventoryManager.Instance.AddItem(plantedCrop.harvestIngredient.ingredientID, plantedCrop.harvestAmount);
            }

            GameEvents.TriggerCropHarvested(this, plantedCrop, plantedCrop.harvestAmount);
            GameEvents.TriggerQuestProgressMade(QuestType.HarvestCrops, plantedCrop.cropID, plantedCrop.harvestAmount);

            VillaDelChef.UI.FloatingTextManager.Instance?.ShowSuccess($"+{plantedCrop.harvestAmount} {plantedCrop.cropName}", transform.position + Vector3.up * 0.5f);

            plantedCrop = null;
            plantTimestampSeconds = 0;
            currentState = PlotState.Empty;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (plantRenderer == null) return;

            if (currentState == PlotState.Empty || plantedCrop == null)
            {
                plantRenderer.sprite = null;
                if (readyVFX != null) readyVFX.SetActive(false);
                return;
            }

            long elapsed = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - plantTimestampSeconds;
            float progress = Mathf.Clamp01((float)elapsed / plantedCrop.totalGrowthTimeSeconds);

            if (progress >= 1f)
            {
                plantRenderer.sprite = plantedCrop.readyStageSprite;
                if (readyVFX != null) readyVFX.SetActive(true);
            }
            else if (progress >= 0.75f)
            {
                plantRenderer.sprite = plantedCrop.stage03Sprite != null ? plantedCrop.stage03Sprite : plantedCrop.readyStageSprite;
                if (readyVFX != null) readyVFX.SetActive(false);
            }
            else if (progress >= 0.5f)
            {
                plantRenderer.sprite = plantedCrop.stage02Sprite != null ? plantedCrop.stage02Sprite : plantedCrop.stage03Sprite;
                if (readyVFX != null) readyVFX.SetActive(false);
            }
            else if (progress >= 0.25f)
            {
                plantRenderer.sprite = plantedCrop.stage01Sprite != null ? plantedCrop.stage01Sprite : plantedCrop.seedStageSprite;
                if (readyVFX != null) readyVFX.SetActive(false);
            }
            else
            {
                plantRenderer.sprite = plantedCrop.seedStageSprite;
                if (readyVFX != null) readyVFX.SetActive(false);
            }
        }
    }
}
