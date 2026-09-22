#if UNITY_EDITOR
using System.IO;
using System.IO.Compression;
using UnityEditor;
using UnityEngine;

namespace VillaDelChef.EditorTools
{
    public static class DropFolderImporter
    {
        private const string DropFolderPath = "Assets/_Drop";
        private const string ArtRootPath = "Assets/_Projet/Art";

        [MenuItem("Tools/Villa del Chef/Import & Organize Assets from _Drop", false, 4)]
        public static void ImportAndOrganizeFromDrop()
        {
            if (!Directory.Exists(DropFolderPath))
            {
                Directory.CreateDirectory(DropFolderPath);
                Debug.Log("[DropFolderImporter] Carpeta _Drop creada.");
                return;
            }

            // 1. Extract any zip files
            string[] zipFiles = Directory.GetFiles(DropFolderPath, "*.zip", SearchOption.AllDirectories);
            foreach (var zip in zipFiles)
            {
                try
                {
                    string extractDir = Path.Combine(DropFolderPath, Path.GetFileNameWithoutExtension(zip));
                    ZipFile.ExtractToDirectory(zip, extractDir);
                    File.Delete(zip);
                    Debug.Log($"[DropFolderImporter] Archivo ZIP descomprimido: {Path.GetFileName(zip)}");
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"[DropFolderImporter] Error al descomprimir {zip}: {ex.Message}");
                }
            }

            // 2. Scan all image files (.png, .jpg)
            string[] imageFiles = Directory.GetFiles(DropFolderPath, "*.*", SearchOption.AllDirectories);
            int importedCount = 0;

            foreach (var file in imageFiles)
            {
                string ext = Path.GetExtension(file).ToLower();
                if (ext != ".png" && ext != ".jpg" && ext != ".jpeg") continue;

                string fileName = Path.GetFileName(file);
                string lowerName = fileName.ToLower();

                // Categorize destination
                string destSubfolder = "Furniture/Decorations";
                if (lowerName.Contains("chair") || lowerName.Contains("silla") || lowerName.Contains("stool") || lowerName.Contains("bench"))
                {
                    destSubfolder = "Furniture/Chairs";
                }
                else if (lowerName.Contains("table") || lowerName.Contains("mesa") || lowerName.Contains("desk"))
                {
                    destSubfolder = "Furniture/Tables";
                }
                else if (lowerName.Contains("stove") || lowerName.Contains("oven") || lowerName.Contains("grill") || lowerName.Contains("kitchen") || lowerName.Contains("cocina") || lowerName.Contains("fridge") || lowerName.Contains("refrigerator"))
                {
                    destSubfolder = "Furniture/Kitchen";
                }
                else if (lowerName.Contains("counter") || lowerName.Contains("bar") || lowerName.Contains("mostrador"))
                {
                    destSubfolder = "Furniture/Counters";
                }
                else if (lowerName.Contains("plant") || lowerName.Contains("flower") || lowerName.Contains("tree") || lowerName.Contains("bush") || lowerName.Contains("maceta"))
                {
                    destSubfolder = "Furniture/Plants";
                }
                else if (lowerName.Contains("customer") || lowerName.Contains("client") || lowerName.Contains("npc") || lowerName.Contains("guest"))
                {
                    destSubfolder = "Characters/Customers";
                }
                else if (lowerName.Contains("worker") || lowerName.Contains("waiter") || lowerName.Contains("mozo") || lowerName.Contains("ayudante") || lowerName.Contains("chef") || lowerName.Contains("cook"))
                {
                    destSubfolder = "Characters/Workers";
                }
                else if (lowerName.Contains("dish") || lowerName.Contains("plate") || lowerName.Contains("meal") || lowerName.Contains("soup") || lowerName.Contains("burger") || lowerName.Contains("pizza") || lowerName.Contains("cake"))
                {
                    destSubfolder = "Food/InUse/PreparedDishes";
                }
                else if (lowerName.Contains("fruit") || lowerName.Contains("apple") || lowerName.Contains("berry") || lowerName.Contains("lemon") || lowerName.Contains("orange"))
                {
                    destSubfolder = "Food/InUse/Ingredients/Fruits";
                }
                else if (lowerName.Contains("coin") || lowerName.Contains("star") || lowerName.Contains("gem") || lowerName.Contains("icon") || lowerName.Contains("ui"))
                {
                    destSubfolder = "UI/InUse";
                }

                string targetDirectory = Path.Combine(ArtRootPath, destSubfolder);
                Directory.CreateDirectory(targetDirectory);

                string targetFilePath = Path.Combine(targetDirectory, fileName);
                File.Copy(file, targetFilePath, true);
                File.Delete(file);

                importedCount++;
            }

            AssetDatabase.Refresh();

            // 3. Configure all new sprites to Point Filter, 16 PPU
            PixelArtAssetPostprocessor.ConfigureAllExistingSprites();

            Debug.Log($"[DropFolderImporter] ¡Se importaron y organizaron {importedCount} sprites desde _Drop hacia sus carpetas correspondientes!");
        }
    }
}
#endif
