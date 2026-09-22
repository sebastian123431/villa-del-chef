using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.Core;
using VillaDelChef.Crafting;
using VillaDelChef.Customers;
using VillaDelChef.Economy;
using VillaDelChef.Farming;
using VillaDelChef.Inventory;
using VillaDelChef.Managers;
using VillaDelChef.NPC;
using VillaDelChef.PlayerInput;
using VillaDelChef.Progression;
using VillaDelChef.Restaurant;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.UI;

namespace VillaDelChef.Core
{
    public class RestaurantBootstrap : MonoBehaviour
    {
        [Header("Auto-Setup Configuration")]
        public bool buildMVPLayoutOnStart = true;

        private void Start()
        {
            if (buildMVPLayoutOnStart)
            {
                SetupMVP();
            }
        }

        public void SetupMVP()
        {
            EnsureCoreManagers();
            CreateDefaultDataAndObjects();
        }

        private void EnsureCoreManagers()
        {
            if (FindAnyObjectByType<SaveManager>() == null)
            {
                new GameObject("SaveManager").AddComponent<SaveManager>();
            }
            if (FindAnyObjectByType<GridManager>() == null)
            {
                var gmObj = new GameObject("GridManager");
                gmObj.AddComponent<GridManager>();
            }
            if (FindAnyObjectByType<BuildManager>() == null)
            {
                var bmObj = new GameObject("BuildManager");
                bmObj.AddComponent<BuildManager>();
            }
            if (FindAnyObjectByType<EconomyManager>() == null)
            {
                var emObj = new GameObject("EconomyManager");
                emObj.AddComponent<EconomyManager>();
            }
            if (FindAnyObjectByType<ProgressionManager>() == null)
            {
                var pmObj = new GameObject("ProgressionManager");
                pmObj.AddComponent<ProgressionManager>();
            }
            if (FindAnyObjectByType<InventoryManager>() == null)
            {
                var imObj = new GameObject("InventoryManager");
                imObj.AddComponent<InventoryManager>();
            }
            if (FindAnyObjectByType<RecipeManager>() == null)
            {
                var rmObj = new GameObject("RecipeManager");
                rmObj.AddComponent<RecipeManager>();
            }
            if (FindAnyObjectByType<FarmingManager>() == null)
            {
                var fmObj = new GameObject("FarmingManager");
                fmObj.AddComponent<FarmingManager>();
            }
            if (FindAnyObjectByType<CustomerManager>() == null)
            {
                var cmObj = new GameObject("CustomerManager");
                cmObj.AddComponent<CustomerManager>();
            }
            if (FindAnyObjectByType<WorkerManager>() == null)
            {
                var wmObj = new GameObject("WorkerManager");
                wmObj.AddComponent<WorkerManager>();
            }
            if (FindAnyObjectByType<QuestManager>() == null)
            {
                var qmObj = new GameObject("QuestManager");
                qmObj.AddComponent<QuestManager>();
            }
            if (FindAnyObjectByType<TouchInputManager>() == null)
            {
                var timObj = new GameObject("TouchInputManager");
                timObj.AddComponent<TouchInputManager>();
            }
            if (FindAnyObjectByType<TutorialManager>() == null)
            {
                var tmObj = new GameObject("TutorialManager");
                tmObj.AddComponent<TutorialManager>();
            }

            // Ensure camera has CameraController2D
            Camera mainCam = Camera.main;
            if (mainCam != null && mainCam.GetComponent<CameraController2D>() == null)
            {
                mainCam.gameObject.AddComponent<CameraController2D>();
            }

            if (FindAnyObjectByType<CraftingManager>() == null)
            {
                var cmObj = new GameObject("CraftingManager");
                cmObj.AddComponent<CraftingManager>();
            }

            if (FindAnyObjectByType<ExpansionManager>() == null)
            {
                var expObj = new GameObject("ExpansionManager");
                expObj.AddComponent<ExpansionManager>();
            }

            EnsureVendorUI();
            EnsureCraftingUI();
            EnsureExpansionUI();
        }

