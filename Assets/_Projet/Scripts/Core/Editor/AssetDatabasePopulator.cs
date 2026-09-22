#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VillaDelChef.ScriptableObjects;

namespace VillaDelChef.EditorTools
{
    public static class AssetDatabasePopulator
    {
        [MenuItem("Tools/Villa del Chef/Populate ScriptableObjects from Sprites", false, 3)]
        public static void GenerateAllScriptableObjects()
        {
            EnsureDirectories();

            // 1. INGREDIENTES CULTIVABLES (Fruits & Nuts pack)
            var ingStrawberry = CreateOrUpdateIngredient("ing_strawberry", "Frutilla", IngredientCategory.Cultivable, FindSpriteByName("strawberry"), 8, 4);
            var ingApple = CreateOrUpdateIngredient("ing_apple", "Manzana Roja", IngredientCategory.Cultivable, FindSpriteByName("apple_red"), 6, 3);
            var ingLemon = CreateOrUpdateIngredient("ing_lemon", "Limón", IngredientCategory.Cultivable, FindSpriteByName("lemon"), 5, 2);
            var ingWatermelon = CreateOrUpdateIngredient("ing_watermelon", "Sandía", IngredientCategory.Cultivable, FindSpriteByName("watermelon"), 14, 7);
            var ingBanana = CreateOrUpdateIngredient("ing_banana", "Plátano", IngredientCategory.Cultivable, FindSpriteByName("banana"), 6, 3);
            var ingBlueberry = CreateOrUpdateIngredient("ing_blueberry", "Arándano", IngredientCategory.Cultivable, FindSpriteByName("blueberry"), 7, 3);
            var ingPineapple = CreateOrUpdateIngredient("ing_pineapple", "Piña", IngredientCategory.Cultivable, FindSpriteByName("pineapple"), 12, 6);
            var ingPeach = CreateOrUpdateIngredient("ing_peach", "Durazno", IngredientCategory.Cultivable, FindSpriteByName("peach"), 7, 3);
            var ingCherry = CreateOrUpdateIngredient("ing_cherry", "Cereza", IngredientCategory.Cultivable, FindSpriteByName("cherry"), 8, 4);
            var ingOrange = CreateOrUpdateIngredient("ing_orange", "Mandarina", IngredientCategory.Cultivable, FindSpriteByName("mandarin"), 6, 3);
            var ingWalnut = CreateOrUpdateIngredient("ing_walnut", "Nuez", IngredientCategory.Cultivable, FindSpriteByName("walnut"), 10, 5);

            // INGREDIENTES COMPRABLES / BÁSICOS (pixelfood)
            var ingMeat = CreateOrUpdateIngredient("ing_meat", "Carne Fresca", IngredientCategory.Comprable, FindSpriteByName("10_beef"), 15, 8);
            var ingBread = CreateOrUpdateIngredient("ing_bread", "Pan de Campo", IngredientCategory.Comprable, FindSpriteByName("65_loafbread"), 6, 3);
            var ingCheese = CreateOrUpdateIngredient("ing_cheese", "Queso Fundido", IngredientCategory.Comprable, FindSpriteByName("24_cheese"), 8, 4);
            var ingPotato = CreateOrUpdateIngredient("ing_potato", "Papa", IngredientCategory.Cultivable, FindSpriteByName("77_potatochips"), 5, 2);
            var ingEgg = CreateOrUpdateIngredient("ing_egg", "Huevo de Granja", IngredientCategory.Comprable, FindSpriteByName("38_friedegg"), 5, 2);
            var ingSalmon = CreateOrUpdateIngredient("ing_salmon", "Salmón Fresco", IngredientCategory.Comprable, FindSpriteByName("88_salmon"), 22, 11);
            var ingBacon = CreateOrUpdateIngredient("ing_bacon", "Tocino Crujiente", IngredientCategory.Comprable, FindSpriteByName("13_bacon"), 12, 6);
            var ingChocolate = CreateOrUpdateIngredient("ing_chocolate", "Chocolate Dulce", IngredientCategory.Comprable, FindSpriteByName("26_chocolate"), 10, 5);

            // 2. ESTACIONES DE COCINA (con los nuevos sprites de professional_kitchen)
            CreateOrUpdateStation("station_cocina", "Cocina a Gas Profesional", StationType.Cocina, FindSpriteByName("station_stove_pro") ?? FindSpriteByName("tools"));
            CreateOrUpdateStation("station_parrilla", "Parrilla de Carbón Industrial", StationType.Parrilla, FindSpriteByName("station_grill_iron") ?? FindSpriteByName("10_beef"));
            CreateOrUpdateStation("station_horno", "Horno de Piedra y Acero", StationType.Horno, FindSpriteByName("station_oven_stone") ?? FindSpriteByName("81_pizza"));
            CreateOrUpdateStation("station_freidora", "Freidora Profesional", StationType.Freidora, FindSpriteByName("station_fryer_basket") ?? FindSpriteByName("44_frenchfries"));
            CreateOrUpdateStation("station_cafetera", "Cafetera Espresso", StationType.Cafetera, FindSpriteByName("station_stove_dual") ?? FindSpriteByName("tools"));

            // 3. RECETAS COMPLETAS (Plato + Plato Servido en Mesa + Ingredientes Reales)
            // Hamburguesa
            CreateOrUpdateRecipe("rec_burger", "Hamburguesa Clásica", StationType.Parrilla, 12f, 130, 25,
                FindSpriteByName("15_burger"),
                FindSpriteByName("16_burger_dish"),
                new[] { (ingBread, 1), (ingMeat, 1), (ingCheese, 1) });

            // Papas Fritas
            CreateOrUpdateRecipe("rec_fries", "Papas Fritas Crujientes", StationType.Freidora, 8f, 80, 15,
                FindSpriteByName("44_frenchfries"),
                FindSpriteByName("45_frenchfries_dish"),
                new[] { (ingPotato, 2) });

            // Pizza Margarita
            CreateOrUpdateRecipe("rec_pizza", "Pizza Margarita", StationType.Horno, 20f, 190, 40,
                FindSpriteByName("81_pizza"),
                FindSpriteByName("82_pizza_dish"),
                new[] { (ingBread, 1), (ingCheese, 2) });

            // Bife Asado
            CreateOrUpdateRecipe("rec_steak", "Bife de Lomo Jugoso", StationType.Parrilla, 16f, 220, 45,
                FindSpriteByName("95_steak"),
                FindSpriteByName("96_steak_dish"),
                new[] { (ingMeat, 2), (ingPotato, 1) });

            // Tacos
            CreateOrUpdateRecipe("rec_tacos", "Tacos con Carne y Queso", StationType.Parrilla, 14f, 160, 30,
                FindSpriteByName("99_taco"),
                FindSpriteByName("100_taco_dish"),
                new[] { (ingMeat, 1), (ingCheese, 1), (ingBread, 1) });

            // Tarta de Frutillas
            CreateOrUpdateRecipe("rec_strawberry_cake", "Tarta de Frutillas Frescas", StationType.Horno, 22f, 240, 50,
                FindSpriteByName("90_strawberrycake"),
                FindSpriteByName("91_strawberrycake_dish"),
                new[] { (ingBread, 1), (ingStrawberry, 3), (ingCheese, 1) });

            // Tarta de Manzana
            CreateOrUpdateRecipe("rec_apple_pie", "Pie de Manzana Horneado", StationType.Horno, 18f, 175, 35,
                FindSpriteByName("05_apple_pie"),
                FindSpriteByName("06_apple_pie_dish"),
                new[] { (ingBread, 1), (ingApple, 3) });

            // Tarta de Limón
            CreateOrUpdateRecipe("rec_lemon_pie", "Pie de Limón Merengado", StationType.Horno, 16f, 165, 32,
                FindSpriteByName("63_lemonpie"),
                FindSpriteByName("64_lemonpie_dish"),
                new[] { (ingBread, 1), (ingLemon, 3), (ingEgg, 1) });

            // Sushi Rolls
            CreateOrUpdateRecipe("rec_sushi", "Rolls de Salmón", StationType.Cocina, 15f, 210, 42,
                FindSpriteByName("97_sushi"),
                FindSpriteByName("98_sushi_dish"),
                new[] { (ingSalmon, 2) });

            // Hot Dog
            CreateOrUpdateRecipe("rec_hotdog", "Hot Dog Americano", StationType.Parrilla, 10f, 110, 20,
                FindSpriteByName("54_hotdog"),
                FindSpriteByName("56_hotdog_dish"),
                new[] { (ingBread, 1), (ingMeat, 1), (ingCheese, 1) });

            // Ramen
            CreateOrUpdateRecipe("rec_ramen", "Ramen Tradicional", StationType.Cocina, 16f, 185, 38,
                FindSpriteByName("87_ramen"),
                FindSpriteByName("87_ramen"),
                new[] { (ingMeat, 1), (ingEgg, 1) });

            // Waffles
            CreateOrUpdateRecipe("rec_waffle", "Waffles con Frutas", StationType.Cocina, 10f, 125, 24,
                FindSpriteByName("101_waffle"),
                FindSpriteByName("102_waffle_dish"),
                new[] { (ingBread, 1), (ingStrawberry, 1), (ingBanana, 1) });

            // 4. CULTIVOS EXTERIORES
            CreateOrUpdateCrop("crop_strawberry", "Sembradero de Frutillas", 60f, ingStrawberry, 4, 15,
                FindSpriteByName("strawberry"),
                FindSpriteByName("strawberry"));

            CreateOrUpdateCrop("crop_lemon", "Limonero de Patio", 90f, ingLemon, 3, 12,
                FindSpriteByName("lemon"),
                FindSpriteByName("lemon"));

            CreateOrUpdateCrop("crop_watermelon", "Huerto de Sandía", 150f, ingWatermelon, 2, 20,
                FindSpriteByName("watermelon"),
                FindSpriteByName("watermelon"));

            CreateOrUpdateCrop("crop_apple", "Manzano Pequeño", 120f, ingApple, 3, 15,
                FindSpriteByName("apple_red"),
                FindSpriteByName("apple_red"));

            CreateOrUpdateCrop("crop_blueberry", "Arbusto de Arándanos", 80f, ingBlueberry, 4, 14,
                FindSpriteByName("blueberry"),
                FindSpriteByName("blueberry"));

            CreateOrUpdateCrop("crop_pineapple", "Sembradero de Piña", 140f, ingPineapple, 2, 18,
                FindSpriteByName("pineapple"),
                FindSpriteByName("pineapple"));

            // 5. MISIONES
            CreateOrUpdateQuest("quest_serve_burgers", "¡Especialidad de la Casa!", "Sirve 5 hamburguesas a los clientes para complacer al público.",
                QuestType.ServeCustomers, "rec_burger", 5, 200, 40, FindSpriteByName("coins"));

            CreateOrUpdateQuest("quest_harvest_strawberries", "Cosecha Frutal", "Cosecha 8 frutillas frescas de tu huerta exterior.",
                QuestType.HarvestCrops, "crop_strawberry", 8, 160, 35, FindSpriteByName("star"));

            CreateOrUpdateQuest("quest_bake_pizzas", "Fiebre de Pizza", "Hornea 3 pizzas crujientes en el horno de piedra.",
                QuestType.CookDishes, "rec_pizza", 3, 220, 45, FindSpriteByName("tools"));

            CreateOrUpdateQuest("quest_earn_coins", "Camino a las Estrellas", "Gana 500 monedas atendiendo clientes y expandiendo tu menú.",
                QuestType.EarnCoins, "", 500, 250, 60, FindSpriteByName("coins"));

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AssetDatabasePopulator] ¡Todos los ScriptableObjects han sido creados y vinculados con los sprites reales!");
        }

