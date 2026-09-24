#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VillaDelChef.Building;
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

            // 6. COMERCIANTES Y NPCS ESPECIALIZADOS (FASE 2)
            // Elena: Semillas y vegetales
            var elenaItems = new List<VendorItemEntry>
            {
                new VendorItemEntry { itemID = "ing_strawberry", displayName = "Semillas de Frutilla", icon = ingStrawberry.icon, buyPrice = 12, maxStock = 15 },
                new VendorItemEntry { itemID = "ing_lemon", displayName = "Semillas de Limón", icon = ingLemon.icon, buyPrice = 10, maxStock = 15 },
                new VendorItemEntry { itemID = "ing_watermelon", displayName = "Semillas de Sandía", icon = ingWatermelon.icon, buyPrice = 20, maxStock = 10 },
                new VendorItemEntry { itemID = "ing_apple", displayName = "Manzana de Huerta", icon = ingApple.icon, buyPrice = 10, maxStock = 15 },
                new VendorItemEntry { itemID = "ing_potato", displayName = "Papas de Campo", icon = ingPotato.icon, buyPrice = 8, maxStock = 20 },
                new VendorItemEntry { itemID = "ing_banana", displayName = "Plátano Maduro", icon = ingBanana.icon, buyPrice = 10, maxStock = 15 },
                new VendorItemEntry { itemID = "ing_blueberry", displayName = "Arándano Silvestre", icon = ingBlueberry.icon, buyPrice = 12, maxStock = 15 }
            };
            var vendorElena = CreateOrUpdateVendor("vendor_elena", "Semillas & Huerto de Elena", 180, elenaItems);
            CreateOrUpdateNPC("npc_elena", "Elena", "Agricultora de la Villa", FindSpriteByName("portrait_elena") ?? ingStrawberry.icon, FindSpriteByName("npc_elena"), "¡Hola Chef! La tierra fértil de la villa nos da las mejores semillas y verduras frescas.", vendorElena, 1);

            // Bruno: Carnes y embutidos
            var brunoItems = new List<VendorItemEntry>
            {
                new VendorItemEntry { itemID = "ing_meat", displayName = "Carne Fresca", icon = ingMeat.icon, buyPrice = 18, maxStock = 20 },
                new VendorItemEntry { itemID = "ing_bacon", displayName = "Tocino Ahumado", icon = ingBacon.icon, buyPrice = 14, maxStock = 15 },
                new VendorItemEntry { itemID = "ing_egg", displayName = "Huevos de Granja", icon = ingEgg.icon, buyPrice = 6, maxStock = 30 }
            };
            var vendorBruno = CreateOrUpdateVendor("vendor_bruno", "Carnicería Criolla de Bruno", 180, brunoItems);
            CreateOrUpdateNPC("npc_bruno", "Bruno", "Maestro Carnicero", FindSpriteByName("portrait_bruno") ?? ingMeat.icon, FindSpriteByName("npc_bruno"), "¡Buenas! Los mejores cortes de carne, costillas y tocino artesanal para tu cocina.", vendorBruno, 1);

            // Tomás: Panadería y lácteos
            var tomasItems = new List<VendorItemEntry>
            {
                new VendorItemEntry { itemID = "ing_bread", displayName = "Pan Rústico Artesanal", icon = ingBread.icon, buyPrice = 8, maxStock = 25 },
                new VendorItemEntry { itemID = "ing_cheese", displayName = "Queso Fundido Suave", icon = ingCheese.icon, buyPrice = 10, maxStock = 20 },
                new VendorItemEntry { itemID = "ing_chocolate", displayName = "Chocolate Dulce", icon = ingChocolate.icon, buyPrice = 12, maxStock = 15 },
                new VendorItemEntry { itemID = "ing_walnut", displayName = "Nueces Tostadas", icon = ingWalnut.icon, buyPrice = 12, maxStock = 15 }
            };
            var vendorTomas = CreateOrUpdateVendor("vendor_tomas", "Panadería & Molino de Tomás", 180, tomasItems);
            CreateOrUpdateNPC("npc_tomas", "Tomás", "Panadero Artesanal", FindSpriteByName("portrait_tomas") ?? ingBread.icon, FindSpriteByName("npc_tomas"), "¡Huele a pan recién horneado! Harina pura, quesos y panecillos dorados para tus recetas.", vendorTomas, 1);

            // Marina: Pescadería
            var marinaItems = new List<VendorItemEntry>
            {
                new VendorItemEntry { itemID = "ing_salmon", displayName = "Salmón Fresco del Muelle", icon = ingSalmon.icon, buyPrice = 25, maxStock = 15 }
            };
            var vendorMarina = CreateOrUpdateVendor("vendor_marina", "Lonja Marina de Pescados", 180, marinaItems);
            CreateOrUpdateNPC("npc_marina", "Marina", "Pescadora del Muelle", FindSpriteByName("portrait_marina") ?? ingSalmon.icon, FindSpriteByName("npc_marina"), "¡Directo del agua cristalina! Salmón fresco y delicias marinas para tu menú.", vendorMarina, 2);

            // Amelia: Maquinaria y estaciones
            var ameliaItems = new List<VendorItemEntry>
            {
                new VendorItemEntry { itemID = "station_cocina", displayName = "Cocina a Gas Profesional", icon = FindSpriteByName("station_stove_pro") ?? FindSpriteByName("tools"), buyPrice = 150, maxStock = 2, isInventoryItem = false },
                new VendorItemEntry { itemID = "station_parrilla", displayName = "Parrilla de Hierro Industrial", icon = FindSpriteByName("station_grill_iron") ?? FindSpriteByName("10_beef"), buyPrice = 200, maxStock = 2, isInventoryItem = false },
                new VendorItemEntry { itemID = "station_horno", displayName = "Horno de Piedra y Acero", icon = FindSpriteByName("station_oven_stone") ?? FindSpriteByName("81_pizza"), buyPrice = 250, maxStock = 1, isInventoryItem = false },
                new VendorItemEntry { itemID = "station_freidora", displayName = "Freidora Profesional", icon = FindSpriteByName("station_fryer_basket") ?? FindSpriteByName("44_frenchfries"), buyPrice = 180, maxStock = 2, isInventoryItem = false }
            };
            var vendorAmelia = CreateOrUpdateVendor("vendor_amelia", "Equipamiento de Cocina Amelia", 300, ameliaItems);
            CreateOrUpdateNPC("npc_amelia", "Amelia", "Proveedora de Cocinas & Equipamiento", FindSpriteByName("portrait_amelia") ?? FindSpriteByName("tools"), FindSpriteByName("npc_amelia"), "¡Hola colega! Te traigo la maquinaria industrial y estaciones de acero para hacer brillar tu cocina.", vendorAmelia, 2);

            // Lucas: Muebles y carpintería
            var lucasItems = new List<VendorItemEntry>
            {
                new VendorItemEntry { itemID = "table_wood", displayName = "Mesa de Madera Rústica", icon = FindSpriteByName("table") ?? FindSpriteByName("tools"), buyPrice = 80, maxStock = 5, isInventoryItem = false },
                new VendorItemEntry { itemID = "chair_wood", displayName = "Silla de Madera", icon = FindSpriteByName("chair") ?? FindSpriteByName("tools"), buyPrice = 30, maxStock = 10, isInventoryItem = false },
                new VendorItemEntry { itemID = "counter_delivery", displayName = "Mesa de Despacho", icon = FindSpriteByName("tools"), buyPrice = 100, maxStock = 2, isInventoryItem = false }
            };
            var vendorLucas = CreateOrUpdateVendor("vendor_lucas", "Carpintería y Muebles Lucas", 300, lucasItems);
            CreateOrUpdateNPC("npc_lucas", "Lucas", "Carpintero & Constructor", FindSpriteByName("portrait_lucas") ?? FindSpriteByName("tools"), FindSpriteByName("npc_lucas"), "Mesas de roble, sillas cómodas y mostradores resistentes. ¡Hagamos tu restaurante acogedor!", vendorLucas, 1);

            // Sofía: Decoraciones y ambientación
            var sofiaItems = new List<VendorItemEntry>
            {
                new VendorItemEntry { itemID = "crop_plot", displayName = "Parcela de Cultivo Decorada", icon = FindSpriteByName("star"), buyPrice = 50, maxStock = 5, isInventoryItem = false }
            };
            var vendorSofia = CreateOrUpdateVendor("vendor_sofia", "Decoraciones & Estilo Sofía", 300, sofiaItems);
            CreateOrUpdateNPC("npc_sofia", "Sofía", "Decoradora & Paisajista", FindSpriteByName("portrait_sofia") ?? FindSpriteByName("star"), FindSpriteByName("npc_sofia"), "¡La estética lo es todo! Plantas ornamentales, lámparas y faroles para enamorar a los clientes.", vendorSofia, 3);

            // 7. INSUMOS Y RECETAS DE CRAFTING INTERMEDIO (FASE 3)
            var ingFlour = CreateOrUpdateIngredient("ing_flour", "Harina Blanca", IngredientCategory.Procesado, FindSpriteByName("ing_flour") ?? FindSpriteByName("tools"), 10, 5);
            var ingDough = CreateOrUpdateIngredient("ing_dough", "Masa Artesanal", IngredientCategory.Procesado, FindSpriteByName("ing_dough") ?? FindSpriteByName("tools"), 14, 7);
            var ingSauce = CreateOrUpdateIngredient("ing_sauce", "Salsa Casera", IngredientCategory.Procesado, FindSpriteByName("ing_sauce") ?? FindSpriteByName("tools"), 12, 6);
            var ingJam = CreateOrUpdateIngredient("ing_jam", "Mermelada Dulce", IngredientCategory.Procesado, FindSpriteByName("ing_jam") ?? FindSpriteByName("tools"), 18, 9);

            // Receta 1: Molienda de Harina (Molino)
            CreateOrUpdateCraftingRecipe("craft_flour", "Molienda de Harina", "Muele granos y tubérculos para obtener harina blanca de repostería.",
                CraftingStationType.Molino, 8f,
                new[] { (ingPotato, 2) },
                ingFlour, 2, 15, FindSpriteByName("ing_flour") ?? FindSpriteByName("tools"));

            // Receta 2: Amasado de Masa (Mesa de Amasado)
            CreateOrUpdateCraftingRecipe("craft_dough", "Amasado Tradicional", "Mezcla harina con huevo de granja para amasar bases de pizza y panes dorados.",
                CraftingStationType.MesaAmasado, 10f,
                new[] { (ingFlour, 1), (ingEgg, 1) },
                ingDough, 2, 20, FindSpriteByName("ing_dough") ?? FindSpriteByName("tools"));

            // Receta 3: Salsa de Tomate & Especias (Procesador)
            CreateOrUpdateCraftingRecipe("craft_sauce", "Salsa Especial de la Casa", "Reduce y sazona salsa rústica espesa para pastas y pizzas al horno.",
                CraftingStationType.Procesador, 12f,
                new[] { (ingPotato, 2) },
                ingSauce, 2, 18, FindSpriteByName("ing_sauce") ?? FindSpriteByName("tools"));

            // Receta 4: Mermelada de Frutilla (Marmita Dulce)
            CreateOrUpdateCraftingRecipe("craft_strawberry_jam", "Mermelada de Frutilla", "Cocción lenta de frutillas cosechadas a fuego lento hasta espesar.",
                CraftingStationType.MarmitaDulce, 15f,
                new[] { (ingStrawberry, 2) },
                ingJam, 1, 25, FindSpriteByName("ing_jam") ?? FindSpriteByName("tools"));

            // Receta 5: Confitura de Manzana (Marmita Dulce)
            CreateOrUpdateCraftingRecipe("craft_apple_jam", "Dulce de Manzana", "Dulce aromático de manzanas frescas para pasteles y tostadas.",
                CraftingStationType.MarmitaDulce, 14f,
                new[] { (ingApple, 2) },
                ingJam, 1, 22, FindSpriteByName("ing_jam") ?? FindSpriteByName("tools"));

            // 8. EXPANSIONES DE LA VILLA Y ZONIFICACIÓN (FASE 4)
            CreateOrUpdateExpansion("exp_terrace", "Terraza del Jardín", "Abre las puertas al patio exterior. Permite colocar mesas y sombrillas al aire libre para que los clientes disfruten del sol y la brisa.",
                ZoneType.Terrace, new RectInt(0, 7, 6, 9), 2, 200, 60, new Vector2(-12.5f, -3.5f), FindSpriteByName("exp_terrace") ?? FindSpriteByName("star"));

            CreateOrUpdateExpansion("exp_crops", "Huerto Alto del Valle", "Tierra fértil rica en minerales para duplicar tu producción agrícola. Desbloquea espacio para nuevas parcelas de cultivo.",
                ZoneType.Farming, new RectInt(0, 16, 12, 8), 3, 450, 100, new Vector2(-10f, 4.5f), FindSpriteByName("exp_crops") ?? FindSpriteByName("star"));

            CreateOrUpdateExpansion("exp_crafting", "Taller de Molienda & Artesanía", "Espacio pavimentado ideal para instalar múltiples molinos, mesas de amasado, prensas y marmitas de salsas.",
                ZoneType.Crafting, new RectInt(18, 16, 14, 8), 4, 750, 150, new Vector2(8f, 4.5f), FindSpriteByName("exp_crafting") ?? FindSpriteByName("tools"));

            CreateOrUpdateExpansion("exp_market", "Plaza del Mercado Gastronómico", "Un amplio bulevar adoquinado donde los 7 comerciantes de la villa pueden establecer sus puestos comerciales permanentes.",
                ZoneType.Market, new RectInt(26, 0, 6, 16), 5, 1200, 250, new Vector2(10.5f, -6f), FindSpriteByName("exp_market") ?? FindSpriteByName("coins"));

            // 9. ARQUETIPOS DE CLIENTES CON REPUTACIÓN Y DINÁMICA (FASE 5)
            CreateOrUpdateCustomer("cust_normal", "Comensal Tranquilo", CustomerArchetype.Normal, 2.5f, 60f, 0.5f, 1.0f, 1, 2, 2, 15, FindSpriteByName("cust_normal") ?? FindSpriteByName("chef_player"));
            CreateOrUpdateCustomer("cust_impatient", "Oficinista Apurado", CustomerArchetype.Impaciente, 3.2f, 30f, 0.35f, 0.8f, 1, 2, 3, 20, FindSpriteByName("cust_impatient") ?? FindSpriteByName("chef_player"));
            CreateOrUpdateCustomer("cust_generous", "Abuela Consentidora", CustomerArchetype.Generoso, 2.2f, 75f, 1.0f, 2.0f, 2, 3, 2, 25, FindSpriteByName("cust_generous") ?? FindSpriteByName("chef_player"));
            CreateOrUpdateCustomer("cust_gourmet", "Sibarita del Buen Comer", CustomerArchetype.Gourmet, 2.4f, 50f, 0.7f, 1.8f, 2, 4, 4, 35, FindSpriteByName("cust_gourmet") ?? FindSpriteByName("chef_player"));
            CreateOrUpdateCustomer("cust_tourist", "Turista Curioso", CustomerArchetype.Turista, 2.6f, 65f, 0.8f, 1.5f, 3, 4, 2, 30, FindSpriteByName("cust_tourist") ?? FindSpriteByName("chef_player"));
            CreateOrUpdateCustomer("cust_critic", "Crítico Gastronómico", CustomerArchetype.CriticoGastronomico, 2.3f, 45f, 0.4f, 1.2f, 4, 15, 10, 100, FindSpriteByName("cust_critic") ?? FindSpriteByName("chef_player"));
            CreateOrUpdateCustomer("cust_vip", "Celebridad Gourmet", CustomerArchetype.VIP, 2.7f, 40f, 0.95f, 3.0f, 5, 8, 6, 80, FindSpriteByName("cust_vip") ?? FindSpriteByName("chef_player"));

            // 10. CADENA DE MISIONES NARRATIVAS CON LOS 7 ESPECIALISTAS (FASE 5)
            CreateOrUpdateQuest("quest_elena_garden", "Huerto en Flor", "Cosecha 6 frutillas dulces de la huerta para los postres de la villa.",
                QuestType.HarvestCrops, "crop_strawberry", 6, 180, 40, FindSpriteByName("strawberry") ?? FindSpriteByName("star"), 5, "Elena");

            CreateOrUpdateQuest("quest_bruno_grill", "Festín del Asador", "Asa 4 hamburguesas a la parrilla con tocino y queso fundido.",
                QuestType.CookDishes, "rec_burger", 4, 240, 50, FindSpriteByName("15_burger") ?? FindSpriteByName("tools"), 6, "Bruno");

            CreateOrUpdateQuest("quest_tomas_flour", "El Secreto de la Molienda", "Muele 3 sacos de harina blanca en el molino de grano para amasar el pan del día.",
                QuestType.CraftItems, "craft_flour", 3, 200, 45, FindSpriteByName("ing_flour") ?? FindSpriteByName("tools"), 6, "Tomás");

            CreateOrUpdateQuest("quest_marina_salmon", "Frescura del Mar", "Sirve 2 platos de salmón fresco a los visitantes del restaurante.",
                QuestType.CookDishes, "rec_salmon", 2, 280, 60, FindSpriteByName("88_salmon") ?? FindSpriteByName("coins"), 8, "Marina");

            CreateOrUpdateQuest("quest_amelia_kitchen", "Cocina a Toda Marcha", "Fríe 5 porciones de papas crujientes en la freidora profesional.",
                QuestType.CookDishes, "rec_fries", 5, 220, 45, FindSpriteByName("44_frenchfries") ?? FindSpriteByName("tools"), 5, "Amelia");

            CreateOrUpdateQuest("quest_lucas_terrace", "Brisa en la Terraza", "Adquiere la expansión de la terraza del jardín para ofrecer mesas al aire libre.",
                QuestType.UnlockExpansion, "exp_terrace", 1, 350, 80, FindSpriteByName("exp_terrace") ?? FindSpriteByName("star"), 10, "Lucas");

            CreateOrUpdateQuest("quest_sofia_prestige", "Villa de Prestigio", "Genera 600 monedas deleitando a clientes de todos los rincones.",
                QuestType.EarnCoins, "", 600, 300, 70, FindSpriteByName("coins"), 12, "Sofía");

            // 9. MUEBLES Y ESTACIONES DATA-DRIVEN (FASE 6.1)
            CreateOrUpdateFurniture("stove_01", "Cocina a Gas", FurnitureCategory.Cocina, 2, 2, 150, 75, 1, FindSpriteByName("station_stove_pro") ?? FindSpriteByName("stove_kitchen"), new List<ZoneType> { ZoneType.Kitchen }, "Cocina profesional a gas para hervir y saltear alimentos.");
            CreateOrUpdateFurniture("grill_01", "Parrilla de Hierro", FurnitureCategory.Cocina, 2, 2, 200, 100, 1, FindSpriteByName("station_grill_iron") ?? FindSpriteByName("grill_iron"), new List<ZoneType> { ZoneType.Kitchen }, "Parrilla de carbón y hierro fundido para carnes y hamburguesas.");
            CreateOrUpdateFurniture("counter_delivery", "Mesa de Entrega", FurnitureCategory.MesaEntrega, 3, 1, 100, 50, 1, FindSpriteByName("counter_delivery") ?? FindSpriteByName("tools"), new List<ZoneType> { ZoneType.Kitchen, ZoneType.Dining }, "Mostrador de servicio donde los cocineros despachan los pedidos.");
            CreateOrUpdateFurniture("table_wood", "Mesa de Madera", FurnitureCategory.Mesa, 2, 2, 80, 40, 1, FindSpriteByName("table_wood"), new List<ZoneType> { ZoneType.Dining, ZoneType.Terrace }, "Mesa rústica de madera noble para cuatro comensales.");
            CreateOrUpdateFurniture("chair_wood", "Silla de Madera", FurnitureCategory.Silla, 1, 1, 30, 15, 1, FindSpriteByName("chair_wood"), new List<ZoneType> { ZoneType.Dining, ZoneType.Terrace }, "Silla cómoda para comensales en el salón comedor o terraza.");
            CreateOrUpdateFurniture("crop_plot", "Sembradero", FurnitureCategory.Sembradero, 2, 2, 50, 25, 1, FindSpriteByName("crop_plot"), new List<ZoneType> { ZoneType.Exterior, ZoneType.Farming }, "Parcela fértil para sembrar y cosechar cultivos frescos.");

            // Estaciones de crafteo como muebles colocables en grilla
            CreateOrUpdateFurniture("station_molino", "Molino de Grano", FurnitureCategory.EstacionCrafting, 2, 2, 250, 125, 1, FindSpriteByName("station_mill") ?? FindSpriteByName("tools"), new List<ZoneType> { ZoneType.Exterior, ZoneType.Kitchen }, "Molino de piedra para transformar granos en harina blanca.");
            CreateOrUpdateFurniture("station_mesaamasado", "Mesa de Amasado", FurnitureCategory.EstacionCrafting, 2, 2, 200, 100, 2, FindSpriteByName("table_round") ?? FindSpriteByName("table_wood"), new List<ZoneType> { ZoneType.Kitchen }, "Mesa de trabajo para amasar masas tradicionales.");
            CreateOrUpdateFurniture("station_procesador", "Procesador de Alimentos", FurnitureCategory.EstacionCrafting, 2, 2, 300, 150, 3, FindSpriteByName("tools"), new List<ZoneType> { ZoneType.Kitchen }, "Triturador y procesador para salsas rústicas.");
            CreateOrUpdateFurniture("station_prensalactea", "Prensa Láctea", FurnitureCategory.EstacionCrafting, 2, 2, 280, 140, 2, FindSpriteByName("24_cheese") ?? FindSpriteByName("tools"), new List<ZoneType> { ZoneType.Kitchen, ZoneType.Exterior }, "Prensa tradicional para cuajar y madurar quesos.");
            CreateOrUpdateFurniture("station_marmitadulce", "Marmita Dulce", FurnitureCategory.EstacionCrafting, 2, 2, 320, 160, 3, FindSpriteByName("tools"), new List<ZoneType> { ZoneType.Kitchen }, "Olla de cobre para confituras y mermeladas frutales.");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[AssetDatabasePopulator] ¡Todos los ScriptableObjects, NPCs, Crafting, Expansiones, Muebles, Clientes y Misiones han sido creados!");
        }

        private static void EnsureDirectories()
        {
            string[] dirs = new[]
            {
                "Assets/_Projet/Resources/Ingredients",
                "Assets/_Projet/Resources/Recipes",
                "Assets/_Projet/Resources/Crops",
                "Assets/_Projet/Resources/Furniture",
                "Assets/_Projet/Resources/Quests",
                "Assets/_Projet/Resources/Stations",
                "Assets/_Projet/Resources/Vendors",
                "Assets/_Projet/Resources/NPC",
                "Assets/_Projet/Resources/CraftingRecipes",
                "Assets/_Projet/Resources/Expansions",
                "Assets/_Projet/Resources/Customers",
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

        private static QuestSO CreateOrUpdateQuest(string id, string title, string desc, QuestType type, string target, int required, int rewardCoins, int rewardXP, Sprite icon, int rewardRep = 0, string speaker = "", RecipeSO unlockRecipe = null)
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
            so.rewardReputation = rewardRep;
            so.storySpeakerName = speaker;
            so.unlockedRecipeReward = unlockRecipe;
            so.icon = icon;
            EditorUtility.SetDirty(so);
            return so;
        }

        private static CustomerSO CreateOrUpdateCustomer(string id, string title, CustomerArchetype archetype, float speed, float patience, float tipProb, float tipMult, int level, int repReward, int repPenalty, int bonusXP, Sprite sprite)
        {
            string path = $"Assets/_Projet/Resources/Customers/{id}.asset";
            CustomerSO so = AssetDatabase.LoadAssetAtPath<CustomerSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<CustomerSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.customerID = id;
            so.customerTitle = title;
            so.archetype = archetype;
            so.movementSpeed = speed;
            so.basePatienceSeconds = patience;
            so.tipProbability = tipProb;
            so.tipMultiplier = tipMult;
            so.unlockLevel = level;
            so.reputationReward = repReward;
            so.reputationPenalty = repPenalty;
            so.bonusXP = bonusXP;
            so.characterSprite = sprite;
            EditorUtility.SetDirty(so);
            return so;
        }

        private static VendorSO CreateOrUpdateVendor(string id, string title, int restockInterval, List<VendorItemEntry> items)
        {
            string path = $"Assets/_Projet/Resources/Vendors/{id}.asset";
            VendorSO so = AssetDatabase.LoadAssetAtPath<VendorSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<VendorSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.vendorID = id;
            so.vendorTitle = title;
            so.restockIntervalSeconds = restockInterval;
            so.catalog = items ?? new List<VendorItemEntry>();
            EditorUtility.SetDirty(so);
            return so;
        }

        private static NPCSO CreateOrUpdateNPC(string id, string name, string role, Sprite portrait, Sprite worldSprite, string greeting, VendorSO vendor, int unlockLevel = 1)
        {
            string path = $"Assets/_Projet/Resources/NPC/{id}.asset";
            NPCSO so = AssetDatabase.LoadAssetAtPath<NPCSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<NPCSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.npcID = id;
            so.npcName = name;
            so.roleTitle = role;
            so.portrait = portrait;
            so.worldSprite = worldSprite;
            so.greetingDialogue = greeting;
            so.vendorData = vendor;
            so.unlockLevel = unlockLevel;
            EditorUtility.SetDirty(so);
            return so;
        }

        private static CraftingRecipeSO CreateOrUpdateCraftingRecipe(string id, string name, string desc, CraftingStationType station, float craftTime, (IngredientSO ing, int count)[] reqs, IngredientSO result, int resultAmt, int xp, Sprite icon)
        {
            string path = $"Assets/_Projet/Resources/CraftingRecipes/{id}.asset";
            CraftingRecipeSO so = AssetDatabase.LoadAssetAtPath<CraftingRecipeSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<CraftingRecipeSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.craftID = id;
            so.recipeName = name;
            so.description = desc;
            so.requiredStation = station;
            so.craftTimeSeconds = craftTime;
            so.resultIngredient = result;
            so.resultAmount = resultAmt;
            so.experienceReward = xp;
            so.icon = icon;
            so.requiredIngredients = new List<IngredientRequirement>();
            if (reqs != null)
            {
                foreach (var (ing, count) in reqs)
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

        private static ExpansionSO CreateOrUpdateExpansion(string id, string name, string desc, ZoneType zone, RectInt bounds, int level, int cost, int xp, Vector2 signPos, Sprite icon)
        {
            string path = $"Assets/_Projet/Resources/Expansions/{id}.asset";
            ExpansionSO so = AssetDatabase.LoadAssetAtPath<ExpansionSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<ExpansionSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.expansionID = id;
            so.displayName = name;
            so.description = desc;
            so.targetZone = zone;
            so.gridBounds = bounds;
            so.requiredRestaurantLevel = level;
            so.costGold = cost;
            so.rewardXP = xp;
            so.worldSignPosition = signPos;
            so.icon = icon;
            EditorUtility.SetDirty(so);
            return so;
        }

        private static FurnitureSO CreateOrUpdateFurniture(string id, string name, FurnitureCategory cat, int sizeX, int sizeY, int cost, int sellPrice, int unlockLevel, Sprite icon, List<ZoneType> allowedZones, string description = "")
        {
            string path = $"Assets/_Projet/Resources/Furniture/{id}.asset";
            FurnitureSO so = AssetDatabase.LoadAssetAtPath<FurnitureSO>(path);
            if (so == null)
            {
                so = ScriptableObject.CreateInstance<FurnitureSO>();
                AssetDatabase.CreateAsset(so, path);
            }
            so.furnitureID = id;
            so.furnitureName = name;
            so.category = cat;
            so.sizeX = sizeX;
            so.sizeY = sizeY;
            so.cost = cost;
            so.sellPrice = sellPrice;
            so.unlockLevel = unlockLevel;
            so.shopIcon = icon;
            so.allowedZones = allowedZones ?? new List<ZoneType>();
            so.description = description;
            EditorUtility.SetDirty(so);
            return so;
        }
    }
}
#endif