        private void CreateDefaultDataAndObjects()
        {
            // 1. Generate or load crisp procedural pixel sprites for restaurant and market
            Sprite woodFloorSprite = GetOrFallbackSprite("Environment/Tiles/floor_restaurant_wood.png", CreatePixelSprite(32, 32, new Color(0.75f, 0.50f, 0.30f), true));
            Sprite kitchenFloorSprite = GetOrFallbackSprite("Environment/Tiles/floor_kitchen_checker.png", CreatePixelSprite(32, 32, new Color(0.85f, 0.85f, 0.85f), true));
            Sprite wallBorderSprite = GetOrFallbackSprite("Construction/border_restaurant_wall.png", CreatePixelSprite(16, 16, new Color(0.40f, 0.25f, 0.15f), true));
            Sprite shopStallSprite = GetOrFallbackSprite("Exterior/shop_market_stall.png", CreatePixelSprite(48, 48, new Color(0.85f, 0.25f, 0.25f), true));
            Sprite millSprite = GetOrFallbackSprite("Kitchen/station_mill.png", CreatePixelSprite(32, 32, new Color(0.55f, 0.55f, 0.58f), true));

            Sprite tableSprite = GetOrFallbackSprite("Furniture/Tables/table_wood.png", CreatePixelSprite(32, 32, new Color(0.58f, 0.35f, 0.20f), true));
            Sprite chairSprite = GetOrFallbackSprite("Furniture/Chairs/chair_wood.png", CreatePixelSprite(16, 16, new Color(0.70f, 0.45f, 0.25f), true));
            Sprite stoveSprite = GetOrFallbackSprite("Furniture/Kitchen/Stations/station_stove_pro.png", GetOrFallbackSprite("Furniture/Kitchen/stove_kitchen.png", CreatePixelSprite(32, 32, new Color(0.35f, 0.38f, 0.45f), true)));
            Sprite grillSprite = GetOrFallbackSprite("Furniture/Kitchen/Stations/station_grill_iron.png", GetOrFallbackSprite("Furniture/Kitchen/grill_iron.png", CreatePixelSprite(32, 32, new Color(0.45f, 0.28f, 0.25f), true)));
            Sprite counterSprite = GetOrFallbackSprite("Furniture/Kitchen/Stations/station_prep_table_steel.png", GetOrFallbackSprite("Furniture/Counters/counter_delivery.png", CreatePixelSprite(48, 16, new Color(0.82f, 0.62f, 0.42f), true)));
            Sprite cropPlotSprite = GetOrFallbackSprite("Exterior/Crops/crop_plot.png", CreatePixelSprite(32, 32, new Color(0.42f, 0.26f, 0.14f), true));

            Sprite tomatoSprite = GetOrFallbackSprite("Food/InUse/Ingredients/Fruits/strawberry.png", CreatePixelSprite(16, 16, new Color(0.92f, 0.20f, 0.18f), false));
            Sprite lettuceSprite = GetOrFallbackSprite("Food/InUse/Ingredients/Fruits/lemon.png", CreatePixelSprite(16, 16, new Color(0.25f, 0.82f, 0.30f), false));
            Sprite meatSprite = GetOrFallbackSprite("Food/InUse/Ingredients/Purchasable/10_beef.png", CreatePixelSprite(16, 16, new Color(0.80f, 0.30f, 0.28f), false));
            Sprite breadSprite = GetOrFallbackSprite("Food/InUse/Ingredients/Purchasable/65_loafbread.png", CreatePixelSprite(16, 16, new Color(0.90f, 0.75f, 0.45f), false));
            Sprite cheeseSprite = GetOrFallbackSprite("Food/InUse/Ingredients/Purchasable/24_cheese.png", CreatePixelSprite(16, 16, new Color(1.0f, 0.88f, 0.30f), false));

            Sprite burgerSprite = GetOrFallbackSprite("Food/InUse/PreparedDishes/15_burger.png", CreatePixelSprite(24, 24, new Color(0.88f, 0.60f, 0.25f), true));
            Sprite saladSprite = GetOrFallbackSprite("Food/InUse/PreparedDishes/44_frenchfries.png", CreatePixelSprite(24, 24, new Color(0.30f, 0.80f, 0.35f), true));
            Sprite steakSprite = GetOrFallbackSprite("Food/InUse/PreparedDishes/95_steak.png", CreatePixelSprite(24, 24, new Color(0.65f, 0.25f, 0.20f), true));

            Sprite customerSprite = GetOrFallbackSprite("Characters/Customers/customer_normal.png", CreatePixelSprite(24, 32, new Color(0.20f, 0.60f, 0.90f), false));
            Sprite workerSprite = GetOrFallbackSprite("Characters/Workers/worker_helper.png", CreatePixelSprite(24, 32, new Color(0.95f, 0.55f, 0.15f), false));

            // 2. Setup Ingredients
            var tomatoSO = ScriptableObject.CreateInstance<IngredientSO>();
            tomatoSO.ingredientID = "tomato";
            tomatoSO.ingredientName = "Tomate";
            tomatoSO.category = IngredientCategory.Cultivable;
            tomatoSO.icon = tomatoSprite;

            var lettuceSO = ScriptableObject.CreateInstance<IngredientSO>();
            lettuceSO.ingredientID = "lettuce";
            lettuceSO.ingredientName = "Lechuga";
            lettuceSO.category = IngredientCategory.Cultivable;
            lettuceSO.icon = lettuceSprite;

            var meatSO = ScriptableObject.CreateInstance<IngredientSO>();
            meatSO.ingredientID = "meat";
            meatSO.ingredientName = "Carne";
            meatSO.category = IngredientCategory.Comprable;
            meatSO.icon = meatSprite;

            var breadSO = ScriptableObject.CreateInstance<IngredientSO>();
            breadSO.ingredientID = "bread";
            breadSO.ingredientName = "Pan";
            breadSO.category = IngredientCategory.Comprable;
            breadSO.icon = breadSprite;

            var cheeseSO = ScriptableObject.CreateInstance<IngredientSO>();
            cheeseSO.ingredientID = "cheese";
            cheeseSO.ingredientName = "Queso";
            cheeseSO.category = IngredientCategory.Comprable;
            cheeseSO.icon = cheeseSprite;

            // 3. Setup Recipes
            var burgerRecipe = ScriptableObject.CreateInstance<RecipeSO>();
            burgerRecipe.recipeID = "burger";
            burgerRecipe.recipeName = "Hamburguesa Clasica";
            burgerRecipe.requiredStation = StationType.Parrilla;
            burgerRecipe.cookTimeSeconds = 6f; // Snappy for mobile testing
            burgerRecipe.sellPrice = 120;
            burgerRecipe.experienceReward = 20;
            burgerRecipe.icon = burgerSprite;
            burgerRecipe.finishedDishSprite = burgerSprite;
            burgerRecipe.requiredIngredients = new List<IngredientRequirement>
            {
                new IngredientRequirement { ingredient = meatSO, amount = 1 },
                new IngredientRequirement { ingredient = breadSO, amount = 1 },
                new IngredientRequirement { ingredient = tomatoSO, amount = 1 }
            };

            var saladRecipe = ScriptableObject.CreateInstance<RecipeSO>();
            saladRecipe.recipeID = "salad";
            saladRecipe.recipeName = "Ensalada Fresca";
            saladRecipe.requiredStation = StationType.Cocina;
            saladRecipe.cookTimeSeconds = 4f;
            saladRecipe.sellPrice = 80;
            saladRecipe.experienceReward = 15;
            saladRecipe.icon = saladSprite;
            saladRecipe.finishedDishSprite = saladSprite;
            saladRecipe.requiredIngredients = new List<IngredientRequirement>
            {
                new IngredientRequirement { ingredient = lettuceSO, amount = 2 },
                new IngredientRequirement { ingredient = tomatoSO, amount = 1 }
            };

            var steakRecipe = ScriptableObject.CreateInstance<RecipeSO>();
            steakRecipe.recipeID = "steak";
            steakRecipe.recipeName = "Carne Asada";
            steakRecipe.requiredStation = StationType.Parrilla;
            steakRecipe.cookTimeSeconds = 8f;
            steakRecipe.sellPrice = 150;
            steakRecipe.experienceReward = 25;
            steakRecipe.icon = steakSprite;
            steakRecipe.finishedDishSprite = steakSprite;
            steakRecipe.requiredIngredients = new List<IngredientRequirement>
            {
                new IngredientRequirement { ingredient = meatSO, amount = 2 }
            };

            // Register recipes
            if (RecipeManager.Instance != null)
            {
                RecipeManager.Instance.allRecipes = new List<RecipeSO> { burgerRecipe, saladRecipe, steakRecipe };
            }

            // 4. Initial Inventory
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddItem("meat", 10);
                InventoryManager.Instance.AddItem("bread", 10);
                InventoryManager.Instance.AddItem("cheese", 10);
                InventoryManager.Instance.AddItem("tomato", 10);
                InventoryManager.Instance.AddItem("lettuce", 10);
            }

