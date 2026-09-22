using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.Core;
using VillaDelChef.Customers;
using VillaDelChef.Restaurant;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.Utilities;

namespace VillaDelChef.Workers
{
    public enum WorkerState
    {
        Idle,
        WalkingToCounter,
        PickingDish,
        DeliveringToTable,
        SearchingDirtyTable,
        WalkingToDirtyTable,
        Cleaning,
        ReturningToIdle
    }

    public class WorkerController : MonoBehaviour
    {
        [Header("Worker Profile")]
        public WorkerSO workerData;
        public WorkerState currentState = WorkerState.Idle;

        [Header("Positions")]
        public Vector2Int idleGridPos = new Vector2Int(0, 0);
        public Transform carrySocket;

        [Header("Carrying State")]
        public DishInstance carryingDish;

        [Header("Cleaning Settings")]
        public float cleanDuration = 1.8f;

        [Header("Visuals")]
        public SpriteRenderer characterRenderer;

        private float moveSpeed = 3.2f;
        private Coroutine activeTaskRoutine;

        private void Start()
        {
            if (workerData != null)
            {
                moveSpeed = workerData.movementSpeed;
                if (characterRenderer != null && workerData.characterSprite != null)
                {
                    characterRenderer.sprite = workerData.characterSprite;
                }
            }

            idleGridPos = GridManager.Instance != null ? GridManager.Instance.WorldToGrid(transform.position) : Vector2Int.zero;
            StartCoroutine(WorkerThinkRoutine());
        }

        private IEnumerator WorkerThinkRoutine()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.4f);

