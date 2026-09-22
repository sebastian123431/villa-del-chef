using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.NPC;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    public class VendorUI : MonoBehaviour
    {
        private static VendorUI _instance;
        public static VendorUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<VendorUI>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("UI Root Elements")]
        public GameObject panelRoot;
        public Button closeButton;

        [Header("NPC Header Elements")]
        public Image npcPortraitImage;
        public Text npcNameText;
        public Text npcRoleText;
        public Text dialogueText;
        public Text restockTimerText;

        [Header("Catalog Container")]
        public Transform itemsContainer;
        public GameObject itemCardPrefab;

        private NPCController currentNPC;
        private VendorController currentVendor;
        private float timerTick = 0f;

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

            GameEvents.OnCoinsChanged += (coins) =>
            {
                if (IsOpen())
                {
                    UpdateCatalogInteractability();
                }
            };
        }

        private void Update()
        {
            if (IsOpen() && currentVendor != null && restockTimerText != null)
            {
                timerTick += Time.deltaTime;
                if (timerTick >= 1f)
                {
                    timerTick = 0f;
                    long remaining = currentVendor.GetRemainingRestockSeconds();
                    long minutes = remaining / 60;
                    long seconds = remaining % 60;
                    restockTimerText.text = $"Reabastecimiento en: {minutes:D2}:{seconds:D2}";
                }
            }
        }

        public bool IsOpen()
        {
            return (panelRoot != null && panelRoot.activeSelf) || gameObject.activeSelf;
        }

        public void OpenForNPC(NPCController npc)
        {
            if (npc == null || npc.npcData == null) return;

            currentNPC = npc;
            currentVendor = npc.vendorController;
            if (currentVendor == null && npc.npcData.vendorData != null)
            {
                currentVendor = npc.gameObject.AddComponent<VendorController>();
                currentVendor.vendorData = npc.npcData.vendorData;
            }

            gameObject.SetActive(true);
            if (panelRoot != null) panelRoot.SetActive(true);

            PopulateHeader();
            PopulateCatalog();
        }

        public void Close()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            else gameObject.SetActive(false);
            currentNPC = null;
            currentVendor = null;
        }

        private void PopulateHeader()
        {
            if (currentNPC == null || currentNPC.npcData == null) return;

            var data = currentNPC.npcData;

            if (npcNameText != null) npcNameText.text = data.npcName;
            if (npcRoleText != null) npcRoleText.text = string.IsNullOrEmpty(data.roleTitle) ? "Comerciante de la Villa" : data.roleTitle;
            if (dialogueText != null) dialogueText.text = data.greetingDialogue;

            if (npcPortraitImage != null)
            {
                if (data.portrait != null)
                {
                    npcPortraitImage.gameObject.SetActive(true);
                    npcPortraitImage.sprite = data.portrait;
                }
                else if (data.worldSprite != null)
                {
                    npcPortraitImage.gameObject.SetActive(true);
                    npcPortraitImage.sprite = data.worldSprite;
                }
                else
                {
                    npcPortraitImage.gameObject.SetActive(false);
                }
            }
        }

        public void PopulateCatalog()
        {
            if (itemsContainer == null || currentVendor == null || currentVendor.vendorData == null) return;

            // Clear previous items
            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            var catalog = currentVendor.vendorData.catalog;
            if (catalog == null || catalog.Count == 0) return;

            foreach (var item in catalog)
            {
                if (item == null) continue;
                CreateItemRow(item);
            }
        }

        private void CreateItemRow(VendorItemEntry item)
        {
            GameObject cardObj = null;
            if (itemCardPrefab != null)
            {
                cardObj = Instantiate(itemCardPrefab, itemsContainer);
            }
            else
            {
                cardObj = BuildDefaultItemCard();
                cardObj.transform.SetParent(itemsContainer, false);
            }

            // Bind card data
            Image iconImg = cardObj.transform.Find("Icon")?.GetComponent<Image>();
            Text nameTxt = cardObj.transform.Find("Name")?.GetComponent<Text>();
            Text priceTxt = cardObj.transform.Find("Price")?.GetComponent<Text>();
            Text stockTxt = cardObj.transform.Find("Stock")?.GetComponent<Text>();
            Button buyBtn = cardObj.transform.Find("BuyButton")?.GetComponent<Button>();

            if (iconImg != null && item.icon != null) iconImg.sprite = item.icon;
            if (nameTxt != null) nameTxt.text = item.displayName;
            if (priceTxt != null) priceTxt.text = $"{item.buyPrice} Oro";

            int stock = currentVendor.GetStock(item.itemID);
            if (stockTxt != null) stockTxt.text = $"Stock: {stock}/{item.maxStock}";

            if (buyBtn != null)
            {
                bool canAfford = EconomyManager.Instance == null || EconomyManager.Instance.HasCoins(item.buyPrice);
                bool hasStock = stock > 0;
                buyBtn.interactable = canAfford && hasStock;

                buyBtn.onClick.RemoveAllListeners();
                buyBtn.onClick.AddListener(() =>
                {
                    if (currentVendor != null && currentVendor.TryPurchaseItem(item, 1))
                    {
                        // Refresh stock display
                        int updatedStock = currentVendor.GetStock(item.itemID);
                        if (stockTxt != null) stockTxt.text = $"Stock: {updatedStock}/{item.maxStock}";
                        buyBtn.interactable = (EconomyManager.Instance == null || EconomyManager.Instance.HasCoins(item.buyPrice)) && updatedStock > 0;
                    }
                });
            }
        }

        private void UpdateCatalogInteractability()
        {
            if (itemsContainer == null || currentVendor == null || currentVendor.vendorData == null) return;

            int index = 0;
            var catalog = currentVendor.vendorData.catalog;
            foreach (Transform child in itemsContainer)
            {
                if (index < catalog.Count)
                {
                    var item = catalog[index];
                    int stock = currentVendor.GetStock(item.itemID);
                    Button buyBtn = child.Find("BuyButton")?.GetComponent<Button>();
                    Text stockTxt = child.Find("Stock")?.GetComponent<Text>();

                    if (stockTxt != null) stockTxt.text = $"Stock: {stock}/{item.maxStock}";
                    if (buyBtn != null)
                    {
                        buyBtn.interactable = (EconomyManager.Instance == null || EconomyManager.Instance.HasCoins(item.buyPrice)) && stock > 0;
                    }
                }
                index++;
            }
        }

        private GameObject BuildDefaultItemCard()
        {
            GameObject card = new GameObject("VendorItemCard", typeof(RectTransform), typeof(Image));
            RectTransform rt = card.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(500, 70);

            Image bg = card.GetComponent<Image>();
            bg.color = new Color(0.18f, 0.16f, 0.14f, 0.95f);

            // Icon
            GameObject iconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(card.transform, false);
            RectTransform iconRt = iconObj.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0.5f);
            iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.sizeDelta = new Vector2(48, 48);
            iconRt.anchoredPosition = new Vector2(35, 0);

            // Name
            GameObject nameObj = new GameObject("Name", typeof(RectTransform), typeof(Text));
            nameObj.transform.SetParent(card.transform, false);
            Text nameT = nameObj.GetComponent<Text>();
            nameT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
            nameT.fontSize = 18;
            nameT.fontStyle = FontStyle.Bold;
            nameT.color = Color.white;
            RectTransform nameRt = nameObj.GetComponent<RectTransform>();
            nameRt.anchorMin = new Vector2(0, 0.5f);
            nameRt.anchorMax = new Vector2(0, 0.5f);
            nameRt.sizeDelta = new Vector2(160, 30);
            nameRt.anchoredPosition = new Vector2(150, 10);

            // Stock
            GameObject stockObj = new GameObject("Stock", typeof(RectTransform), typeof(Text));
            stockObj.transform.SetParent(card.transform, false);
            Text stockT = stockObj.GetComponent<Text>();
            stockT.font = nameT.font;
            stockT.fontSize = 14;
            stockT.color = new Color(0.8f, 0.8f, 0.8f, 1f);
            RectTransform stockRt = stockObj.GetComponent<RectTransform>();
            stockRt.anchorMin = new Vector2(0, 0.5f);
            stockRt.anchorMax = new Vector2(0, 0.5f);
            stockRt.sizeDelta = new Vector2(160, 24);
            stockRt.anchoredPosition = new Vector2(150, -12);

            // Price
            GameObject priceObj = new GameObject("Price", typeof(RectTransform), typeof(Text));
            priceObj.transform.SetParent(card.transform, false);
            Text priceT = priceObj.GetComponent<Text>();
            priceT.font = nameT.font;
            priceT.fontSize = 16;
            priceT.fontStyle = FontStyle.Bold;
            priceT.color = new Color(1f, 0.85f, 0.2f, 1f);
            RectTransform priceRt = priceObj.GetComponent<RectTransform>();
            priceRt.anchorMin = new Vector2(1, 0.5f);
            priceRt.anchorMax = new Vector2(1, 0.5f);
            priceRt.sizeDelta = new Vector2(90, 30);
            priceRt.anchoredPosition = new Vector2(-125, 0);

            // Buy Button
            GameObject btnObj = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button));
            btnObj.transform.SetParent(card.transform, false);
            RectTransform btnRt = btnObj.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(1, 0.5f);
            btnRt.anchorMax = new Vector2(1, 0.5f);
            btnRt.sizeDelta = new Vector2(80, 42);
            btnRt.anchoredPosition = new Vector2(-48, 0);

            Image btnImg = btnObj.GetComponent<Image>();
            btnImg.color = new Color(0.25f, 0.65f, 0.35f, 1f);

            GameObject btnTxtObj = new GameObject("Text", typeof(RectTransform), typeof(Text));
            btnTxtObj.transform.SetParent(btnObj.transform, false);
            Text btnT = btnTxtObj.GetComponent<Text>();
            btnT.font = nameT.font;
            btnT.fontSize = 16;
            btnT.fontStyle = FontStyle.Bold;
            btnT.color = Color.white;
            btnT.alignment = TextAnchor.MiddleCenter;
            btnT.text = "Comprar";
            btnTxtObj.GetComponent<RectTransform>().sizeDelta = btnRt.sizeDelta;

            return card;
        }
    }
}
