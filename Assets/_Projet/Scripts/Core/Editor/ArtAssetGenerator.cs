#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace VillaDelChef.EditorTools
{
    public static class ArtAssetGenerator
    {
        [MenuItem("Tools/Villa del Chef/Generate Pixel Art Floor & Shop Sprites", false, 3)]
        public static void GenerateAllSprites()
        {
            EnsureDirectories();

            GenerateWoodFloorTexture();
            GenerateKitchenCheckerTexture();
            GenerateShopMarketStallTexture();
            GenerateWallBorderTexture();
            GenerateNPCSprites();

            AssetDatabase.Refresh();
            Debug.Log("[ArtAssetGenerator] ¡Texturas de pisos, tienda física y sprites de NPCs generados con éxito!");
        }

        private static void EnsureDirectories()
        {
            string envTiles = "Assets/_Projet/Art/Environment/Tiles";
            string exterior = "Assets/_Projet/Art/Exterior";
            string constr = "Assets/_Projet/Art/Construction";
            string npcDir = "Assets/_Projet/Art/Characters/NPC";

            if (!Directory.Exists(envTiles)) Directory.CreateDirectory(envTiles);
            if (!Directory.Exists(exterior)) Directory.CreateDirectory(exterior);
            if (!Directory.Exists(constr)) Directory.CreateDirectory(constr);
            if (!Directory.Exists(npcDir)) Directory.CreateDirectory(npcDir);
        }

        // 1. Piso de madera cálida para el salón (32x32)
        private static void GenerateWoodFloorTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color plankBase1 = new Color(0.78f, 0.53f, 0.33f); // Miel cálido
            Color plankBase2 = new Color(0.72f, 0.47f, 0.28f); // Tono medio
            Color plankBase3 = new Color(0.65f, 0.42f, 0.24f); // Sombra sutil
            Color seamColor = new Color(0.38f, 0.22f, 0.12f);  // Ranura oscura
            Color highlight = new Color(0.85f, 0.60f, 0.38f);  // Brillo superior de tabla

            for (int y = 0; y < size; y++)
            {
                int plankRow = y / 8; // 4 tablas de 8px de alto
                bool isPlankSeamY = (y % 8 == 0);

                for (int x = 0; x < size; x++)
                {
                    int xOffset = (plankRow % 2 == 0) ? 0 : 16;
                    int adjustedX = (x + xOffset) % size;
                    bool isPlankSeamX = (adjustedX == 0);

                    if (isPlankSeamY || isPlankSeamX)
                    {
                        tex.SetPixel(x, y, seamColor);
                    }
                    else if (y % 8 == 7)
                    {
                        tex.SetPixel(x, y, highlight);
                    }
                    else
                    {
                        int hash = (x * 7 + y * 13 + plankRow * 19) % 5;
                        Color c = (hash == 0) ? plankBase3 : (hash == 4 ? highlight : (plankRow % 2 == 0 ? plankBase1 : plankBase2));
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Environment/Tiles/floor_restaurant_wood.png");
        }

        // 2. Baldosas damero de cocina (32x32)
        private static void GenerateKitchenCheckerTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color lightTile = new Color(0.92f, 0.89f, 0.84f); // Crema porcelana
            Color lightShade = new Color(0.84f, 0.80f, 0.74f);
            Color darkTile = new Color(0.36f, 0.44f, 0.48f);  // Pizarra azulada/acero
            Color darkShade = new Color(0.28f, 0.35f, 0.39f);
            Color grout = new Color(0.20f, 0.22f, 0.25f);      // Fragua oscura

            for (int y = 0; y < size; y++)
            {
                int tileY = y / 8;
                bool seamY = (y % 8 == 0);

                for (int x = 0; x < size; x++)
                {
                    int tileX = x / 8;
                    bool seamX = (x % 8 == 0);

                    if (seamY || seamX)
                    {
                        tex.SetPixel(x, y, grout);
                    }
                    else
                    {
                        bool isLight = ((tileX + tileY) % 2 == 0);
                        bool isEdge = (x % 8 == 7 || y % 8 == 7);

                        if (isLight)
                        {
                            tex.SetPixel(x, y, isEdge ? lightShade : lightTile);
                        }
                        else
                        {
                            tex.SetPixel(x, y, isEdge ? darkShade : darkTile);
                        }
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Environment/Tiles/floor_kitchen_checker.png");
        }

        // 3. Puesto físico de Tienda del Mercado (48x48)
        private static void GenerateShopMarketStallTexture()
        {
            int w = 48;
            int h = 48;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color clear = new Color(0, 0, 0, 0);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    tex.SetPixel(x, y, clear);
                }
            }

            Color canopyRed = new Color(0.86f, 0.20f, 0.20f);
            Color canopyWhite = new Color(0.96f, 0.96f, 0.94f);
            Color woodDark = new Color(0.38f, 0.24f, 0.16f);
            Color woodMid = new Color(0.55f, 0.36f, 0.24f);
            Color woodLight = new Color(0.68f, 0.46f, 0.30f);
            Color greenProduce = new Color(0.28f, 0.75f, 0.32f);
            Color redProduce = new Color(0.92f, 0.22f, 0.20f);
            Color yellowProduce = new Color(0.98f, 0.78f, 0.18f);
            Color npcSkin = new Color(0.96f, 0.78f, 0.65f);
            Color npcShirt = new Color(0.20f, 0.50f, 0.85f);
            Color npcHat = new Color(0.95f, 0.95f, 0.95f);

            // A. Postes de madera laterales (y: 14..36)
            for (int y = 14; y <= 36; y++)
            {
                for (int x = 4; x <= 6; x++) tex.SetPixel(x, y, woodMid);
                for (int x = 41; x <= 43; x++) tex.SetPixel(x, y, woodMid);
            }

            // B. NPC Mercader visible detrás del mostrador (y: 20..35, x: 19..28)
            for (int y = 30; y <= 36; y++)
            {
                for (int x = 20; x <= 27; x++) tex.SetPixel(x, y, npcHat);
            }
            for (int y = 24; y <= 29; y++)
            {
                for (int x = 21; x <= 26; x++) tex.SetPixel(x, y, npcSkin);
            }
            tex.SetPixel(22, 27, Color.black);
            tex.SetPixel(25, 27, Color.black);
            tex.SetPixel(23, 25, new Color(0.6f, 0.2f, 0.2f));
            tex.SetPixel(24, 25, new Color(0.6f, 0.2f, 0.2f));
            for (int y = 18; y <= 23; y++)
            {
                for (int x = 18; x <= 29; x++) tex.SetPixel(x, y, npcShirt);
            }

            // C. Mostrador de madera (y: 14..20, x: 3..44)
            for (int y = 14; y <= 20; y++)
            {
                for (int x = 3; x <= 44; x++)
                {
                    if (y == 20) tex.SetPixel(x, y, woodLight);
                    else if (y == 14) tex.SetPixel(x, y, woodDark);
                    else tex.SetPixel(x, y, woodMid);
                }
            }

            // D. Toldo a rayas en el techo (y: 35..46, x: 1..46)
            for (int y = 35; y <= 46; y++)
            {
                for (int x = 1; x <= 46; x++)
                {
                    int stripe = (x / 6) % 2;
                    Color c = (stripe == 0) ? canopyRed : canopyWhite;
                    if (y == 46) c *= 0.9f;
                    tex.SetPixel(x, y, c);
                }
            }
            for (int x = 1; x <= 46; x++)
            {
                if (x % 6 < 4)
                {
                    tex.SetPixel(x, 34, canopyWhite);
                }
            }

            // E. Cajones con frutas/verduras al frente (y: 2..13, x: 5..42)
            for (int y = 2; y <= 13; y++)
            {
                for (int x = 5; x <= 42; x++)
                {
                    if (x == 5 || x == 17 || x == 30 || x == 42 || y == 2 || y == 13)
                    {
                        tex.SetPixel(x, y, woodDark);
                    }
                    else
                    {
                        if (x > 5 && x < 17)
                        {
                            tex.SetPixel(x, y, ((x + y) % 3 == 0) ? greenProduce : redProduce);
                        }
                        else if (x > 17 && x < 30)
                        {
                            tex.SetPixel(x, y, yellowProduce);
                        }
                        else
                        {
                            tex.SetPixel(x, y, greenProduce);
                        }
                    }
                }
            }

            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Exterior/shop_market_stall.png");
        }

        // 4. Muro de borde de madera (32x32)
        private static void GenerateWallBorderTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color wallTop = new Color(0.42f, 0.28f, 0.18f);
            Color wallBody = new Color(0.58f, 0.40f, 0.26f);
            Color wallSeam = new Color(0.32f, 0.20f, 0.12f);
            Color wallBase = new Color(0.28f, 0.16f, 0.10f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (y >= 26)
                    {
                        tex.SetPixel(x, y, (y == 31) ? wallTop * 1.2f : wallTop);
                    }
                    else if (y < 6)
                    {
                        tex.SetPixel(x, y, wallBase);
                    }
                    else
                    {
                        bool isVerticalSeam = (x % 8 == 0);
                        tex.SetPixel(x, y, isVerticalSeam ? wallSeam : wallBody);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Construction/border_restaurant_wall.png");
        }

        private struct NPCVisualData
        {
            public string id;
            public Color skinColor;
            public Color hairColor;
            public Color hatColor;
            public Color shirtColor;
            public Color pantsColor;
            public Color accentColor;
            public bool hasHat;
            public bool hasApron;
            public bool hasBeard;
            public bool isChefToque;
        }

        [MenuItem("Tools/Villa del Chef/Generate NPC Pixel Art Sprites", false, 4)]
        public static void GenerateNPCSprites()
        {
            EnsureDirectories();

            var npcs = new[]
            {
                new NPCVisualData {
                    id = "elena",
                    skinColor = new Color(0.98f, 0.82f, 0.68f),
                    hairColor = new Color(0.42f, 0.22f, 0.12f),
                    hatColor = new Color(0.92f, 0.82f, 0.45f), // Straw hat
                    shirtColor = new Color(0.35f, 0.68f, 0.38f), // Farmer green
                    pantsColor = new Color(0.42f, 0.28f, 0.16f), // Overalls brown
                    accentColor = new Color(0.85f, 0.40f, 0.35f), // Red bandana
                    hasHat = true,
                    hasApron = false,
                    hasBeard = false,
                    isChefToque = false
                },
                new NPCVisualData {
                    id = "bruno",
                    skinColor = new Color(0.95f, 0.78f, 0.65f),
                    hairColor = new Color(0.12f, 0.10f, 0.10f),
                    hatColor = Color.black,
                    shirtColor = new Color(0.72f, 0.18f, 0.18f), // Butcher crimson
                    pantsColor = new Color(0.20f, 0.22f, 0.28f),
                    accentColor = new Color(0.92f, 0.92f, 0.92f), // White butcher apron
                    hasHat = false,
                    hasApron = true,
                    hasBeard = true,
                    isChefToque = false
                },
                new NPCVisualData {
                    id = "tomas",
                    skinColor = new Color(0.96f, 0.84f, 0.72f),
                    hairColor = new Color(0.78f, 0.60f, 0.30f), // Warm blonde
                    hatColor = new Color(0.96f, 0.96f, 0.96f), // Tall baker toque
                    shirtColor = new Color(0.88f, 0.82f, 0.72f), // Flour beige
                    pantsColor = new Color(0.45f, 0.32f, 0.22f),
                    accentColor = new Color(0.80f, 0.50f, 0.25f),
                    hasHat = true,
                    hasApron = true,
                    hasBeard = false,
                    isChefToque = true
                },
                new NPCVisualData {
                    id = "marina",
                    skinColor = new Color(0.92f, 0.76f, 0.64f),
                    hairColor = new Color(0.65f, 0.25f, 0.15f), // Auburn red
                    hatColor = new Color(0.12f, 0.22f, 0.48f), // Sailor cap
                    shirtColor = new Color(0.95f, 0.80f, 0.18f), // Fisher yellow
                    pantsColor = new Color(0.18f, 0.38f, 0.58f), // Ocean blue
                    accentColor = new Color(0.20f, 0.75f, 0.80f),
                    hasHat = true,
                    hasApron = false,
                    hasBeard = false,
                    isChefToque = false
                },
                new NPCVisualData {
                    id = "amelia",
                    skinColor = new Color(0.96f, 0.80f, 0.68f),
                    hairColor = new Color(0.30f, 0.18f, 0.12f),
                    hatColor = new Color(0.85f, 0.45f, 0.12f), // Orange welder goggles
                    shirtColor = new Color(0.25f, 0.48f, 0.52f), // Industrial teal
                    pantsColor = new Color(0.22f, 0.30f, 0.35f),
                    accentColor = new Color(0.70f, 0.55f, 0.20f),
                    hasHat = true,
                    hasApron = true,
                    hasBeard = false,
                    isChefToque = false
                },
                new NPCVisualData {
                    id = "lucas",
                    skinColor = new Color(0.94f, 0.78f, 0.65f),
                    hairColor = new Color(0.68f, 0.35f, 0.15f), // Ginger
                    hatColor = new Color(0.40f, 0.28f, 0.18f), // Brown cap
                    shirtColor = new Color(0.78f, 0.42f, 0.22f), // Plaid warm orange
                    pantsColor = new Color(0.22f, 0.32f, 0.50f), // Denim blue
                    accentColor = new Color(0.50f, 0.32f, 0.18f),
                    hasHat = true,
                    hasApron = false,
                    hasBeard = true,
                    isChefToque = false
                },
                new NPCVisualData {
                    id = "sofia",
                    skinColor = new Color(0.97f, 0.84f, 0.72f),
                    hairColor = new Color(0.15f, 0.12f, 0.16f), // Glossy dark hair
                    hatColor = new Color(0.55f, 0.22f, 0.62f), // Violet beret
                    shirtColor = new Color(0.75f, 0.52f, 0.82f), // Chic lavender
                    pantsColor = new Color(0.18f, 0.16f, 0.22f), // Elegant black
                    accentColor = new Color(0.95f, 0.85f, 0.40f),
                    hasHat = true,
                    hasApron = false,
                    hasBeard = false,
                    isChefToque = false
                }
            };

            foreach (var npc in npcs)
            {
                GenerateNPCWorldSprite(npc);
                GenerateNPCPortrait(npc);
            }
        }

        private static void GenerateNPCWorldSprite(NPCVisualData data)
        {
            int w = 16;
            int h = 24;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color transparent = Color.clear;
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, transparent);

            // Soft shadow at feet
            Color shadowCol = new Color(0f, 0f, 0f, 0.25f);
            for (int x = 4; x <= 11; x++)
            {
                tex.SetPixel(x, 0, shadowCol);
            }

            // Shoes
            Color shoeCol = new Color(0.18f, 0.14f, 0.12f);
            for (int y = 1; y <= 2; y++)
            {
                for (int x = 4; x <= 6; x++) tex.SetPixel(x, y, shoeCol);
                for (int x = 9; x <= 11; x++) tex.SetPixel(x, y, shoeCol);
            }

            // Pants
            for (int y = 3; y <= 6; y++)
            {
                for (int x = 4; x <= 11; x++)
                {
                    if (y <= 4 && x == 7 || y <= 4 && x == 8)
                        tex.SetPixel(x, y, data.pantsColor * 0.8f); // Inseam shading
                    else
                        tex.SetPixel(x, y, data.pantsColor);
                }
            }

            // Shirt / Torso
            for (int y = 7; y <= 13; y++)
            {
                for (int x = 3; x <= 12; x++)
                {
                    tex.SetPixel(x, y, data.shirtColor);
                }
            }

            // Apron
            if (data.hasApron)
            {
                for (int y = 6; y <= 12; y++)
                {
                    for (int x = 5; x <= 10; x++)
                    {
                        tex.SetPixel(x, y, data.accentColor);
                    }
                }
            }

            // Hands
            for (int y = 7; y <= 8; y++)
            {
                tex.SetPixel(2, y, data.skinColor);
                tex.SetPixel(13, y, data.skinColor);
            }

            // Neck & Face
            for (int y = 13; y <= 17; y++)
            {
                for (int x = 5; x <= 10; x++)
                {
                    tex.SetPixel(x, y, data.skinColor);
                }
            }

            // Beard
            if (data.hasBeard)
            {
                for (int y = 13; y <= 15; y++)
                {
                    for (int x = 5; x <= 10; x++)
                    {
                        tex.SetPixel(x, y, data.hairColor);
                    }
                }
            }

            // Eyes
            Color eyeCol = new Color(0.12f, 0.12f, 0.14f);
            tex.SetPixel(6, 16, eyeCol);
            tex.SetPixel(9, 16, eyeCol);

            // Cheeks blush
            Color blushCol = new Color(0.95f, 0.55f, 0.55f, 0.6f);
            tex.SetPixel(5, 15, blushCol);
            tex.SetPixel(10, 15, blushCol);

            // Hair
            for (int y = 17; y <= 20; y++)
            {
                for (int x = 4; x <= 11; x++)
                {
                    if (y >= 19 || x == 4 || x == 11)
                        tex.SetPixel(x, y, data.hairColor);
                }
            }

            // Hat
            if (data.hasHat)
            {
                int hatTop = data.isChefToque ? 23 : 21;
                for (int y = 19; y <= hatTop; y++)
                {
                    int minX = (y == hatTop) ? 4 : 3;
                    int maxX = (y == hatTop) ? 11 : 12;
                    for (int x = minX; x <= maxX; x++)
                    {
                        tex.SetPixel(x, y, data.hatColor);
                    }
                }
            }

            tex.Apply();
            SaveTextureAsPNG(tex, $"Assets/_Projet/Art/Characters/NPC/npc_{data.id}.png");
        }

        private static void GenerateNPCPortrait(NPCVisualData data)
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color transparent = Color.clear;
            Color bgCircle = new Color(0.20f, 0.22f, 0.28f);
            Color bgBorder = new Color(0.35f, 0.38f, 0.46f);

            // Medallion background
            float center = 15.5f;
            float radius = 15.0f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist < radius - 1.2f)
                        tex.SetPixel(x, y, bgCircle);
                    else if (dist < radius)
                        tex.SetPixel(x, y, bgBorder);
                    else
                        tex.SetPixel(x, y, transparent);
                }
            }

            // Shoulders & Clothes
            for (int y = 2; y <= 9; y++)
            {
                int minX = 7 - (y - 2);
                int maxX = 24 + (y - 2);
                for (int x = Mathf.Clamp(minX, 3, 28); x <= Mathf.Clamp(maxX, 3, 28); x++)
                {
                    tex.SetPixel(x, y, data.shirtColor);
                }
            }

            if (data.hasApron)
            {
                for (int y = 2; y <= 8; y++)
                {
                    for (int x = 12; x <= 19; x++)
                    {
                        tex.SetPixel(x, y, data.accentColor);
                    }
                }
            }

            // Neck
            for (int y = 9; y <= 12; y++)
            {
                for (int x = 13; x <= 18; x++)
                {
                    tex.SetPixel(x, y, data.skinColor * 0.95f);
                }
            }

            // Hair Back
            for (int y = 11; y <= 24; y++)
            {
                for (int x = 8; x <= 23; x++)
                {
                    tex.SetPixel(x, y, data.hairColor);
                }
            }

            // Face
            for (int y = 12; y <= 22; y++)
            {
                for (int x = 10; x <= 21; x++)
                {
                    tex.SetPixel(x, y, data.skinColor);
                }
            }

            // Beard
            if (data.hasBeard)
            {
                for (int y = 12; y <= 16; y++)
                {
                    for (int x = 10; x <= 21; x++)
                    {
                        tex.SetPixel(x, y, data.hairColor);
                    }
                }
            }

            // Eyes & Highlights
            Color eyeCol = new Color(0.10f, 0.10f, 0.12f);
            tex.SetPixel(12, 17, eyeCol);
            tex.SetPixel(13, 17, eyeCol);
            tex.SetPixel(18, 17, eyeCol);
            tex.SetPixel(19, 17, eyeCol);

            tex.SetPixel(12, 18, Color.white);
            tex.SetPixel(18, 18, Color.white);

            // Smile
            Color mouthCol = new Color(0.65f, 0.28f, 0.28f);
            for (int x = 14; x <= 17; x++) tex.SetPixel(x, 14, mouthCol);

            // Cheeks
            Color blush = new Color(0.95f, 0.50f, 0.50f, 0.5f);
            tex.SetPixel(11, 16, blush);
            tex.SetPixel(20, 16, blush);

            // Hair Front
            for (int y = 21; y <= 25; y++)
            {
                for (int x = 9; x <= 22; x++)
                {
                    if (y >= 23 || x == 9 || x == 22)
                        tex.SetPixel(x, y, data.hairColor);
                }
            }

            // Hat
            if (data.hasHat)
            {
                int hatTop = data.isChefToque ? 30 : 27;
                for (int y = 23; y <= hatTop; y++)
                {
                    int minX = (y == hatTop) ? 9 : 7;
                    int maxX = (y == hatTop) ? 22 : 24;
                    for (int x = minX; x <= maxX; x++)
                    {
                        tex.SetPixel(x, y, data.hatColor);
                    }
                }
            }

            tex.Apply();
            SaveTextureAsPNG(tex, $"Assets/_Projet/Art/Characters/NPC/portrait_{data.id}.png");
        }

        private static void SaveTextureAsPNG(Texture2D tex, string path)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(path, bytes);
            Object.DestroyImmediate(tex);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.filterMode = FilterMode.Point;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.mipmapEnabled = false;
                importer.spritePixelsPerUnit = 16;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