            // 5. Setup Crops
            var tomatoCrop = ScriptableObject.CreateInstance<CropSO>();
            tomatoCrop.cropID = "crop_tomato";
            tomatoCrop.cropName = "Planta de Tomate";
            tomatoCrop.totalGrowthTimeSeconds = 15f; // Fast for cozy mobile testing
            tomatoCrop.harvestIngredient = tomatoSO;
            tomatoCrop.harvestAmount = 3;
            tomatoCrop.seedStageSprite = CreatePixelSprite(16, 16, new Color(0.4f, 0.6f, 0.2f), false);
            tomatoCrop.stage01Sprite = CreatePixelSprite(20, 20, new Color(0.3f, 0.7f, 0.2f), false);
            tomatoCrop.stage02Sprite = CreatePixelSprite(24, 24, new Color(0.2f, 0.8f, 0.2f), false);
            tomatoCrop.readyStageSprite = CreatePixelSprite(28, 28, new Color(0.9f, 0.25f, 0.2f), false);

            var lettuceCrop = ScriptableObject.CreateInstance<CropSO>();
            lettuceCrop.cropID = "crop_lettuce";
            lettuceCrop.cropName = "Lechuga";
            lettuceCrop.totalGrowthTimeSeconds = 12f;
            lettuceCrop.harvestIngredient = lettuceSO;
            lettuceCrop.harvestAmount = 2;
            lettuceCrop.seedStageSprite = tomatoCrop.seedStageSprite;
            lettuceCrop.readyStageSprite = CreatePixelSprite(26, 26, new Color(0.2f, 0.85f, 0.25f), false);

            if (FarmingManager.Instance != null)
            {
                FarmingManager.Instance.allCrops = new List<CropSO> { tomatoCrop, lettuceCrop };
                FarmingManager.Instance.selectedCrop = tomatoCrop;
            }

            // 6. Setup Customer archetype & Worker archetype
            var normalCustomer = ScriptableObject.CreateInstance<CustomerSO>();
            normalCustomer.customerID = "cust_normal";
            normalCustomer.customerTitle = "Comensal Alegre";
            normalCustomer.characterSprite = customerSprite;
            normalCustomer.movementSpeed = 2.8f;
            normalCustomer.basePatienceSeconds = 50f;
            normalCustomer.preferredFoods = new List<RecipeSO> { burgerRecipe, saladRecipe, steakRecipe };

            var impatientCustomer = ScriptableObject.CreateInstance<CustomerSO>();
            impatientCustomer.customerID = "cust_impatient";
            impatientCustomer.customerTitle = "Cliente Apurado";
            impatientCustomer.characterSprite = GetOrFallbackSprite("Characters/Customers/customer_impatient.png", customerSprite);
            impatientCustomer.movementSpeed = 3.5f;
            impatientCustomer.basePatienceSeconds = 30f;
            impatientCustomer.tipProbability = 0.3f;
            impatientCustomer.preferredFoods = new List<RecipeSO> { burgerRecipe, saladRecipe };

            var vipCustomer = ScriptableObject.CreateInstance<CustomerSO>();
            vipCustomer.customerID = "cust_vip";
            vipCustomer.customerTitle = "Crítico VIP";
            vipCustomer.characterSprite = GetOrFallbackSprite("Characters/Customers/customer_vip.png", customerSprite);
            vipCustomer.movementSpeed = 2.4f;
            vipCustomer.basePatienceSeconds = 70f;
            vipCustomer.tipProbability = 0.9f;
            vipCustomer.tipMultiplier = 2.5f;
            vipCustomer.preferredFoods = new List<RecipeSO> { steakRecipe, burgerRecipe };

            if (CustomerManager.Instance != null)
            {
                CustomerManager.Instance.availableCustomerTypes = new List<CustomerSO> { normalCustomer, impatientCustomer, vipCustomer };
            }

            var workerSO = ScriptableObject.CreateInstance<WorkerSO>();
            workerSO.workerID = "worker_01";
            workerSO.workerName = "Tomas el Ayudante";
            workerSO.characterSprite = workerSprite;
            workerSO.movementSpeed = 3.5f;

            // 7. Place Initial Furniture on Grid
            // Stove & Grill in Kitchen (bottom rows)
            var stoveSO = ScriptableObject.CreateInstance<FurnitureSO>();
            stoveSO.furnitureID = "stove_01";
            stoveSO.furnitureName = "Cocina a Gas";
            stoveSO.category = FurnitureCategory.Cocina;
            stoveSO.sizeX = 2;
            stoveSO.sizeY = 2;
            stoveSO.cost = 150;
            stoveSO.shopIcon = stoveSprite;

            var grillSO = ScriptableObject.CreateInstance<FurnitureSO>();
            grillSO.furnitureID = "grill_01";
            grillSO.furnitureName = "Parrilla de Hierro";
            grillSO.category = FurnitureCategory.Cocina;
            grillSO.sizeX = 2;
            grillSO.sizeY = 2;
            grillSO.cost = 200;
            grillSO.shopIcon = grillSprite;

            var counterSO = ScriptableObject.CreateInstance<FurnitureSO>();
            counterSO.furnitureID = "counter_delivery";
            counterSO.furnitureName = "Mesa de Entrega";
            counterSO.category = FurnitureCategory.MesaEntrega;
            counterSO.sizeX = 3;
            counterSO.sizeY = 1;
            counterSO.cost = 100;
            counterSO.shopIcon = counterSprite;

            var tableSO = ScriptableObject.CreateInstance<FurnitureSO>();
            tableSO.furnitureID = "table_wood";
            tableSO.furnitureName = "Mesa de Madera";
            tableSO.category = FurnitureCategory.Mesa;
            tableSO.sizeX = 2;
            tableSO.sizeY = 2;
            tableSO.cost = 80;
            tableSO.shopIcon = tableSprite;
            tableSO.allowedZones = new List<ZoneType> { ZoneType.Dining, ZoneType.Terrace };

            var chairSO = ScriptableObject.CreateInstance<FurnitureSO>();
            chairSO.furnitureID = "chair_wood";
            chairSO.furnitureName = "Silla de Madera";
            chairSO.category = FurnitureCategory.Silla;
            chairSO.sizeX = 1;
            chairSO.sizeY = 1;
            chairSO.cost = 30;
            chairSO.shopIcon = chairSprite;
            chairSO.allowedZones = new List<ZoneType> { ZoneType.Dining, ZoneType.Terrace };

