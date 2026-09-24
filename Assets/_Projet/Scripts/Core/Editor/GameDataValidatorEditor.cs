using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using VillaDelChef.Building;
using VillaDelChef.Cooking;
using VillaDelChef.Crafting;
using VillaDelChef.Customers;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.Core.Editor
{
    public static class GameDataValidatorEditor
    {
        [MenuItem("Tools/Villa del Chef/Validate Game Data", priority = 10)]
        public static void ValidateAllGameData()
        {
            int errorCount = 0;
            int warningCount = 0;

            Debug.Log("<color=cyan><b>[GameDataValidator] Iniciando validación completa de datos de juego...</b></color>");

            // 1. Ingredients
            var ingredients = Resources.LoadAll<IngredientSO>("Ingredients");
            var ingredientIds = new HashSet<string>();
            foreach (var ing in ingredients)
            {
                if (ing == null) continue;
                if (string.IsNullOrWhiteSpace(ing.ingredientID))
                {
                    Debug.LogError($"[GameDataValidator] IngredientSO '{ing.name}' no tiene ingredientID asignado.", ing);
                    errorCount++;
                }
                else if (!ingredientIds.Add(ing.ingredientID))
                {
                    Debug.LogError($"[GameDataValidator] ID duplicado en IngredientSO: '{ing.ingredientID}' en asset '{ing.name}'.", ing);
                    errorCount++;
                }
            }

            // 2. Recipes
            var recipes = Resources.LoadAll<RecipeSO>("Recipes");
            var recipeIds = new HashSet<string>();
            foreach (var rec in recipes)
            {
                if (rec == null) continue;
                if (string.IsNullOrWhiteSpace(rec.recipeID))
                {
                    Debug.LogError($"[GameDataValidator] RecipeSO '{rec.name}' no tiene recipeID asignado.", rec);
                    errorCount++;
                }
                else if (!recipeIds.Add(rec.recipeID))
                {
                    Debug.LogError($"[GameDataValidator] ID duplicado en RecipeSO: '{rec.recipeID}' en asset '{rec.name}'.", rec);
                    errorCount++;
                }

                if (rec.requiredIngredients == null || rec.requiredIngredients.Count == 0)
                {
                    Debug.LogWarning($"[GameDataValidator] RecipeSO '{rec.recipeName}' ({rec.name}) no tiene ingredientes requeridos.", rec);
                    warningCount++;
                }
            }

            // 3. Crafting Recipes
            var craftingRecipes = Resources.LoadAll<CraftingRecipeSO>("CraftingRecipes");
            var craftIds = new HashSet<string>();
            foreach (var cr in craftingRecipes)
            {
                if (cr == null) continue;
                if (string.IsNullOrWhiteSpace(cr.craftID))
                {
                    Debug.LogError($"[GameDataValidator] CraftingRecipeSO '{cr.name}' no tiene craftID asignado.", cr);
                    errorCount++;
                }
                else if (!craftIds.Add(cr.craftID))
                {
                    Debug.LogError($"[GameDataValidator] ID duplicado en CraftingRecipeSO: '{cr.craftID}' en asset '{cr.name}'.", cr);
                    errorCount++;
                }
            }

            // 4. Vendors
            var vendors = Resources.LoadAll<VendorSO>("Vendors");
            var vendorIds = new HashSet<string>();
            foreach (var v in vendors)
            {
                if (v == null) continue;
                if (string.IsNullOrWhiteSpace(v.vendorID))
                {
                    Debug.LogError($"[GameDataValidator] VendorSO '{v.name}' no tiene vendorID asignado.", v);
                    errorCount++;
                }
                else if (!vendorIds.Add(v.vendorID))
                {
                    Debug.LogError($"[GameDataValidator] ID duplicado en VendorSO: '{v.vendorID}' en asset '{v.name}'.", v);
                    errorCount++;
                }
            }

            // 5. NPCs
            var npcs = Resources.LoadAll<NPCSO>("NPC");
            var npcIds = new HashSet<string>();
            foreach (var npc in npcs)
            {
                if (npc == null) continue;
                if (string.IsNullOrWhiteSpace(npc.npcID))
                {
                    Debug.LogError($"[GameDataValidator] NPCSO '{npc.name}' no tiene npcID asignado.", npc);
                    errorCount++;
                }
                else if (!npcIds.Add(npc.npcID))
                {
                    Debug.LogError($"[GameDataValidator] ID duplicado en NPCSO: '{npc.npcID}' en asset '{npc.name}'.", npc);
                    errorCount++;
                }

                if (npc.vendorData == null)
                {
                    Debug.LogWarning($"[GameDataValidator] NPCSO '{npc.npcName}' ({npc.name}) no tiene vendorData asignado.", npc);
                    warningCount++;
                }

                if (npc.characterReference == null)
                {
                    Debug.Log($"[GameDataValidator] NPCSO '{npc.npcName}' ({npc.npcID}): [PENDIENTE ARTE NPC OFICIAL - Placeholder temporal preservado]");
                }
            }

            // 6. Customers
            var customers = Resources.LoadAll<CustomerSO>("Customers");
            var customerIds = new HashSet<string>();
            foreach (var cust in customers)
            {
                if (cust == null) continue;
                if (string.IsNullOrWhiteSpace(cust.customerID))
                {
                    Debug.LogError($"[GameDataValidator] CustomerSO '{cust.name}' no tiene customerID asignado.", cust);
                    errorCount++;
                }
                else if (!customerIds.Add(cust.customerID))
                {
                    Debug.LogError($"[GameDataValidator] ID duplicado en CustomerSO: '{cust.customerID}' en asset '{cust.name}'.", cust);
                    errorCount++;
                }
            }

            // 7. Expansions & Grid Bounds
            const int gridW = 32;
            const int gridH = 24;
            var expansions = Resources.LoadAll<ExpansionSO>("Expansions");
            var expIds = new HashSet<string>();
            foreach (var exp in expansions)
            {
                if (exp == null) continue;
                if (string.IsNullOrWhiteSpace(exp.expansionID))
                {
                    Debug.LogError($"[GameDataValidator] ExpansionSO '{exp.name}' no tiene expansionID asignado.", exp);
                    errorCount++;
                }
                else if (!expIds.Add(exp.expansionID))
                {
                    Debug.LogError($"[GameDataValidator] ID duplicado en ExpansionSO: '{exp.expansionID}' en asset '{exp.name}'.", exp);
                    errorCount++;
                }

                if (exp.gridBounds.xMin < 0 || exp.gridBounds.yMin < 0 ||
                    exp.gridBounds.xMax > gridW || exp.gridBounds.yMax > gridH)
                {
                    Debug.LogError($"[GameDataValidator] ExpansionSO '{exp.displayName}' ({exp.name}) está fuera de los límites del Grid ({gridW}x{gridH}): Bounds={exp.gridBounds}", exp);
                    errorCount++;
                }

                // Check intersection with public Market promenade (y >= 19)
                if (exp.gridBounds.yMax > 19)
                {
                    Debug.LogError($"[GameDataValidator] ExpansionSO '{exp.displayName}' invade la zona Market pública (y >= 19): Bounds={exp.gridBounds}", exp);
                    errorCount++;
                }
            }

            // 8. Furniture
            var furnitures = Resources.LoadAll<FurnitureSO>("Furniture");
            var furnitureIds = new HashSet<string>();
            foreach (var f in furnitures)
            {
                if (f == null) continue;
                if (string.IsNullOrWhiteSpace(f.furnitureID))
                {
                    Debug.LogError($"[GameDataValidator] FurnitureSO '{f.name}' no tiene furnitureID asignado.", f);
                    errorCount++;
                }
                else if (!furnitureIds.Add(f.furnitureID))
                {
                    Debug.LogError($"[GameDataValidator] ID duplicado en FurnitureSO: '{f.furnitureID}' en asset '{f.name}'.", f);
                    errorCount++;
                }
            }

            // 9. Characters (Fase 7)
            var characters = Resources.LoadAll<CharacterSO>("Characters");
            var charIds = new HashSet<string>();
            foreach (var ch in characters)
            {
                if (ch == null) continue;
                if (string.IsNullOrWhiteSpace(ch.characterID))
                {
                    Debug.LogError($"[GameDataValidator] CharacterSO '{ch.name}' no tiene characterID asignado.", ch);
                    errorCount++;
                }
                else if (!charIds.Add(ch.characterID))
                {
                    Debug.LogError($"[GameDataValidator] ID duplicado en CharacterSO: '{ch.characterID}' en asset '{ch.name}'.", ch);
                    errorCount++;
                }

                if (string.IsNullOrWhiteSpace(ch.displayName))
                {
                    Debug.LogWarning($"[GameDataValidator] CharacterSO '{ch.characterID}' no tiene displayName asignado.", ch);
                    warningCount++;
                }

                if (ch.normalPreview == null)
                {
                    Debug.LogWarning($"[GameDataValidator] CharacterSO '{ch.characterID}' no tiene normalPreview (_rnormal).", ch);
                    warningCount++;
                }

                if (ch.blackChefPreview == null)
                {
                    Debug.LogWarning($"[GameDataValidator] CharacterSO '{ch.characterID}' no tiene blackChefPreview (_rnchef).", ch);
                    warningCount++;
                }

                if (ch.whiteChefPreview == null)
                {
                    Debug.LogWarning($"[GameDataValidator] CharacterSO '{ch.characterID}' no tiene whiteChefPreview (_rbchef).", ch);
                    warningCount++;
                }

                if (ch.normalAnimator == null && ch.normalPreview != null)
                {
                    Debug.LogWarning($"[GameDataValidator] CharacterSO '{ch.characterID}' no tiene normalAnimator configurado.", ch);
                    warningCount++;
                }

                if (ch.blackChefAnimator == null && ch.blackChefPreview != null)
                {
                    Debug.LogWarning($"[GameDataValidator] CharacterSO '{ch.characterID}' no tiene blackChefAnimator configurado.", ch);
                    warningCount++;
                }

                if (ch.whiteChefAnimator == null && ch.whiteChefPreview != null)
                {
                    Debug.LogWarning($"[GameDataValidator] CharacterSO '{ch.characterID}' no tiene whiteChefAnimator configurado.", ch);
                    warningCount++;
                }
            }

            // 10. Friends Art Directory Audit (Check ambiguous files without touching PNGs)
            string friendsPath = "Assets/_Projet/Art/Characters/Friends";
            if (System.IO.Directory.Exists(friendsPath))
            {
                string[] allPngs = System.IO.Directory.GetFiles(friendsPath, "*.png", System.IO.SearchOption.AllDirectories);
                foreach (string pngPath in allPngs)
                {
                    string fileName = System.IO.Path.GetFileName(pngPath).ToLower();
                    if (fileName.Contains("rbnormal"))
                    {
                        Debug.LogWarning($"[GameDataValidator] NOMENCLATURA AMBIGUA DETECTADA: Archivo '{pngPath}' contiene 'rbnormal' (clave no oficial). Conservar intacto y no renombrar automáticamente.");
                        warningCount++;
                    }
                    if (fileName == "movimientos.png")
                    {
                        Debug.LogWarning($"[GameDataValidator] NOMENCLATURA AMBIGUA DETECTADA: Archivo '{pngPath}' no especifica sufijo de vestuario (_rnormal, _rnchef, _rbchef). Conservar intacto.");
                        warningCount++;
                    }
                }
            }

            // Summary
            if (errorCount == 0 && warningCount == 0)
            {
                Debug.Log("<color=green><b>[GameDataValidator] ¡Validación exitosa! 0 errores, 0 advertencias en la base de datos.</b></color>");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Validación de Datos", "¡Validación completada con éxito!\n0 errores encontrados.", "Aceptar");
                }
            }
            else
            {
                Debug.LogWarning($"<color=yellow><b>[GameDataValidator] Validación finalizada con {errorCount} error(es) y {warningCount} advertencia(s). Revisa la Consola.</b></color>");
                if (!Application.isBatchMode)
                {
                    EditorUtility.DisplayDialog("Validación de Datos", $"Validación finalizada con:\n• {errorCount} errores\n• {warningCount} advertencias\n\nRevisa la Consola para ver los detalles.", "Aceptar");
                }
            }
        }
    }
}
