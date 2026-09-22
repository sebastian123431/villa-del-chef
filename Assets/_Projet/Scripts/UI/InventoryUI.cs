using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Core;
using VillaDelChef.Inventory;
using VillaDelChef.Managers;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    public class InventoryUI : MonoBehaviour
    {
        private static InventoryUI _instance;
        public static InventoryUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("UI Root")]
        public GameObject panelRoot;
        public Transform itemsContainer;
        public Button closeButton;
        public Text emptyMessageText;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            if (closeButton != null) closeButton.onClick.AddListener(Close);

            GameEvents.OnInventoryUpdated += (id, count) =>
            {
                if (IsOpen())
                {
                    RefreshInventory();
                }
            };
        }

        public bool IsOpen()
        {
            return (panelRoot != null && panelRoot.activeSelf) || gameObject.activeSelf;
        }

        public void Open()
        {
            gameObject.SetActive(true);
            if (panelRoot != null) panelRoot.SetActive(true);
            RefreshInventory();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            else gameObject.SetActive(false);
        }

        public void Toggle()
        {
            if (IsOpen()) Close();
            else Open();
        }

        public void RefreshInventory()
        {
            if (itemsContainer == null) return;

            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            // Load all known ingredients
            IngredientSO[] allIngredients = Resources.LoadAll<IngredientSO>("Ingredients");
            int populatedCount = 0;

            if (InventoryManager.Instance != null && allIngredients != null)
            {
                foreach (var ing in allIngredients)
                {
                    if (ing == null) continue;
                    int count = InventoryManager.Instance.GetItemCount(ing.ingredientID);
                    if (count > 0)
                    {
                        CreateItemCard(ing, count);
                        populatedCount++;
                    }
                }
            }

            if (emptyMessageText != null)
            {
                emptyMessageText.gameObject.SetActive(populatedCount == 0);
            }
        }

        private void CreateItemCard(IngredientSO ing, int count)
        {
            GameObject cardGO = new GameObject($"Slot_{ing.ingredientName}");
            cardGO.transform.SetParent(itemsContainer, false);

            RectTransform rt = cardGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(140, 150);

            Image bg = cardGO.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.20f, 0.25f, 0.95f);

            // Icon
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(cardGO.transform, false);
            RectTransform iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(64, 64);
            iconRT.anchoredPosition = new Vector2(0f, 25f);
            Image iconImg = iconGO.AddComponent<Image>();
            if (ing.icon != null) iconImg.sprite = ing.icon;

            // Name
            GameObject nameGO = new GameObject("Name");
            nameGO.transform.SetParent(cardGO.transform, false);
            RectTransform nameRT = nameGO.AddComponent<RectTransform>();
            nameRT.sizeDelta = new Vector2(130, 30);
            nameRT.anchoredPosition = new Vector2(0f, -25f);
            Text nameText = nameGO.AddComponent<Text>();
            nameText.text = ing.ingredientName;
            nameText.fontSize = 15;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = Color.white;

            // Count badge
            GameObject badgeGO = new GameObject("CountBadge");
            badgeGO.transform.SetParent(cardGO.transform, false);
            RectTransform badgeRT = badgeGO.AddComponent<RectTransform>();
            badgeRT.sizeDelta = new Vector2(120, 25);
            badgeRT.anchoredPosition = new Vector2(0f, -55f);
            Text countText = badgeGO.AddComponent<Text>();
            countText.text = $"x{count}";
            countText.fontSize = 18;
            countText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            countText.alignment = TextAnchor.MiddleCenter;
            countText.color = new Color(1f, 0.85f, 0.2f);
        }
    }
}
