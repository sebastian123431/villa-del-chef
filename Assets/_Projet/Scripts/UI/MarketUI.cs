using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.Inventory;
using VillaDelChef.Managers;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    public class MarketUI : MonoBehaviour
    {
        private static MarketUI _instance;
        public static MarketUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<MarketUI>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("UI Root")]
        public GameObject panelRoot;
        public Transform itemsContainer;
        public Button closeButton;
        public Text merchantGreetingText;

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

            GameEvents.OnCoinsChanged += (coins) =>
            {
                if (IsOpen())
                {
                    RefreshMarket();
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
            RefreshMarket();
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

        public void RefreshMarket()
        {
            if (itemsContainer == null) return;

            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            IngredientSO[] allIngredients = Resources.LoadAll<IngredientSO>("Ingredients");
            if (allIngredients != null)
            {
                foreach (var ing in allIngredients)
                {
                    if (ing == null) continue;
                    CreateMarketCard(ing);
                }
            }
        }

        private void CreateMarketCard(IngredientSO ing)
        {
            GameObject cardGO = new GameObject($"Market_{ing.ingredientName}");
            cardGO.transform.SetParent(itemsContainer, false);

            RectTransform rt = cardGO.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(170, 190);

            Image bg = cardGO.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.18f, 0.23f, 0.95f);

            // Icon
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(cardGO.transform, false);
            RectTransform iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(56, 56);
            iconRT.anchoredPosition = new Vector2(0f, 45f);
            Image iconImg = iconGO.AddComponent<Image>();
            if (ing.icon != null) iconImg.sprite = ing.icon;

            // Name
            GameObject nameGO = new GameObject("Name");
            nameGO.transform.SetParent(cardGO.transform, false);
            RectTransform nameRT = nameGO.AddComponent<RectTransform>();
            nameRT.sizeDelta = new Vector2(150, 25);
            nameRT.anchoredPosition = new Vector2(0f, 2f);
            Text nameText = nameGO.AddComponent<Text>();
            nameText.text = ing.ingredientName;
            nameText.fontSize = 14;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.alignment = TextAnchor.MiddleCenter;
            nameText.color = Color.white;

            // Stock badge (current in inventory)
            int currentStock = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(ing.ingredientID) : 0;
            GameObject stockGO = new GameObject("Stock");
            stockGO.transform.SetParent(cardGO.transform, false);
            RectTransform stockRT = stockGO.AddComponent<RectTransform>();
            stockRT.sizeDelta = new Vector2(150, 20);
            stockRT.anchoredPosition = new Vector2(0f, -22f);
            Text stockText = stockGO.AddComponent<Text>();
            stockText.text = $"Tienes: {currentStock}";
            stockText.fontSize = 12;
            stockText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            stockText.alignment = TextAnchor.MiddleCenter;
            stockText.color = new Color(0.7f, 0.8f, 0.9f);

            // Buy Button
            GameObject buyBtnGO = new GameObject("BuyBtn");
            buyBtnGO.transform.SetParent(cardGO.transform, false);
            RectTransform btnRT = buyBtnGO.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(140, 36);
            btnRT.anchoredPosition = new Vector2(0f, -58f);

            int price = ing.buyPrice > 0 ? ing.buyPrice : 10;
            bool canAfford = EconomyManager.Instance != null && EconomyManager.Instance.HasCoins(price);

            Image btnImg = buyBtnGO.AddComponent<Image>();
            btnImg.color = canAfford ? new Color(0.2f, 0.65f, 0.35f) : new Color(0.4f, 0.4f, 0.45f);

            Button buyBtn = buyBtnGO.AddComponent<Button>();
            buyBtn.interactable = canAfford;

            GameObject btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(buyBtnGO.transform, false);
            RectTransform btRT = btnTextGO.AddComponent<RectTransform>();
            btRT.anchorMin = Vector2.zero;
            btRT.anchorMax = Vector2.one;
            btRT.offsetMin = Vector2.zero;
            btRT.offsetMax = Vector2.zero;
            Text btnText = btnTextGO.AddComponent<Text>();
            btnText.text = $"Comprar ${price}";
            btnText.fontSize = 14;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;

            buyBtn.onClick.AddListener(() =>
            {
                BuyItem(ing, price);
            });
        }

        public void BuyItem(IngredientSO ing, int price)
        {
            if (ing == null) return;
            if (EconomyManager.Instance == null || !EconomyManager.Instance.SpendCoins(price))
            {
                Debug.LogWarning("[MarketUI] No tienes suficientes monedas.");
                return;
            }

            InventoryManager.Instance?.AddItem(ing.ingredientID, 1);
            AudioManager.Instance?.PlayCoin();
            RefreshMarket();
        }

        public static bool QuickBuy(IngredientSO ing, int amount = 1)
        {
            if (ing == null || amount <= 0) return false;
            int unitPrice = ing.buyPrice > 0 ? ing.buyPrice : 10;
            int totalPrice = unitPrice * amount;

            if (EconomyManager.Instance != null && EconomyManager.Instance.SpendCoins(totalPrice))
            {
                InventoryManager.Instance?.AddItem(ing.ingredientID, amount);
                AudioManager.Instance?.PlayCoin();
                return true;
            }
            return false;
        }
    }
}
