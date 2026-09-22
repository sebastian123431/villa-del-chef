using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.Restaurant;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.Utilities;
using VillaDelChef.Managers;

namespace VillaDelChef.Customers
{
    public enum CustomerState
    {
        Entering,
        FindingTable,
        WalkingToTable,
        DecidingOrder,
        WaitingForFood,
        Eating,
        Paying,
        Leaving
    }

    public class CustomerController : MonoBehaviour
    {
        [Header("Data")]
        public CustomerSO customerData;
        public CustomerState currentState = CustomerState.Entering;

        [Header("Target Info")]
        public Table assignedTable;
        public Chair assignedChair;
        public RecipeSO orderedDish;
        public Vector2Int exitGridPos;

        [Header("Timers & Patience")]
        public float currentPatience = 60f;
        public float maxPatience = 60f;
        public float eatingDuration = 6f;

        [Header("Visual Components")]
        public SpriteRenderer characterRenderer;
        public GameObject orderBubble;
        public SpriteRenderer orderIconRenderer;
        public GameObject patienceBar;

        private List<Vector2Int> currentPath;
        private int currentPathIndex = 0;
        private float moveSpeed = 2.5f;

        public void Setup(CustomerSO data, Vector2Int spawnGrid, Vector2Int exitGrid)
        {
            this.customerData = data;
            this.exitGridPos = exitGrid;
            this.moveSpeed = data != null ? data.movementSpeed : 2.5f;
            this.maxPatience = data != null ? data.basePatienceSeconds : 60f;
            this.currentPatience = maxPatience;

            if (characterRenderer != null && data != null && data.characterSprite != null)
            {
                characterRenderer.sprite = data.characterSprite;
            }

            if (orderBubble != null) orderBubble.SetActive(false);

            transform.position = GridManager.Instance != null ? GridManager.Instance.GridToWorld(spawnGrid) : (Vector3)(Vector2)spawnGrid;
            currentState = CustomerState.Entering;
            StartCoroutine(CustomerLifecycleRoutine());
        }

        private IEnumerator CustomerLifecycleRoutine()
        {
            yield return new WaitForSeconds(0.2f);

            // 1. Find Table
            currentState = CustomerState.FindingTable;
            Table foundTable = FindAvailableTable();
            if (foundTable == null)
            {
                // No tables available -> Leave
                yield return StartCoroutine(WalkToRoutine(exitGridPos));
                GameEvents.TriggerCustomerLeft(this);
                Destroy(gameObject);
                yield break;
            }

            assignedTable = foundTable;
            assignedChair = foundTable.GetFirstAvailableChair();
            if (assignedChair != null) assignedChair.SetOccupied(true);
            assignedTable.ReserveForCustomer(this);

            // 2. Walk to Table
            currentState = CustomerState.WalkingToTable;
            yield return StartCoroutine(WalkToRoutine(assignedTable.gridPosition));

            // Snap to chair position
            if (assignedChair != null)
            {
                transform.position = assignedChair.GetSitPosition();
            }
            assignedTable.CustomerSeated(this);

            // 3. Deciding Order
            currentState = CustomerState.DecidingOrder;
            yield return new WaitForSeconds(2.5f);

            // Pick an order
            if (customerData != null && customerData.preferredFoods.Count > 0)
            {
                orderedDish = customerData.preferredFoods[Random.Range(0, customerData.preferredFoods.Count)];
            }
            else
            {
                // Fallback to table or manager recipe
                orderedDish = RecipeManager.Instance != null ? RecipeManager.Instance.GetRandomUnlockedRecipe() : null;
            }

            if (orderedDish == null)
            {
                yield return StartCoroutine(LeaveRestaurantRoutine());
                yield break;
            }

            assignedTable.SetWaitingOrder(orderedDish);


            // 4. Waiting for Food
            currentState = CustomerState.WaitingForFood;
            if (orderBubble != null) orderBubble.SetActive(true);
            if (orderIconRenderer != null && orderedDish != null)
            {
                orderIconRenderer.sprite = orderedDish.icon;
            }
            GameEvents.TriggerCustomerOrdered(this);

            // Patience countdown
            while (currentState == CustomerState.WaitingForFood)
            {
                currentPatience -= Time.deltaTime;
                if (currentPatience <= 0f)
                {
                    // Ran out of patience - anger penalty
                    if (orderBubble != null) orderBubble.SetActive(false);
                    int repPenalty = (customerData != null) ? customerData.reputationPenalty : 2;
                    if (EconomyManager.Instance != null)
                    {
                        EconomyManager.Instance.ModifyReputation(-repPenalty);
                    }
                    GameEvents.TriggerCustomerServed(this, false);
                    yield return StartCoroutine(LeaveRestaurantRoutine());
                    yield break;
                }
                yield return null;
            }

            // 5. Eating
            currentState = CustomerState.Eating;
            if (orderBubble != null) orderBubble.SetActive(false);
            yield return new WaitForSeconds(eatingDuration);

            // 6. Paying
            currentState = CustomerState.Paying;
            int payAmount = orderedDish != null ? orderedDish.sellPrice : 30;
            int tip = 0;

            float satisfactionRatio = currentPatience / maxPatience;
            if (satisfactionRatio > 0.6f && Random.value < (customerData != null ? customerData.tipProbability : 0.5f))
            {
                tip = Mathf.RoundToInt(payAmount * 0.25f * (customerData != null ? customerData.tipMultiplier : 1f));
            }

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddCoins(payAmount + tip);
                if (orderedDish != null)
                {
                    EconomyManager.Instance.AddExperience(orderedDish.experienceReward);
                }

                // Reputation reward and bonus XP
                int repReward = (customerData != null) ? customerData.reputationReward : 1;
                EconomyManager.Instance.ModifyReputation(repReward);

                if (customerData != null && customerData.bonusXP > 0)
                {
                    EconomyManager.Instance.AddExperience(customerData.bonusXP);
                }
            }

