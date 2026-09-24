#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.Core;
using VillaDelChef.Economy;
using VillaDelChef.Inventory;
using VillaDelChef.Managers;
using VillaDelChef.PlayerInput;
using VillaDelChef.Progression;
using VillaDelChef.Save;
using VillaDelChef.UI;

namespace VillaDelChef.EditorTools
{
    public static class RestaurantSceneSetupEditor
    {
        [InitializeOnLoadMethod]
        private static void AutoSetupScenesOnEditorLoad()
        {
            EditorApplication.delayCall += () =>
            {
                SetPlayModeStartSceneToBoot();

                // Only generate scenes if they are completely missing on disk
                if (!System.IO.File.Exists("Assets/_Projet/Scenes/00_Boot.unity") ||
                    !System.IO.File.Exists("Assets/_Projet/Scenes/01_MainMenu.unity") ||
                    !System.IO.File.Exists("Assets/_Projet/Scenes/02_Restaurant.unity") ||
                    !System.IO.File.Exists("Assets/_Projet/Scenes/03_Prologue.unity"))
                {
                    Debug.Log("[RestaurantSceneSetupEditor] Escenas faltantes en disco. Inicializando configuración automática...");
                    SetupAllScenes();
                }
            };
        }

        public static void SetPlayModeStartSceneToBoot()
        {
            var bootSceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/_Projet/Scenes/00_Boot.unity");
            if (bootSceneAsset != null)
            {
                EditorSceneManager.playModeStartScene = bootSceneAsset;
            }
        }

        [MenuItem("Tools/Villa del Chef/Setup ALL Scenes (Boot, Menu, Restaurant, Prologue)", false, 0)]
        public static void SetupAllScenes()
        {
            if (!System.IO.Directory.Exists("Assets/_Projet/Scenes"))
            {
                System.IO.Directory.CreateDirectory("Assets/_Projet/Scenes");
            }

            // 0. Generate procedural floor/stall pixel art and configure ScriptableObjects
            ArtAssetGenerator.GenerateAllSprites();
            PixelArtAssetPostprocessor.ConfigureAllExistingSprites();
            AssetDatabasePopulator.GenerateAllScriptableObjects();

            SetupBootScene();
            SetupMainMenuScene();
            SetupRestaurantScene();
            SetupPrologueScene();

            // Set all 4 in Build Settings
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene("Assets/_Projet/Scenes/00_Boot.unity", true),
                new EditorBuildSettingsScene("Assets/_Projet/Scenes/01_MainMenu.unity", true),
                new EditorBuildSettingsScene("Assets/_Projet/Scenes/02_Restaurant.unity", true),
                new EditorBuildSettingsScene("Assets/_Projet/Scenes/03_Prologue.unity", true)
            };

            SetPlayModeStartSceneToBoot();

            Debug.Log("[RestaurantSceneSetupEditor] ¡Las 4 escenas (00_Boot, 01_MainMenu, 02_Restaurant, 03_Prologue) han sido generadas y registradas con éxito!");
        }