            var plotSO = ScriptableObject.CreateInstance<FurnitureSO>();
            plotSO.furnitureID = "crop_plot";
            plotSO.furnitureName = "Sembradero";
            plotSO.category = FurnitureCategory.Sembradero;
            plotSO.sizeX = 2;
            plotSO.sizeY = 2;
            plotSO.cost = 50;
            plotSO.shopIcon = cropPlotSprite;
            plotSO.allowedZones = new List<ZoneType> { ZoneType.Exterior, ZoneType.Farming };

            if (BuildUI.Instance != null)
            {
                BuildUI.Instance.catalogItems = new List<FurnitureSO> { stoveSO, grillSO, counterSO, tableSO, chairSO, plotSO };
                BuildUI.Instance.PopulateCatalog();
            }

            // 7. Create Restaurant Architectural Floors and Borders
            CreateRestaurantFloors(woodFloorSprite, kitchenFloorSprite, wallBorderSprite);

            // 8. Place Initial Furniture on Valid Grid Coordinates (x: 0..31, y: 0..23)
            // Cocina a Gas (size: 2x2) in kitchen area
            SpawnCookingStation(stoveSO, StationType.Cocina, new Vector2Int(10, 2), stoveSprite);

            // Parrilla de Hierro (size: 2x2) in kitchen area
            SpawnCookingStation(grillSO, StationType.Parrilla, new Vector2Int(13, 2), grillSprite);

            // Mesa de Entrega / Despacho (size: 3x1) dividing kitchen and dining
            SpawnDeliveryCounter(counterSO, new Vector2Int(17, 3), counterSprite);

            // 4 Tables with Chairs in Dining Area (x: 8..23, y: 7..15)
            // Row 1 (y: 8)
            SpawnTableWithChairs(tableSO, chairSO, new Vector2Int(10, 8), tableSprite, chairSprite);
            SpawnTableWithChairs(tableSO, chairSO, new Vector2Int(18, 8), tableSprite, chairSprite);

            // Row 2 (y: 12)
            SpawnTableWithChairs(tableSO, chairSO, new Vector2Int(10, 12), tableSprite, chairSprite);
            SpawnTableWithChairs(tableSO, chairSO, new Vector2Int(18, 12), tableSprite, chairSprite);

            // Exterior Garden Area: 3 Crop Plots in garden
            SpawnCropPlot(plotSO, new Vector2Int(16, 18), cropPlotSprite, tomatoCrop);
            SpawnCropPlot(plotSO, new Vector2Int(20, 18), cropPlotSprite, lettuceCrop);
            SpawnCropPlot(plotSO, new Vector2Int(24, 18), cropPlotSprite, null);

            // Physical Merchant Shop Stall ("Tienda del Proveedor") in garden exterior
            SpawnMerchantStall(new Vector2Int(9, 18), shopStallSprite);

            // Crafting Station: Molino de Grano next to the garden
            SpawnCraftingStation(CraftingStationType.Molino, "Molino de Grano", new Vector2Int(13, 18), millSprite);

            // Helper Worker inside the kitchen
            if (WorkerManager.Instance != null)
            {
                WorkerManager.Instance.SpawnWorker(workerSO, new Vector2Int(16, 4));
            }

            // Spawn Villa Expansion Signs
            SpawnInitialExpansionSigns();

            Debug.Log("[RestaurantBootstrap] ¡Restaurante, pisos, tienda física y expansiones configurados con éxito!");
        }

        private void SpawnCookingStation(FurnitureSO so, StationType type, Vector2Int gridPos, Sprite sprite)
        {
            GameObject go = new GameObject($"Station_{so.furnitureName}");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;
            go.AddComponent<BoxCollider2D>().size = new Vector2(so.sizeX, so.sizeY);

            CookingStation station = go.AddComponent<CookingStation>();
            station.stationType = type;
            station.Setup(so, gridPos);

            BuildManager.Instance?.activeFurniture.Add(station);
        }

        private void SpawnDeliveryCounter(FurnitureSO so, Vector2Int gridPos, Sprite sprite)
        {
            GameObject go = new GameObject("DeliveryCounter");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 2;
            go.AddComponent<BoxCollider2D>().size = new Vector2(so.sizeX, so.sizeY);

            DeliveryCounter counter = go.AddComponent<DeliveryCounter>();
            counter.Setup(so, gridPos);

            BuildManager.Instance?.activeFurniture.Add(counter);
        }

        private void SpawnTableWithChairs(FurnitureSO tableSO, FurnitureSO chairSO, Vector2Int gridPos, Sprite tableSprite, Sprite chairSprite)
        {
            GameObject tableGO = new GameObject($"Table_{gridPos.x}_{gridPos.y}");
            SpriteRenderer sr = tableGO.AddComponent<SpriteRenderer>();
            sr.sprite = tableSprite;
            sr.sortingOrder = 2;
            tableGO.AddComponent<BoxCollider2D>().size = new Vector2(tableSO.sizeX, tableSO.sizeY);

            Table table = tableGO.AddComponent<Table>();
            table.Setup(tableSO, gridPos);
            BuildManager.Instance?.activeFurniture.Add(table);

            // Left chair
            Vector2Int leftChairPos = new Vector2Int(gridPos.x - 1, gridPos.y);
            GameObject cLeft = new GameObject("Chair_Left");
            cLeft.transform.SetParent(tableGO.transform);
            SpriteRenderer srL = cLeft.AddComponent<SpriteRenderer>();
            srL.sprite = chairSprite;
            srL.sortingOrder = 1;
            Chair chair1 = cLeft.AddComponent<Chair>();
            chair1.attachedTable = table;
            chair1.Setup(chairSO, leftChairPos);
            table.chairs.Add(chair1);

            // Right chair
            Vector2Int rightChairPos = new Vector2Int(gridPos.x + tableSO.sizeX, gridPos.y);
            GameObject cRight = new GameObject("Chair_Right");
            cRight.transform.SetParent(tableGO.transform);
            SpriteRenderer srR = cRight.AddComponent<SpriteRenderer>();
            srR.sprite = chairSprite;
            srR.sortingOrder = 1;
            Chair chair2 = cRight.AddComponent<Chair>();
            chair2.attachedTable = table;
            chair2.Setup(chairSO, rightChairPos);
            table.chairs.Add(chair2);
        }