        private static void EnsureDirectories()
        {
            string[] dirs = new[]
            {
                "Assets/_Projet/ScriptableObjects/Ingredients",
                "Assets/_Projet/ScriptableObjects/Recipes",
                "Assets/_Projet/ScriptableObjects/Crops",
                "Assets/_Projet/ScriptableObjects/Furniture",
                "Assets/_Projet/ScriptableObjects/Customers",
                "Assets/_Projet/ScriptableObjects/Workers",
                "Assets/_Projet/ScriptableObjects/Quests",
                "Assets/_Projet/ScriptableObjects/Stations"
            };
            foreach (var d in dirs)
            {
                if (!Directory.Exists(d)) Directory.CreateDirectory(d);
            }
        }

        private static Sprite FindSpriteByName(string name)
        {
            string[] guids = AssetDatabase.FindAssets($"{name} t:Sprite");
            if (guids != null && guids.Length > 0)
            {
                // 1. Prefer InUse folder
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("Assets/_Projet/Art/") && path.Contains("InUse"))
                    {
                        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (s != null) return s;
                    }
                }
                // 2. Fallback to any in Assets/_Projet/Art/
                foreach (var guid in guids)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path.Contains("Assets/_Projet/Art/"))
                    {
                        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                        if (s != null) return s;
                    }
                }
                string fallbackPath = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<Sprite>(fallbackPath);
            }
            return null;
        }

        private static IngredientSO CreateOrUpdateIngredient(string id, string name, IngredientCategory cat, Sprite icon, int buy, int sell)
        {
            string path = $"Assets/_Projet/Resources/Ingredients/{id}.asset";
            IngredientSO so = AssetDatabase.LoadAssetAtPath<IngredientSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<IngredientSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.ingredientID = id;
            so.ingredientName = name;
            so.category = cat;
            so.icon = icon;
            so.buyPrice = buy;
            so.sellPrice = sell;
            EditorUtility.SetDirty(so);
            return so;
        }

        private static StationSO CreateOrUpdateStation(string id, string name, StationType type, Sprite icon)
        {
            string path = $"Assets/_Projet/Resources/Stations/{id}.asset";
            StationSO so = AssetDatabase.LoadAssetAtPath<StationSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<StationSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.stationID = id;
            so.stationName = name;
            so.stationType = type;
            so.icon = icon;
            EditorUtility.SetDirty(so);
            return so;
        }

        private static RecipeSO CreateOrUpdateRecipe(string id, string name, StationType station, float cookTime, int sellPrice, int xp, Sprite icon, Sprite dishSprite, (IngredientSO ing, int count)[] ingredients)
        {
            string path = $"Assets/_Projet/Resources/Recipes/{id}.asset";
            RecipeSO so = AssetDatabase.LoadAssetAtPath<RecipeSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<RecipeSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.recipeID = id;
            so.recipeName = name;
            so.requiredStation = station;
            so.cookTimeSeconds = cookTime;
            so.sellPrice = sellPrice;
            so.experienceReward = xp;
            so.icon = icon;
            so.finishedDishSprite = dishSprite != null ? dishSprite : icon;
            so.requiredIngredients = new List<IngredientRequirement>();
            if (ingredients != null)
            {
                foreach (var (ing, count) in ingredients)
                {
                    if (ing != null)
                    {
                        so.requiredIngredients.Add(new IngredientRequirement { ingredient = ing, amount = count });
                    }
                }
            }
            EditorUtility.SetDirty(so);
            return so;
        }

        private static CropSO CreateOrUpdateCrop(string id, string name, float growthTime, IngredientSO harvestIng, int harvestAmt, int cost, Sprite seedIcon, Sprite readySprite)
        {
            string path = $"Assets/_Projet/Resources/Crops/{id}.asset";
            CropSO so = AssetDatabase.LoadAssetAtPath<CropSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<CropSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.cropID = id;
            so.cropName = name;
            so.totalGrowthTimeSeconds = growthTime;
            so.harvestIngredient = harvestIng;
            so.harvestAmount = harvestAmt;
            so.seedCost = cost;
            so.seedIcon = seedIcon;
            so.readyStageSprite = readySprite;
            so.seedStageSprite = seedIcon;
            EditorUtility.SetDirty(so);
            return so;
        }

        private static QuestSO CreateOrUpdateQuest(string id, string title, string desc, QuestType type, string target, int required, int rewardCoins, int rewardXP, Sprite icon)
        {
            string path = $"Assets/_Projet/Resources/Quests/{id}.asset";
            QuestSO so = AssetDatabase.LoadAssetAtPath<QuestSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<QuestSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.questID = id;
            so.title = title;
            so.description = desc;
            so.questType = type;
            so.targetID = target;
            so.requiredAmount = required;
            so.rewardCoins = rewardCoins;
            so.rewardXP = rewardXP;
            so.icon = icon;
            EditorUtility.SetDirty(so);
            return so;
        }
    }
}
#endif