        [MenuItem("Tools/Villa del Chef/Setup Restaurant MVP Scene", false, 1)]
        public static void SetupRestaurantScene()
        {
            // 0. Ensure all sprites are point filtered and ScriptableObjects populated
            ArtAssetGenerator.GenerateAllSprites();
            PixelArtAssetPostprocessor.ConfigureAllExistingSprites();
            AssetDatabasePopulator.GenerateAllScriptableObjects();

            // 1. Create or Open 02_Restaurant scene
            string scenePath = "Assets/_Projet/Scenes/02_Restaurant.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 2. Setup Camera
            GameObject camGO = new GameObject("Main Camera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 9f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.16f, 0.14f);
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 100f;
            camGO.tag = "MainCamera";
            camGO.transform.position = new Vector3(0f, 4f, -10f);
            camGO.AddComponent<AudioListener>();
            CameraController2D camCtrl = camGO.AddComponent<CameraController2D>();
            camCtrl.targetCamera = cam;
            camCtrl.minBounds = new Vector2(-25f, -18f);
            camCtrl.maxBounds = new Vector2(25f, 25f);

            // 3. Setup World Scenery Background
            SetupWorldBackground();

            // 4. Setup EventSystem
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // 5. Setup Canvas
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Safe Area Root
            GameObject safeAreaGO = new GameObject("SafeAreaContainer");
            safeAreaGO.transform.SetParent(canvasGO.transform, false);
            RectTransform safeAreaRT = safeAreaGO.AddComponent<RectTransform>();
            safeAreaRT.anchorMin = Vector2.zero;
            safeAreaRT.anchorMax = Vector2.one;
            safeAreaRT.offsetMin = Vector2.zero;
            safeAreaRT.offsetMax = Vector2.zero;
            safeAreaGO.AddComponent<SafeAreaFitter>();

            // HUD Root
            GameObject hudGO = new GameObject("HUD");
            hudGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform hudRT = hudGO.AddComponent<RectTransform>();
            hudRT.anchorMin = Vector2.zero;
            hudRT.anchorMax = Vector2.one;
            hudRT.offsetMin = Vector2.zero;
            hudRT.offsetMax = Vector2.zero;
            HUDController hud = hudGO.AddComponent<HUDController>();

            // Top Bar
            GameObject topBarGO = new GameObject("TopBar");
            topBarGO.transform.SetParent(hudGO.transform, false);
            RectTransform topBarRT = topBarGO.AddComponent<RectTransform>();
            topBarRT.anchorMin = new Vector2(0f, 1f);
            topBarRT.anchorMax = new Vector2(1f, 1f);
            topBarRT.pivot = new Vector2(0.5f, 1f);
            topBarRT.sizeDelta = new Vector2(0f, 90f);
            Image topBarBg = topBarGO.AddComponent<Image>();
            topBarBg.color = new Color(0.12f, 0.12f, 0.16f, 0.90f);

            // Top Bar Texts & Icons (Left aligned so tutorial never covers them)
            hud.levelText = CreateTextElement(topBarGO, "LevelText", "⭐ Nivel 1", 24, new Vector2(120f, 0f), new Vector2(160f, 50f), Color.white, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            hud.coinsText = CreateTextElement(topBarGO, "CoinsText", "💰 $ 250", 24, new Vector2(300f, 0f), new Vector2(180f, 50f), Color.yellow, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            hud.reputationText = CreateTextElement(topBarGO, "RepText", "🏆 Rep: 10", 24, new Vector2(490f, 0f), new Vector2(160f, 50f), new Color(1f, 0.65f, 0.25f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));

            // Lateral Action Buttons
            GameObject sideBarGO = new GameObject("SideBar");
            sideBarGO.transform.SetParent(hudGO.transform, false);
            RectTransform sideBarRT = sideBarGO.AddComponent<RectTransform>();
            sideBarRT.anchorMin = new Vector2(1f, 0.5f);
            sideBarRT.anchorMax = new Vector2(1f, 0.5f);
            sideBarRT.pivot = new Vector2(1f, 0.5f);
            sideBarRT.sizeDelta = new Vector2(145f, 430f);
            sideBarRT.anchoredPosition = new Vector2(-15f, 0f);

            hud.buildModeButton = CreateButton(sideBarGO, "BuildBtn", "CONSTRUIR", new Vector2(0f, 130f), new Vector2(135f, 65f), new Color(0.2f, 0.55f, 0.9f));
            hud.inventoryButton = CreateButton(sideBarGO, "InvBtn", "INVENTARIO", new Vector2(0f, 45f), new Vector2(135f, 65f), new Color(0.25f, 0.65f, 0.35f));
            hud.questButton = CreateButton(sideBarGO, "QuestBtn", "MISIONES", new Vector2(0f, -40f), new Vector2(135f, 65f), new Color(0.9f, 0.55f, 0.15f));
            hud.marketButton = CreateButton(sideBarGO, "MarketBtn", "🛒 TIENDA", new Vector2(0f, -125f), new Vector2(135f, 65f), new Color(0.85f, 0.30f, 0.65f));

            // Build Mode Bottom Panel
            GameObject buildPanelGO = new GameObject("BuildPanel");
            buildPanelGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform bpRT = buildPanelGO.AddComponent<RectTransform>();
            bpRT.anchorMin = new Vector2(0f, 0f);
            bpRT.anchorMax = new Vector2(1f, 0f);
            bpRT.pivot = new Vector2(0.5f, 0f);
            bpRT.sizeDelta = new Vector2(0f, 170f);
            Image bpBg = buildPanelGO.AddComponent<Image>();
            bpBg.color = new Color(0.10f, 0.10f, 0.14f, 0.95f);

            BuildUI buildUI = buildPanelGO.AddComponent<BuildUI>();
            buildUI.panelRoot = buildPanelGO;
            buildUI.rotateButton = CreateButton(buildPanelGO, "RotateBtn", "ROTAR\n(90°)", new Vector2(80f, 85f), new Vector2(110f, 90f), new Color(0.35f, 0.45f, 0.85f));
            buildUI.closeButton = CreateButton(buildPanelGO, "DoneBtn", "LISTO", new Vector2(1920f - 180f, 85f), new Vector2(110f, 90f), new Color(0.2f, 0.75f, 0.4f));

            GameObject catalogContainer = new GameObject("CatalogContainer");
            catalogContainer.transform.SetParent(buildPanelGO.transform, false);
            RectTransform ccRT = catalogContainer.AddComponent<RectTransform>();
            ccRT.anchorMin = new Vector2(0.12f, 0.08f);
            ccRT.anchorMax = new Vector2(0.85f, 0.92f);
            ccRT.offsetMin = Vector2.zero;
            ccRT.offsetMax = Vector2.zero;
            GridLayoutGroup glg = catalogContainer.AddComponent<GridLayoutGroup>();
            glg.cellSize = new Vector2(130, 130);
            glg.spacing = new Vector2(15, 10);
            buildUI.itemsContainer = catalogContainer.transform;

            hud.buildPanel = buildPanelGO;
            buildPanelGO.SetActive(false);

            // Cook Station UI Dialog
            GameObject cookStationModalGO = new GameObject("CookStationModal");
            cookStationModalGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform csRT = cookStationModalGO.AddComponent<RectTransform>();
            csRT.anchorMin = new Vector2(0.5f, 0.5f);
            csRT.anchorMax = new Vector2(0.5f, 0.5f);
            csRT.sizeDelta = new Vector2(740f, 520f);
            Image csBg = cookStationModalGO.AddComponent<Image>();
            csBg.color = new Color(0.14f, 0.15f, 0.20f, 0.96f);

            CookStationUI cookUI = cookStationModalGO.AddComponent<CookStationUI>();
            cookUI.panelRoot = cookStationModalGO;
            cookUI.stationTitleText = CreateTextElement(cookStationModalGO, "Title", "Estación de Cocina", 30, new Vector2(370, -40), new Vector2(500, 50), Color.white);
            cookUI.closeButton = CreateButton(cookStationModalGO, "CloseBtn", "X", new Vector2(680, 470), new Vector2(50, 50), new Color(0.85f, 0.25f, 0.25f));

            GameObject recipeScroll = new GameObject("RecipeScroll");
            recipeScroll.transform.SetParent(cookStationModalGO.transform, false);
            RectTransform rsRT = recipeScroll.AddComponent<RectTransform>();
            rsRT.anchorMin = new Vector2(0.05f, 0.05f);
            rsRT.anchorMax = new Vector2(0.95f, 0.82f);
            rsRT.offsetMin = Vector2.zero;
            rsRT.offsetMax = Vector2.zero;
            VerticalLayoutGroup vlg = recipeScroll.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 10;
            vlg.childForceExpandHeight = false;
            cookUI.recipeListContainer = recipeScroll.transform;
            cookStationModalGO.SetActive(false);

            // Inventory Modal Dialog
            GameObject invModalGO = new GameObject("InventoryModal");
            invModalGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform invRT = invModalGO.AddComponent<RectTransform>();
            invRT.anchorMin = new Vector2(0.5f, 0.5f);
            invRT.anchorMax = new Vector2(0.5f, 0.5f);
            invRT.sizeDelta = new Vector2(760f, 540f);
            Image invBg = invModalGO.AddComponent<Image>();
            invBg.color = new Color(0.12f, 0.14f, 0.18f, 0.96f);

            InventoryUI invUI = invModalGO.AddComponent<InventoryUI>();
            invUI.panelRoot = invModalGO;
            CreateTextElement(invModalGO, "Title", "Mochila de Ingredientes", 30, new Vector2(380, -40), new Vector2(500, 50), Color.white);
            invUI.closeButton = CreateButton(invModalGO, "CloseBtn", "X", new Vector2(700, 480), new Vector2(50, 50), new Color(0.85f, 0.25f, 0.25f));

            GameObject invScroll = new GameObject("InventoryScroll");
            invScroll.transform.SetParent(invModalGO.transform, false);
            RectTransform invScrollRT = invScroll.AddComponent<RectTransform>();
            invScrollRT.anchorMin = new Vector2(0.06f, 0.06f);
            invScrollRT.anchorMax = new Vector2(0.94f, 0.84f);
            invScrollRT.offsetMin = Vector2.zero;
            invScrollRT.offsetMax = Vector2.zero;
            GridLayoutGroup invGlg = invScroll.AddComponent<GridLayoutGroup>();
            invGlg.cellSize = new Vector2(140, 150);
            invGlg.spacing = new Vector2(15, 15);
            invUI.emptyMessageText = CreateTextElement(invModalGO, "EmptyMessage", "Tu mochila de ingredientes está vacía.\n¡Cultiva cosechas o compra suministros!", 22, new Vector2(380, -250), new Vector2(520, 80), new Color(0.7f, 0.75f, 0.85f));
            invUI.itemsContainer = invScroll.transform;
            hud.inventoryPanel = invModalGO;
            invModalGO.SetActive(false);

            // Quest Modal Dialog
            GameObject questModalGO = new GameObject("QuestModal");
            questModalGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform qRT = questModalGO.AddComponent<RectTransform>();
            qRT.anchorMin = new Vector2(0.5f, 0.5f);
            qRT.anchorMax = new Vector2(0.5f, 0.5f);
            qRT.sizeDelta = new Vector2(760f, 540f);
            Image qBg = questModalGO.AddComponent<Image>();
            qBg.color = new Color(0.13f, 0.15f, 0.20f, 0.96f);

            QuestUI questUI = questModalGO.AddComponent<QuestUI>();
            questUI.panelRoot = questModalGO;
            CreateTextElement(questModalGO, "Title", "Misiones del Restaurante", 30, new Vector2(380, -40), new Vector2(500, 50), Color.white);
            questUI.closeButton = CreateButton(questModalGO, "CloseBtn", "X", new Vector2(700, 480), new Vector2(50, 50), new Color(0.85f, 0.25f, 0.25f));

            GameObject questScroll = new GameObject("QuestScroll");
            questScroll.transform.SetParent(questModalGO.transform, false);
            RectTransform qScrollRT = questScroll.AddComponent<RectTransform>();
            qScrollRT.anchorMin = new Vector2(0.06f, 0.06f);
            qScrollRT.anchorMax = new Vector2(0.94f, 0.84f);
            qScrollRT.offsetMin = Vector2.zero;
            qScrollRT.offsetMax = Vector2.zero;
            VerticalLayoutGroup qVlg = questScroll.AddComponent<VerticalLayoutGroup>();
            qVlg.spacing = 15;
            qVlg.childForceExpandHeight = false;
            questUI.emptyMessageText = CreateTextElement(questModalGO, "EmptyMessage", "¡No tienes misiones pendientes!\nSigue gestionando tu restaurante.", 22, new Vector2(380, -250), new Vector2(520, 80), new Color(0.7f, 0.75f, 0.85f));
            questUI.questListContainer = questScroll.transform;
            hud.questPanel = questModalGO;
            questModalGO.SetActive(false);

            // Market Modal Dialog
            GameObject marketModalGO = new GameObject("MarketModal");
            marketModalGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform mRT = marketModalGO.AddComponent<RectTransform>();
            mRT.anchorMin = new Vector2(0.5f, 0.5f);
            mRT.anchorMax = new Vector2(0.5f, 0.5f);
            mRT.sizeDelta = new Vector2(820f, 560f);
            Image mBg = marketModalGO.AddComponent<Image>();
            mBg.color = new Color(0.12f, 0.14f, 0.19f, 0.97f);

            MarketUI marketUI = marketModalGO.AddComponent<MarketUI>();
            marketUI.panelRoot = marketModalGO;
            CreateTextElement(marketModalGO, "Title", "Mercado del Proveedor NPC", 30, new Vector2(410, -40), new Vector2(500, 50), new Color(1f, 0.85f, 0.3f));
            marketUI.closeButton = CreateButton(marketModalGO, "CloseBtn", "X", new Vector2(750, 500), new Vector2(50, 50), new Color(0.85f, 0.25f, 0.25f));

            GameObject marketScroll = new GameObject("MarketScroll");
            marketScroll.transform.SetParent(marketModalGO.transform, false);
            RectTransform mScrollRT = marketScroll.AddComponent<RectTransform>();
            mScrollRT.anchorMin = new Vector2(0.05f, 0.05f);
            mScrollRT.anchorMax = new Vector2(0.95f, 0.84f);
            mScrollRT.offsetMin = Vector2.zero;
            mScrollRT.offsetMax = Vector2.zero;
            GridLayoutGroup mGlg = marketScroll.AddComponent<GridLayoutGroup>();
            mGlg.cellSize = new Vector2(170, 190);
            mGlg.spacing = new Vector2(15, 15);
            marketUI.itemsContainer = marketScroll.transform;
            hud.marketPanel = marketModalGO;
            marketModalGO.SetActive(false);

            // Vendor Modal Dialog (NPC Specialist Shop)
            GameObject vendorModalGO = new GameObject("VendorModal");
            vendorModalGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform vRT = vendorModalGO.AddComponent<RectTransform>();
            vRT.anchorMin = new Vector2(0.5f, 0.5f);
            vRT.anchorMax = new Vector2(0.5f, 0.5f);
            vRT.sizeDelta = new Vector2(860f, 580f);
            Image vBg = vendorModalGO.AddComponent<Image>();
            vBg.color = new Color(0.12f, 0.14f, 0.18f, 0.98f);

            VendorUI vendorUI = vendorModalGO.AddComponent<VendorUI>();
            vendorUI.panelRoot = vendorModalGO;
            vendorUI.closeButton = CreateButton(vendorModalGO, "CloseBtn", "X", new Vector2(790, 520), new Vector2(50, 50), new Color(0.85f, 0.25f, 0.25f));

            // NPC Header: Portrait, Name, Role, Dialogue, Restock Timer
            GameObject vPortraitGO = new GameObject("Portrait", typeof(RectTransform), typeof(Image));
            vPortraitGO.transform.SetParent(vendorModalGO.transform, false);
            RectTransform vpRT = vPortraitGO.GetComponent<RectTransform>();
            vpRT.anchorMin = new Vector2(0f, 1f);
            vpRT.anchorMax = new Vector2(0f, 1f);
            vpRT.sizeDelta = new Vector2(70, 70);
            vpRT.anchoredPosition = new Vector2(55, -50);
            vendorUI.npcPortraitImage = vPortraitGO.GetComponent<Image>();

            vendorUI.npcNameText = CreateTextElement(vendorModalGO, "NPCName", "Elena", 26, new Vector2(230, -35), new Vector2(260, 36), new Color(1f, 0.88f, 0.35f));
            vendorUI.npcRoleText = CreateTextElement(vendorModalGO, "NPCRole", "Agricultora de la Villa", 16, new Vector2(230, -65), new Vector2(260, 26), new Color(0.75f, 0.85f, 0.75f));
            vendorUI.dialogueText = CreateTextElement(vendorModalGO, "Dialogue", "¡Hola Chef! Aquí tienes los ingredientes más frescos de la villa.", 15, new Vector2(540, -40), new Vector2(360, 50), Color.white);
            vendorUI.restockTimerText = CreateTextElement(vendorModalGO, "Timer", "Reabastecimiento en: 03:00", 15, new Vector2(540, -75), new Vector2(360, 24), new Color(0.4f, 0.9f, 1f));

            GameObject vendorScroll = new GameObject("VendorScroll");
            vendorScroll.transform.SetParent(vendorModalGO.transform, false);
            RectTransform vScrollRT = vendorScroll.AddComponent<RectTransform>();
            vScrollRT.anchorMin = new Vector2(0.04f, 0.04f);
            vScrollRT.anchorMax = new Vector2(0.96f, 0.80f);
            vScrollRT.offsetMin = Vector2.zero;
            vScrollRT.offsetMax = Vector2.zero;
            ScrollRect vSr = vendorScroll.AddComponent<ScrollRect>();
            vSr.horizontal = false;
            vSr.vertical = true;

            GameObject vContent = new GameObject("Content");
            vContent.transform.SetParent(vendorScroll.transform, false);
            RectTransform vContentRT = vContent.AddComponent<RectTransform>();
            vContentRT.anchorMin = new Vector2(0f, 1f);
            vContentRT.anchorMax = new Vector2(1f, 1f);
            vContentRT.pivot = new Vector2(0.5f, 1f);
            vContentRT.sizeDelta = new Vector2(0, 400);

            VerticalLayoutGroup vVlg = vContent.AddComponent<VerticalLayoutGroup>();
            vVlg.spacing = 10;
            vVlg.childControlWidth = true;
            vVlg.childControlHeight = false;
            vVlg.childForceExpandWidth = true;
            vVlg.childForceExpandHeight = false;

            ContentSizeFitter vCsf = vContent.AddComponent<ContentSizeFitter>();
            vCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            vSr.content = vContentRT;
            vendorUI.itemsContainer = vContent.transform;
            vendorModalGO.SetActive(false);

            // Crafting Modal Dialog (Processing Insumos)
            GameObject craftingModalGO = new GameObject("CraftingModal");
            craftingModalGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform cRT = craftingModalGO.AddComponent<RectTransform>();
            cRT.anchorMin = new Vector2(0.5f, 0.5f);
            cRT.anchorMax = new Vector2(0.5f, 0.5f);
            cRT.sizeDelta = new Vector2(860f, 580f);
            Image cBg = craftingModalGO.AddComponent<Image>();
            cBg.color = new Color(0.13f, 0.12f, 0.17f, 0.98f);

            CraftingUI craftingUI = craftingModalGO.AddComponent<CraftingUI>();
            craftingUI.panelRoot = craftingModalGO;
            craftingUI.closeButton = CreateButton(craftingModalGO, "CloseBtn", "X", new Vector2(790, 520), new Vector2(50, 50), new Color(0.85f, 0.25f, 0.25f));

            craftingUI.titleText = CreateTextElement(craftingModalGO, "Title", "Estación de Elaboración", 26, new Vector2(300, -35), new Vector2(400, 36), new Color(1f, 0.85f, 0.3f));
            craftingUI.subtitleText = CreateTextElement(craftingModalGO, "Subtitle", "Selecciona una receta para procesar:", 16, new Vector2(300, -68), new Vector2(400, 26), new Color(0.75f, 0.85f, 0.9f));

            // Recipe Selection View
            GameObject rViewGO = new GameObject("RecipeSelectionView");
            rViewGO.transform.SetParent(craftingModalGO.transform, false);
            RectTransform rvRT = rViewGO.AddComponent<RectTransform>();
            rvRT.anchorMin = Vector2.zero;
            rvRT.anchorMax = Vector2.one;
            rvRT.offsetMin = Vector2.zero;
            rvRT.offsetMax = Vector2.zero;
            craftingUI.recipeSelectionView = rViewGO;

            GameObject craftScroll = new GameObject("CraftScroll");
            craftScroll.transform.SetParent(rViewGO.transform, false);
            RectTransform crsRT = craftScroll.AddComponent<RectTransform>();
            crsRT.anchorMin = new Vector2(0.04f, 0.04f);
            crsRT.anchorMax = new Vector2(0.96f, 0.80f);
            crsRT.offsetMin = Vector2.zero;
            crsRT.offsetMax = Vector2.zero;
            ScrollRect crSr = craftScroll.AddComponent<ScrollRect>();
            crSr.horizontal = false;
            crSr.vertical = true;

            GameObject cContent = new GameObject("Content");
            cContent.transform.SetParent(craftScroll.transform, false);
            RectTransform cContentRT = cContent.AddComponent<RectTransform>();
            cContentRT.anchorMin = new Vector2(0f, 1f);
            cContentRT.anchorMax = new Vector2(1f, 1f);
            cContentRT.pivot = new Vector2(0.5f, 1f);
            cContentRT.sizeDelta = new Vector2(0, 400);

            VerticalLayoutGroup cVlg = cContent.AddComponent<VerticalLayoutGroup>();
            cVlg.spacing = 10;
            cVlg.childControlWidth = true;
            cVlg.childControlHeight = false;
            cVlg.childForceExpandWidth = true;
            cVlg.childForceExpandHeight = false;

            ContentSizeFitter cCsf = cContent.AddComponent<ContentSizeFitter>();
            cCsf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            crSr.content = cContentRT;
            craftingUI.recipesContainer = cContent.transform;

            // Active Crafting View
            GameObject activeViewGO = new GameObject("ActiveCraftingView");
            activeViewGO.transform.SetParent(craftingModalGO.transform, false);
            RectTransform actvRT = activeViewGO.AddComponent<RectTransform>();
            actvRT.anchorMin = Vector2.zero;
            actvRT.anchorMax = Vector2.one;
            actvRT.offsetMin = Vector2.zero;
            actvRT.offsetMax = Vector2.zero;
            craftingUI.activeCraftingView = activeViewGO;

            GameObject pIconGO = new GameObject("ProductIcon", typeof(RectTransform), typeof(Image));
            pIconGO.transform.SetParent(activeViewGO.transform, false);
            RectTransform piRT = pIconGO.GetComponent<RectTransform>();
            piRT.anchoredPosition = new Vector2(430, -200);
            piRT.sizeDelta = new Vector2(96, 96);
            craftingUI.activeProductIcon = pIconGO.GetComponent<Image>();

            craftingUI.activeProductNameText = CreateTextElement(activeViewGO, "ProductName", "Harina Blanca x2", 24, new Vector2(430, -270), new Vector2(400, 36), Color.white);

            // Progress bar
            GameObject pbBg = new GameObject("ProgressBarBg", typeof(RectTransform), typeof(Image));
            pbBg.transform.SetParent(activeViewGO.transform, false);
            RectTransform pbBgRT = pbBg.GetComponent<RectTransform>();
            pbBgRT.anchoredPosition = new Vector2(430, -320);
            pbBgRT.sizeDelta = new Vector2(400, 28);
            pbBg.GetComponent<Image>().color = new Color(0.2f, 0.22f, 0.28f);

            GameObject pbFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            pbFill.transform.SetParent(pbBg.transform, false);
            RectTransform pbFillRT = pbFill.GetComponent<RectTransform>();
            pbFillRT.anchorMin = Vector2.zero;
            pbFillRT.anchorMax = Vector2.one;
            pbFillRT.offsetMin = Vector2.zero;
            pbFillRT.offsetMax = Vector2.zero;
            Image pbFillImg = pbFill.GetComponent<Image>();
            pbFillImg.color = new Color(0.25f, 0.75f, 0.45f);
            pbFillImg.type = Image.Type.Filled;
            pbFillImg.fillMethod = Image.FillMethod.Horizontal;
            craftingUI.progressBarFill = pbFillImg;

            craftingUI.progressTimerText = CreateTextElement(activeViewGO, "TimerText", "Tiempo restante: 8s", 18, new Vector2(430, -360), new Vector2(300, 30), new Color(0.7f, 0.85f, 1f));

            craftingUI.speedUpButton = CreateButton(activeViewGO, "SpeedUpBtn", "⚡ Acelerar", new Vector2(350, -430), new Vector2(160, 48), new Color(0.3f, 0.6f, 0.85f));
            craftingUI.collectButton = CreateButton(activeViewGO, "CollectBtn", "✨ ¡Recolectar!", new Vector2(510, -430), new Vector2(180, 52), new Color(0.25f, 0.75f, 0.35f));

            activeViewGO.SetActive(false);
            craftingModalGO.SetActive(false);

            // Expansion Modal Dialog (Villa Expansions)
            GameObject expansionModalGO = new GameObject("ExpansionModal");
            expansionModalGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform expRT = expansionModalGO.AddComponent<RectTransform>();
            expRT.anchorMin = new Vector2(0.5f, 0.5f);
            expRT.anchorMax = new Vector2(0.5f, 0.5f);
            expRT.sizeDelta = new Vector2(680f, 480f);
            Image expBg = expansionModalGO.AddComponent<Image>();
            expBg.color = new Color(0.12f, 0.14f, 0.20f, 0.98f);

            ExpansionUI expansionUI = expansionModalGO.AddComponent<ExpansionUI>();
            expansionUI.panelRoot = expansionModalGO;
            expansionUI.closeButton = CreateButton(expansionModalGO, "CloseBtn", "X", new Vector2(620, 420), new Vector2(48, 48), new Color(0.85f, 0.25f, 0.25f));
            expansionUI.titleText = CreateTextElement(expansionModalGO, "Title", "Expansión de la Villa", 26, new Vector2(340, -45), new Vector2(450, 40), new Color(1f, 0.85f, 0.25f));
            expansionUI.descriptionText = CreateTextElement(expansionModalGO, "Desc", "Desbloquea este terreno para ampliar tu restaurante y villa gastronómica.", 18, new Vector2(340, -130), new Vector2(520, 70), Color.white);
            expansionUI.levelRequirementText = CreateTextElement(expansionModalGO, "LevelReq", "Nivel requerido: 2", 18, new Vector2(340, -210), new Vector2(450, 30), new Color(0.4f, 1f, 0.5f));
            expansionUI.costText = CreateTextElement(expansionModalGO, "Cost", "Costo: $200 Monedas", 20, new Vector2(340, -260), new Vector2(450, 35), new Color(1f, 0.9f, 0.3f));
            expansionUI.xpRewardText = CreateTextElement(expansionModalGO, "XPReward", "+60 XP", 16, new Vector2(340, -300), new Vector2(450, 25), new Color(0.6f, 0.85f, 1f));
            expansionUI.unlockButton = CreateButton(expansionModalGO, "UnlockBtn", "✨ ¡Desbloquear Zona!", new Vector2(340, -380), new Vector2(250, 56), new Color(0.25f, 0.75f, 0.35f));
            expansionUI.unlockButtonText = expansionUI.unlockButton.GetComponentInChildren<Text>();
            expansionModalGO.SetActive(false);

            // Level Up Modal Dialog
            GameObject levelUpModalGO = new GameObject("LevelUpModal");
            levelUpModalGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform luRT = levelUpModalGO.AddComponent<RectTransform>();
            luRT.anchorMin = new Vector2(0.5f, 0.5f);
            luRT.anchorMax = new Vector2(0.5f, 0.5f);
            luRT.sizeDelta = new Vector2(680f, 440f);
            Image luBg = levelUpModalGO.AddComponent<Image>();
            luBg.color = new Color(0.10f, 0.12f, 0.18f, 0.98f);

            LevelUpModalUI levelUpUI = levelUpModalGO.AddComponent<LevelUpModalUI>();
            levelUpUI.panelRoot = levelUpModalGO;
            levelUpUI.levelNumberText = CreateTextElement(levelUpModalGO, "LevelNum", "¡NIVEL 2!", 46, new Vector2(340, -70), new Vector2(500, 70), new Color(1f, 0.85f, 0.2f));
            levelUpUI.rewardText = CreateTextElement(levelUpModalGO, "Reward", "+$100 Monedas de Bonificación", 24, new Vector2(340, -160), new Vector2(500, 50), new Color(0.4f, 1f, 0.5f));
            levelUpUI.descriptionText = CreateTextElement(levelUpModalGO, "Desc", "¡Tu restaurante es cada vez más popular! Sigue cocinando para desbloquear nuevas recetas.", 18, new Vector2(340, -230), new Vector2(550, 70), Color.white);
            levelUpUI.claimButton = CreateButton(levelUpModalGO, "ClaimBtn", "¡GENIAL!", new Vector2(340, -360), new Vector2(220, 60), new Color(0.2f, 0.75f, 0.4f));
            levelUpModalGO.SetActive(false);

            // Tutorial UI Banner
            GameObject tutBannerGO = new GameObject("TutorialBanner");
            tutBannerGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform tutRT = tutBannerGO.AddComponent<RectTransform>();
            tutRT.anchorMin = new Vector2(0.5f, 1f);
            tutRT.anchorMax = new Vector2(0.5f, 1f);
            tutRT.pivot = new Vector2(0.5f, 1f);
            tutRT.sizeDelta = new Vector2(850f, 110f);
            tutRT.anchoredPosition = new Vector2(0f, -165f);
            Image tutBg = tutBannerGO.AddComponent<Image>();
            tutBg.color = new Color(0.12f, 0.15f, 0.22f, 0.96f);

            TutorialUI tutUI = tutBannerGO.AddComponent<TutorialUI>();
            tutUI.panelRoot = tutBannerGO;

            // Avatar
            GameObject avGO = new GameObject("Avatar");
            avGO.transform.SetParent(tutBannerGO.transform, false);
            RectTransform avRT = avGO.AddComponent<RectTransform>();
            avRT.sizeDelta = new Vector2(76, 76);
            avRT.anchoredPosition = new Vector2(-360f, 0f);
            Image avImg = avGO.AddComponent<Image>();
            Sprite chefSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Projet/Art/Characters/Player/chef_player.png");
            if (chefSprite != null) avImg.sprite = chefSprite;
            tutUI.avatarImage = avImg;

            tutUI.speakerNameText = CreateTextElement(tutBannerGO, "SpeakerName", "Chef Mentor", 18, new Vector2(-120, 30), new Vector2(360, 30), new Color(1f, 0.85f, 0.3f));
            tutUI.instructionText = CreateTextElement(tutBannerGO, "Instruction", "¡Bienvenido a Villa del Chef!", 15, new Vector2(-120, -10), new Vector2(460, 50), Color.white);
            tutUI.nextButton = CreateButton(tutBannerGO, "NextBtn", "¡Entendido!", new Vector2(320f, 0f), new Vector2(130f, 45f), new Color(0.25f, 0.65f, 0.9f));
            tutUI.nextButton.gameObject.SetActive(false);

            // 6. Setup Game Systems & Bootstrap
            GameObject systemsGO = new GameObject("--- GAME SYSTEMS ---");
            systemsGO.AddComponent<GridManager>();
            systemsGO.AddComponent<BuildManager>();
            systemsGO.AddComponent<EconomyManager>();
            systemsGO.AddComponent<ProgressionManager>();
            systemsGO.AddComponent<InventoryManager>();
            systemsGO.AddComponent<RecipeManager>();
            systemsGO.AddComponent<FarmingManager>();
            systemsGO.AddComponent<CustomerManager>();
            systemsGO.AddComponent<WorkerManager>();
            systemsGO.AddComponent<QuestManager>();
            systemsGO.AddComponent<TutorialManager>();
            systemsGO.AddComponent<TouchInputManager>();
            systemsGO.AddComponent<SaveManager>();
            systemsGO.AddComponent<VillaDelChef.Managers.CraftingManager>();
            systemsGO.AddComponent<VillaDelChef.Managers.ExpansionManager>();
            systemsGO.AddComponent<VillaDelChef.Managers.RestaurantOperatingManager>();
            systemsGO.AddComponent<RestaurantBootstrap>();

            // 7. Setup Audio System
            GameObject audioGO = new GameObject("AudioManager");
            AudioManager audioMgr = audioGO.AddComponent<AudioManager>();
            audioMgr.musicSource = audioGO.AddComponent<AudioSource>();
            audioMgr.sfxSource = audioGO.AddComponent<AudioSource>();
            audioMgr.backgroundMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Projet/Audio/Music/cozy_restaurant_bgm.wav");
            audioMgr.coinSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Projet/Audio/SFX/coin_clink.wav");
            audioMgr.buttonClickSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Projet/Audio/SFX/button_click.wav");
            audioMgr.dishReadySFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Projet/Audio/SFX/dish_ready.wav");
            audioMgr.harvestSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Projet/Audio/SFX/crop_harvest.wav");
            audioMgr.cookingSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Projet/Audio/SFX/cooking_sizzle.wav");
            audioMgr.levelUpSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Projet/Audio/SFX/level_up.wav");

            // Save Scene
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[RestaurantSceneSetupEditor] Restaurant MVP Scene successfully created at {scenePath}!");
        }

        public static void SetupBootScene()
        {
            string scenePath = "Assets/_Projet/Scenes/00_Boot.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            GameObject camGO = new GameObject("Main Camera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.1f, 0.12f, 0.15f);

            GameObject bootGO = new GameObject("BootManager");
            bootGO.AddComponent<BootManager>();

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[RestaurantSceneSetupEditor] 00_Boot scene created at {scenePath}!");
        }

        public static void SetupMainMenuScene()
        {
            string scenePath = "Assets/_Projet/Scenes/01_MainMenu.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Camera
            GameObject camGO = new GameObject("Main Camera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.14f, 0.18f, 0.20f);
            camGO.AddComponent<AudioListener>();

            // Scenery Background
            SetupWorldBackground();

            // Event System
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // Canvas
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // Safe Area Container
            GameObject safeAreaGO = new GameObject("SafeAreaContainer");
            safeAreaGO.transform.SetParent(canvasGO.transform, false);
            RectTransform safeAreaRT = safeAreaGO.AddComponent<RectTransform>();
            safeAreaRT.anchorMin = Vector2.zero;
            safeAreaRT.anchorMax = Vector2.one;
            safeAreaRT.offsetMin = Vector2.zero;
            safeAreaRT.offsetMax = Vector2.zero;
            safeAreaGO.AddComponent<SafeAreaFitter>();

            MainMenuController menu = safeAreaGO.AddComponent<MainMenuController>();

            // Optional Background Graphic container
            GameObject bgGO = new GameObject("BackgroundImage");
            bgGO.transform.SetParent(safeAreaGO.transform, false);
            bgGO.transform.SetAsFirstSibling();
            RectTransform bgRT = bgGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.sizeDelta = Vector2.zero;
            Image bgImg = bgGO.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0f); // transparent if no background image yet
            Sprite mainmenuBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/_Projet/Art/UI/MainMenu/mainmenu_background.png");
            if (mainmenuBg != null)
            {
                bgImg.sprite = mainmenuBg;
                bgImg.color = Color.white;
            }
            menu.backgroundImage = bgImg;

            // Exotic Title Banner with layered shadow, tropical subtitle, and badge (Centered)
            CreateTextElement(safeAreaGO, "TitleShadow", "VILLA DEL CHEF", 76, new Vector2(0f, 258f), new Vector2(950, 110), new Color(0.18f, 0.08f, 0.02f, 0.85f));
            CreateTextElement(safeAreaGO, "Title", "VILLA DEL CHEF", 76, new Vector2(0f, 260f), new Vector2(950, 110), new Color(1f, 0.88f, 0.25f));

            CreateTextElement(safeAreaGO, "Badge", "★ OASIS CULINARIO & RESTAURANTE GOURMET ★", 20, new Vector2(0f, 195f), new Vector2(900, 35), new Color(1f, 0.65f, 0.25f));
            CreateTextElement(safeAreaGO, "Subtitle", "🌴 ¡Cocina, Cultiva, Decora y Atiende a tus Clientes! 🍽️", 26, new Vector2(0f, 140f), new Vector2(900, 45), new Color(0.95f, 0.95f, 1f));

            // Center Menu Buttons
            menu.continueButton = CreateButton(safeAreaGO, "ContinueBtn", "▶️ CONTINUAR", new Vector2(0f, 15f), new Vector2(320, 65), new Color(0.2f, 0.75f, 0.4f));
            menu.playButton = CreateButton(safeAreaGO, "PlayBtn", "🍽️ NUEVA PARTIDA", new Vector2(0f, -60f), new Vector2(320, 60), new Color(0.85f, 0.55f, 0.2f));
            menu.optionsButton = CreateButton(safeAreaGO, "OptionsBtn", "⚙️ OPCIONES", new Vector2(0f, -130f), new Vector2(320, 55), new Color(0.25f, 0.5f, 0.85f));
            menu.creditsButton = CreateButton(safeAreaGO, "CreditsBtn", "📜 CRÉDITOS", new Vector2(0f, -195f), new Vector2(320, 55), new Color(0.45f, 0.35f, 0.75f));
            menu.quitButton = CreateButton(safeAreaGO, "QuitBtn", "SALIR", new Vector2(0f, -260f), new Vector2(320, 50), new Color(0.75f, 0.25f, 0.25f));

            // New Game Confirmation Panel
            GameObject cnfPanelGO = new GameObject("ConfirmNewGamePanel");
            cnfPanelGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform cnfRT = cnfPanelGO.AddComponent<RectTransform>();
            cnfRT.anchorMin = new Vector2(0.5f, 0.5f);
            cnfRT.anchorMax = new Vector2(0.5f, 0.5f);
            cnfRT.sizeDelta = new Vector2(640, 320);
            cnfRT.anchoredPosition = Vector2.zero;
            Image cnfBg = cnfPanelGO.AddComponent<Image>();
            cnfBg.color = new Color(0.1f, 0.12f, 0.16f, 0.98f);

            CreateTextElement(cnfPanelGO, "CnfText", "¿Comenzar una nueva partida?\nSe perderá el progreso guardado actual.", 24, new Vector2(0f, 40f), new Vector2(580, 120), Color.white);
            menu.confirmNewGameBtn = CreateButton(cnfPanelGO, "ConfirmBtn", "CONFIRMAR", new Vector2(130f, -80f), new Vector2(180, 55), new Color(0.8f, 0.25f, 0.25f));
            menu.cancelNewGameBtn = CreateButton(cnfPanelGO, "CancelBtn", "CANCELAR", new Vector2(-130f, -80f), new Vector2(180, 55), new Color(0.3f, 0.5f, 0.8f));
            menu.confirmNewGamePanel = cnfPanelGO;
            cnfPanelGO.SetActive(false);

            // Options Panel
            GameObject optPanelGO = new GameObject("OptionsPanel");
            optPanelGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform optRT = optPanelGO.AddComponent<RectTransform>();
            optRT.anchorMin = new Vector2(0.5f, 0.5f);
            optRT.anchorMax = new Vector2(0.5f, 0.5f);
            optRT.sizeDelta = new Vector2(650, 480);
            optRT.anchoredPosition = Vector2.zero;
            Image optBg = optPanelGO.AddComponent<Image>();
            optBg.color = new Color(0.12f, 0.14f, 0.18f, 0.97f);

            CreateTextElement(optPanelGO, "OptTitle", "OPCIONES", 36, new Vector2(0f, 170f), new Vector2(400, 50), Color.white);
            CreateTextElement(optPanelGO, "MusicLabel", "Música de Fondo", 24, new Vector2(0f, 80f), new Vector2(400, 40), Color.white);
            CreateTextElement(optPanelGO, "SFXLabel", "Efectos de Sonido", 24, new Vector2(0f, -10f), new Vector2(400, 40), Color.white);

            menu.closeOptionsButton = CreateButton(optPanelGO, "CloseOptBtn", "VOLVER", new Vector2(0f, -170f), new Vector2(200, 60), new Color(0.3f, 0.5f, 0.8f));
            menu.optionsPanel = optPanelGO;
            optPanelGO.SetActive(false);

            // Credits Panel
            GameObject credPanelGO = new GameObject("CreditsPanel");
            credPanelGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform credRT = credPanelGO.AddComponent<RectTransform>();
            credRT.anchorMin = new Vector2(0.5f, 0.5f);
            credRT.anchorMax = new Vector2(0.5f, 0.5f);
            credRT.sizeDelta = new Vector2(650, 480);
            credRT.anchoredPosition = Vector2.zero;
            Image credBg = credPanelGO.AddComponent<Image>();
            credBg.color = new Color(0.12f, 0.14f, 0.18f, 0.97f);

            CreateTextElement(credPanelGO, "CredTitle", "CRÉDITOS", 36, new Vector2(0f, 170f), new Vector2(400, 50), Color.white);
            CreateTextElement(credPanelGO, "CredBody", "VILLA DEL CHEF\n\nDesarrollo, Diseño y Arte Pixel Art\nEquipo de Villa del Chef\n\nHecho con pasión en Unity 6", 22, new Vector2(0f, 30f), new Vector2(520, 200), new Color(0.9f, 0.9f, 0.95f));
            menu.closeCreditsButton = CreateButton(credPanelGO, "CloseCredBtn", "VOLVER", new Vector2(0f, -170f), new Vector2(200, 60), new Color(0.3f, 0.5f, 0.8f));
            menu.creditsPanel = credPanelGO;
            credPanelGO.SetActive(false);

            // Audio Manager in Main Menu
            GameObject audioGO = new GameObject("AudioManager");
            AudioManager audioMgr = audioGO.AddComponent<AudioManager>();
            audioMgr.musicSource = audioGO.AddComponent<AudioSource>();
            audioMgr.sfxSource = audioGO.AddComponent<AudioSource>();
            audioMgr.backgroundMusic = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Projet/Audio/Music/cozy_restaurant_bgm.wav");
            audioMgr.buttonClickSFX = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/_Projet/Audio/SFX/button_click.wav");

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[RestaurantSceneSetupEditor] 01_MainMenu scene created at {scenePath}!");
        }

        private static void SetupWorldBackground()
        {
            Sprite leftBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/esenario/esenario.png");
            Sprite rightBg = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/esenario/ecenario lado derecho.png");

            if (leftBg != null || rightBg != null)
            {
                GameObject bgRoot = new GameObject("Scenery_Background");

                float bgScale = 2.4f;
                float bgY = 4.5f;

                if (leftBg != null)
                {
                    GameObject leftGO = new GameObject("Scenery_Left");
                    leftGO.transform.SetParent(bgRoot.transform);
                    leftGO.transform.localScale = new Vector3(bgScale, bgScale, 1f);
                    SpriteRenderer sr = leftGO.AddComponent<SpriteRenderer>();
                    sr.sprite = leftBg;
                    sr.sortingOrder = -100;
                    float width = leftBg.bounds.size.x * bgScale;
                    leftGO.transform.position = new Vector3(-width * 0.5f, bgY, 0f);
                }

                if (rightBg != null)
                {
                    GameObject rightGO = new GameObject("Scenery_Right");
                    rightGO.transform.SetParent(bgRoot.transform);
                    rightGO.transform.localScale = new Vector3(bgScale, bgScale, 1f);
                    SpriteRenderer sr = rightGO.AddComponent<SpriteRenderer>();
                    sr.sprite = rightBg;
                    sr.sortingOrder = -100;
                    float width = rightBg.bounds.size.x * bgScale;
                    rightGO.transform.position = new Vector3(width * 0.5f, bgY, 0f);
                }
            }
        }

        private static Text CreateTextElement(GameObject parent, string name, string initialText, int fontSize, Vector2 anchoredPos, Vector2 size, Color? color = null, Vector2? anchorMin = null, Vector2? anchorMax = null)
        {
            GameObject textGO = new GameObject(name);
            textGO.transform.SetParent(parent.transform, false);
            RectTransform rt = textGO.AddComponent<RectTransform>();
            if (anchorMin.HasValue) rt.anchorMin = anchorMin.Value;
            if (anchorMax.HasValue) rt.anchorMax = anchorMax.Value;
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            Text t = textGO.AddComponent<Text>();
            t.text = initialText;
            t.fontSize = fontSize;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.alignment = TextAnchor.MiddleCenter;
            t.color = color.HasValue ? color.Value : Color.white;
            return t;
        }

        private static Button CreateButton(GameObject parent, string name, string label, Vector2 anchoredPos, Vector2 size, Color? btnColor = null)
        {
            GameObject btnGO = new GameObject(name);
            btnGO.transform.SetParent(parent.transform, false);
            RectTransform rt = btnGO.AddComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            Image img = btnGO.AddComponent<Image>();
            img.color = btnColor.HasValue ? btnColor.Value : new Color(0.24f, 0.50f, 0.85f, 0.95f);

            Button btn = btnGO.AddComponent<Button>();

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(btnGO.transform, false);
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(4, 4);
            textRT.offsetMax = new Vector2(-4, -4);

            Text t = textGO.AddComponent<Text>();
            t.text = label;
            t.fontSize = 18;
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.alignment = TextAnchor.MiddleCenter;
            t.color = Color.white;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = 11;
            t.resizeTextMaxSize = 20;

            return btn;
        }

        public static void SetupPrologueScene()
        {
            string scenePath = "Assets/_Projet/Scenes/03_Prologue.unity";
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            GameObject camGO = new GameObject("Main Camera");
            Camera cam = camGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.12f, 0.14f, 0.18f);
            camGO.AddComponent<AudioListener>();

            // 2. Background Scenery
            SetupWorldBackground();

            // 3. Event System
            GameObject eventSystemGO = new GameObject("EventSystem");
            eventSystemGO.AddComponent<EventSystem>();
            eventSystemGO.AddComponent<StandaloneInputModule>();

            // 4. Canvas
            GameObject canvasGO = new GameObject("Canvas");
            Canvas canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGO.AddComponent<GraphicRaycaster>();

            // 5. SafeArea Container
            GameObject safeAreaGO = new GameObject("SafeAreaContainer");
            safeAreaGO.transform.SetParent(canvasGO.transform, false);
            RectTransform safeAreaRT = safeAreaGO.AddComponent<RectTransform>();
            safeAreaRT.anchorMin = Vector2.zero;
            safeAreaRT.anchorMax = Vector2.one;
            safeAreaRT.offsetMin = Vector2.zero;
            safeAreaRT.offsetMax = Vector2.zero;
            safeAreaGO.AddComponent<SafeAreaFitter>();

            var prologue = safeAreaGO.AddComponent<PrologueController>();

            // Background Shade
            GameObject bgShadeGO = new GameObject("BackgroundShade");
            bgShadeGO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform bgRT = bgShadeGO.AddComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero;
            bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero;
            bgRT.offsetMax = Vector2.zero;
            Image bgImg = bgShadeGO.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.1f, 0.14f, 0.92f);

            // ==========================================
            // PANEL 1: NOMBRE
            // ==========================================
            GameObject p1GO = new GameObject("Step1_NameInputPanel");
            p1GO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform p1RT = p1GO.AddComponent<RectTransform>();
            p1RT.anchorMin = new Vector2(0.5f, 0.5f);
            p1RT.anchorMax = new Vector2(0.5f, 0.5f);
            p1RT.sizeDelta = new Vector2(760, 480);
            p1RT.anchoredPosition = Vector2.zero;
            Image p1Bg = p1GO.AddComponent<Image>();
            p1Bg.color = new Color(0.14f, 0.16f, 0.22f, 0.98f);

            CreateTextElement(p1GO, "Title", "¡BIENVENIDO A VILLA DEL CHEF!", 34, new Vector2(0, 160), new Vector2(700, 50), new Color(1f, 0.88f, 0.25f));
            CreateTextElement(p1GO, "Prompt", "Hola... ¿cómo te llamas, nuevo Chef?", 24, new Vector2(0, 80), new Vector2(650, 40), Color.white);
            prologue.nameInputField = CreateInputField(p1GO, "InputField", "Ingresa tu nombre...", new Vector2(0, 0), new Vector2(420, 60));
            prologue.submitNameBtn = CreateButton(p1GO, "SubmitBtn", "CONTINUAR ▶", new Vector2(0, -90), new Vector2(260, 60), new Color(0.2f, 0.75f, 0.4f));
            prologue.nameErrorText = CreateTextElement(p1GO, "ErrorText", "", 18, new Vector2(0, -170), new Vector2(600, 30), new Color(1f, 0.4f, 0.4f));
            prologue.nameInputPanel = p1GO;

            // ==========================================
            // PANEL 2: SELECTOR DE PERSONAJE (Preview rnormal)
            // ==========================================
            GameObject p2GO = new GameObject("Step2_CharacterSelectPanel");
            p2GO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform p2RT = p2GO.AddComponent<RectTransform>();
            p2RT.anchorMin = new Vector2(0.5f, 0.5f);
            p2RT.anchorMax = new Vector2(0.5f, 0.5f);
            p2RT.sizeDelta = new Vector2(900, 640);
            p2RT.anchoredPosition = Vector2.zero;
            Image p2Bg = p2GO.AddComponent<Image>();
            p2Bg.color = new Color(0.14f, 0.16f, 0.22f, 0.98f);

            CreateTextElement(p2GO, "Title", "ELIGE A TU PROTAGONISTA", 34, new Vector2(0, 250), new Vector2(800, 50), new Color(1f, 0.88f, 0.25f));
            CreateTextElement(p2GO, "Subtitle", "Ropa casual — Este aspecto te identificará como habitante de la villa", 18, new Vector2(0, 205), new Vector2(800, 30), new Color(0.85f, 0.88f, 0.95f));

            // Preview Box
            GameObject prevBoxGO = new GameObject("PreviewBox");
            prevBoxGO.transform.SetParent(p2GO.transform, false);
            RectTransform pbRT = prevBoxGO.AddComponent<RectTransform>();
            pbRT.sizeDelta = new Vector2(280, 360);
            pbRT.anchoredPosition = new Vector2(0, 20);
            Image pbBg = prevBoxGO.AddComponent<Image>();
            pbBg.color = new Color(0.08f, 0.1f, 0.14f, 0.7f);

            GameObject prevImgGO = new GameObject("CharacterImage");
            prevImgGO.transform.SetParent(prevBoxGO.transform, false);
            RectTransform piRT = prevImgGO.AddComponent<RectTransform>();
            piRT.sizeDelta = new Vector2(260, 340);
            piRT.anchoredPosition = Vector2.zero;
            prologue.characterPreviewImage = prevImgGO.AddComponent<Image>();
            prologue.characterPreviewImage.preserveAspect = true;

            // Nav Buttons
            prologue.prevCharacterBtn = CreateButton(p2GO, "PrevBtn", "◀", new Vector2(-220, 20), new Vector2(70, 70), new Color(0.3f, 0.45f, 0.7f));
            prologue.nextCharacterBtn = CreateButton(p2GO, "NextBtn", "▶", new Vector2(220, 20), new Vector2(70, 70), new Color(0.3f, 0.45f, 0.7f));

            prologue.characterNameText = CreateTextElement(p2GO, "CharName", "Nombre de Personaje", 28, new Vector2(0, -185), new Vector2(500, 40), Color.white);
            prologue.characterLoreText = CreateTextElement(p2GO, "CharLore", "Descripción", 16, new Vector2(0, -225), new Vector2(700, 40), new Color(0.8f, 0.85f, 0.9f));
            prologue.selectCharacterBtn = CreateButton(p2GO, "SelectBtn", "ELEGIR ESTE CHEF ▶", new Vector2(0, -275), new Vector2(300, 55), new Color(0.2f, 0.75f, 0.4f));
            prologue.characterSelectPanel = p2GO;
            p2GO.SetActive(false);

            // ==========================================
            // PANEL 3: CONFIRMACIÓN PERMANENTE
            // ==========================================
            GameObject p3GO = new GameObject("Step3_ConfirmPanel");
            p3GO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform p3RT = p3GO.AddComponent<RectTransform>();
            p3RT.anchorMin = new Vector2(0.5f, 0.5f);
            p3RT.anchorMax = new Vector2(0.5f, 0.5f);
            p3RT.sizeDelta = new Vector2(720, 420);
            p3RT.anchoredPosition = Vector2.zero;
            Image p3Bg = p3GO.AddComponent<Image>();
            p3Bg.color = new Color(0.12f, 0.14f, 0.20f, 0.99f);

            CreateTextElement(p3GO, "Title", "CONFIRMAR PROTAGONISTA", 32, new Vector2(0, 140), new Vector2(650, 45), new Color(1f, 0.88f, 0.25f));
            prologue.confirmPromptText = CreateTextElement(p3GO, "Prompt", "¿Estás seguro?\n\nEste será el protagonista de tu historia.\nNo podrás cambiarlo durante esta partida.", 22, new Vector2(0, 20), new Vector2(620, 160), Color.white);
            prologue.confirmCharacterBtn = CreateButton(p3GO, "ConfirmBtn", "CONFIRMAR", new Vector2(150, -130), new Vector2(220, 60), new Color(0.2f, 0.75f, 0.4f));
            prologue.backToSelectionBtn = CreateButton(p3GO, "BackBtn", "VOLVER", new Vector2(-150, -130), new Vector2(220, 60), new Color(0.5f, 0.55f, 0.6f));
            prologue.confirmCharacterPanel = p3GO;
            p3GO.SetActive(false);

            // ==========================================
            // PANEL 4: ELECCIÓN DE UNIFORME
            // ==========================================
            GameObject p4GO = new GameObject("Step4_OutfitPanel");
            p4GO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform p4RT = p4GO.AddComponent<RectTransform>();
            p4RT.anchorMin = new Vector2(0.5f, 0.5f);
            p4RT.anchorMax = new Vector2(0.5f, 0.5f);
            p4RT.sizeDelta = new Vector2(980, 680);
            p4RT.anchoredPosition = Vector2.zero;
            Image p4Bg = p4GO.AddComponent<Image>();
            p4Bg.color = new Color(0.14f, 0.16f, 0.22f, 0.98f);

            CreateTextElement(p4GO, "Title", "¿QUÉ UNIFORME DE CHEF QUIERES USAR?", 34, new Vector2(0, 270), new Vector2(900, 50), new Color(1f, 0.88f, 0.25f));
            CreateTextElement(p4GO, "Subtitle", "Elige tu vestimenta para trabajar dentro de la cocina del restaurante", 18, new Vector2(0, 225), new Vector2(850, 30), new Color(0.85f, 0.88f, 0.95f));

            // Card 1: Chef Negro
            GameObject cardBlackGO = new GameObject("Card_ChefBlack");
            cardBlackGO.transform.SetParent(p4GO.transform, false);
            RectTransform cbRT = cardBlackGO.AddComponent<RectTransform>();
            cbRT.sizeDelta = new Vector2(360, 460);
            cbRT.anchoredPosition = new Vector2(-220, -20);
            Image cbBg = cardBlackGO.AddComponent<Image>();
            cbBg.color = new Color(0.18f, 0.2f, 0.26f, 0.95f);

            CreateTextElement(cardBlackGO, "Title", "UNIFORME NEGRO", 24, new Vector2(0, 185), new Vector2(320, 35), Color.white);
            GameObject bImgGO = new GameObject("BlackImg");
            bImgGO.transform.SetParent(cardBlackGO.transform, false);
            RectTransform biRT = bImgGO.AddComponent<RectTransform>();
            biRT.sizeDelta = new Vector2(240, 300);
            biRT.anchoredPosition = new Vector2(0, 10);
            prologue.blackUniformPreviewImage = bImgGO.AddComponent<Image>();
            prologue.blackUniformPreviewImage.preserveAspect = true;
            prologue.chooseBlackOutfitBtn = CreateButton(cardBlackGO, "ChooseBlackBtn", "ELEGIR CHEF NEGRO", new Vector2(0, -180), new Vector2(280, 50), new Color(0.2f, 0.22f, 0.28f));

            // Card 2: Chef Blanco
            GameObject cardWhiteGO = new GameObject("Card_ChefWhite");
            cardWhiteGO.transform.SetParent(p4GO.transform, false);
            RectTransform cwRT = cardWhiteGO.AddComponent<RectTransform>();
            cwRT.sizeDelta = new Vector2(360, 460);
            cwRT.anchoredPosition = new Vector2(220, -20);
            Image cwBg = cardWhiteGO.AddComponent<Image>();
            cwBg.color = new Color(0.24f, 0.27f, 0.35f, 0.95f);

            CreateTextElement(cardWhiteGO, "Title", "UNIFORME BLANCO", 24, new Vector2(0, 185), new Vector2(320, 35), Color.white);
            GameObject wImgGO = new GameObject("WhiteImg");
            wImgGO.transform.SetParent(cardWhiteGO.transform, false);
            RectTransform wiRT = wImgGO.AddComponent<RectTransform>();
            wiRT.sizeDelta = new Vector2(240, 300);
            wiRT.anchoredPosition = new Vector2(0, 10);
            prologue.whiteUniformPreviewImage = wImgGO.AddComponent<Image>();
            prologue.whiteUniformPreviewImage.preserveAspect = true;
            prologue.chooseWhiteOutfitBtn = CreateButton(cardWhiteGO, "ChooseWhiteBtn", "ELEGIR CHEF BLANCO", new Vector2(0, -180), new Vector2(280, 50), new Color(0.85f, 0.72f, 0.3f));

            prologue.outfitSelectPanel = p4GO;
            p4GO.SetActive(false);

            // ==========================================
            // PANEL 5: BIENVENIDA Y APERTURA
            // ==========================================
            GameObject p5GO = new GameObject("Step5_WelcomeStoryPanel");
            p5GO.transform.SetParent(safeAreaGO.transform, false);
            RectTransform p5RT = p5GO.AddComponent<RectTransform>();
            p5RT.anchorMin = new Vector2(0.5f, 0.5f);
            p5RT.anchorMax = new Vector2(0.5f, 0.5f);
            p5RT.sizeDelta = new Vector2(800, 500);
            p5RT.anchoredPosition = Vector2.zero;
            Image p5Bg = p5GO.AddComponent<Image>();
            p5Bg.color = new Color(0.12f, 0.14f, 0.20f, 0.99f);

            CreateTextElement(p5GO, "Title", "¡LAS PUERTAS SE ABREN!", 36, new Vector2(0, 170), new Vector2(750, 50), new Color(1f, 0.88f, 0.25f));
            prologue.welcomeStoryText = CreateTextElement(p5GO, "Story", "¡Bienvenido a Villa del Chef!\n\nLas mesas están puestas, la huerta tiene sus primeros brotes y los clientes comenzarán a llegar pronto.\n\nPrepara tus fogones para deleitar al valle.", 22, new Vector2(0, 30), new Vector2(700, 180), Color.white);
            prologue.enterRestaurantBtn = CreateButton(p5GO, "EnterBtn", "ENTRAR AL RESTAURANTE 🍽️", new Vector2(0, -160), new Vector2(360, 65), new Color(0.2f, 0.75f, 0.4f));
            prologue.welcomeStoryPanel = p5GO;
            p5GO.SetActive(false);

            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[RestaurantSceneSetupEditor] 03_Prologue scene successfully created at {scenePath}!");
        }

