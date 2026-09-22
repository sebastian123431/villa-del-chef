using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Core;
using VillaDelChef.Crafting;
using VillaDelChef.Inventory;
using VillaDelChef.Managers;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    public class CraftingUI : MonoBehaviour
    {
        private static CraftingUI _instance;
        public static CraftingUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<CraftingUI>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("UI Root")]
        public GameObject panelRoot;
        public Text titleText;
        public Text subtitleText;
        public Button closeButton;

        [Header("Recipe Selection View")]
        public GameObject recipeSelectionView;
        public Transform recipesContainer;
        public GameObject recipeCardPrefab;

        [Header("Active Crafting View")]
        public GameObject activeCraftingView;
        public Image activeProductIcon;
        public Text activeProductNameText;
        public Image progressBarFill;
        public Text progressTimerText;
        public Button speedUpButton;
        public Button collectButton;

        private CraftingStation currentStation;
        private float tickTimer = 0f;

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
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (speedUpButton != null)
            {
                speedUpButton.onClick.AddListener(() =>
                {
                    if (currentStation != null)
                    {
                        currentStation.SpeedUpInstantly();
                        RefreshStationView();
                    }
                });
            }

            if (collectButton != null)
            {
                collectButton.onClick.AddListener(() =>
                {
                    if (currentStation != null)
                    {
                        currentStation.CollectCraftedItem();
                        Close();
                    }
                });
            }

            GameEvents.OnInventoryUpdated += (id, count) =>
            {
                if (IsOpen() && currentStation != null && currentStation.currentState == CraftingState.Idle)
                {
                    PopulateRecipes();
                }
            };
        }

        private void Update()
        {
            if (IsOpen() && currentStation != null)
            {
                if (currentStation.currentState == CraftingState.Crafting)
                {
                    tickTimer += Time.deltaTime;
                    if (tickTimer >= 0.1f)
                    {
                        tickTimer = 0f;
                        UpdateProgressVisuals();
                    }
                }
                else if (currentStation.currentState == CraftingState.ReadyToCollect)
                {
                    if (activeCraftingView != null && !collectButton.gameObject.activeSelf)
                    {
                        RefreshStationView();
                    }
                }
            }
        }

        public bool IsOpen()
        {
            return (panelRoot != null && panelRoot.activeSelf) || gameObject.activeSelf;
        }

        public void OpenForStation(CraftingStation station)
        {
            if (station == null) return;

            currentStation = station;
            gameObject.SetActive(true);
            if (panelRoot != null) panelRoot.SetActive(true);

            if (titleText != null)
            {
                titleText.text = station.stationName;
            }

            RefreshStationView();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            else gameObject.SetActive(false);
            currentStation = null;
        }

        public void RefreshStationView()
        {
            if (currentStation == null) return;

            if (currentStation.currentState == CraftingState.Idle)
            {
                if (recipeSelectionView != null) recipeSelectionView.SetActive(true);
                if (activeCraftingView != null) activeCraftingView.SetActive(false);
                if (subtitleText != null) subtitleText.text = "Selecciona un insumo para procesar:";
                PopulateRecipes();
            }
            else
            {
                if (recipeSelectionView != null) recipeSelectionView.SetActive(false);
                if (activeCraftingView != null) activeCraftingView.SetActive(true);

                var recipe = currentStation.currentRecipe;
                if (recipe != null)
                {
                    if (activeProductIcon != null && recipe.icon != null)
                    {
                        activeProductIcon.sprite = recipe.icon;
                    }
                    if (activeProductNameText != null)
                    {
                        activeProductNameText.text = $"{recipe.recipeName} x{recipe.resultAmount}";
                    }
                }

                bool isReady = (currentStation.currentState == CraftingState.ReadyToCollect);
                if (collectButton != null) collectButton.gameObject.SetActive(isReady);
                if (speedUpButton != null) speedUpButton.gameObject.SetActive(!isReady);

                if (subtitleText != null)
                {
                    subtitleText.text = isReady ? "¡Elaboración completada! Toca para recolectar." : "Procesando ingredientes...";
                }

                UpdateProgressVisuals();
            }
        }

        private void UpdateProgressVisuals()
        {
            if (currentStation == null) return;

            float progress = currentStation.GetProgress();
            if (progressBarFill != null)
            {
                progressBarFill.fillAmount = progress;
            }

            if (progressTimerText != null)
            {
                if (currentStation.currentState == CraftingState.ReadyToCollect)
                {
                    progressTimerText.text = "¡LISTO!";
                }
                else
                {
                    int remaining = Mathf.CeilToInt(currentStation.GetRemainingSeconds());
                    progressTimerText.text = $"Tiempo restante: {remaining}s";
                }
            }
        }

        public void PopulateRecipes()
        {
            if (recipesContainer == null || currentStation == null) return;

            foreach (Transform child in recipesContainer)
            {
                Destroy(child.gameObject);
            }

            List<CraftingRecipeSO> recipes = CraftingManager.Instance != null
                ? CraftingManager.Instance.GetRecipesForStation(currentStation.stationType)
                : new List<CraftingRecipeSO>();

            if (recipes.Count == 0)
            {
                if (subtitleText != null) subtitleText.text = "No hay recetas disponibles para esta estación.";
                return;
            }

            foreach (var r in recipes)
            {
                if (r == null) continue;
                CreateRecipeRow(r);
            }
        }

        private void CreateRecipeRow(CraftingRecipeSO recipe)
        {
            GameObject card = BuildDefaultCraftCard(recipe);
            card.transform.SetParent(recipesContainer, false);
        }

        private GameObject BuildDefaultCraftCard(CraftingRecipeSO recipe)
        {
            GameObject card = new GameObject($"Card_{recipe.craftID}", typeof(RectTransform), typeof(Image));
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(520, 80);

            Image bg = card.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.16f, 0.22f, 0.95f);

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(card.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0.5f);
            iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.sizeDelta = new Vector2(52, 52);
            iconRt.anchoredPosition = new Vector2(38, 0);

            Image iconImg = iconObj.GetComponent<Image>();
            if (recipe.icon != null) iconImg.sprite = recipe.icon;

            // Name & Output Amount
            GameObject nameObj = new GameObject("Name", typeof(RectTransform), typeof(Text));
            nameObj.transform.SetParent(card.transform, false);
            Text nameT = nameObj.GetComponent<Text>();
            nameT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            nameT.fontSize = 17;
            nameT.fontStyle = FontStyle.Bold;
            nameT.color = Color.white;
            nameT.text = $"{recipe.recipeName} x{recipe.resultAmount}";
            RectTransform nameRt = nameObj.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.5f);
            nameRt.anchorMax = new Vector2(0, 0.5f);
            nameRt.sizeDelta = new Vector2(200, 26);
            nameRt.anchoredPosition = new Vector2(175, 14);

            // Requirements line (e.g. "Trigo x2")
            string reqText = "";
            bool hasAll = true;
            if (recipe.requiredIngredients != null)
            {
                foreach (var req in recipe.requiredIngredients)
                {
                    if (req.ingredient == null) continue;
                    int playerHas = InventoryManager.Instance != null ? InventoryManager.Instance.GetItemCount(req.ingredient.ingredientID) : 0;
                    if (playerHas < req.amount) hasAll = false;
                    reqText += $"{req.ingredient.ingredientName}: {playerHas}/{req.amount}   ";
                }
            }

            GameObject reqObj = new GameObject("Reqs", typeof(RectTransform), typeof(Text));
            reqObj.transform.SetParent(card.transform, false);
            Text reqT = reqObj.GetComponent<Text>();
            reqT.font = nameT.font;
            reqT.fontSize = 13;
            reqT.color = hasAll ? new Color(0.4f, 0.95f, 0.45f) : new Color(0.95f, 0.45f, 0.45f);
            reqT.text = reqText;
            RectTransform reqRt = reqObj.GetComponent<RectTransform>();
            reqRt.anchorMin = new Vector2(0, 0.5f);
            reqRt.anchorMax = new Vector2(0, 0.5f);
            reqRt.sizeDelta = new Vector2(250, 22);
            reqRt.anchoredPosition = new Vector2(200, -14);

            // Time text
            GameObject timeObj = new GameObject("Time", typeof(RectTransform), typeof(Text));
            timeObj.transform.SetParent(card.transform, false);
            Text timeT = timeObj.GetComponent<Text>();
            timeT.font = nameT.font;
            timeT.fontSize = 14;
            timeT.color = new Color(0.7f, 0.85f, 1f);
            timeT.text = $"⏱ {recipe.craftTimeSeconds}s";
            RectTransform timeRt = timeObj.GetComponent<RectTransform>();
            timeRt.anchorMin = new Vector2(1, 0.5f);
            timeRt.anchorMax = new Vector2(1, 0.5f);
            timeRt.sizeDelta = new Vector2(70, 24);
            timeRt.anchoredPosition = new Vector2(-120, 0);

            // Produce Button
            GameObject btnObj = new GameObject("ProduceBtn", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(card.transform, false);
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1, 0.5f);
            btnRt.anchorMax = new Vector2(1, 0.5f);
            btnRt.sizeDelta = new Vector2(85, 42);
            btnRt.anchoredPosition = new Vector2(-52, 0);

            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = hasAll ? new Color(0.2f, 0.65f, 0.35f) : new Color(0.4f, 0.4f, 0.4f);

            Button btn = btnObj.GetComponent<Button>();
            btn.interactable = hasAll;

            GameObject btnTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            btnTxtObj.transform.SetParent(btnObj.transform, false);
            Text btnT = btnTxtObj.GetComponent<Text>();
            btnT.font = nameT.font;
            btnT.fontSize = 15;
            btnT.fontStyle = FontStyle.Bold;
            btnT.color = Color.white;
            btnT.alignment = TextAnchor.MiddleCenter;
            btnT.text = "Elaborar";
            btnTxtObj.GetComponent<RectTransform>().sizeDelta = btnRt.sizeDelta;

            btn.onClick.AddListener(() =>
            {
                if (currentStation != null && currentStation.StartCrafting(recipe))
                {
                    RefreshStationView();
                }
            });

            return card;
        }
    }
}
