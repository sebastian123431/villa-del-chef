using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Farming;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Managers
{
    public class FarmingManager : MonoBehaviour
    {
        public static FarmingManager Instance { get; private set; }

        [Header("Crops Database")]
        public List<CropSO> allCrops = new List<CropSO>();
        public CropSO selectedCrop;

        [Header("Active Plots in World")]
        public List<CropPlot> activePlots = new List<CropPlot>();

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
            if (allCrops == null || allCrops.Count == 0)
            {
                allCrops = new List<CropSO>(Resources.LoadAll<CropSO>("Crops"));
            }

            if (allCrops.Count > 0 && selectedCrop == null)
            {
                selectedCrop = allCrops[0];
            }

            LoadPlotStates();
            GameEvents.OnCropPlanted += (plot, crop) => SavePlotStates();
            GameEvents.OnCropHarvested += (plot, crop, amount) => SavePlotStates();

            StartCoroutine(CentralizedFarmingTickRoutine());
        }

        private System.Collections.IEnumerator CentralizedFarmingTickRoutine()
        {
            var wait = new WaitForSeconds(1f);
            while (true)
            {
                yield return wait;
                long now = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                for (int i = 0; i < activePlots.Count; i++)
                {
                    if (activePlots[i] != null && activePlots[i].currentState == PlotState.Growing)
                    {
                        activePlots[i].TickGrowth(now);
                    }
                }
            }
        }

        public CropSO GetSelectedOrAvailableCrop()
        {
            if (selectedCrop != null) return selectedCrop;
            return (allCrops.Count > 0) ? allCrops[0] : null;
        }

        public void SelectCrop(CropSO crop)
        {
            selectedCrop = crop;
        }

        public void RegisterPlot(CropPlot plot)
        {
            if (!activePlots.Contains(plot))
            {
                plot.plotIndex = activePlots.Count;
                activePlots.Add(plot);
                RestorePlotFromSave(plot);
            }
        }

        public void UnregisterPlot(CropPlot plot)
        {
            activePlots.Remove(plot);
        }

        private void SavePlotStates()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSave == null) return;

            SaveManager.Instance.CurrentSave.cropPlots.Clear();
            for (int i = 0; i < activePlots.Count; i++)
            {
                CropPlot plot = activePlots[i];
                if (plot != null)
                {
                    SaveManager.Instance.CurrentSave.cropPlots.Add(new CropPlotData
                    {
                        plotIndex = i,
                        cropID = plot.plantedCrop != null ? plot.plantedCrop.cropID : "",
                        plantTimestampSeconds = plot.plantTimestampSeconds,
                        isPlanted = plot.currentState != PlotState.Empty
                    });
                }
            }
        }

        private void LoadPlotStates()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.CurrentSave == null) return;
            var savedPlots = SaveManager.Instance.CurrentSave.cropPlots;
            if (savedPlots == null || savedPlots.Count == 0) return;

            for (int i = 0; i < activePlots.Count; i++)
            {
                RestorePlotFromSave(activePlots[i]);
            }
        }

        private void RestorePlotFromSave(CropPlot plot)
        {
            if (plot == null || SaveManager.Instance == null || SaveManager.Instance.CurrentSave == null) return;
            var savedPlots = SaveManager.Instance.CurrentSave.cropPlots;
            if (savedPlots == null || savedPlots.Count == 0) return;

            var data = savedPlots.Find(p => p != null && p.plotIndex == plot.plotIndex);
            if (data != null && data.isPlanted && !string.IsNullOrEmpty(data.cropID))
            {
                CropSO crop = allCrops.Find(c => c != null && c.cropID == data.cropID);
                if (crop != null)
                {
                    plot.RestoreState(crop, data.plantTimestampSeconds);
                }
            }
        }
    }
}
