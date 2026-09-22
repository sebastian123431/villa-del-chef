using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.Workers;

namespace VillaDelChef.Managers
{
    public class WorkerManager : MonoBehaviour
    {
        public static WorkerManager Instance { get; private set; }

        [Header("Worker Roster")]
        public List<WorkerSO> availableWorkerTypes = new List<WorkerSO>();
        public List<WorkerController> activeWorkers = new List<WorkerController>();
        public GameObject workerPrefab;

        [Header("Default Helper Position")]
        public Vector2Int initialWorkerGrid = new Vector2Int(0, 5);

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

        public void SpawnWorker(WorkerSO workerData, Vector2Int gridPos)
        {
            GameObject wObj;
            if (workerPrefab != null)
            {
                wObj = Instantiate(workerPrefab);
            }
            else
            {
                wObj = new GameObject("Worker_" + workerData.workerName);
                SpriteRenderer sr = wObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 5;
            }

            WorkerController controller = wObj.GetComponent<WorkerController>();
            if (controller == null)
            {
                controller = wObj.AddComponent<WorkerController>();
            }

            controller.workerData = workerData;
            controller.idleGridPos = gridPos;
            if (GridManager.Instance != null)
            {
                wObj.transform.position = GridManager.Instance.GridToWorld(gridPos);
            }

            activeWorkers.Add(controller);
        }
    }
}
