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
using VillaDelChef.UI;

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

    public class CustomerController : MonoBehaviour, IPoolable
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

        [Header("Social Identity (Friends Integration)")]
        public CharacterSO characterAppearance;
        public Characters.CharacterAppearanceController appearanceController;
        private Animator animator;

        private List<Vector2Int> currentPath;
        private int currentPathIndex = 0;
        private float moveSpeed = 2.5f;

        public void OnSpawnFromPool()
        {
            currentState = CustomerState.Entering;
            assignedTable = null;
            assignedChair = null;
            orderedDish = null;
            currentPath = null;
            if (characterRenderer != null) characterRenderer.color = Color.white;
            if (orderBubble != null) orderBubble.SetActive(false);
            if (patienceBar != null) patienceBar.SetActive(false);
            if (animator != null && animator.enabled)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("IsThinking", false);
            }
        }

        public void OnReturnToPool()
        {
            StopAllCoroutines();
            ReleaseTableReference();
            orderedDish = null;
            currentPath = null;
            if (orderBubble != null) orderBubble.SetActive(false);
            if (patienceBar != null) patienceBar.SetActive(false);
            if (animator != null && animator.enabled)
            {
                animator.SetFloat("Speed", 0f);
                animator.SetBool("IsThinking", false);
            }
            characterAppearance = null;
        }

        public void ReleaseTableReference()
        {
            if (assignedChair != null)
            {
                assignedChair.SetOccupied(false);
                assignedChair = null;
            }
            if (assignedTable != null)
            {
                if (assignedTable.currentCustomer == this)
                {
                    assignedTable.currentCustomer = null;
                }
                assignedTable = null;
            }
        }

        public void Setup(CustomerSO behaviorData, CharacterSO friendAppearance, Vector2Int spawnGrid, Vector2Int exitGrid)
        {
            customerData = behaviorData;
            characterAppearance = friendAppearance;
            exitGridPos = exitGrid;

            if (characterRenderer == null) characterRenderer = GetComponentInChildren<SpriteRenderer>();
            if (animator == null) animator = GetComponent<Animator>();

            // Configuración visual: Si existe Friend real, usar vestuario Normal (rnormal + movimientos_rnormal)
            if (characterAppearance != null)
            {
                if (appearanceController == null)
                {
                    appearanceController = GetComponent<Characters.CharacterAppearanceController>() 
                        ?? gameObject.AddComponent<Characters.CharacterAppearanceController>();
                }
                appearanceController.ApplyCharacter(characterAppearance, ScriptableObjects.CharacterOutfit.Normal, allowCrossOutfitFallback: false);
                if (animator == null) animator = GetComponent<Animator>();
            }
            else
            {
                // Fallback técnico procedural legacy
                if (animator != null) animator.enabled = false;
                if (customerData != null && customerData.characterSprite != null && characterRenderer != null)
                {
                    characterRenderer.sprite = customerData.characterSprite;
                }
            }

            moveSpeed = customerData != null ? customerData.movementSpeed : 2.5f;
            maxPatience = customerData != null ? customerData.basePatienceSeconds : 60f;
            currentPatience = maxPatience;
            eatingDuration = 6f;

            transform.position = GridManager.Instance != null ? GridManager.Instance.GridToWorld(spawnGrid) : (Vector3)(Vector2)spawnGrid;
            currentState = CustomerState.Entering;
            StartCoroutine(CustomerLifecycleRoutine());
        }

        public void Setup(CustomerSO data, Vector2Int spawnGrid, Vector2Int exitGrid)
        {
            Setup(data, null, spawnGrid, exitGrid);
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
                DespawnCustomer();
                yield break;
            }

            assignedTable = foundTable;
            assignedChair = foundTable.GetFirstAvailableChair();
            if (assignedChair != null) assignedChair.SetOccupied(true);
            assignedTable.ReserveForCustomer(this);

            // 2. Walk to Table
            currentState = CustomerState.WalkingToTable;
            Vector2Int chairGrid = assignedChair != null ? assignedChair.gridPosition : assignedTable.gridPosition;
            yield return StartCoroutine(WalkToRoutine(chairGrid));

            // Snap to chair position
            if (assignedChair != null)
            {
                transform.position = assignedChair.GetSitPosition();
            }
            assignedTable.CustomerSeated(this);

            // 3. Deciding Order
            currentState = CustomerState.DecidingOrder;
            yield return new WaitForSeconds(2.5f);

            orderedDish = SelectDishFromMenu();
            if (orderedDish == null)
            {
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
            if (animator != null && animator.enabled) animator.SetBool("IsThinking", true);
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
                    if (animator != null && animator.enabled) animator.SetBool("IsThinking", false);
                    int repPenalty = (customerData != null) ? customerData.reputationPenalty : 2;
                    if (EconomyManager.Instance != null)
                    {
                        EconomyManager.Instance.ModifyReputation(-repPenalty);
                    }
                    FloatingTextManager.Instance?.ShowReputation(-repPenalty, transform.position + Vector3.up * 0.4f);
                    FloatingTextManager.Instance?.ShowWarning("¡Paciencia agotada!", transform.position + Vector3.up * 0.7f);

                    GameEvents.TriggerCustomerServed(this, false);
                    yield return StartCoroutine(LeaveRestaurantRoutine());
                    yield break;
                }
                yield return null;
            }

            // 5. Eating
            currentState = CustomerState.Eating;
            if (animator != null && animator.enabled) animator.SetBool("IsThinking", false);
            if (orderBubble != null) orderBubble.SetActive(false);
            yield return new WaitForSeconds(eatingDuration);

            // 6. Paying
            currentState = CustomerState.Paying;
            int payAmount = orderedDish != null ? orderedDish.sellPrice : 15;
            int tip = 0;

            float satisfactionRatio = currentPatience / maxPatience;
            if (satisfactionRatio > 0.6f && Random.value < (customerData != null ? customerData.tipProbability : 0.5f))
            {
                tip = Mathf.RoundToInt(payAmount * 0.25f * (customerData != null ? customerData.tipMultiplier : 1f));
            }

            if (satisfactionRatio > 0.6f && animator != null && animator.enabled)
            {
                animator.SetTrigger("Celebrate");
            }

            int totalGold = payAmount + tip;
            int repReward = (customerData != null) ? customerData.reputationReward : 1;

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddCoins(totalGold);
                if (orderedDish != null)
                {
                    EconomyManager.Instance.AddExperience(orderedDish.experienceReward);
                }

                EconomyManager.Instance.ModifyReputation(repReward);

                if (customerData != null && customerData.bonusXP > 0)
                {
                    EconomyManager.Instance.AddExperience(customerData.bonusXP);
                }
            }

            FloatingTextManager.Instance?.ShowGold(totalGold, transform.position + Vector3.up * 0.4f);
            FloatingTextManager.Instance?.ShowReputation(repReward, transform.position + Vector3.up * 0.7f);

            GameEvents.TriggerCustomerServed(this, true);
            GameEvents.TriggerQuestProgressMade(QuestType.ServeCustomers, "", 1);

            // 7. Leaving
            yield return StartCoroutine(LeaveRestaurantRoutine());
        }

        private RecipeSO SelectDishFromMenu()
        {
            if (customerData != null && customerData.preferredFoods != null && customerData.preferredFoods.Count > 0)
            {
                return customerData.preferredFoods[Random.Range(0, customerData.preferredFoods.Count)];
            }
            return RecipeManager.Instance != null ? RecipeManager.Instance.GetRandomUnlockedRecipe() : null;
        }

        public bool ReceiveDish(DishInstance dish)
        {
            if (currentState != CustomerState.WaitingForFood)
            {
                Debug.LogWarning($"[CustomerController] Cliente no está esperando comida (Estado actual: {currentState}).");
                return false;
            }

            if (dish == null || dish.recipeData == null || orderedDish == null)
            {
                Debug.LogWarning("[CustomerController] Intento de entrega con plato o receta nula.");
                return false;
            }

            if (dish.recipeData.recipeID != orderedDish.recipeID)
            {
                Debug.LogWarning($"[CustomerController] Pedido no coincide. Esperado: {orderedDish.recipeID}, Recibido: {dish.recipeData.recipeID}");
                return false;
            }

            if (assignedTable != null)
            {
                assignedTable.PlaceDish(dish);
            }
            currentState = CustomerState.Eating;
            return true;
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
                ReleaseTableReference();
            }
            yield return StartCoroutine(WalkToRoutine(exitGridPos));
            GameEvents.TriggerCustomerLeft(this);
            DespawnCustomer();
        }

        public void DespawnCustomer()
        {
            StopAllCoroutines();
            if (ObjectPoolManager.Instance != null)
            {
                ObjectPoolManager.Instance.Despawn("Customers", gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
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
                    Vector3 moveDir = (targetWorld - transform.position).normalized;
                    transform.position = Vector3.MoveTowards(transform.position, targetWorld, moveSpeed * Time.deltaTime);

                    if (animator != null && animator.enabled)
                    {
                        animator.SetFloat("MoveX", moveDir.x);
                        animator.SetFloat("MoveY", moveDir.y);
                        animator.SetFloat("Speed", 1f);
                    }
                    else if (characterRenderer != null && moveDir.x != 0)
                    {
                        characterRenderer.flipX = moveDir.x < 0;
                    }
                    yield return null;
                }
                currentPathIndex++;
            }

            if (animator != null && animator.enabled)
            {
                animator.SetFloat("Speed", 0f);
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
