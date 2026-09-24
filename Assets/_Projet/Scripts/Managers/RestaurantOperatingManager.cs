using System;
using UnityEngine;
using VillaDelChef.Core;
using VillaDelChef.Save;

namespace VillaDelChef.Managers
{
    /// <summary>
    /// Gestiona el estado de apertura/cierre del restaurante (Fase 7).
    /// Cuando está CERRADO: no llegan nuevos clientes, pero los sentados completan su ciclo.
    /// Cuando está ABIERTO: CustomerManager reanuda el ciclo normal de spawns periódicos.
    /// </summary>
    public class RestaurantOperatingManager : MonoBehaviour
    {
        public static RestaurantOperatingManager Instance { get; private set; }

        [Header("State")]
        [SerializeField] private bool isOpen = false;

        public bool IsOpen => isOpen;

        public event Action<bool> OnOperatingStateChanged;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }
        }

        private void Start()
        {
            // Cargar estado inicial desde SaveManager si está disponible
            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                isOpen = SaveManager.Instance.SaveData.restaurantOpen;
            }

            OnOperatingStateChanged?.Invoke(isOpen);
        }

        public void OpenRestaurant()
        {
            if (isOpen) return;

            isOpen = true;
            PersistState();
            OnOperatingStateChanged?.Invoke(true);
            Debug.Log("[RestaurantOperatingManager] <color=green>¡El restaurante ahora está ABIERTO!</color>");
        }

        public void CloseRestaurant()
        {
            if (!isOpen) return;

            isOpen = false;
            PersistState();
            OnOperatingStateChanged?.Invoke(false);
            Debug.Log("[RestaurantOperatingManager] <color=yellow>El restaurante ahora está CERRADO. Los clientes actuales terminarán su pedido.</color>");
        }

        public void ToggleOperatingState()
        {
            if (isOpen)
            {
                CloseRestaurant();
            }
            else
            {
                OpenRestaurant();
            }
        }

        private void PersistState()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                SaveManager.Instance.SaveData.restaurantOpen = isOpen;
                SaveManager.Instance.SaveGame();
            }
        }
    }
}
