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
            GenerateCraftingSprites();
            GenerateExpansionSprites();
            GenerateCustomerSprites();

            AssetDatabase.Refresh();
            Debug.Log("[ArtAssetGenerator] ¡Texturas de pisos, tienda física, NPCs, crafting, expansiones y clientes generadas con éxito!");
        }

        private static void EnsureDirectories()
        {
            string envTiles = "Assets/_Projet/Art/Environment/Tiles";
            string exterior = "Assets/_Projet/Art/Exterior";
            string constr = "Assets/_Projet/Art/Construction";
            string npcDir = "Assets/_Projet/Art/Characters/NPC";
            string custDir = "Assets/_Projet/Art/Characters/Customers";
            string kitchenDir = "Assets/_Projet/Art/Kitchen";
            string foodDir = "Assets/_Projet/Art/Food";

            if (!Directory.Exists(envTiles)) Directory.CreateDirectory(envTiles);
            if (!Directory.Exists(exterior)) Directory.CreateDirectory(exterior);
            if (!Directory.Exists(constr)) Directory.CreateDirectory(constr);
            if (!Directory.Exists(npcDir)) Directory.CreateDirectory(npcDir);
            if (!Directory.Exists(custDir)) Directory.CreateDirectory(custDir);
            if (!Directory.Exists(kitchenDir)) Directory.CreateDirectory(kitchenDir);
            if (!Directory.Exists(foodDir)) Directory.CreateDirectory(foodDir);
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

        [MenuItem("Tools/Villa del Chef/Generate Crafting Pixel Art Sprites", false, 5)]
        public static void GenerateCraftingSprites()
        {
            EnsureDirectories();

            // 1. Molino de Grano (32x32)
            GenerateMillTexture();
            // 2. Mesa de Amasado (32x32)
            GenerateDoughTableTexture();
            // 3. Procesador de Salsa (32x32)
            GenerateSaucePotTexture();
            // 4. Marmita de Mermelada (32x32)
            GenerateJamPotTexture();

            // 5. Insumos procesados (16x16)
            GenerateFlourSackTexture();
            GenerateDoughBallTexture();
            GenerateSauceJarTexture();
            GenerateJamJarTexture();

            AssetDatabase.Refresh();
            Debug.Log("[ArtAssetGenerator] ¡Sprites de Crafting y nuevos ingredientes generados con éxito!");
        }

        private static void GenerateMillTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color stoneBase = new Color(0.50f, 0.52f, 0.55f);
            Color stoneDark = new Color(0.35f, 0.38f, 0.40f);
            Color stoneLight = new Color(0.65f, 0.68f, 0.70f);
            Color woodBeam = new Color(0.55f, 0.35f, 0.18f);
            Color woodDark = new Color(0.38f, 0.22f, 0.10f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Transparent margins
                    if (x < 2 || x > 29 || y < 2)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    // Stone grinding base (y: 2..16, x: 4..27)
                    if (y <= 16 && x >= 4 && x <= 27)
                    {
                        bool isRim = (y == 16 || x == 4 || x == 27);
                        bool isShade = (y <= 5 || x <= 6);
                        tex.SetPixel(x, y, isRim ? stoneLight : (isShade ? stoneDark : stoneBase));
                    }
                    // Wooden grain hopper / funnel (y: 17..28, x: 8..23)
                    else if (y > 16 && y <= 28)
                    {
                        int minX = 15 - (y - 16);
                        int maxX = 16 + (y - 16);
                        if (x >= Mathf.Clamp(minX, 7, 15) && x <= Mathf.Clamp(maxX, 16, 24))
                        {
                            tex.SetPixel(x, y, (x % 3 == 0) ? woodDark : woodBeam);
                        }
                        else
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Kitchen/station_mill.png");
        }

        private static void GenerateDoughTableTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color woodTable = new Color(0.72f, 0.48f, 0.28f);
            Color woodHighlight = new Color(0.85f, 0.60f, 0.38f);
            Color woodShadow = new Color(0.42f, 0.26f, 0.14f);
            Color flourDust = new Color(0.96f, 0.95f, 0.90f, 0.85f);
            Color rollingPin = new Color(0.88f, 0.70f, 0.45f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Table Legs (y: 2..12, x: 4..7, 24..27)
                    if (y >= 2 && y <= 12 && ((x >= 4 && x <= 7) || (x >= 24 && x <= 27)))
                    {
                        tex.SetPixel(x, y, (x == 4 || x == 24) ? woodShadow : woodTable);
                    }
                    // Table Top surface (y: 13..22, x: 2..29)
                    else if (y >= 13 && y <= 22 && x >= 2 && x <= 29)
                    {
                        if (y == 22) tex.SetPixel(x, y, woodHighlight);
                        else if (y == 13) tex.SetPixel(x, y, woodShadow);
                        else
                        {
                            // Flour dusted cutting board in the center
                            if (x >= 9 && x <= 22 && y >= 15 && y <= 20)
                            {
                                // Rolling pin at angle
                                if (x == y) tex.SetPixel(x, y, rollingPin);
                                else tex.SetPixel(x, y, flourDust);
                            }
                            else
                            {
                                tex.SetPixel(x, y, woodTable);
                            }
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Kitchen/station_dough_table.png");
        }

        private static void GenerateSaucePotTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color metalPot = new Color(0.40f, 0.42f, 0.45f);
            Color metalHighlight = new Color(0.65f, 0.68f, 0.72f);
            Color metalDark = new Color(0.24f, 0.25f, 0.28f);
            Color sauceRed = new Color(0.82f, 0.20f, 0.16f);
            Color sauceHighlight = new Color(0.95f, 0.35f, 0.22f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Pot base and body (y: 4..22, x: 5..26)
                    if (y >= 4 && y <= 22 && x >= 5 && x <= 26)
                    {
                        // Stew inside top rim (y: 19..22, x: 7..24)
                        if (y >= 19 && x >= 7 && x <= 24)
                        {
                            bool isBubble = (x == 12 && y == 20) || (x == 18 && y == 21);
                            tex.SetPixel(x, y, isBubble ? sauceHighlight : sauceRed);
                        }
                        else
                        {
                            bool isBorder = (x == 5 || x == 26 || y == 4);
                            tex.SetPixel(x, y, isBorder ? metalDark : (x == 8 ? metalHighlight : metalPot));
                        }
                    }
                    // Side handles
                    else if (y >= 14 && y <= 17 && ((x >= 2 && x <= 4) || (x >= 27 && x <= 29)))
                    {
                        tex.SetPixel(x, y, metalDark);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Kitchen/station_sauce_pot.png");
        }

        private static void GenerateJamPotTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color copperPot = new Color(0.82f, 0.45f, 0.25f);
            Color copperDark = new Color(0.55f, 0.28f, 0.14f);
            Color copperHighlight = new Color(0.95f, 0.65f, 0.40f);
            Color berryJam = new Color(0.68f, 0.12f, 0.35f);
            Color jamHighlight = new Color(0.85f, 0.25f, 0.50f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (y >= 5 && y <= 23 && x >= 6 && x <= 25)
                    {
                        if (y >= 20 && x >= 8 && x <= 23)
                        {
                            bool isBubble = (x == 14 && y == 21) || (x == 19 && y == 22);
                            tex.SetPixel(x, y, isBubble ? jamHighlight : berryJam);
                        }
                        else
                        {
                            bool isBorder = (x == 6 || x == 25 || y == 5);
                            tex.SetPixel(x, y, isBorder ? copperDark : (x == 9 ? copperHighlight : copperPot));
                        }
                    }
                    else if (y >= 15 && y <= 18 && ((x >= 3 && x <= 5) || (x >= 26 && x <= 28)))
                    {
                        tex.SetPixel(x, y, copperDark);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Kitchen/station_preserve_pot.png");
        }

        private static void GenerateFlourSackTexture()
        {
            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color sack = new Color(0.92f, 0.86f, 0.74f);
            Color sackShadow = new Color(0.72f, 0.64f, 0.50f);
            Color rope = new Color(0.55f, 0.35f, 0.18f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (y >= 2 && y <= 10 && x >= 3 && x <= 12)
                    {
                        tex.SetPixel(x, y, (x <= 4 || y == 2) ? sackShadow : sack);
                    }
                    else if (y == 11 && x >= 5 && x <= 10)
                    {
                        tex.SetPixel(x, y, rope);
                    }
                    else if (y >= 12 && y <= 13 && x >= 4 && x <= 11)
                    {
                        tex.SetPixel(x, y, (y == 13 && (x == 4 || x == 11)) ? Color.clear : sack);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Food/ing_flour.png");
        }

        private static void GenerateDoughBallTexture()
        {
            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color dough = new Color(0.95f, 0.88f, 0.72f);
            Color doughShade = new Color(0.80f, 0.70f, 0.52f);
            Color doughHighlight = new Color(0.98f, 0.95f, 0.85f);

            float center = 7.5f;
            float radius = 5.5f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    if (dist <= radius)
                    {
                        if (y >= 9 && x <= 7) tex.SetPixel(x, y, doughHighlight);
                        else if (y <= 5 || x >= 11) tex.SetPixel(x, y, doughShade);
                        else tex.SetPixel(x, y, dough);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Food/ing_dough.png");
        }

        private static void GenerateSauceJarTexture()
        {
            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color glass = new Color(0.85f, 0.92f, 0.95f, 0.7f);
            Color sauce = new Color(0.85f, 0.22f, 0.18f);
            Color lid = new Color(0.90f, 0.75f, 0.25f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (y >= 2 && y <= 10 && x >= 4 && x <= 11)
                    {
                        if (x == 4 || x == 11 || y == 2) tex.SetPixel(x, y, glass);
                        else tex.SetPixel(x, y, sauce);
                    }
                    else if (y == 11 && x >= 5 && x <= 10)
                    {
                        tex.SetPixel(x, y, glass);
                    }
                    else if (y >= 12 && y <= 13 && x >= 4 && x <= 11)
                    {
                        tex.SetPixel(x, y, lid);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Food/ing_sauce.png");
        }

        private static void GenerateJamJarTexture()
        {
            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color glass = new Color(0.85f, 0.92f, 0.95f, 0.7f);
            Color jam = new Color(0.65f, 0.15f, 0.38f);
            Color clothCover = new Color(0.88f, 0.28f, 0.35f);
            Color ribbon = new Color(0.95f, 0.85f, 0.35f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    if (y >= 2 && y <= 9 && x >= 4 && x <= 11)
                    {
                        if (x == 4 || x == 11 || y == 2) tex.SetPixel(x, y, glass);
                        else tex.SetPixel(x, y, jam);
                    }
                    else if (y == 10 && x >= 5 && x <= 10)
                    {
                        tex.SetPixel(x, y, ribbon);
                    }
                    else if (y >= 11 && y <= 13 && x >= 3 && x <= 12)
                    {
                        tex.SetPixel(x, y, clothCover);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Food/ing_jam.png");
        }

        private static void GenerateExpansionSprites()
        {
            GenerateExpansionSignTexture();
            GenerateFenceRusticTexture();
            GenerateTerraceIconTexture();
            GenerateCropsIconTexture();
            GenerateCraftingIconTexture();
            GenerateMarketIconTexture();
        }

        private static void GenerateExpansionSignTexture()
        {
            int width = 16;
            int height = 24;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color woodPost = new Color(0.48f, 0.30f, 0.16f);
            Color woodPostShadow = new Color(0.35f, 0.20f, 0.10f);
            Color boardBorder = new Color(0.32f, 0.18f, 0.08f);
            Color boardBg = new Color(0.85f, 0.68f, 0.44f);
            Color starGold = new Color(1.0f, 0.82f, 0.20f);
            Color starBorder = new Color(0.60f, 0.42f, 0.10f);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Wooden post (y: 0 to 11, x: 7 to 8)
                    if (y <= 11 && (x == 7 || x == 8))
                    {
                        tex.SetPixel(x, y, (x == 7) ? woodPost : woodPostShadow);
                    }
                    // Signboard (y: 11 to 22, x: 1 to 14)
                    else if (y >= 11 && y <= 22 && x >= 1 && x <= 14)
                    {
                        if (x == 1 || x == 14 || y == 11 || y == 22)
                        {
                            tex.SetPixel(x, y, boardBorder);
                        }
                        // Star in center (x: 6 to 9, y: 15 to 18)
                        else if ((x == 7 || x == 8) && (y >= 14 && y <= 19))
                        {
                            tex.SetPixel(x, y, starGold);
                        }
                        else if ((y == 16 || y == 17) && (x >= 5 && x <= 10))
                        {
                            tex.SetPixel(x, y, starGold);
                        }
                        else
                        {
                            tex.SetPixel(x, y, boardBg);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Environment/sign_for_sale.png");
        }

        private static void GenerateFenceRusticTexture()
        {
            int size = 16;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color woodPicket = new Color(0.72f, 0.52f, 0.33f);
            Color woodPicketLight = new Color(0.82f, 0.62f, 0.42f);
            Color woodPicketDark = new Color(0.45f, 0.28f, 0.15f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    // Vertical pickets at x: 2-4 and x: 11-13
                    bool isPicket1 = (x >= 2 && x <= 4 && y >= 1 && y <= 14);
                    bool isPicket2 = (x >= 11 && x <= 13 && y >= 1 && y <= 14);
                    // Pointed tips
                    if ((x == 2 || x == 4) && y == 14) isPicket1 = false;
                    if ((x == 11 || x == 13) && y == 14) isPicket2 = false;

                    // Horizontal rails at y: 4-5 and y: 9-10
                    bool isRail = (y >= 4 && y <= 5) || (y >= 9 && y <= 10);

                    if (isPicket1 || isPicket2)
                    {
                        tex.SetPixel(x, y, (x % 3 == 0) ? woodPicketLight : woodPicket);
                    }
                    else if (isRail)
                    {
                        tex.SetPixel(x, y, woodPicketDark);
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/Environment/fence_rustic.png");
        }

        private static void GenerateTerraceIconTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color bgCircle = new Color(0.25f, 0.65f, 0.45f); // Garden green
            Color umbrellaWhite = new Color(0.95f, 0.95f, 0.95f);
            Color umbrellaYellow = new Color(1.0f, 0.82f, 0.22f);
            Color wood = new Color(0.55f, 0.35f, 0.20f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    if (dist <= 14.5f)
                    {
                        // Umbrella top dome (y: 16 to 28, x: 5 to 27)
                        if (y >= 16 && y <= 27 && x >= 5 && x <= 27 && dist <= 12f)
                        {
                            tex.SetPixel(x, y, ((x / 4) % 2 == 0) ? umbrellaYellow : umbrellaWhite);
                        }
                        // Pole (x: 15-16, y: 6 to 16)
                        else if ((x == 15 || x == 16) && y >= 6 && y <= 16)
                        {
                            tex.SetPixel(x, y, wood);
                        }
                        // Table disk (y: 8 to 11, x: 9 to 23)
                        else if (y >= 8 && y <= 11 && x >= 9 && x <= 23)
                        {
                            tex.SetPixel(x, y, wood);
                        }
                        else
                        {
                            tex.SetPixel(x, y, bgCircle);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/UI/exp_terrace.png");
        }

        private static void GenerateCropsIconTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color bgCircle = new Color(0.40f, 0.58f, 0.25f);
            Color soil = new Color(0.42f, 0.26f, 0.15f);
            Color plantGreen = new Color(0.25f, 0.82f, 0.35f);
            Color carrotOrange = new Color(0.98f, 0.52f, 0.15f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    if (dist <= 14.5f)
                    {
                        // Soil mound (y: 6 to 14, x: 6 to 26)
                        if (y >= 6 && y <= 13 && x >= 6 && x <= 26)
                        {
                            tex.SetPixel(x, y, soil);
                        }
                        // Plant leaves (y: 14 to 25)
                        else if (y >= 14 && y <= 24 && ((x >= 8 && x <= 13) || (x >= 18 && x <= 23)))
                        {
                            tex.SetPixel(x, y, plantGreen);
                        }
                        // Carrot in soil
                        else if (y >= 10 && y <= 16 && (x >= 14 && x <= 17))
                        {
                            tex.SetPixel(x, y, carrotOrange);
                        }
                        else
                        {
                            tex.SetPixel(x, y, bgCircle);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/UI/exp_crops.png");
        }

        private static void GenerateCraftingIconTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color bgCircle = new Color(0.35f, 0.45f, 0.65f);
            Color gearSteel = new Color(0.85f, 0.88f, 0.92f);
            Color gearCenter = new Color(0.55f, 0.60f, 0.68f);
            Color hammerWood = new Color(0.65f, 0.40f, 0.20f);
            Color hammerHead = new Color(0.35f, 0.38f, 0.45f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    if (dist <= 14.5f)
                    {
                        float gearDist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                        // Gear teeth and body
                        if (gearDist >= 4f && gearDist <= 9.5f)
                        {
                            tex.SetPixel(x, y, gearSteel);
                        }
                        else if (gearDist < 4f)
                        {
                            tex.SetPixel(x, y, gearCenter);
                        }
                        else
                        {
                            tex.SetPixel(x, y, bgCircle);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/UI/exp_crafting.png");
        }

        private static void GenerateMarketIconTexture()
        {
            int size = 32;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            Color bgCircle = new Color(0.85f, 0.55f, 0.25f);
            Color canopyRed = new Color(0.88f, 0.25f, 0.25f);
            Color canopyWhite = new Color(0.96f, 0.96f, 0.96f);
            Color goldCoin = new Color(1.0f, 0.82f, 0.22f);
            Color wood = new Color(0.52f, 0.32f, 0.16f);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(16, 16));
                    if (dist <= 14.5f)
                    {
                        // Canopy awning (y: 17 to 25, x: 6 to 26)
                        if (y >= 17 && y <= 25 && x >= 6 && x <= 26)
                        {
                            tex.SetPixel(x, y, ((x / 4) % 2 == 0) ? canopyRed : canopyWhite);
                        }
                        // Counter (y: 8 to 13, x: 7 to 25)
                        else if (y >= 8 && y <= 13 && x >= 7 && x <= 25)
                        {
                            tex.SetPixel(x, y, wood);
                        }
                        // Coin stack (y: 11 to 16, x: 13 to 19)
                        else if (y >= 11 && y <= 16 && x >= 13 && x <= 19)
                        {
                            tex.SetPixel(x, y, goldCoin);
                        }
                        else
                        {
                            tex.SetPixel(x, y, bgCircle);
                        }
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }
            }
            tex.Apply();
            SaveTextureAsPNG(tex, "Assets/_Projet/Art/UI/exp_market.png");
        }

        private static void GenerateCustomerSprites()
        {
            var customers = new[]
            {
                new NPCVisualData {
                    id = "cust_normal",
                    skinColor = new Color(0.95f, 0.78f, 0.65f),
                    hairColor = new Color(0.35f, 0.20f, 0.10f),
                    shirtColor = new Color(0.30f, 0.65f, 0.40f),
                    pantsColor = new Color(0.25f, 0.30f, 0.40f),
                    hasHat = false
                },
                new NPCVisualData {
                    id = "cust_impatient",
                    skinColor = new Color(0.96f, 0.80f, 0.68f),
                    hairColor = new Color(0.15f, 0.12f, 0.10f),
                    shirtColor = new Color(0.18f, 0.25f, 0.45f), // Navy suit
                    pantsColor = new Color(0.15f, 0.18f, 0.28f),
                    accentColor = new Color(0.85f, 0.20f, 0.20f), // Red tie
                    hasHat = false
                },
                new NPCVisualData {
                    id = "cust_generous",
                    skinColor = new Color(0.98f, 0.82f, 0.70f),
                    hairColor = new Color(0.85f, 0.85f, 0.88f), // Grey hair
                    shirtColor = new Color(0.75f, 0.45f, 0.60f), // Warm lilac sweater
                    pantsColor = new Color(0.35f, 0.25f, 0.30f),
                    hasHat = false
                },
                new NPCVisualData {
                    id = "cust_gourmet",
                    skinColor = new Color(0.94f, 0.76f, 0.62f),
                    hairColor = new Color(0.25f, 0.18f, 0.12f),
                    hatColor = new Color(0.45f, 0.20f, 0.50f), // Purple beret
                    shirtColor = new Color(0.88f, 0.82f, 0.70f),
                    pantsColor = new Color(0.20f, 0.20f, 0.22f),
                    hasHat = true
                },
                new NPCVisualData {
                    id = "cust_tourist",
                    skinColor = new Color(0.92f, 0.72f, 0.58f),
                    hairColor = new Color(0.60f, 0.40f, 0.20f),
                    hatColor = new Color(0.90f, 0.80f, 0.40f), // Straw hat
                    shirtColor = new Color(0.20f, 0.70f, 0.75f), // Turquoise Hawaiian shirt
                    pantsColor = new Color(0.80f, 0.75f, 0.60f), // Khaki shorts
                    hasHat = true
                },
                new NPCVisualData {
                    id = "cust_critic",
                    skinColor = new Color(0.96f, 0.80f, 0.66f),
                    hairColor = new Color(0.40f, 0.38f, 0.36f),
                    shirtColor = new Color(0.35f, 0.36f, 0.40f), // Dark charcoal coat
                    pantsColor = new Color(0.20f, 0.20f, 0.22f),
                    accentColor = new Color(0.95f, 0.95f, 0.80f), // Notepad
                    hasHat = false
                },
                new NPCVisualData {
                    id = "cust_vip",
                    skinColor = new Color(0.97f, 0.82f, 0.68f),
                    hairColor = new Color(0.85f, 0.70f, 0.30f), // Platinum blonde
                    shirtColor = new Color(0.95f, 0.85f, 0.35f), // Golden dress/suit
                    pantsColor = new Color(0.15f, 0.15f, 0.18f),
                    accentColor = new Color(0.10f, 0.10f, 0.12f), // Sunglasses
                    hasHat = false
                }
            };

            foreach (var cust in customers)
            {
                GenerateCustomerWorldSprite(cust);
            }
        }

        private static void GenerateCustomerWorldSprite(NPCVisualData data)
        {
            int w = 16;
            int h = 24;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Point;

            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                    tex.SetPixel(x, y, Color.clear);

            // Shadow
            Color shadowCol = new Color(0f, 0f, 0f, 0.25f);
            for (int x = 4; x <= 11; x++) tex.SetPixel(x, 0, shadowCol);

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
                    if (y <= 4 && (x == 7 || x == 8))
                        tex.SetPixel(x, y, data.pantsColor * 0.8f);
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

            // Tie or Accent
            if (data.accentColor.a > 0.05f)
            {
                for (int y = 8; y <= 12; y++)
                {
                    tex.SetPixel(7, y, data.accentColor);
                    tex.SetPixel(8, y, data.accentColor);
                }
            }

            // Head / Neck
            for (int y = 13; y <= 14; y++)
            {
                for (int x = 6; x <= 9; x++) tex.SetPixel(x, y, data.skinColor);
            }

            for (int y = 14; y <= 20; y++)
            {
                for (int x = 4; x <= 11; x++)
                {
                    tex.SetPixel(x, y, data.skinColor);
                }
            }

            // Eyes
            Color eyeCol = new Color(0.12f, 0.12f, 0.14f);
            tex.SetPixel(6, 16, eyeCol);
            tex.SetPixel(9, 16, eyeCol);

            // Cheeks
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
                for (int y = 19; y <= 21; y++)
                {
                    for (int x = 3; x <= 12; x++)
                    {
                        tex.SetPixel(x, y, data.hatColor);
                    }
                }
            }

            tex.Apply();
            SaveTextureAsPNG(tex, $"Assets/_Projet/Art/Characters/Customers/{data.id}.png");
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
