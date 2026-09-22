using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Cooking;
using VillaDelChef.Inventory;
using VillaDelChef.Managers;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    public class CookStationUI : MonoBehaviour
    {
        private static CookStationUI _instance;
        public static CookStationUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<CookStationUI>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("UI Root")]
        public GameObject panelRoot;
        public Text stationTitleText;
        public Transform recipeListContainer;
        public GameObject recipeCardPrefab;
        public Button closeButton;

        private CookingStation targetStation;

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
        }

        public void OpenForStation(CookingStation station)
        {
            if (station == null || station.currentState != StationState.Idle) return;

            targetStation = station;
            if (stationTitleText != null)
            {
                stationTitleText.text = station.stationData != null ? station.stationData.stationName : station.stationType.ToString();
            }

            gameObject.SetActive(true);
            if (panelRoot != null) panelRoot.SetActive(true);
            PopulateRecipes();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            else gameObject.SetActive(false);
            targetStation = null;
        }

        private void PopulateRecipes()
        {
            if (recipeListContainer == null || targetStation == null || RecipeManager.Instance == null) return;

            // Clear old items
            foreach (Transform child in recipeListContainer)
            {
                Destroy(child.gameObject);
            }

            List<RecipeSO> recipes = RecipeManager.Instance.GetRecipesForStation(targetStation.stationType);

            foreach (var recipe in recipes)
            {
                CreateRecipeCard(recipe);
            }
        }

        private void CreateRecipeCard(RecipeSO recipe)
        {
            if (recipe == null) return;

            GameObject card = new GameObject($"Card_{recipe.recipeName}");
            card.transform.SetParent(recipeListContainer, false);

            RectTransform rt = card.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(660, 120);

            Image bg = card.AddComponent<Image>();
            bg.color = new Color(0.16f, 0.18f, 0.24f, 0.95f);

            // Icon
            GameObject iconGO = new GameObject("Icon");
            iconGO.transform.SetParent(card.transform, false);
            RectTransform iconRT = iconGO.AddComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(64, 64);
            iconRT.anchoredPosition = new Vector2(-280f, 0f);
            Image iconImg = iconGO.AddComponent<Image>();
            if (recipe.icon != null) iconImg.sprite = recipe.icon;

            // Name & Time
            GameObject nameGO = new GameObject("NameText");
            nameGO.transform.SetParent(card.transform, false);
            RectTransform nameRT = nameGO.AddComponent<RectTransform>();
            nameRT.sizeDelta = new Vector2(340, 26);
            nameRT.anchoredPosition = new Vector2(-60f, 32f);
            Text nameText = nameGO.AddComponent<Text>();
            nameText.text = $"{recipe.recipeName}  <color=#88CCFF>({recipe.cookTimeSeconds}s)</color>";
            nameText.fontSize = 18;
            nameText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            nameText.alignment = TextAnchor.MiddleLeft;
            nameText.color = Color.white;
            nameText.supportRichText = true;

            // Ingredients requirements breakdown
            bool hasAll = true;
            IngredientSO firstMissingIng = null;

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if (recipe.requiredIngredients != null)
            {
                foreach (var req in recipe.requiredIngredients)
                {
                    if (req.ingredient == null) continue;
                    int count = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(req.ingredient.ingredientID) : 0;
                    bool ok = count >= req.amount;
                    if (!ok)
                    {
                        hasAll = false;
                        if (firstMissingIng == null) firstMissingIng = req.ingredient;
                    }

                    string colorHex = ok ? "#55FF55" : "#FF6666";
                    sb.Append($"<color={colorHex}>{req.ingredient.ingredientName}: {count}/{req.amount}</color>  ");
                }
            }

            GameObject reqGO = new GameObject("ReqText");
            reqGO.transform.SetParent(card.transform, false);
            RectTransform reqRT = reqGO.AddComponent<RectTransform>();
            reqRT.sizeDelta = new Vector2(340, 45);
            reqRT.anchoredPosition = new Vector2(-60f, -8f);
            Text reqText = reqGO.AddComponent<Text>();
            reqText.text = sb.ToString();
            reqText.fontSize = 13;
            reqText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            reqText.alignment = TextAnchor.MiddleLeft;
            reqText.supportRichText = true;

            // Action Button: Either "COCINAR" or "COMPRAR INGREDIENTE"
            GameObject btnGO = new GameObject("ActionButton");
            btnGO.transform.SetParent(card.transform, false);
            RectTransform btnRT = btnGO.AddComponent<RectTransform>();
            btnRT.sizeDelta = new Vector2(140, 48);
            btnRT.anchoredPosition = new Vector2(240f, 0f);

            Image btnImg = btnGO.AddComponent<Image>();
            btnImg.color = hasAll ? new Color(0.2f, 0.72f, 0.35f) : new Color(0.9f, 0.55f, 0.2f);

            Button btn = btnGO.AddComponent<Button>();

            GameObject btnTextGO = new GameObject("Text");
            btnTextGO.transform.SetParent(btnGO.transform, false);
            RectTransform btRT = btnTextGO.AddComponent<RectTransform>();
            btRT.anchorMin = Vector2.zero;
            btRT.anchorMax = Vector2.one;
            btRT.offsetMin = Vector2.zero;
            btRT.offsetMax = Vector2.zero;
            Text btnText = btnTextGO.AddComponent<Text>();
            btnText.fontSize = 15;
            btnText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            btnText.alignment = TextAnchor.MiddleCenter;
            btnText.color = Color.white;

            if (hasAll)
            {
                btnText.text = "COCINAR";
                btn.onClick.AddListener(() =>
                {
                    if (targetStation != null && targetStation.StartCooking(recipe))
                    {
                        Close();
                    }
                });
            }
            else
            {
                int price = firstMissingIng != null && firstMissingIng.buyPrice > 0 ? firstMissingIng.buyPrice : 10;
                btnText.text = $"+ Comprar\n(${price})";
                btn.onClick.AddListener(() =>
                {
                    if (firstMissingIng != null)
                    {
                        if (MarketUI.QuickBuy(firstMissingIng, 1))
                        {
                            PopulateRecipes();
                        }
                        else
                        {
                            Debug.LogWarning("[CookStationUI] Monedas insuficientes para comprar este ingrediente.");
                        }
                    }
                });
            }
        }
    }
}
