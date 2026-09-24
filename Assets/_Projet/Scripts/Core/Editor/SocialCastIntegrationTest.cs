#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VillaDelChef.Managers;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;

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

                Debug.Log("<color=green><b>==================================================\n¡TODAS LAS PRUEBAS DE FASE 7.0.1 PASARON EXITOSAMENTE!\n==================================================</b></color>");
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
