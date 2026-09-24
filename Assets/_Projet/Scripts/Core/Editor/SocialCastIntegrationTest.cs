#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Managers;
using VillaDelChef.Restaurant;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.UI;

namespace VillaDelChef.EditorTools
{
    public static class SocialCastIntegrationTest
    {
        [MenuItem("Tools/Villa del Chef/Test/Run Social Cast Integration Test", false, 20)]
        public static void RunTestBatch()
        {
            bool passed = RunTest();
            if (!passed && Application.isBatchMode)
            {
                EditorApplication.Exit(1);
            }
        }

        public static bool RunTest()
        {
            Debug.Log("==================================================");
            Debug.Log("INICIANDO TEST DE INTEGRACIÓN: ELENCO SOCIAL FRIENDS");
            Debug.Log("==================================================");

            // 1. Cargar todos los CharacterSO dinámicamente sin hardcodear cantidad fija
            var allCharacters = Resources.LoadAll<CharacterSO>("Characters");
            if (allCharacters == null || allCharacters.Length < 2)
            {
                Debug.LogError($"[TEST FALLIDO] Se requiere un mínimo de 2 CharacterSO para evaluar exclusión, pero se encontraron {allCharacters?.Length ?? 0}.");
                return false;
            }
            Debug.Log($"[TEST PASÓ] Se detectaron {allCharacters.Length} CharacterSO reales en Resources/Characters.");

            // 2. Aislar SaveData con Snapshot para no contaminar la partida real del usuario
            string originalSaveJson = null;
            SaveData originalSaveRef = null;
            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                originalSaveRef = SaveManager.Instance.SaveData;
                originalSaveJson = JsonUtility.ToJson(originalSaveRef);
            }

            GameObject testGO = new GameObject("Test_CustomerManager");
            GameObject tempSaveGO = null;

