using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Core;
using VillaDelChef.Customers;
using VillaDelChef.Economy;
using VillaDelChef.Restaurant;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Managers
{
    public class CustomerManager : MonoBehaviour
    {
        public static CustomerManager Instance { get; private set; }

        [Header("Customer Archetypes")]
        public List<CustomerSO> availableCustomerTypes = new List<CustomerSO>();
        public GameObject customerPrefab;

        [Header("Spawn Positions (Grid Coordinates)")]
        public Vector2Int entranceGridPos = new Vector2Int(0, 15);
        public Vector2Int exitGridPos = new Vector2Int(0, 15);

        [Header("Spawning Dynamics")]
        public float minSpawnInterval = 8f;
        public float maxSpawnInterval = 18f;
        public int maxSimultaneousCustomers = 5;

        private List<CustomerController> activeCustomers = new List<CustomerController>();
        private Coroutine spawnRoutine;

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
            if (availableCustomerTypes == null || availableCustomerTypes.Count == 0)
            {
                availableCustomerTypes = new List<CustomerSO>(Resources.LoadAll<CustomerSO>("Customers"));
            }

            if (ObjectPoolManager.Instance != null && customerPrefab != null)
            {
                ObjectPoolManager.Instance.Prewarm("Customers", customerPrefab, 8);
            }

            GameEvents.OnCustomerLeft += HandleCustomerLeft;
            GameEvents.OnCustomerServed += HandleCustomerServed;
            spawnRoutine = StartCoroutine(SpawnLoop());
        }

        private void OnDestroy()
        {
            GameEvents.OnCustomerLeft -= HandleCustomerLeft;
            GameEvents.OnCustomerServed -= HandleCustomerServed;
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        }

        private void HandleCustomerServed(object customerObj, bool satisfied)
        {
            if (satisfied && Save.SaveManager.Instance != null && Save.SaveManager.Instance.SaveData != null)
            {
                Save.SaveManager.Instance.SaveData.customersServedTotal++;
                Save.SaveManager.Instance.SaveGame();

                if (Save.SaveManager.Instance.SaveData.customersServedTotal >= 3 &&
                    !Save.SaveManager.Instance.SaveData.helperIntroTriggered)
                {
                    UI.HelperIntroDialogUI.ShowIfAvailable();
                }
            }
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(3f); // Initial delay

            while (true)
            {
                int currentRep = (EconomyManager.Instance != null) ? EconomyManager.Instance.Reputation : 10;
                float repMultiplier = Mathf.Clamp(1f - (currentRep * 0.004f), 0.55f, 1.15f);
                float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval) * repMultiplier;
                yield return new WaitForSeconds(waitTime);

                bool isOpen = RestaurantOperatingManager.Instance == null || RestaurantOperatingManager.Instance.IsOpen;
                if (isOpen && activeCustomers.Count < maxSimultaneousCustomers && HasAvailableTables())
                {
                    SpawnCustomer();
                }
            }
        }

        private bool HasAvailableTables()
        {
            if (BuildManager.Instance == null) return false;
            foreach (var obj in BuildManager.Instance.activeFurniture)
            {
                Table t = obj as Table;
                if (t != null && t.HasAvailableChair()) return true;
            }
            return false;
        }

        public void SpawnCustomer()
        {
            if (availableCustomerTypes == null || availableCustomerTypes.Count == 0)
            {
                availableCustomerTypes = new List<CustomerSO>(Resources.LoadAll<CustomerSO>("Customers"));
                if (availableCustomerTypes.Count == 0) return;
            }

            CustomerSO selectedType = SelectCustomerType();
            if (selectedType == null) return;

            Vector3 spawnPos = GridManager.Instance != null ? GridManager.Instance.GridToWorld(entranceGridPos) : (Vector3)(Vector2)entranceGridPos;
            GameObject cObj;

            if (ObjectPoolManager.Instance != null)
            {
                cObj = ObjectPoolManager.Instance.Spawn("Customers", spawnPos, Quaternion.identity, customerPrefab);
            }
            else if (customerPrefab != null)
            {
                cObj = Instantiate(customerPrefab, spawnPos, Quaternion.identity);
            }
            else
            {
                cObj = new GameObject("Customer_" + selectedType.customerTitle);
                cObj.transform.position = spawnPos;
                SpriteRenderer sr = cObj.AddComponent<SpriteRenderer>();
                sr.sortingOrder = 5;
            }

            CustomerController controller = cObj.GetComponent<CustomerController>();
            if (controller == null)
            {
                controller = cObj.AddComponent<CustomerController>();
            }

            activeCustomers.Add(controller);
            controller.Setup(selectedType, entranceGridPos, exitGridPos);
            GameEvents.TriggerCustomerArrived(controller);
        }

        private void HandleCustomerLeft(object customerObj)
        {
            CustomerController c = customerObj as CustomerController;
            if (c != null)
            {
                activeCustomers.Remove(c);
            }
        }

        private CustomerSO SelectCustomerType()
        {
            int currentLevel = (Progression.ProgressionManager.Instance != null) ? Progression.ProgressionManager.Instance.CurrentLevel : 1;
            int currentRep = (EconomyManager.Instance != null) ? EconomyManager.Instance.Reputation : 10;

            List<CustomerSO> eligible = new List<CustomerSO>();
            foreach (var c in availableCustomerTypes)
            {
                if (c != null && c.unlockLevel <= currentLevel)
                {
                    eligible.Add(c);
                }
            }

            if (eligible.Count == 0) return availableCustomerTypes[0];

            // Weight calculation
            List<float> weights = new List<float>();
            foreach (var c in eligible)
            {
                float w = 10f; // Base weight
                switch (c.archetype)
                {
                    case CustomerArchetype.Normal:
                        w = 25f;
                        break;
                    case CustomerArchetype.Impaciente:
                        w = 15f;
                        break;
                    case CustomerArchetype.Generoso:
                        w = 15f;
                        break;
                    case CustomerArchetype.Turista:
                        w = Mathf.Clamp(currentRep * 0.3f, 5f, 20f);
                        break;
                    case CustomerArchetype.CriticoGastronomico:
                        // Only spawns with decent reputation
                        w = (currentRep >= 35) ? Mathf.Clamp((currentRep - 30) * 0.2f, 2f, 10f) : 0f;
                        break;
                    case CustomerArchetype.VIP:
                    case CustomerArchetype.Celebridad:
                        // Prestige customers appear when reputation is high
                        w = (currentRep >= 50) ? Mathf.Clamp((currentRep - 45) * 0.25f, 3f, 12f) : 0f;
                        break;
                    default:
                        w = 10f;
                        break;
                }
                weights.Add(w);
            }

            // Weighted random selection
            float totalWeight = 0f;
            foreach (var w in weights) totalWeight += w;
            if (totalWeight <= 0f) return eligible[Random.Range(0, eligible.Count)];

            float roll = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            for (int i = 0; i < eligible.Count; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    return eligible[i];
                }
            }

            return eligible[0];
        }
    }
}