                if (currentState == WorkerState.Idle && carryingDish == null)
                {
                    CheckForWork();
                }
            }
        }

        private void CheckForWork()
        {
            // PRIORITY 1: Find a ready dish that matches a customer's waiting order
            if (DeliveryCounter.Instance != null && DeliveryCounter.Instance.readyDishes.Count > 0)
            {
                if (FindMatchingOrderAndDish(out Table waitingTable, out DishInstance matchingDish))
                {
                    if (activeTaskRoutine != null) StopCoroutine(activeTaskRoutine);
                    activeTaskRoutine = StartCoroutine(DeliveryTaskRoutine(waitingTable, matchingDish));
                    return;
                }
            }

            // PRIORITY 2: Find a dirty table that needs cleaning
            Table dirtyTable = FindDirtyTable();
            if (dirtyTable != null)
            {
                if (activeTaskRoutine != null) StopCoroutine(activeTaskRoutine);
                activeTaskRoutine = StartCoroutine(CleaningTaskRoutine(dirtyTable));
                return;
            }
        }

        private bool FindMatchingOrderAndDish(out Table matchingTable, out DishInstance matchingDish)
        {
            matchingTable = null;
            matchingDish = null;

            if (BuildManager.Instance == null || DeliveryCounter.Instance == null) return false;

            // Search all tables waiting for food
            foreach (var obj in BuildManager.Instance.activeFurniture)
            {
                Table table = obj as Table;
                if (table != null && table.currentOrder != null && table.servedDish == null && (table.tableState == TableState.WaitingFood || table.tableState == TableState.Occupied))
                {
                    DishInstance readyDish = DeliveryCounter.Instance.FindMatchingDish(table.currentOrder);
                    if (readyDish != null)
                    {
                        matchingTable = table;
                        matchingDish = readyDish;
                        return true;
                    }
                }
            }

            return false;
        }

        private Table FindDirtyTable()
        {
            if (BuildManager.Instance == null) return null;

            foreach (var obj in BuildManager.Instance.activeFurniture)
            {
                Table table = obj as Table;
                if (table != null && (table.tableState == TableState.Dirty || table.needsCleaning))
                {
                    return table;
                }
            }

            return null;
        }

        private IEnumerator DeliveryTaskRoutine(Table targetTable, DishInstance targetDish)
        {
            // 1. Walk to Delivery Counter
            currentState = WorkerState.WalkingToCounter;
            bool reachedCounter = false;
            yield return StartCoroutine(WalkToRoutine(DeliveryCounter.Instance.gridPosition, success => reachedCounter = success));

            if (!reachedCounter)
            {
                currentState = WorkerState.Idle;
                yield break;
            }

            // 2. Pick up specific matching dish
            currentState = WorkerState.PickingDish;
            carryingDish = DeliveryCounter.Instance.TakeSpecificDish(targetDish);
            if (carryingDish == null)
            {
                // Fallback if another worker picked it up in between
                carryingDish = DeliveryCounter.Instance.TakeNextDish();
                if (carryingDish == null)
                {
                    currentState = WorkerState.Idle;
                    yield break;
                }
            }

            carryingDish.transform.SetParent(carrySocket != null ? carrySocket : transform);
            carryingDish.transform.localPosition = carrySocket != null ? Vector3.zero : new Vector3(0f, 0.4f, 0f);

            // 3. Deliver to Table
            currentState = WorkerState.DeliveringToTable;
            bool reachedTable = false;
            yield return StartCoroutine(WalkToRoutine(targetTable.gridPosition, success => reachedTable = success));

            if (!reachedTable)
            {
                // Could not reach table - return dish to counter if space allows
                Debug.LogWarning($"[WorkerController] No pudo alcanzar la mesa en {targetTable.gridPosition}. Devolviendo plato.");
                if (DeliveryCounter.Instance != null && DeliveryCounter.Instance.HasSpace())
                {
                    DeliveryCounter.Instance.AddDish(carryingDish);
                    carryingDish = null;
                }
                currentState = WorkerState.Idle;
                yield break;
            }

            // Deliver dish directly to table and notify customer
            targetTable.PlaceDish(carryingDish);
            if (targetTable.currentCustomer != null)
            {
                targetTable.currentCustomer.ReceiveDish(carryingDish);
            }

            GameEvents.TriggerDishDelivered(carryingDish, targetTable);
            carryingDish = null;

            // 4. Return to Idle spot
            currentState = WorkerState.ReturningToIdle;
            yield return StartCoroutine(WalkToRoutine(idleGridPos, null));
            currentState = WorkerState.Idle;
        }

        private IEnumerator CleaningTaskRoutine(Table targetTable)
        {
            currentState = WorkerState.WalkingToDirtyTable;
            targetTable.StartCleaning();

            bool reached = false;
            yield return StartCoroutine(WalkToRoutine(targetTable.gridPosition, success => reached = success));

            if (!reached)
            {
                // Abort cleaning if table unreachable
                targetTable.tableState = TableState.Dirty;
                currentState = WorkerState.Idle;
                yield break;
            }

            // Cleaning in progress
            currentState = WorkerState.Cleaning;
            yield return new WaitForSeconds(cleanDuration);

            targetTable.FinishCleaning();

            // Return to Idle spot
            currentState = WorkerState.ReturningToIdle;
            yield return StartCoroutine(WalkToRoutine(idleGridPos, null));
            currentState = WorkerState.Idle;
        }

        private IEnumerator WalkToRoutine(Vector2Int targetGrid, System.Action<bool> onComplete)
        {
            Vector2Int startGrid = GridManager.Instance != null ? GridManager.Instance.WorldToGrid(transform.position) : Vector2Int.zero;
            List<Vector2Int> path = GridPathfinding.FindPath(startGrid, targetGrid);

            if (path == null || path.Count == 0)
            {
                Debug.LogWarning($"[WorkerController] No se encontró ruta transitable desde {startGrid} hasta {targetGrid}. Tarea cancelada sin teletransporte.");
                onComplete?.Invoke(false);
                yield break;
            }

            for (int i = 0; i < path.Count; i++)
            {
                Vector3 targetWorld = GridManager.Instance.GridToWorld(path[i]);
                while (Vector3.Distance(transform.position, targetWorld) > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);
                    if (characterRenderer != null && (targetWorld.x - transform.position.x) != 0)
                    {
                        characterRenderer.flipX = (targetWorld.x - transform.position.x) < 0;
                    }
                    yield return null;
                }
            }

            onComplete?.Invoke(true);
        }
    }
}
