#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VillaDelChef.EditorTools
{
    public class PixelArtAssetPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (assetPath.Contains("Assets/_Projet/Art/"))
            {
                TextureImporter importer = (TextureImporter)assetImporter;
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;

                if (assetPath.Contains("PreparedDishes") || assetPath.Contains("PreparedFood") || assetPath.Contains("UI"))
                {
                    importer.spritePixelsPerUnit = 32;
                }
                else
                {
                    importer.spritePixelsPerUnit = 16;
                }
            }
        }

        [MenuItem("Tools/Villa del Chef/Configure All Sprites to Point Filter", false, 2)]
        public static void ConfigureAllExistingSprites()
        {
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/_Projet/Art" });
            int count = 0;

            AssetDatabase.StartAssetEditing();
            try
            {
                foreach (string guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
                    if (importer != null)
                    {
                        importer.textureType = TextureImporterType.Sprite;
                        importer.filterMode = FilterMode.Point;
                        importer.textureCompression = TextureImporterCompression.Uncompressed;
                        importer.mipmapEnabled = false;

                        if (path.Contains("PreparedDishes") || path.Contains("PreparedFood") || path.Contains("UI"))
                        {
                            importer.spritePixelsPerUnit = 32;
                        }
                        else
                        {
                            importer.spritePixelsPerUnit = 16;
                        }

                        importer.SaveAndReimport();
                        count++;
                    }
                }
            }
            finally
            {
                AssetDatabase.StopAssetEditing();
            }

            Debug.Log($"[PixelArtAssetPostprocessor] Successfully configured {count} sprites to crisp Point Filter Mode!");
        }
    }
}
#endif
