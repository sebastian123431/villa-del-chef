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

                Debug.Log("<color=green><b>==================================================\n¡TODAS LAS PRUEBAS DEL ELENCO SOCIAL PASARON EXITOSAMENTE!\n==================================================</b></color>");
                return true;
            }
            finally
            {
                // Limpieza de GameObjects temporales
                if (testGO != null) Object.DestroyImmediate(testGO);
                if (tempSaveGO != null) Object.DestroyImmediate(tempSaveGO);

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
