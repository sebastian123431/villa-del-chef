using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Core;
using VillaDelChef.Customers;
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
            GameEvents.OnCustomerLeft += HandleCustomerLeft;
            spawnRoutine = StartCoroutine(SpawnLoop());
        }

        private void OnDestroy()
        {
            GameEvents.OnCustomerLeft -= HandleCustomerLeft;
            if (spawnRoutine != null) StopCoroutine(spawnRoutine);
        }

        private IEnumerator SpawnLoop()
        {
            yield return new WaitForSeconds(3f); // Initial delay

            while (true)
            {
                float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(waitTime);

                if (activeCustomers.Count < maxSimultaneousCustomers && HasAvailableTables())
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
            if (availableCustomerTypes == null || availableCustomerTypes.Count == 0) return;

            CustomerSO selectedType = availableCustomerTypes[Random.Range(0, availableCustomerTypes.Count)];

            GameObject cObj;
            if (customerPrefab != null)
            {
                cObj = Instantiate(customerPrefab);
            }
            else
            {
                cObj = new GameObject("Customer_" + selectedType.customerTitle);
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
    }
}
