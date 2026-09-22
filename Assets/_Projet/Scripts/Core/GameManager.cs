using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Economy;
using VillaDelChef.Managers;
using VillaDelChef.Progression;
using VillaDelChef.Save;

namespace VillaDelChef.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("System References")]
        public GridManager gridManager;
        public BuildManager buildManager;
        public EconomyManager economyManager;
        public ProgressionManager progressionManager;
        public CustomerManager customerManager;
        public WorkerManager workerManager;
        public FarmingManager farmingManager;

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
            ProcessOfflineProgression();
        }

        private void ProcessOfflineProgression()
        {
            if (SaveManager.Instance == null) return;

            long offlineSeconds = SaveManager.Instance.OfflineSecondsElapsed;
            if (offlineSeconds > 30)
            {
                long minutes = offlineSeconds / 60;
                Debug.Log($"[GameManager] Welcome back! You were away for {minutes} minutes ({offlineSeconds} seconds). Your crops continued growing!");
            }
        }

        public void SetTargetFrameRate(int fps)
        {
            Application.targetFrameRate = fps;
            if (SaveManager.Instance != null && SaveManager.Instance.CurrentSave != null)
            {
                SaveManager.Instance.CurrentSave.targetFrameRate = fps;
            }
        }
    }
}
