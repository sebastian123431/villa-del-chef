using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Building;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.UI
{
    public class BuildUI : MonoBehaviour
    {
        private static BuildUI _instance;
        public static BuildUI Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Object.FindAnyObjectByType<BuildUI>(FindObjectsInactive.Include);
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Catalog Items")]
        public List<FurnitureSO> catalogItems = new List<FurnitureSO>();

        [Header("UI References")]
        public GameObject panelRoot;
        public Transform itemsContainer;
        public GameObject furnitureCardPrefab;
        public Button rotateButton;
        public Button closeButton;

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
            if (catalogItems == null || catalogItems.Count == 0)
            {
                catalogItems = new List<FurnitureSO>(Resources.LoadAll<FurnitureSO>("Furniture"));
            }
            if (rotateButton != null) rotateButton.onClick.AddListener(OnRotateClicked);
            if (closeButton != null) closeButton.onClick.AddListener(OnCloseClicked);
            PopulateCatalog();
        }

        private void OnRotateClicked()
        {
            BuildManager.Instance?.RotateSelection();
        }

        private void OnCloseClicked()
        {
            BuildManager.Instance?.SetBuildMode(false);
        }

        public void PopulateCatalog()
        {
            if (itemsContainer == null) return;

            foreach (Transform child in itemsContainer)
            {
                Destroy(child.gameObject);
            }

            foreach (var item in catalogItems)
            {
                if (item == null) continue;

                GameObject card = furnitureCardPrefab != null ? Instantiate(furnitureCardPrefab, itemsContainer) : new GameObject(item.furnitureName);
                if (furnitureCardPrefab == null)
                {
                    card.transform.SetParent(itemsContainer, false);
                }

                Text nameText = card.transform.Find("NameText")?.GetComponent<Text>();
                if (nameText != null) nameText.text = item.furnitureName;

                Text priceText = card.transform.Find("PriceText")?.GetComponent<Text>();
                if (priceText != null) priceText.text = $"${item.cost}";

                Image iconImg = card.transform.Find("Icon")?.GetComponent<Image>();
                if (iconImg != null && item.shopIcon != null) iconImg.sprite = item.shopIcon;

                Button selectBtn = card.GetComponent<Button>() ?? card.transform.Find("SelectButton")?.GetComponent<Button>();
                if (selectBtn != null)
                {
                    selectBtn.onClick.AddListener(() =>
                    {
                        BuildManager.Instance?.SelectFurnitureToBuild(item);
                    });
                }
            }
        }
    }
}
