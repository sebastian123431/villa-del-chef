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

            // 1. Cargar todos los CharacterSO
            var allCharacters = Resources.LoadAll<CharacterSO>("Characters");
            if (allCharacters == null || allCharacters.Length != 19)
            {
                Debug.LogError($"[TEST FALLIDO] Se esperaban 19 CharacterSO en Resources/Characters, pero se encontraron {allCharacters?.Length ?? 0}.");
                return false;
            }
            Debug.Log($"[TEST PASÓ] Se cargaron correctamente los 19 CharacterSO reales.");

            // 2. Simular SaveData con Player = dafne y Helper = alex
            SaveData testSave = new SaveData();
            testSave.selectedPlayerCharacterID = "dafne";
            testSave.selectedChefOutfit = CharacterOutfit.ChefBlack;
            testSave.selectedHelperCharacterID = "alex";
            testSave.helperChefOutfit = CharacterOutfit.ChefWhite;

            // Instanciar o configurar dummy CustomerManager
            GameObject testGO = new GameObject("Test_CustomerManager");
            CustomerManager cm = testGO.AddComponent<CustomerManager>();
            cm.availableFriends = new List<CharacterSO>(allCharacters);

            // Inyectar save simulado temporal
            if (SaveManager.Instance == null)
            {
                GameObject saveGO = new GameObject("Test_SaveManager");
                SaveManager sm = saveGO.AddComponent<SaveManager>();
                sm.GetType().GetField("currentSaveData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(sm, testSave);
            }
            else
            {
                SaveManager.Instance.SaveData.selectedPlayerCharacterID = "dafne";
                SaveManager.Instance.SaveData.selectedHelperCharacterID = "alex";
            }

            // 3. Probar 100 selecciones consecutivas de comensales
            HashSet<string> pickedFriends = new HashSet<string>();
            for (int i = 0; i < 100; i++)
            {
                CharacterSO picked = cm.SelectEligibleFriendAppearance();
                if (picked == null)
                {
                    Debug.LogError("[TEST FALLIDO] SelectEligibleFriendAppearance retornó null.");
                    Object.DestroyImmediate(testGO);
                    return false;
                }

                if (picked.characterID.Equals("dafne", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("[TEST FALLIDO] VIOLACIÓN DE EXCLUSIÓN: El protagonista 'dafne' apareció en el pool de clientes!");
                    Object.DestroyImmediate(testGO);
                    return false;
                }

                if (picked.characterID.Equals("alex", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("[TEST FALLIDO] VIOLACIÓN DE EXCLUSIÓN: El ayudante activo 'alex' apareció en el pool de clientes!");
                    Object.DestroyImmediate(testGO);
                    return false;
                }

                // Verificar que tenga arte y animador normal
                if (picked.normalPreview == null)
                {
                    Debug.LogError($"[TEST FALLIDO] El amigo '{picked.characterID}' no tiene normalPreview asignado.");
                    Object.DestroyImmediate(testGO);
                    return false;
                }

                if (picked.normalAnimator == null)
                {
                    Debug.LogError($"[TEST FALLIDO] El amigo '{picked.characterID}' no tiene normalAnimator asignado.");
                    Object.DestroyImmediate(testGO);
                    return false;
                }

                pickedFriends.Add(picked.characterID);
            }

            Debug.Log($"[TEST PASÓ] Exclusión de Player (dafne) y Helper (alex) verificada con 100 muestreos. Clientes únicos generados: {pickedFriends.Count}/17 elegibles.");

            // 4. Probar cambio dinámico de ayudante: alex -> carlos
            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                SaveManager.Instance.SaveData.selectedHelperCharacterID = "carlos";
            }

            bool alexReentered = false;
            for (int i = 0; i < 100; i++)
            {
                CharacterSO picked = cm.SelectEligibleFriendAppearance();
                if (picked.characterID.Equals("carlos", System.StringComparison.OrdinalIgnoreCase))
                {
                    Debug.LogError("[TEST FALLIDO] VIOLACIÓN DE EXCLUSIÓN: El nuevo ayudante 'carlos' apareció en el pool de clientes!");
                    Object.DestroyImmediate(testGO);
                    return false;
                }

                if (picked.characterID.Equals("alex", System.StringComparison.OrdinalIgnoreCase))
                {
                    alexReentered = true;
                }
            }

            if (!alexReentered)
            {
                Debug.LogError("[TEST FALLIDO] 'alex' no volvió al pool de clientes tras dejar de ser ayudante.");
                Object.DestroyImmediate(testGO);
                return false;
            }

            Debug.Log("[TEST PASÓ] Rotación dinámica de ayudantes verificada: 'carlos' fue excluido y 'alex' se reintegró exitosamente al pool de clientes.");

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
                            Object.DestroyImmediate(testGO);
                            return false;
                        }
                    }
                }
            }
            Debug.Log("[TEST PASÓ] Los 7 NPCs comerciantes oficiales conservan su estado independiente [PENDIENTE ARTE NPC OFICIAL].");

            Object.DestroyImmediate(testGO);
            Debug.Log("<color=green><b>==================================================\n¡TODAS LAS PRUEBAS DEL ELENCO SOCIAL PASARON EXITOSAMENTE!\n==================================================</b></color>");
            return true;
        }
    }
}
#endif
