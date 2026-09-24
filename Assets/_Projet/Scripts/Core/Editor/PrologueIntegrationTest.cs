#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using VillaDelChef.Save;
using VillaDelChef.ScriptableObjects;
using VillaDelChef.UI;

namespace VillaDelChef.EditorTools
{
    /// <summary>
    /// Suite de pruebas automatizadas dedicadas para PrologueController, Resume determinista y seguridad de identidad (Fase 7.0.4).
    /// </summary>
    public static class PrologueIntegrationTest
    {
        [MenuItem("Tools/Villa del Chef/Test/Run Prologue Integration Test", false, 21)]
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
            Debug.Log("INICIANDO TEST DE INTEGRACIÓN: PRÓLOGO Y RESUME SAFETY (FASE 7.0.4)");
            Debug.Log("==================================================");

            // Aislamiento riguroso del SaveData del usuario
            SaveData originalSaveRef = null;
            string originalSaveJson = null;
            if (SaveManager.Instance != null && SaveManager.Instance.SaveData != null)
            {
                originalSaveRef = SaveManager.Instance.SaveData;
                originalSaveJson = JsonUtility.ToJson(originalSaveRef);
            }

            GameObject tempSaveGO = null;
            if (SaveManager.Instance == null)
            {
                tempSaveGO = new GameObject("Temp_SaveManager_PrologueTest");
                var sm = tempSaveGO.AddComponent<SaveManager>();
                typeof(SaveManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.SetValue(null, sm);
                sm.GetType().GetField("currentSaveData", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(sm, new SaveData());
            }

            GameObject testRoot = new GameObject("Test_PrologueIntegrationRoot");

            try
            {
                var allCharacters = Resources.LoadAll<CharacterSO>("Characters");
                if (allCharacters == null || allCharacters.Length == 0)
                {
                    Debug.LogError("[TEST FALLIDO] No se encontraron CharacterSO en Resources/Characters.");
                    return false;
                }

                CharacterSO andresArica = System.Array.Find(allCharacters, c => c.characterID.Equals("andres_arica", System.StringComparison.OrdinalIgnoreCase));
                CharacterSO alex = allCharacters[0];

                if (andresArica == null)
                {
                    Debug.LogError("[TEST FALLIDO] Andrés Arica no fue localizado en Resources/Characters.");
                    return false;
                }

                // ----------------------------------------------------
                // 1. ResolveResumeStep: Traje guardado inválido fuerza Paso 4
                // ----------------------------------------------------
                var saveTest1 = new SaveData();
                saveTest1.playerName = "Chef Test";
                saveTest1.selectedPlayerCharacterID = andresArica.characterID;
                saveTest1.playerCharacterLocked = true;
                saveTest1.selectedChefOutfit = CharacterOutfit.ChefWhite; // Incompleto para Andrés
                saveTest1.prologueStep = 5;
                saveTest1.prologueCompleted = false;

                int step1 = PrologueController.ResolveResumeStep(saveTest1, andresArica);
                if (step1 != 4)
                {
                    Debug.LogError($"[TEST FALLIDO] ResolveResumeStep retornó paso {step1} en vez de 4 ante un uniforme guardado incompleto.");
                    return false;
                }
                if (saveTest1.selectedChefOutfit != CharacterOutfit.ChefWhite)
                {
                    Debug.LogError("[TEST FALLIDO] ResolveResumeStep mutó silenciosamente el SaveData al resolver el paso.");
                    return false;
                }
                Debug.Log("[TEST PASÓ] 1. ResolveResumeStep fuerza correctamente Paso 4 ante atuendo guardado incompleto sin mutar el Save.");

                // ----------------------------------------------------
                // 2. ResolveResumeStep: Traje guardado válido preserva Paso 5
                // ----------------------------------------------------
                var saveTest2 = new SaveData();
                saveTest2.playerName = "Chef Test";
                saveTest2.selectedPlayerCharacterID = andresArica.characterID;
                saveTest2.playerCharacterLocked = true;
                saveTest2.selectedChefOutfit = CharacterOutfit.ChefBlack; // Completo para Andrés
                saveTest2.prologueStep = 5;
                saveTest2.prologueCompleted = false;

                int step2 = PrologueController.ResolveResumeStep(saveTest2, andresArica);
                if (step2 != 5)
                {
                    Debug.LogError($"[TEST FALLIDO] ResolveResumeStep retornó paso {step2} en vez de 5 ante un uniforme válido.");
                    return false;
                }
                Debug.Log("[TEST PASÓ] 2. ResolveResumeStep preserva Paso 5 cuando el uniforme guardado es 100% completo.");

                // ----------------------------------------------------
                // 3. ResolveResumeStep: Retroceso a Paso 2 si personaje es nulo en Paso >= 4
                // ----------------------------------------------------
                var saveTest3 = new SaveData();
                saveTest3.playerName = "Chef Test";
                saveTest3.prologueStep = 4;
                saveTest3.prologueCompleted = false;

                int step3 = PrologueController.ResolveResumeStep(saveTest3, null);
                if (step3 != 2)
                {
                    Debug.LogError($"[TEST FALLIDO] ResolveResumeStep retornó paso {step3} en vez de 2 cuando selectedCharacter es null.");
                    return false;
                }
                Debug.Log("[TEST PASÓ] 3. ResolveResumeStep retrocede a Paso 2 si no hay personaje válido seleccionado.");

                // ----------------------------------------------------
                // 4. ResolveResumeStep: Retroceso a Paso 1 si nombre está vacío
                // ----------------------------------------------------
                var saveTest4 = new SaveData();
                saveTest4.playerName = "";
                saveTest4.prologueStep = 3;
                saveTest4.prologueCompleted = false;

                int step4 = PrologueController.ResolveResumeStep(saveTest4, null);
                if (step4 != 1)
                {
                    Debug.LogError($"[TEST FALLIDO] ResolveResumeStep retornó paso {step4} en vez de 1 cuando playerName está vacío.");
                    return false;
                }
                Debug.Log("[TEST PASÓ] 4. ResolveResumeStep retrocede a Paso 1 si el nombre del jugador no ha sido guardado.");

                // ----------------------------------------------------
                // 5. CanEnterRestaurant: Rechazo estricto ante selectedCharacter == null
                // ----------------------------------------------------
                var saveTest5 = new SaveData();
                saveTest5.playerName = "Chef Test";
                bool allowed5 = PrologueController.CanEnterRestaurant(saveTest5, null, CharacterOutfit.ChefBlack, out string reason5);
                if (allowed5 || string.IsNullOrEmpty(reason5))
                {
                    Debug.LogError("[TEST FALLIDO] CanEnterRestaurant permitió continuar con selectedCharacter == null (falla de eliminación de fallback silencioso).");
                    return false;
                }
                Debug.Log("[TEST PASÓ] 5. CanEnterRestaurant rechaza terminantemente el ingreso si selectedCharacter es null (0 fallback silencioso a Alex).");

                // ----------------------------------------------------
                // 6. CanEnterRestaurant: Rechazo por discrepancia de identidad en partida bloqueada
                // ----------------------------------------------------
                var saveTest6 = new SaveData();
                saveTest6.playerName = "Chef Test";
                saveTest6.playerCharacterLocked = true;
                saveTest6.selectedPlayerCharacterID = "diego_vallenar";

                bool allowed6 = PrologueController.CanEnterRestaurant(saveTest6, alex, CharacterOutfit.ChefBlack, out string reason6);
                if (allowed6 || !reason6.Contains("Discrepancia de identidad"))
                {
                    Debug.LogError("[TEST FALLIDO] CanEnterRestaurant no detectó la discrepancia entre el ID bloqueado y el personaje suministrado.");
                    return false;
                }
                Debug.Log("[TEST PASÓ] 6. CanEnterRestaurant bloquea la sustitución de identidad en partidas con protagonista bloqueado.");

                // ----------------------------------------------------
                // 7. CanEnterRestaurant: Rechazo de atuendo incompleto
                // ----------------------------------------------------
                var saveTest7 = new SaveData();
                saveTest7.playerName = "Chef Test";
                saveTest7.playerCharacterLocked = true;
                saveTest7.selectedPlayerCharacterID = andresArica.characterID;

                bool allowed7 = PrologueController.CanEnterRestaurant(saveTest7, andresArica, CharacterOutfit.ChefWhite, out string reason7);
                if (allowed7)
                {
                    Debug.LogError("[TEST FALLIDO] CanEnterRestaurant permitió ingresar con ChefWhite incompleto en Andrés Arica.");
                    return false;
                }
                Debug.Log("[TEST PASÓ] 7. CanEnterRestaurant rechaza el ingreso con atuendo de chef incompleto.");

                // ----------------------------------------------------
                // 8. CanEnterRestaurant: Admisión de protagonista y atuendo válidos
                // ----------------------------------------------------
                bool allowed8 = PrologueController.CanEnterRestaurant(saveTest7, andresArica, CharacterOutfit.ChefBlack, out string reason8);
                if (!allowed8)
                {
                    Debug.LogError($"[TEST FALLIDO] CanEnterRestaurant rechazó una combinación 100% válida: {reason8}");
                    return false;
                }
                Debug.Log("[TEST PASÓ] 8. CanEnterRestaurant admite el ingreso al restaurante cuando personaje y atuendo son válidos y consistentes.");

                // ----------------------------------------------------
                // 9. Simulación de Ciclo de Vida y UI de PrologueController
                // ----------------------------------------------------
                var prologueGO = new GameObject("Test_PrologueControllerActor");
                prologueGO.transform.SetParent(testRoot.transform);
                var controller = prologueGO.AddComponent<PrologueController>();

                // Mock de jerarquía UI
                controller.nameInputPanel = new GameObject("P1_Name");
                controller.nameInputPanel.transform.SetParent(prologueGO.transform);

                controller.characterSelectPanel = new GameObject("P2_Character");
                controller.characterSelectPanel.transform.SetParent(prologueGO.transform);

                controller.confirmCharacterPanel = new GameObject("P3_Confirm");
                controller.confirmCharacterPanel.transform.SetParent(prologueGO.transform);

                controller.outfitSelectPanel = new GameObject("P4_Outfit");
                controller.outfitSelectPanel.transform.SetParent(prologueGO.transform);

                controller.welcomeStoryPanel = new GameObject("P5_Welcome");
                controller.welcomeStoryPanel.transform.SetParent(prologueGO.transform);

                var blackImgGO = new GameObject("BlackImg");
                blackImgGO.transform.SetParent(controller.outfitSelectPanel.transform);
                controller.blackUniformPreviewImage = blackImgGO.AddComponent<Image>();

                var whiteImgGO = new GameObject("WhiteImg");
                whiteImgGO.transform.SetParent(controller.outfitSelectPanel.transform);
                controller.whiteUniformPreviewImage = whiteImgGO.AddComponent<Image>();

                var blackBtnGO = new GameObject("BlackBtn");
                blackBtnGO.transform.SetParent(controller.outfitSelectPanel.transform);
                controller.chooseBlackOutfitBtn = blackBtnGO.AddComponent<Button>();

                var whiteBtnGO = new GameObject("WhiteBtn");
                whiteBtnGO.transform.SetParent(controller.outfitSelectPanel.transform);
                controller.chooseWhiteOutfitBtn = whiteBtnGO.AddComponent<Button>();

                // Inyectar personaje seleccionado
                var charField = typeof(PrologueController).GetField("selectedCharacter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                charField?.SetValue(controller, andresArica);

                // Disparar paso 4 (Selección de atuendo)
                controller.ShowStep(4);

                if (!controller.chooseBlackOutfitBtn.interactable)
                {
                    Debug.LogError("[TEST FALLIDO] chooseBlackOutfitBtn debería estar interactuable para Andrés Arica.");
                    return false;
                }

                if (controller.chooseWhiteOutfitBtn.interactable)
                {
                    Debug.LogError("[TEST FALLIDO] chooseWhiteOutfitBtn debería estar deshabilitado para Andrés Arica.");
                    return false;
                }

                if (controller.whiteUniformPreviewImage.sprite != null)
                {
                    Debug.LogError("[TEST FALLIDO] whiteUniformPreviewImage tiene un sprite asignado cuando el preview de ChefWhite no existe (violación de fallback estricto).");
                    return false;
                }

                // Simular elección de atuendo incompleto por código
                var chosenMethod = typeof(PrologueController).GetMethod("OnOutfitChosen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                chosenMethod?.Invoke(controller, new object[] { CharacterOutfit.ChefWhite });

                var outfitField = typeof(PrologueController).GetField("selectedOutfit", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                var currentOutfit = (CharacterOutfit)(outfitField?.GetValue(controller) ?? CharacterOutfit.Normal);

                if (currentOutfit == CharacterOutfit.ChefWhite)
                {
                    Debug.LogError("[TEST FALLIDO] OnOutfitChosen asignó ChefWhite a pesar de no estar completo.");
                    return false;
                }

                // Elegir atuendo válido (ChefBlack)
                chosenMethod?.Invoke(controller, new object[] { CharacterOutfit.ChefBlack });
                currentOutfit = (CharacterOutfit)(outfitField?.GetValue(controller) ?? CharacterOutfit.Normal);

                if (currentOutfit != CharacterOutfit.ChefBlack || !controller.OutfitConfirmedThisSession)
                {
                    Debug.LogError("[TEST FALLIDO] OnOutfitChosen no confirmó correctamente ChefBlack.");
                    return false;
                }

                Debug.Log("[TEST PASÓ] 9. Simulación completa de PrologueController: Botones e imágenes configurados según disponibilidad real y rechazo estricto de atuendo incompleto en runtime.");

                Debug.Log("<color=green><b>==================================================\n¡TODAS LAS 9 PRUEBAS DE PROLOGUE INTEGRATION PASARON EXITOSAMENTE (100%)!\n==================================================</b></color>");
                return true;
            }
            finally
            {
                if (testRoot != null) Object.DestroyImmediate(testRoot);

                if (tempSaveGO != null)
                {
                    typeof(SaveManager).GetProperty("Instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)?.SetValue(null, null);
                    Object.DestroyImmediate(tempSaveGO);
                }

                // Restaurar SaveData original del usuario
                if (originalSaveRef != null && !string.IsNullOrEmpty(originalSaveJson))
                {
                    JsonUtility.FromJsonOverwrite(originalSaveJson, originalSaveRef);
                }
            }
        }
    }
}
#endif