        private void SpawnCropPlot(FurnitureSO so, Vector2Int gridPos, Sprite sprite, CropSO initialCrop)
        {
            GameObject go = new GameObject($"CropPlot_{gridPos.x}_{gridPos.y}");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 1;
            go.AddComponent<BoxCollider2D>().size = new Vector2(so.sizeX, so.sizeY);

            // Plant visual layer
            GameObject plantLayer = new GameObject("PlantVisual");
            plantLayer.transform.SetParent(go.transform);
            plantLayer.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            SpriteRenderer plantSR = plantLayer.AddComponent<SpriteRenderer>();
            plantSR.sortingOrder = 3;

            CropPlot plot = go.AddComponent<CropPlot>();
            plot.plotBaseRenderer = sr;
            plot.plantRenderer = plantSR;
            plot.Setup(so, gridPos);

            if (initialCrop != null)
            {
                plot.Plant(initialCrop);
            }

            BuildManager.Instance?.activeFurniture.Add(plot);
            FarmingManager.Instance?.RegisterPlot(plot);
        }

        private Sprite GetOrFallbackSprite(string subpath, Sprite fallback)
        {
#if UNITY_EDITOR
            string fullPath = "Assets/_Projet/Art/" + subpath;
            Sprite loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
            if (loaded != null) return loaded;
#endif
            return fallback;
        }