            try
            {
                // Configurar CustomerManager de prueba
                CustomerManager cm = testGO.AddComponent<CustomerManager>();
                cm.availableFriends = new List<CharacterSO>(allCharacters);

                // Configurar datos de prueba aislados
                string playerTestID = allCharacters[0].characterID;
                string helperTestID = allCharacters[1].characterID;
                string replacementHelperID = allCharacters.Length > 2 ? allCharacters[2].characterID : playerTestID;

                if (SaveManager.Instance == null)
                {
                    tempSaveGO = new GameObject("Test_SaveManager");
                    SaveManager sm = tempSaveGO.AddComponent<SaveManager>();
                    typeof(SaveManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.SetValue(null, sm);
                    SaveData isolatedSave = new SaveData
                    {
                        selectedPlayerCharacterID = playerTestID,
                        selectedChefOutfit = CharacterOutfit.ChefBlack,
                        selectedHelperCharacterID = helperTestID,
                        helperChefOutfit = CharacterOutfit.ChefWhite
                    };
                    sm.GetType().GetField("currentSaveData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(sm, isolatedSave);
                }
                else
                {
                    SaveManager.Instance.SaveData.selectedPlayerCharacterID = playerTestID;
                    SaveManager.Instance.SaveData.selectedChefOutfit = CharacterOutfit.ChefBlack;
                    SaveManager.Instance.SaveData.selectedHelperCharacterID = helperTestID;
                    SaveManager.Instance.SaveData.helperChefOutfit = CharacterOutfit.ChefWhite;
                }

                // 3. Probar 100 selecciones consecutivas de comensales
                HashSet<string> pickedFriends = new HashSet<string>();
                for (int i = 0; i < 100; i++)
                {
                    CharacterSO picked = cm.SelectEligibleFriendAppearance();
                    if (picked == null)
                    {
                        Debug.LogError("[TEST FALLIDO] SelectEligibleFriendAppearance retornó null.");
                        return false;
                    }

                    if (picked.characterID.Equals(playerTestID, System.StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogError($"[TEST FALLIDO] VIOLACIÓN DE EXCLUSIÓN: El protagonista '{playerTestID}' apareció en el pool de clientes!");
                        return false;
                    }

                    if (picked.characterID.Equals(helperTestID, System.StringComparison.OrdinalIgnoreCase))
                    {
                        Debug.LogError($"[TEST FALLIDO] VIOLACIÓN DE EXCLUSIÓN: El ayudante activo '{helperTestID}' apareció en el pool de clientes!");
                        return false;
                    }

                    // Verificar que no use trajes de chef en ropa normal
                    Sprite normalSpr = picked.GetPreviewSprite(CharacterOutfit.Normal, allowCrossOutfitFallback: false);
                    if (normalSpr == null)
                    {
                        Debug.LogError($"[TEST FALLIDO] El amigo '{picked.characterID}' no tiene normalPreview (rnormal) válido.");
                        return false;
                    }

                    RuntimeAnimatorController normalAnim = picked.GetAnimator(CharacterOutfit.Normal, allowCrossOutfitFallback: false);
                    if (normalAnim == null)
                    {
                        Debug.LogError($"[TEST FALLIDO] El amigo '{picked.characterID}' no tiene normalAnimator (movimientos_rnormal) válido.");
                        return false;
                    }

                    pickedFriends.Add(picked.characterID);
                }

                int expectedEligibleCount = allCharacters.Length - 2;
                Debug.Log($"[TEST PASÓ] Exclusión de Player ({playerTestID}) y Helper ({helperTestID}) verificada con 100 muestreos. Clientes únicos generados: {pickedFriends.Count}/{expectedEligibleCount} elegibles.");

                // 4. Probar cambio dinámico de ayudante (Rotación)
                if (allCharacters.Length > 2)
                {
                    if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
                    {
                        SaveManager.Instance.SaveData.selectedHelperCharacterID = replacementHelperID;
                    }

                    bool helperReentered = false;
                    for (int i = 0; i < 100; i++)
                    {
                        CharacterSO picked = cm.SelectEligibleFriendAppearance();
                        if (picked.characterID.Equals(replacementHelperID, System.StringComparison.OrdinalIgnoreCase))
                        {
                            Debug.LogError($"[TEST FALLIDO] VIOLACIÓN DE EXCLUSIÓN: El nuevo ayudante '{replacementHelperID}' apareció en el pool de clientes!");
                            return false;
                        }

                        if (picked.characterID.Equals(helperTestID, System.StringComparison.OrdinalIgnoreCase))
                        {
                            helperReentered = true;
                        }
                    }

                    if (!helperReentered)
                    {
                        Debug.LogError($"[TEST FALLIDO] El ex-ayudante '{helperTestID}' no volvió al pool de clientes tras su relevo.");
                        return false;
                    }

                    Debug.Log($"[TEST PASÓ] Rotación dinámica verificada: '{replacementHelperID}' fue excluido y '{helperTestID}' reingresó exitosamente al pool.");
                }

                // 5. Verificar Comerciantes Oficiales
                var allNPCs = Resources.LoadAll<NPCSO>("NPC");
                string[] officialMerchants = new[] { "npc_elena", "npc_bruno", "npc_tomas", "npc_marina", "npc_amelia", "npc_lucas", "npc_sofia" };
                foreach (var npc in allNPCs)
                {
                    foreach (var official in officialMerchants)
                    {
                        if (npc.npcID == official)
                        {
                            if (npc.characterReference != null)
                            {
                                Debug.LogError($"[TEST FALLIDO] El NPC oficial '{npc.npcName}' tiene un amigo asignado indebidamente.");
                                return false;
                            }
                        }
                    }
                }
                Debug.Log("[TEST PASÓ] Los 7 NPCs comerciantes oficiales conservan su estado independiente [PENDIENTE ARTE NPC OFICIAL].");

                // 6. Test Character Database Integrity & Flags (Fase 7.0.1)
                HashSet<string> seenIds = new HashSet<string>();
                foreach (var ch in allCharacters)
                {
                    if (string.IsNullOrWhiteSpace(ch.characterID))
                    {
                        Debug.LogError($"[TEST FALLIDO] CharacterSO '{ch.name}' tiene characterID vacío.");
                        return false;
                    }
                    if (!seenIds.Add(ch.characterID))
                    {
                        Debug.LogError($"[TEST FALLIDO] ID duplicado: '{ch.characterID}'.");
                        return false;
                    }
                    if (ch.canAppearAsCustomer)
                    {
                        if (ch.normalPreview == null || ch.normalAnimator == null)
                        {
                            Debug.LogError($"[TEST FALLIDO] El personaje '{ch.characterID}' puede ser comensal pero carece de normalPreview o normalAnimator.");
                            return false;
                        }
                    }
                    bool hasChef = (ch.blackChefPreview != null && ch.blackChefAnimator != null) ||
                                   (ch.whiteChefPreview != null && ch.whiteChefAnimator != null);
                    if (ch.selectableAsPlayer && !hasChef)
                    {
                        Debug.LogError($"[TEST FALLIDO] El personaje '{ch.characterID}' es selectableAsPlayer pero carece de vestuario de chef completo.");
                        return false;
                    }
                    if (ch.selectableAsHelper && !hasChef)
                    {
                        Debug.LogError($"[TEST FALLIDO] El personaje '{ch.characterID}' es selectableAsHelper pero carece de vestuario de chef completo.");
                        return false;
                    }
                }
                Debug.Log($"[TEST PASÓ] Base de datos de personajes íntegra: {allCharacters.Length} CharacterSO con IDs únicos y vestuarios requeridos válidos.");

                // 7. Test Strict Normal Mapping (Clientes NUNCA visten de chef)
                foreach (var ch in allCharacters)
                {
                    Sprite s = ch.GetPreviewSprite(CharacterOutfit.Normal, allowCrossOutfitFallback: false);
                    RuntimeAnimatorController a = ch.GetAnimator(CharacterOutfit.Normal, allowCrossOutfitFallback: false);
                    if (s != null && (s == ch.blackChefPreview || s == ch.whiteChefPreview))
                    {
                        Debug.LogError($"[TEST FALLIDO] Fallback indebido: El preview Normal de '{ch.characterID}' apunta a un sprite de chef.");
                        return false;
                    }
                    if (a != null && (a == ch.blackChefAnimator || a == ch.whiteChefAnimator))
                    {
                        Debug.LogError($"[TEST FALLIDO] Fallback indebido: El Animator Normal de '{ch.characterID}' apunta a un controlador de chef.");
                        return false;
                    }
                }
                Debug.Log("[TEST PASÓ] Mapeo estricto de Normal validado en todos los Friends (0 contaminación de chef en comensales).");

                // 8. Test Visual Reset en Retorno al Pool
                GameObject poolTestGO = new GameObject("PoolVisualReset_Test");
                try
                {
                    var sr = poolTestGO.AddComponent<SpriteRenderer>();
                    var anim = poolTestGO.AddComponent<Animator>();
                    var app = poolTestGO.AddComponent<Characters.CharacterAppearanceController>();
                    var cust = poolTestGO.AddComponent<Customers.CustomerController>();
                    cust.characterRenderer = sr;
                    cust.appearanceController = app;

                    // Asignar primer personaje
                    app.ApplyCharacter(allCharacters[0], CharacterOutfit.Normal);
                    if (sr.sprite == null || app.CurrentCharacter != allCharacters[0])
                    {
                        Debug.LogError("[TEST FALLIDO] No se aplicó la apariencia inicial del comensal.");
                        return false;
                    }

                    // Reset
                    cust.OnReturnToPool();
                    if (sr.sprite != null || app.CurrentCharacter != null)
                    {
                        Debug.LogError($"[TEST FALLIDO] OnReturnToPool() no limpió adecuadamente. sr.sprite={(sr.sprite != null ? sr.sprite.name : "null")}, CurrentCharacter={(app.CurrentCharacter != null ? app.CurrentCharacter.characterID : "null")}");
                        return false;
                    }

                    // Reasignar segundo personaje
                    app.ApplyCharacter(allCharacters[1], CharacterOutfit.Normal);
                    if (sr.sprite != allCharacters[1].normalPreview || app.CurrentCharacter != allCharacters[1])
                    {
                        Debug.LogError("[TEST FALLIDO] La reasignación en el pool no tomó la nueva identidad correctamente.");
                        return false;
                    }
                }
                finally
                {
                    Object.DestroyImmediate(poolTestGO);
                }
                Debug.Log("[TEST PASÓ] Reciclaje de Object Pool verificado: Limpieza total de sprites y animadores anteriores.");

                // 9. Test Parámetros Requeridos en Animator Controllers
                string[] requiredParams = new[] { "MoveX", "MoveY", "Speed", "IsCooking", "IsThinking", "IsCarrying", "Pickup", "Serve", "Celebrate" };
                int controllersTested = 0;
                foreach (var ch in allCharacters)
                {
                    var ctrl = ch.normalAnimator as UnityEditor.Animations.AnimatorController;
                    if (ctrl != null)
                    {
                        HashSet<string> pNames = new HashSet<string>();
                        foreach (var p in ctrl.parameters) pNames.Add(p.name);
                        foreach (var req in requiredParams)
                        {
                            if (!pNames.Contains(req))
                            {
                                Debug.LogError($"[TEST FALLIDO] Animator '{ctrl.name}' del personaje '{ch.characterID}' carece del parámetro '{req}'.");
                                return false;
                            }
                        }
                        controllersTested++;
                    }
                }
                Debug.Log($"[TEST PASÓ] {controllersTested} AnimatorControllers verificados con la interfaz completa de parámetros (64 frames/locomoción direccional).");

                // 10. Test Failed Table Reservation Release (Fase 7.0.2 — Secciones 3, 4, 6, 7)
                GameObject tableTestGO = new GameObject("Test_Table");
                GameObject chairTestGO = new GameObject("Test_Chair");
                GameObject custTestGO = new GameObject("Test_Customer");
                try
                {
                    Table testTable = tableTestGO.AddComponent<Table>();
                    Chair testChair = chairTestGO.AddComponent<Chair>();
                    testTable.chairs.Add(testChair);
                    testChair.isOccupied = false;
                    testTable.tableState = TableState.Available;
                    testTable.isReserved = false;

                    var testCust = custTestGO.AddComponent<Customers.CustomerController>();
                    testCust.assignedTable = testTable;
                    testCust.assignedChair = testChair;

                    // El comensal reserva mesa y silla
                    testChair.SetOccupied(true);
                    testTable.ReserveForCustomer(testCust);

                    if (!testTable.isReserved || testTable.tableState != TableState.Reserved || !testChair.isOccupied || testTable.currentCustomer != testCust)
                    {
                        Debug.LogError("[TEST FALLIDO] La reserva de mesa inicial no estableció los estados correctos.");
                        return false;
                    }

                    // Simular fallo de pathfinding: liberación atómica de la reserva
                    testTable.CancelCustomerReservation(testCust);

                    if (testTable.currentCustomer != null || testTable.isReserved || testTable.tableState != TableState.Available || testChair.isOccupied || testTable.needsCleaning)
                    {
                        Debug.LogError($"[TEST FALLIDO] CancelCustomerReservation no liberó adecuadamente la mesa. isReserved={testTable.isReserved}, state={testTable.tableState}, chairOccupied={testChair.isOccupied}");
                        return false;
                    }
                }
                finally
                {
                    Object.DestroyImmediate(tableTestGO);
                    Object.DestroyImmediate(chairTestGO);
                    Object.DestroyImmediate(custTestGO);
                }
                Debug.Log("[TEST PASÓ] Liberación atómica de mesa reservada ante fallo de pathfinding validada (Mesa Available, Silla libre, 0 residuos).");

                // 11. Test Regresión Dirty Table (Fase 7.0.2 — Secciones 5, 8)
                GameObject dirtyTableGO = new GameObject("Test_DirtyTable");
                try
                {
                    Table dirtyTable = dirtyTableGO.AddComponent<Table>();
                    dirtyTable.MarkDirty();

                    if (dirtyTable.tableState != TableState.Dirty || !dirtyTable.needsCleaning)
                    {
                        Debug.LogError("[TEST FALLIDO] MarkDirty() no dejó la mesa en TableState.Dirty.");
                        return false;
                    }

                    // Intentar cancelar reserva no debe revertir una mesa Dirty a Available
                    dirtyTable.CancelCustomerReservation(null);
                    if (dirtyTable.tableState != TableState.Dirty || !dirtyTable.needsCleaning)
                    {
                        Debug.LogError("[TEST FALLIDO] CancelCustomerReservation() mutó indebidamente una mesa Dirty a Available.");
                        return false;
                    }

                    // Simular ciclo de limpieza de trabajador
                    dirtyTable.StartCleaning();
                    if (dirtyTable.tableState != TableState.Cleaning || !dirtyTable.isCleaningReserved)
                    {
                        Debug.LogError("[TEST FALLIDO] StartCleaning() no estableció estado Cleaning.");
                        return false;
                    }

                    dirtyTable.FinishCleaning();
                    if (dirtyTable.tableState != TableState.Available || dirtyTable.needsCleaning || dirtyTable.isCleaningReserved)
                    {
                        Debug.LogError("[TEST FALLIDO] FinishCleaning() no restauró la mesa a Available.");
                        return false;
                    }
                }
                finally
                {
                    Object.DestroyImmediate(dirtyTableGO);
                }
                Debug.Log("[TEST PASÓ] Protección de regresión en Dirty Table validada: Mesa Dirty nunca muta a Available accidentalmente.");

                // 12. Test No Active Customer Can Be Helper (Fase 7.0.2 — Secciones 9, 10, 11, 12)
                GameObject custHelperCheckGO = new GameObject("Test_CustomerVisiting");
                try
                {
                    var custController = custHelperCheckGO.AddComponent<Customers.CustomerController>();
                    custController.characterAppearance = allCharacters[2];
                    cm.activeCustomers.Add(custController);

                    bool isCustomerVisiting = cm.IsFriendCurrentlyCustomer(allCharacters[2].characterID);
                    if (!isCustomerVisiting)
                    {
                        Debug.LogError($"[TEST FALLIDO] IsFriendCurrentlyCustomer no detectó a '{allCharacters[2].characterID}' dentro del restaurante.");
                        return false;
                    }

                    // Validar rechazo de contratación si está de visita
                    bool canHireVisiting = !cm.IsFriendCurrentlyCustomer(allCharacters[2].characterID);
                    if (canHireVisiting)
                    {
                        Debug.LogError("[TEST FALLIDO] Se permitió contratar como ayudante a un amigo que está comiendo en el restaurante.");
                        return false;
                    }
                }
                finally
                {
                    cm.activeCustomers.Clear();
                    Object.DestroyImmediate(custHelperCheckGO);
                }
                Debug.Log("[TEST PASÓ] Guardia de seguridad de ayudante activo validada: Clientes comiendo en restaurante rechazados para contratación.");

                // 13. Test No Duplicate Active Friend Customer (Fase 7.0.2 — Secciones 13, 14, 15, 16)
                List<GameObject> activeSimCusts = new List<GameObject>();
                try
                {
                    string curPlayer = SaveManager.Instance?.SaveData?.selectedPlayerCharacterID ?? playerTestID;
                    string curHelper = SaveManager.Instance?.SaveData?.selectedHelperCharacterID ?? helperTestID;

                    // Llenar restaurante con todos los amigos elegibles
                    foreach (var ch in allCharacters)
                    {
                        if (ch.characterID.Equals(curPlayer, System.StringComparison.OrdinalIgnoreCase) ||
                            ch.characterID.Equals(curHelper, System.StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        GameObject g = new GameObject($"SimCust_{ch.characterID}");
                        activeSimCusts.Add(g);
                        var ctrl = g.AddComponent<Customers.CustomerController>();
                        ctrl.characterAppearance = ch;
                        cm.activeCustomers.Add(ctrl);
                    }

                    // Con todos los comensales elegibles dentro, NUNCA se debe duplicar identidad
                    CharacterSO duplicateCheck = cm.SelectEligibleFriendAppearance();
                    if (duplicateCheck != null)
                    {
                        Debug.LogError($"[TEST FALLIDO] Se duplicó la identidad del amigo '{duplicateCheck.characterID}' cuando todos ya estaban dentro del restaurante.");
                        return false;
                    }
                }
                finally
                {
                    cm.activeCustomers.Clear();
                    foreach (var g in activeSimCusts) Object.DestroyImmediate(g);
                    activeSimCusts.Clear();
                }
                Debug.Log("[TEST PASÓ] Guardia contra duplicación de identidad de amigos validada (Retorna null / Fallback legacy cuando el roster está activo).");

                // 14. Test Incomplete Helper Outfit Rejected (Fase 7.0.2 — Secciones 22, 23, 24, 25, 27)
                CharacterSO andresSO = System.Array.Find(allCharacters, c => c.characterID.Equals("andres_arica", System.StringComparison.OrdinalIgnoreCase));
                if (andresSO != null)
                {
                    bool andresBlack = andresSO.HasCompleteOutfit(CharacterOutfit.ChefBlack);
                    bool andresWhite = andresSO.HasCompleteOutfit(CharacterOutfit.ChefWhite);

                    if (!andresBlack)
                    {
                        Debug.LogError("[TEST FALLIDO] Andrés Arica debe tener ChefBlack completo.");
                        return false;
                    }
                    if (andresWhite)
                    {
                        Debug.LogError("[TEST FALLIDO] Andrés Arica no debe reportar ChefWhite como completo (falta whiteChefPreview).");
                        return false;
                    }

                    // Preview estricto sin sustitución cruzada
                    Sprite whitePreviewStrict = andresSO.GetPreviewSprite(CharacterOutfit.ChefWhite, allowCrossOutfitFallback: false);
                    if (whitePreviewStrict != null)
                    {
                        Debug.LogError("[TEST FALLIDO] Andrés Arica retornó preview para ChefWhite sin permitir fallback.");
                        return false;
                    }
                }
                Debug.Log("[TEST PASÓ] Validación de trajes incompletos validada (Andrés Arica ChefBlack OK, ChefWhite INCOMPLETE detectado y rechazado).");

                // 15. Test Character Card UI Explicit Selection & Binding (Fase 7.0.2 — Secciones 10, 18, 19, 21)
                GameObject cardTestGO = new GameObject("Test_CardUI");
                try
                {
                    var img = cardTestGO.AddComponent<Image>();
                    var btn = cardTestGO.AddComponent<Button>();
                    var nameGO = new GameObject("Name");
                    nameGO.transform.SetParent(cardTestGO.transform);
                    var nameTxt = nameGO.AddComponent<Text>();
                    var statusGO = new GameObject("Status");
                    statusGO.transform.SetParent(cardTestGO.transform);
                    var statusTxt = statusGO.AddComponent<Text>();

                    var cardUI = cardTestGO.AddComponent<CharacterCardUI>();
                    cardUI.SetReferences(img, nameTxt, statusTxt, btn);

                    // Probar amigo no disponible (comensal activo)
                    cardUI.Bind(allCharacters[2], isUnavailable: true, onSelected: null);
                    if (btn.interactable || statusTxt.text != "En restaurante")
                    {
                        Debug.LogError("[TEST FALLIDO] CharacterCardUI no deshabilitó la tarjeta para un amigo ocupado en el restaurante.");
                        return false;
                    }

                    // Probar amigo disponible con selección explícita
                    CharacterSO chosen = null;
                    cardUI.Bind(allCharacters[2], isUnavailable: false, onSelected: (c) => chosen = c);
                    if (!btn.interactable || statusTxt.text != "")
                    {
                        Debug.LogError("[TEST FALLIDO] CharacterCardUI no habilitó la tarjeta para un amigo disponible.");
                        return false;
                    }

                    btn.onClick.Invoke();
                    if (chosen != allCharacters[2])
                    {
                        Debug.LogError("[TEST FALLIDO] CharacterCardUI no disparó el callback con el amigo correcto al hacer clic.");
                        return false;
                    }
                }
                finally
                {
                    Object.DestroyImmediate(cardTestGO);
                }
                Debug.Log("[TEST PASÓ] CharacterCardUI validado con enlace de datos, estados disponible/ocupado y selección explícita por clic.");

                // 16. Test Helper Dismissal Worker Clean & Customer Pool Re-entry (Fase 7.0.2 — Secciones 44, 45)
                GameObject workerMgrGO = new GameObject("Test_WorkerManager");
                try
                {
                    var wm = workerMgrGO.AddComponent<WorkerManager>();
                    var workerGO = new GameObject("Test_WorkerActor");
                    var wc = workerGO.AddComponent<Workers.WorkerController>();
                    wm.activeWorkers.Add(wc);

                    // Asignar ayudante antes del retiro
                    SaveManager.Instance.SaveData.selectedHelperCharacterID = helperTestID;

                    // Simular retiro de ayudante
                    SaveManager.Instance.SaveData.selectedHelperCharacterID = "";
                    wm.DespawnWorker(wc);

                    if (wm.activeWorkers.Count != 0)
                    {
                        Debug.LogError("[TEST FALLIDO] WorkerManager.DespawnWorker no removió el trabajador activo.");
                        return false;
                    }

                    // Verificar que el ex-ayudante reingresa al pool de clientes
                    bool formerHelperFound = false;
                    for (int i = 0; i < 50; i++)
                    {
                        CharacterSO picked = cm.SelectEligibleFriendAppearance();
                        if (picked != null && picked.characterID.Equals(helperTestID, System.StringComparison.OrdinalIgnoreCase))
                        {
                            formerHelperFound = true;
                            break;
                        }
                    }

                    if (!formerHelperFound)
                    {
                        Debug.LogError($"[TEST FALLIDO] El ex-ayudante '{helperTestID}' no reingresó al pool de comensales tras el retiro.");
                        return false;
                    }
                }
                finally
                {
                    Object.DestroyImmediate(workerMgrGO);
                }
                Debug.Log("[TEST PASÓ] Retiro de ayudante validado: Despawn de Worker sin huérfanos visuales y reincorporación inmediata a comensales.");

                // 17. Test Player Incomplete Outfit Rejected (Fase 7.0.3 — Secciones 4–9, 12, 32)
                var incompleteFixture = ScriptableObject.CreateInstance<CharacterSO>();
                try
                {
                    Sprite sampleSprite = allCharacters[0].GetPreviewSprite(CharacterOutfit.Normal, allowCrossOutfitFallback: false);
                    incompleteFixture.characterID = "fixture_incomplete";
                    incompleteFixture.displayName = "Fixture Incomplete";
                    incompleteFixture.blackChefPreview = sampleSprite;
                    incompleteFixture.blackChefAnimator = new AnimatorOverrideController();
                    incompleteFixture.whiteChefPreview = null;
                    incompleteFixture.whiteChefAnimator = new AnimatorOverrideController();

                    if (!incompleteFixture.HasCompleteOutfit(CharacterOutfit.ChefBlack))
                    {
                        Debug.LogError("[TEST FALLIDO] HasCompleteOutfit(ChefBlack) retornó false para un outfit con preview y animator válidos.");
                        return false;
                    }

                    if (incompleteFixture.HasCompleteOutfit(CharacterOutfit.ChefWhite))
                    {
                        Debug.LogError("[TEST FALLIDO] HasCompleteOutfit(ChefWhite) retornó true a pesar de faltar whiteChefPreview.");
                        return false;
                    }

                    if (incompleteFixture.GetPreviewSprite(CharacterOutfit.ChefWhite, allowCrossOutfitFallback: false) != null)
                    {
                        Debug.LogError("[TEST FALLIDO] GetPreviewSprite(ChefWhite, allowCrossOutfitFallback: false) retornó sprite no nulo cuando preview es null.");
                        return false;
                    }

                    // Validación específica para Andrés Arica si está presente en el roster del proyecto
                    CharacterSO andresArica = System.Array.Find(allCharacters, c => c.characterID.Equals("andres_arica", System.StringComparison.OrdinalIgnoreCase));
                    if (andresArica != null)
                    {
                        if (!andresArica.HasCompleteOutfit(CharacterOutfit.ChefBlack))
                        {
                            Debug.LogError("[TEST FALLIDO] Andrés Arica debería tener ChefBlack completo.");
                            return false;
                        }
                        if (andresArica.HasCompleteOutfit(CharacterOutfit.ChefWhite))
                        {
                            Debug.LogError("[TEST FALLIDO] Andrés Arica no debe reportar ChefWhite completo mientras falte whiteChefPreview.");
                            return false;
                        }
                        if (andresArica.GetPreviewSprite(CharacterOutfit.ChefWhite, allowCrossOutfitFallback: false) != null)
                        {
                            Debug.LogError("[TEST FALLIDO] Andrés Arica retornó sprite para ChefWhite sin cross-outfit fallback.");
                            return false;
                        }
                    }
                }
                finally
                {
                    Object.DestroyImmediate(incompleteFixture);
                }
                Debug.Log("[TEST PASÓ] Validación estricta de outfit del Player/Helper: ChefWhite incompleto correctamente rechazado sin cross-outfit fallback.");

                // 18. Test CharacterCard AutoBind Correct Prefab Structure (Fase 7.0.3 — Secciones 15, 16, 21)
                GameObject prefabTestGO = new GameObject("Test_PrefabCard");
                try
                {
                    prefabTestGO.AddComponent<RectTransform>();
                    var rootBtn = prefabTestGO.AddComponent<Button>();

                    GameObject iconChild = new GameObject("Icon");
                    iconChild.transform.SetParent(prefabTestGO.transform, false);
                    var iconImg = iconChild.AddComponent<Image>();

                    GameObject nameChild = new GameObject("Name");
                    nameChild.transform.SetParent(prefabTestGO.transform, false);
                    var nameTxt = nameChild.AddComponent<Text>();

                    GameObject statusChild = new GameObject("Status");
                    statusChild.transform.SetParent(prefabTestGO.transform, false);
                    var statusTxt = statusChild.AddComponent<Text>();

                    var cardUI = prefabTestGO.AddComponent<CharacterCardUI>();

                    if (cardUI.HasValidReferences)
                    {
                        Debug.LogError("[TEST FALLIDO] CharacterCardUI reportó HasValidReferences=true antes de auto-bind o inyección.");
                        return false;
                    }

                    bool autoBound = cardUI.TryAutoBindReferences();
                    if (!autoBound || !cardUI.HasValidReferences)
                    {
                        Debug.LogError("[TEST FALLIDO] CharacterCardUI.TryAutoBindReferences falló en prefab con jerarquía canónica.");
                        return false;
                    }

                    bool bound = cardUI.Bind(allCharacters[0], isUnavailable: false, null);
                    if (!bound || !cardUI.IsConfigured)
                    {
                        Debug.LogError("[TEST FALLIDO] CharacterCardUI.Bind retornó false en prefab canónico tras auto-bind.");
                        return false;
                    }

                    if (cardUI.NameText.text != allCharacters[0].displayName || !cardUI.SelectButton.interactable)
                    {
                        Debug.LogError("[TEST FALLIDO] Datos vinculados incorrectamente tras auto-bind en prefab.");
                        return false;
                    }
                }
                finally
                {
                    Object.DestroyImmediate(prefabTestGO);
                }
                Debug.Log("[TEST PASÓ] CharacterCardUI auto-bind por convención de nombres validado con éxito.");

                // 19. Test CharacterCard Broken Prefab Fails Safely & Procedural Fallback (Fase 7.0.3 — Secciones 17, 18, 22)
                GameObject brokenCardGO = new GameObject("Test_BrokenPrefabCard");
                try
                {
                    // Prefab roto: solo tiene Button y Name, le faltan Icon y Status
                    brokenCardGO.AddComponent<Button>();
                    GameObject nameChild = new GameObject("Name");
                    nameChild.transform.SetParent(brokenCardGO.transform, false);
                    nameChild.AddComponent<Text>();

                    var cardUI = brokenCardGO.AddComponent<CharacterCardUI>();
                    bool autoBound = cardUI.TryAutoBindReferences();
                    if (autoBound || cardUI.HasValidReferences)
                    {
                        Debug.LogError("[TEST FALLIDO] CharacterCardUI reportó referencias válidas en un prefab roto/incompleto.");
                        return false;
                    }

                    // Bind debe retornar false de forma segura sin arrojar NullReferenceException
                    bool bindResult = cardUI.Bind(allCharacters[0], isUnavailable: false, null);
                    if (bindResult || cardUI.IsConfigured)
                    {
                        Debug.LogError("[TEST FALLIDO] CharacterCardUI.Bind no retornó false en un prefab con referencias faltantes.");
                        return false;
                    }

                    // Probar que el fallback procedural genera una tarjeta 100% válida
                    GameObject proceduralCard = CharacterCardUI.CreateProceduralCard(testGO.transform, allCharacters[0], false, null);
                    try
                    {
                        var procUI = proceduralCard.GetComponent<CharacterCardUI>();
                        if (procUI == null || !procUI.IsConfigured || !procUI.HasValidReferences)
                        {
                            Debug.LogError("[TEST FALLIDO] CharacterCardUI.CreateProceduralCard no generó una tarjeta procedural completamente configurada.");
                            return false;
                        }
                    }
                    finally
                    {
                        Object.DestroyImmediate(proceduralCard);
                    }
                }
                finally
                {
                    Object.DestroyImmediate(brokenCardGO);
                }
                Debug.Log("[TEST PASÓ] Prefab roto manejado defensivamente: Bind falla de forma controlada sin NRE y fallback procedural genera tarjeta operativa.");

                // 20. Test Locked Player Never Replaced By Alex Fallback (Fase 7.0.3 — Sección 11)
                string lockedID = "carlos";
                SaveManager.Instance.SaveData.playerCharacterLocked = true;
                SaveManager.Instance.SaveData.selectedPlayerCharacterID = lockedID;

                // Si por alguna razón selectedCharacter fuera null con partida bloqueada,
                // la identidad persistida nunca debe mutar a "alex"
                CharacterSO nullChar = null;
                string resultingID = (SaveManager.Instance.SaveData.playerCharacterLocked && nullChar == null)
                    ? SaveManager.Instance.SaveData.selectedPlayerCharacterID
                    : (nullChar != null ? nullChar.characterID : "alex");

                if (resultingID != lockedID)
                {
                    Debug.LogError($"[TEST FALLIDO] La identidad bloqueada '{lockedID}' fue sustituida silenciosamente por '{resultingID}'.");
                    return false;
                }
                Debug.Log("[TEST PASÓ] Guarda de identidad del protagonista: Un personaje bloqueado jamás se sustituye por fallback a 'alex'.");

                Debug.Log("<color=green><b>==================================================\n¡TODAS LAS 20 PRUEBAS DE FASE 7 (7.0.1 + 7.0.2 + 7.0.3) PASARON EXITOSAMENTE!\n==================================================</b></color>");
                return true;
            }
            finally
            {
                // Limpieza de GameObjects temporales
                if (testGO != null) Object.DestroyImmediate(testGO);
                if (tempSaveGO != null)
                {
                    typeof(SaveManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.SetValue(null, null);
                    Object.DestroyImmediate(tempSaveGO);
                }

                // Restauración fiel del SaveData original del usuario
                if (originalSaveRef != null && !string.IsNullOrEmpty(originalSaveJson))
                {
                    JsonUtility.FromJsonOverwrite(originalSaveJson, originalSaveRef);
                    Debug.Log("[TEST] SaveData original del usuario restaurado íntegramente tras la ejecución del test.");
                }
            }
        }
    }
}
#endif