        private static InputField CreateInputField(GameObject parent, string name, string placeholderText, Vector2 anchoredPos, Vector2 size)
        {
            GameObject inputGO = new GameObject(name);
            inputGO.transform.SetParent(parent.transform, false);
            RectTransform rt = inputGO.AddComponent<RectTransform>();
            rt.sizeDelta = size;
            rt.anchoredPosition = anchoredPos;

            Image bg = inputGO.AddComponent<Image>();
            bg.color = new Color(0.18f, 0.22f, 0.30f);

            InputField inputField = inputGO.AddComponent<InputField>();

            GameObject textGO = new GameObject("Text");
            textGO.transform.SetParent(inputGO.transform, false);
            RectTransform textRT = textGO.AddComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = new Vector2(10, 5);
            textRT.offsetMax = new Vector2(-10, -5);

            Text text = textGO.AddComponent<Text>();
            text.fontSize = 22;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleLeft;

            GameObject phGO = new GameObject("Placeholder");
            phGO.transform.SetParent(inputGO.transform, false);
            RectTransform phRT = phGO.AddComponent<RectTransform>();
            phRT.anchorMin = Vector2.zero;
            phRT.anchorMax = Vector2.one;
            phRT.offsetMin = new Vector2(10, 5);
            phRT.offsetMax = new Vector2(-10, -5);

            Text placeholder = phGO.AddComponent<Text>();
            placeholder.text = placeholderText;
            placeholder.fontSize = 20;
            placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            placeholder.fontStyle = FontStyle.Italic;
            placeholder.color = new Color(0.65f, 0.70f, 0.80f, 0.8f);
            placeholder.alignment = TextAnchor.MiddleLeft;

            inputField.textComponent = text;
            inputField.placeholder = placeholder;

            return inputField;
        }
    }
}
#endif