        private Sprite CreatePixelSprite(int width, int height, Color color, bool hasBorder)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color borderColor = color * 0.7f;
            borderColor.a = 1f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (hasBorder && (x == 0 || x == width - 1 || y == 0 || y == height - 1))
                    {
                        texture.SetPixel(x, y, borderColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, color);
                    }
                }
            }
            texture.Apply();

            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 16f);
        }

        private void CreateRestaurantFloors(Sprite woodSprite, Sprite checkerSprite, Sprite wallSprite)
        {
            GameObject floorRoot = new GameObject("--- RESTAURANT STRUCTURE & FLOORS ---");

            // Kitchen Floor (grid x: 8..23, y: 1..6 -> width: 16, height: 6)
            Vector3 kitchenCenter = GridManager.Instance != null 
                ? GridManager.Instance.GridToWorld(new Vector2Int(8, 1), 16, 6) 
                : new Vector3(0f, -8.5f, 0f);
            GameObject kitchenFloorGO = new GameObject("Floor_Kitchen");
            kitchenFloorGO.transform.SetParent(floorRoot.transform);
            kitchenFloorGO.transform.position = kitchenCenter;
            SpriteRenderer kSR = kitchenFloorGO.AddComponent<SpriteRenderer>();
            kSR.sprite = checkerSprite;
            kSR.drawMode = SpriteDrawMode.Tiled;
            kSR.size = new Vector2(16f, 6f);
            kSR.sortingOrder = -50;

            // Dining Floor (grid x: 8..23, y: 7..15 -> width: 16, height: 9)
            Vector3 diningCenter = GridManager.Instance != null 
                ? GridManager.Instance.GridToWorld(new Vector2Int(8, 7), 16, 9) 
                : new Vector3(0f, -1f, 0f);
            GameObject diningFloorGO = new GameObject("Floor_Dining");
            diningFloorGO.transform.SetParent(floorRoot.transform);
            diningFloorGO.transform.position = diningCenter;
            SpriteRenderer dSR = diningFloorGO.AddComponent<SpriteRenderer>();
            dSR.sprite = woodSprite;
            dSR.drawMode = SpriteDrawMode.Tiled;
            dSR.size = new Vector2(16f, 9f);
            dSR.sortingOrder = -50;

            // Boundary walls / decorative wooden wainscoting
            if (wallSprite != null)
            {
                // Left Border Wall
                Vector3 leftWallPos = GridManager.Instance != null ? GridManager.Instance.GridToWorld(new Vector2Int(7, 1), 1, 15) : new Vector3(-8.5f, -4f, 0f);
                GameObject leftWall = new GameObject("Wall_Left");
                leftWall.transform.SetParent(floorRoot.transform);
                leftWall.transform.position = leftWallPos;
                SpriteRenderer lWSR = leftWall.AddComponent<SpriteRenderer>();
                lWSR.sprite = wallSprite;
                lWSR.drawMode = SpriteDrawMode.Tiled;
                lWSR.size = new Vector2(1f, 15f);
                lWSR.sortingOrder = -40;

                // Right Border Wall
                Vector3 rightWallPos = GridManager.Instance != null ? GridManager.Instance.GridToWorld(new Vector2Int(24, 1), 1, 15) : new Vector3(8.5f, -4f, 0f);
                GameObject rightWall = new GameObject("Wall_Right");
                rightWall.transform.SetParent(floorRoot.transform);
                rightWall.transform.position = rightWallPos;
                SpriteRenderer rWSR = rightWall.AddComponent<SpriteRenderer>();
                rWSR.sprite = wallSprite;
                rWSR.drawMode = SpriteDrawMode.Tiled;
                rWSR.size = new Vector2(1f, 15f);
                rWSR.sortingOrder = -40;

                // Bottom Border Wall
                Vector3 bottomWallPos = GridManager.Instance != null ? GridManager.Instance.GridToWorld(new Vector2Int(7, 0), 18, 1) : new Vector3(0f, -11.5f, 0f);
                GameObject bottomWall = new GameObject("Wall_Bottom");
                bottomWall.transform.SetParent(floorRoot.transform);
                bottomWall.transform.position = bottomWallPos;
                SpriteRenderer bWSR = bottomWall.AddComponent<SpriteRenderer>();
                bWSR.sprite = wallSprite;
                bWSR.drawMode = SpriteDrawMode.Tiled;
                bWSR.size = new Vector2(18f, 1f);
                bWSR.sortingOrder = -40;

                // Top Partition Walls with doorway (Entrance between dining and garden)
                Vector3 topWallLeftPos = GridManager.Instance != null ? GridManager.Instance.GridToWorld(new Vector2Int(7, 16), 7, 1) : new Vector3(-5f, 4.5f, 0f);
                GameObject topWallL = new GameObject("Wall_Top_Left");
                topWallL.transform.SetParent(floorRoot.transform);
                topWallL.transform.position = topWallLeftPos;
                SpriteRenderer tWLSR = topWallL.AddComponent<SpriteRenderer>();
                tWLSR.sprite = wallSprite;
                tWLSR.drawMode = SpriteDrawMode.Tiled;
                tWLSR.size = new Vector2(7f, 1f);
                tWLSR.sortingOrder = -40;

                Vector3 topWallRightPos = GridManager.Instance != null ? GridManager.Instance.GridToWorld(new Vector2Int(18, 16), 7, 1) : new Vector3(5f, 4.5f, 0f);
                GameObject topWallR = new GameObject("Wall_Top_Right");
                topWallR.transform.SetParent(floorRoot.transform);
                topWallR.transform.position = topWallRightPos;
                SpriteRenderer tWRSR = topWallR.AddComponent<SpriteRenderer>();
                tWRSR.sprite = wallSprite;
                tWRSR.drawMode = SpriteDrawMode.Tiled;
                tWRSR.size = new Vector2(7f, 1f);
                tWRSR.sortingOrder = -40;
            }
        }

        private void SpawnMerchantStall(Vector2Int gridPos, Sprite stallSprite)
        {
            GameObject stallGO = new GameObject("MerchantStall_Physical");
            Vector3 worldPos = GridManager.Instance != null 
                ? GridManager.Instance.GridToWorld(gridPos, 3, 2) 
                : new Vector3(-6.5f, 6.5f, 0f);
            stallGO.transform.position = worldPos;

            SpriteRenderer sr = stallGO.AddComponent<SpriteRenderer>();
            sr.sprite = stallSprite;
            sr.sortingOrder = 5;

            BoxCollider2D col = stallGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(3f, 2.5f);
            col.offset = new Vector2(0f, 0.2f);

            MerchantStall stall = stallGO.AddComponent<MerchantStall>();
            stall.stallRenderer = sr;

            // Floating sign above stall
            GameObject signGO = new GameObject("Stall_Sign");
            signGO.transform.SetParent(stallGO.transform, false);
            signGO.transform.localPosition = new Vector3(0f, 1.8f, 0f);

            TextMesh tm = signGO.AddComponent<TextMesh>();
            tm.text = "🛒 TIENDA\n[Tocar]";
            tm.characterSize = 0.16f;
            tm.fontSize = 28;
            tm.alignment = TextAlignment.Center;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = new Color(1f, 0.95f, 0.35f);
            MeshRenderer mr = signGO.GetComponent<MeshRenderer>();
            mr.sortingOrder = 20;

            stall.floatingIndicator = signGO;

            // Spawn NPC Vendor next to the physical stall
            NPCSO elenaData = Resources.Load<NPCSO>("NPC/npc_elena");
            if (elenaData == null)
            {
                var allNPCs = Resources.LoadAll<NPCSO>("NPC");
                if (allNPCs != null && allNPCs.Length > 0) elenaData = allNPCs[0];
            }

            if (elenaData != null)
            {
                GameObject npcGO = new GameObject($"NPC_{elenaData.npcName}");
                npcGO.transform.position = worldPos + new Vector3(-0.6f, -0.6f, 0f);

                SpriteRenderer npcSR = npcGO.AddComponent<SpriteRenderer>();
                npcSR.sortingOrder = 7;
                npcSR.sprite = elenaData.worldSprite ?? elenaData.portrait;

                BoxCollider2D npcCol = npcGO.AddComponent<BoxCollider2D>();
                npcCol.size = new Vector2(1f, 1.4f);

                NPCController npcCtrl = npcGO.AddComponent<NPCController>();
                npcCtrl.npcData = elenaData;
                npcCtrl.characterRenderer = npcSR;
                stall.associatedNPC = npcCtrl;
            }

            if (GridManager.Instance != null)
            {
                GridManager.Instance.SetOccupancy(gridPos.x, gridPos.y, 3, 2, null, true);
            }
        }

        private void EnsureVendorUI()
        {
            if (VendorUI.Instance == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject vModal = new GameObject("VendorModal_Bootstrap", typeof(RectTransform), typeof(Image), typeof(VendorUI));
                    vModal.transform.SetParent(canvas.transform, false);
                    RectTransform vRT = vModal.GetComponent<RectTransform>();
                    vRT.anchorMin = new Vector2(0.5f, 0.5f);
                    vRT.anchorMax = new Vector2(0.5f, 0.5f);
                    vRT.sizeDelta = new Vector2(700, 520);

                    Image img = vModal.GetComponent<Image>();
                    img.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

                    VendorUI vUI = vModal.GetComponent<VendorUI>();
                    vUI.panelRoot = vModal;

                    // Title
                    GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
                    titleObj.transform.SetParent(vModal.transform, false);
                    Text titleT = titleObj.GetComponent<Text>();
                    titleT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                    titleT.fontSize = 24;
                    titleT.fontStyle = FontStyle.Bold;
                    titleT.color = new Color(1f, 0.85f, 0.2f);
                    titleT.text = "Puesto del Proveedor";
                    RectTransform titleRt = titleObj.GetComponent<RectTransform>();
                    titleRt.anchorMin = new Vector2(0.5f, 1f);
                    titleRt.anchorMax = new Vector2(0.5f, 1f);
                    titleRt.anchoredPosition = new Vector2(0, -35);
                    titleRt.sizeDelta = new Vector2(400, 40);
                    vUI.npcNameText = titleT;

                    // Close Button
                    GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                    closeBtnObj.transform.SetParent(vModal.transform, false);
                    RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
                    closeRt.anchorMin = new Vector2(1f, 1f);
                    closeRt.anchorMax = new Vector2(1f, 1f);
                    closeRt.anchoredPosition = new Vector2(-30, -30);
                    closeRt.sizeDelta = new Vector2(40, 40);
                    closeBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.25f, 0.25f);
                    vUI.closeButton = closeBtnObj.GetComponent<Button>();

                    // Items Container
                    GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
                    contentObj.transform.SetParent(vModal.transform, false);
                    RectTransform contentRt = contentObj.GetComponent<RectTransform>();
                    contentRt.anchorMin = new Vector2(0.05f, 0.05f);
                    contentRt.anchorMax = new Vector2(0.95f, 0.82f);
                    contentRt.offsetMin = Vector2.zero;
                    contentRt.offsetMax = Vector2.zero;

                    var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
                    vlg.spacing = 8;
                    vUI.itemsContainer = contentObj.transform;

                    vModal.SetActive(false);
                }
            }
        }

        private void SpawnCraftingStation(CraftingStationType type, string name, Vector2Int gridPos, Sprite sprite)
        {
            GameObject go = new GameObject($"CraftStation_{name}");
            SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 3;
            var col = go.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2f, 2f);

            var stationSO = ScriptableObject.CreateInstance<FurnitureSO>();
            stationSO.furnitureID = $"station_{type.ToString().ToLower()}";
            stationSO.furnitureName = name;
            stationSO.category = FurnitureCategory.EstacionCrafting;
            stationSO.sizeX = 2;
            stationSO.sizeY = 2;

            CraftingStation station = go.AddComponent<CraftingStation>();
            station.stationType = type;
            station.stationName = name;
            station.stationRenderer = sr;
            station.Setup(stationSO, gridPos);

            // Floating ready indicator
            GameObject indicatorGO = new GameObject("ReadyIndicator");
            indicatorGO.transform.SetParent(go.transform, false);
            indicatorGO.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            SpriteRenderer indSR = indicatorGO.AddComponent<SpriteRenderer>();
            indSR.sortingOrder = 20;
            indicatorGO.SetActive(false);
            station.readyIndicator = indSR;

            BuildManager.Instance?.activeFurniture.Add(station);
            CraftingManager.Instance?.RegisterStation(station);
        }

        private void EnsureCraftingUI()
        {
            if (CraftingUI.Instance == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject cModal = new GameObject("CraftingModal_Bootstrap", typeof(RectTransform), typeof(Image), typeof(CraftingUI));
                    cModal.transform.SetParent(canvas.transform, false);
                    RectTransform cRT = cModal.GetComponent<RectTransform>();
                    cRT.anchorMin = new Vector2(0.5f, 0.5f);
                    cRT.anchorMax = new Vector2(0.5f, 0.5f);
                    cRT.sizeDelta = new Vector2(700, 520);

                    Image img = cModal.GetComponent<Image>();
                    img.color = new Color(0.13f, 0.12f, 0.18f, 0.98f);

                    CraftingUI cUI = cModal.GetComponent<CraftingUI>();
                    cUI.panelRoot = cModal;

                    // Title
                    GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
                    titleObj.transform.SetParent(cModal.transform, false);
                    Text titleT = titleObj.GetComponent<Text>();
                    titleT.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");
                    titleT.fontSize = 24;
                    titleT.fontStyle = FontStyle.Bold;
                    titleT.color = new Color(1f, 0.85f, 0.2f);
                    titleT.text = "Estación de Elaboración";
                    RectTransform titleRt = titleObj.GetComponent<RectTransform>();
                    titleRt.anchorMin = new Vector2(0.5f, 1f);
                    titleRt.anchorMax = new Vector2(0.5f, 1f);
                    titleRt.anchoredPosition = new Vector2(0, -35);
                    titleRt.sizeDelta = new Vector2(400, 40);
                    cUI.titleText = titleT;

                    // Close Button
                    GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                    closeBtnObj.transform.SetParent(cModal.transform, false);
                    RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
                    closeRt.anchorMin = new Vector2(1f, 1f);
                    closeRt.anchorMax = new Vector2(1f, 1f);
                    closeRt.anchoredPosition = new Vector2(-30, -30);
                    closeRt.sizeDelta = new Vector2(40, 40);
                    closeBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.25f, 0.25f);
                    cUI.closeButton = closeBtnObj.GetComponent<Button>();

                    // Items Container
                    GameObject contentObj = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
                    contentObj.transform.SetParent(cModal.transform, false);
                    RectTransform contentRt = contentObj.GetComponent<RectTransform>();
                    contentRt.anchorMin = new Vector2(0.05f, 0.05f);
                    contentRt.anchorMax = new Vector2(0.95f, 0.82f);
                    contentRt.offsetMin = Vector2.zero;
                    contentRt.offsetMax = Vector2.zero;

                    var vlg = contentObj.GetComponent<VerticalLayoutGroup>();
                    vlg.spacing = 8;
                    cUI.recipesContainer = contentObj.transform;

                    cModal.SetActive(false);
                }
            }
        }

        private void SpawnInitialExpansionSigns()
        {
            Sprite signSprite = GetOrFallbackSprite("Environment/sign_for_sale.png", CreatePixelSprite(16, 24, new Color(0.75f, 0.55f, 0.25f), false));
            var expansions = Resources.LoadAll<ExpansionSO>("Expansions");

            foreach (var exp in expansions)
            {
                if (exp == null) continue;
                if (ExpansionManager.Instance != null && ExpansionManager.Instance.IsExpansionUnlocked(exp.expansionID))
                {
                    continue;
                }

                // Check if sign already exists in scene
                string signName = $"ExpansionSign_{exp.expansionID}";
                if (GameObject.Find(signName) == null)
                {
                    SpawnExpansionSign(exp, signSprite);
                }
            }
        }

        private void SpawnExpansionSign(ExpansionSO exp, Sprite signSprite)
        {
            GameObject signGO = new GameObject($"ExpansionSign_{exp.expansionID}");
            signGO.transform.position = new Vector3(exp.worldSignPosition.x, exp.worldSignPosition.y, 0f);

            SpriteRenderer sr = signGO.AddComponent<SpriteRenderer>();
            sr.sprite = signSprite;
            sr.sortingOrder = 5;

            BoxCollider2D col = signGO.AddComponent<BoxCollider2D>();
            col.size = new Vector2(1.2f, 1.6f);
            col.isTrigger = true;

            ExpansionSign sign = signGO.AddComponent<ExpansionSign>();
            sign.expansionData = exp;
            sign.signRenderer = sr;

            // Add subtle floating gold star
            GameObject starGO = new GameObject("FloatingIcon");
            starGO.transform.SetParent(signGO.transform, false);
            starGO.transform.localPosition = new Vector3(0f, 1.2f, 0f);
            SpriteRenderer starSR = starGO.AddComponent<SpriteRenderer>();
            starSR.sprite = GetOrFallbackSprite("UI/star.png", CreatePixelSprite(16, 16, new Color(1f, 0.85f, 0.2f), false));
            starSR.sortingOrder = 10;
            sign.floatingIcon = starSR;

            if (ExpansionManager.Instance != null)
            {
                ExpansionManager.Instance.RegisterSign(sign);
            }
        }

        private void EnsureExpansionUI()
        {
            if (Object.FindAnyObjectByType<ExpansionUI>(FindObjectsInactive.Include) == null)
            {
                Canvas canvas = FindAnyObjectByType<Canvas>();
                if (canvas != null)
                {
                    GameObject expModal = new GameObject("ExpansionModal_Bootstrap", typeof(RectTransform), typeof(Image), typeof(ExpansionUI));
                    expModal.transform.SetParent(canvas.transform, false);
                    RectTransform expRT = expModal.GetComponent<RectTransform>();
                    expRT.anchorMin = new Vector2(0.5f, 0.5f);
                    expRT.anchorMax = new Vector2(0.5f, 0.5f);
                    expRT.sizeDelta = new Vector2(620, 460);

                    Image img = expModal.GetComponent<Image>();
                    img.color = new Color(0.12f, 0.14f, 0.20f, 0.98f);

                    ExpansionUI expUI = expModal.GetComponent<ExpansionUI>();
                    expUI.panelRoot = expModal;

                    Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

                    // Title
                    GameObject titleObj = new GameObject("Title", typeof(RectTransform), typeof(Text));
                    titleObj.transform.SetParent(expModal.transform, false);
                    Text titleT = titleObj.GetComponent<Text>();
                    titleT.font = defaultFont;
                    titleT.fontSize = 24;
                    titleT.fontStyle = FontStyle.Bold;
                    titleT.color = new Color(1f, 0.85f, 0.25f);
                    titleT.alignment = TextAnchor.MiddleCenter;
                    titleT.text = "Expansión de la Villa";
                    RectTransform titleRt = titleObj.GetComponent<RectTransform>();
                    titleRt.anchorMin = new Vector2(0.5f, 1f);
                    titleRt.anchorMax = new Vector2(0.5f, 1f);
                    titleRt.anchoredPosition = new Vector2(0, -40);
                    titleRt.sizeDelta = new Vector2(480, 40);
                    expUI.titleText = titleT;

                    // Close Button
                    GameObject closeBtnObj = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                    closeBtnObj.transform.SetParent(expModal.transform, false);
                    RectTransform closeRt = closeBtnObj.GetComponent<RectTransform>();
                    closeRt.anchorMin = new Vector2(1f, 1f);
                    closeRt.anchorMax = new Vector2(1f, 1f);
                    closeRt.anchoredPosition = new Vector2(-28, -28);
                    closeRt.sizeDelta = new Vector2(40, 40);
                    closeBtnObj.GetComponent<Image>().color = new Color(0.85f, 0.25f, 0.25f);
                    expUI.closeButton = closeBtnObj.GetComponent<Button>();

                    // Description
                    GameObject descObj = new GameObject("Desc", typeof(RectTransform), typeof(Text));
                    descObj.transform.SetParent(expModal.transform, false);
                    Text descT = descObj.GetComponent<Text>();
                    descT.font = defaultFont;
                    descT.fontSize = 17;
                    descT.color = new Color(0.9f, 0.92f, 0.95f);
                    descT.alignment = TextAnchor.MiddleCenter;
                    descT.text = "Desbloquea este nuevo terreno para expandir tu villa.";
                    RectTransform descRt = descObj.GetComponent<RectTransform>();
                    descRt.anchorMin = new Vector2(0.5f, 0.5f);
                    descRt.anchorMax = new Vector2(0.5f, 0.5f);
                    descRt.anchoredPosition = new Vector2(0, 30);
                    descRt.sizeDelta = new Vector2(500, 70);
                    expUI.descriptionText = descT;

                    // Requirements (Level & Cost)
                    GameObject levelObj = new GameObject("LevelReq", typeof(RectTransform), typeof(Text));
                    levelObj.transform.SetParent(expModal.transform, false);
                    Text levelT = levelObj.GetComponent<Text>();
                    levelT.font = defaultFont;
                    levelT.fontSize = 17;
                    levelT.alignment = TextAnchor.MiddleCenter;
                    RectTransform levelRt = levelObj.GetComponent<RectTransform>();
                    levelRt.anchorMin = new Vector2(0.5f, 0.5f);
                    levelRt.anchorMax = new Vector2(0.5f, 0.5f);
                    levelRt.anchoredPosition = new Vector2(0, -40);
                    levelRt.sizeDelta = new Vector2(450, 30);
                    expUI.levelRequirementText = levelT;

                    GameObject costObj = new GameObject("CostText", typeof(RectTransform), typeof(Text));
                    costObj.transform.SetParent(expModal.transform, false);
                    Text costT = costObj.GetComponent<Text>();
                    costT.font = defaultFont;
                    costT.fontSize = 18;
                    costT.fontStyle = FontStyle.Bold;
                    costT.alignment = TextAnchor.MiddleCenter;
                    RectTransform costRt = costObj.GetComponent<RectTransform>();
                    costRt.anchorMin = new Vector2(0.5f, 0.5f);
                    costRt.anchorMax = new Vector2(0.5f, 0.5f);
                    costRt.anchoredPosition = new Vector2(0, -80);
                    costRt.sizeDelta = new Vector2(450, 35);
                    expUI.costText = costT;

                    // Unlock Button
                    GameObject unlockBtnObj = new GameObject("UnlockBtn", typeof(RectTransform), typeof(Image), typeof(Button));
                    unlockBtnObj.transform.SetParent(expModal.transform, false);
                    RectTransform unlockRt = unlockBtnObj.GetComponent<RectTransform>();
                    unlockRt.anchorMin = new Vector2(0.5f, 0f);
                    unlockRt.anchorMax = new Vector2(0.5f, 0f);
                    unlockRt.anchoredPosition = new Vector2(0, 50);
                    unlockRt.sizeDelta = new Vector2(240, 52);
                    unlockBtnObj.GetComponent<Image>().color = new Color(0.25f, 0.75f, 0.35f);
                    expUI.unlockButton = unlockBtnObj.GetComponent<Button>();

                    GameObject btnTxtObj = new GameObject("BtnText", typeof(RectTransform), typeof(Text));
                    btnTxtObj.transform.SetParent(unlockBtnObj.transform, false);
                    Text btnT = btnTxtObj.GetComponent<Text>();
                    btnT.font = defaultFont;
                    btnT.fontSize = 18;
                    btnT.fontStyle = FontStyle.Bold;
                    btnT.color = Color.white;
                    btnT.alignment = TextAnchor.MiddleCenter;
                    btnT.text = "¡Desbloquear Zona!";
                    RectTransform btnTxtRt = btnTxtObj.GetComponent<RectTransform>();
                    btnTxtRt.anchorMin = Vector2.zero;
                    btnTxtRt.anchorMax = Vector2.one;
                    btnTxtRt.offsetMin = Vector2.zero;
                    btnTxtRt.offsetMax = Vector2.zero;
                    expUI.unlockButtonText = btnT;

                    expModal.SetActive(false);
                }
            }
        }
    }
}
