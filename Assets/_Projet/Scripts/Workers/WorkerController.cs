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
                yield return new WaitForSeconds(0.5f);

                if (currentState == WorkerState.Idle && carryingDish == null)
                {
                    CheckForWork();
                }
            }
        }

        private void CheckForWork()
        {
            if (DeliveryCounter.Instance == null || DeliveryCounter.Instance.readyDishes.Count == 0)
            {
                return;
            }

            // Find a table that ordered this dish
            DishInstance dishCandidate = DeliveryCounter.Instance.readyDishes[0];
            Table targetTable = FindTableWaitingForDish(dishCandidate.recipeData);

            if (targetTable != null)
            {
                if (activeTaskRoutine != null) StopCoroutine(activeTaskRoutine);
                activeTaskRoutine = StartCoroutine(DeliveryTaskRoutine(targetTable));
            }
        }

        private Table FindTableWaitingForDish(RecipeSO recipe)
        {
            if (BuildManager.Instance == null || recipe == null) return null;

            foreach (var obj in BuildManager.Instance.activeFurniture)
            {
                Table table = obj as Table;
                if (table != null && table.currentOrder != null && table.currentOrder.recipeID == recipe.recipeID && table.servedDish == null)
                {
                    return table;
                }
            }
            return null;
        }

        private IEnumerator DeliveryTaskRoutine(Table targetTable)
        {
            // 1. Walk to Delivery Counter
            currentState = WorkerState.WalkingToCounter;
            yield return StartCoroutine(WalkToRoutine(DeliveryCounter.Instance.gridPosition));

            // 2. Pick up dish
            currentState = WorkerState.PickingDish;
            carryingDish = DeliveryCounter.Instance.TakeNextDish();
            if (carryingDish == null)
            {
                currentState = WorkerState.Idle;
                yield break;
            }

            carryingDish.transform.SetParent(carrySocket != null ? carrySocket : transform);
            carryingDish.transform.localPosition = carrySocket != null ? Vector3.zero : new Vector3(0f, 0.4f, 0f);

            // 3. Deliver to Table
            currentState = WorkerState.DeliveringToTable;
            yield return StartCoroutine(WalkToRoutine(targetTable.gridPosition));

            // Deliver dish to table
            targetTable.PlaceDish(carryingDish);
            CustomerController customer = targetTable.GetComponentInChildren<CustomerController>();
            if (customer == null)
            {
                // Also check if customer is nearby
                customer = FindAnyObjectByType<CustomerController>();
            }
            if (customer != null)
            {
                customer.ReceiveDish(carryingDish);
            }

            GameEvents.TriggerDishDelivered(carryingDish, targetTable);
            carryingDish = null;

            // 4. Return to Idle spot
            currentState = WorkerState.ReturningToIdle;
            yield return StartCoroutine(WalkToRoutine(idleGridPos));
            currentState = WorkerState.Idle;
        }

        private IEnumerator WalkToRoutine(Vector2Int targetGrid)
        {
            Vector2Int startGrid = GridManager.Instance != null ? GridManager.Instance.WorldToGrid(transform.position) : Vector2Int.zero;
            List<Vector2Int> path = GridPathfinding.FindPath(startGrid, targetGrid);

            if (path == null || path.Count == 0)
            {
                if (GridManager.Instance != null)
                {
                    transform.position = GridManager.Instance.GridToWorld(targetGrid);
                }
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
        }
    }
}