            GameEvents.TriggerCustomerServed(this, true);
            GameEvents.TriggerQuestProgressMade(QuestType.ServeCustomers, "", 1);

            // 7. Leaving
            yield return StartCoroutine(LeaveRestaurantRoutine());
        }

        public void ReceiveDish(DishInstance dish)
        {
            if (currentState == CustomerState.WaitingForFood)
            {
                if (assignedTable != null)
                {
                    assignedTable.PlaceDish(dish);
                }
                currentState = CustomerState.Eating;
            }
        }

        private IEnumerator LeaveRestaurantRoutine()
        {
            bool hadEaten = (currentState == CustomerState.Paying || currentState == CustomerState.Eating);
            currentState = CustomerState.Leaving;
            if (assignedTable != null)
            {
                if (hadEaten)
                {
                    assignedTable.MarkDirty();
                }
                else
                {
                    assignedTable.ClearTable();
                }
            }
            yield return StartCoroutine(WalkToRoutine(exitGridPos));
            GameEvents.TriggerCustomerLeft(this);
            Destroy(gameObject);
        }

        private IEnumerator WalkToRoutine(Vector2Int targetGrid)
        {
            Vector2Int startGrid = GridManager.Instance != null ? GridManager.Instance.WorldToGrid(transform.position) : Vector2Int.zero;
            currentPath = GridPathfinding.FindPath(startGrid, targetGrid);

            if (currentPath == null || currentPath.Count == 0)
            {
                Debug.LogWarning($"[CustomerController] No se encontró ruta caminable desde {startGrid} hasta {targetGrid}. Esperando sin teletransporte.");
                yield break;
            }


            currentPathIndex = 0;
            while (currentPathIndex < currentPath.Count)
            {
                Vector3 targetWorld = GridManager.Instance.GridToWorld(currentPath[currentPathIndex]);
                while (Vector3.Distance(transform.position, targetWorld) > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);

                    // Flip sprite horizontally depending on move direction
                    if (characterRenderer != null && (targetWorld.x - transform.position.x) != 0)
                    {
                        characterRenderer.flipX = (targetWorld.x - transform.position.x) < 0;
                    }
                    yield return null;
                }
                currentPathIndex++;
            }
        }

        private Table FindAvailableTable()
        {
            if (BuildManager.Instance == null) return null;
            foreach (var gridObj in BuildManager.Instance.activeFurniture)
            {
                Table table = gridObj as Table;
                if (table != null && table.HasAvailableChair())
                {
                    return table;
                }
            }
            return null;
        }
    }
}
