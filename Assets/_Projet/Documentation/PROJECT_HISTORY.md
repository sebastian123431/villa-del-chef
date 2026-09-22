# PROJECT_HISTORY.md — Villa del Chef

Historial general persistente del desarrollo del proyecto. Cada cambio, fase o avance debe registrarse cronológicamente al final de este archivo sin eliminar entradas previas.

------------------------------------------------------------
FECHA:
2026-09-21 / 2026-09-22

VERSIÓN / FASE:
0.1.0 - Core MVP & Arquitectura Base

OBJETIVO:
Establecer la base jugable de Villa del Chef: sistemas de cocina, grilla 2D, clientes, economía, cultivos, inventario, trabajadores básicos, persistencia en JSON y escenas de Unity.

CAMBIOS REALIZADOS:
- Estructura modular creada en `Assets/_Projet/`.
- Sistema desacoplado de eventos estáticos en `GameEvents.cs`.
- Implementación de 3 escenas: `00_Boot.unity`, `01_MainMenu.unity` y `02_Restaurant.unity`.
- `RestaurantBootstrap.cs` como inicializador dinámico y fallback de runtime.
- Estaciones de cocina (`CookingStation.cs`), recetas (`RecipeSO`) e instancias de platos (`DishInstance.cs`).
- Mostrador de entrega (`DeliveryCounter.cs`) para conectar la cocina con los trabajadores.
- Parcelas de cultivo (`CropPlot.cs`) con tiempos UTC y fases de crecimiento.
- Clientes con máquina de estados (`CustomerController.cs`, `CustomerManager.cs`).
- Trabajador camarero básico (`WorkerController.cs`, `WorkerManager.cs`).
- Construcción y rotación en cuadrícula 2D (`GridManager.cs`, `BuildManager.cs`, `GridObject.cs`).
- Guardado y carga local JSON (`SaveManager.cs`, `SaveData.cs`).
- Interfaz táctil y gestos de cámara móvil (`TouchInputManager.cs`, `CameraController2D.cs`, `SafeAreaFitter.cs`).
- Editor Tools (`Tools > Villa del Chef`): Generador de Pixel Art procedimental, postprocesador PPU 16 y poblador de base de datos ScriptableObjects.

ARCHIVOS CREADOS:
- Todo el árbol inicial bajo `Assets/_Projet/Scripts/` (Building, Cooking, Core, Customers, Economy, Farming, Input, Inventory, Managers, Progression, Restaurant, Save, ScriptableObjects, UI, Utilities, Workers).
- Assets ScriptableObjects en `Assets/_Projet/Resources/` (Crops, Furniture, Ingredients, Quests, Recipes, Stations).
- Escenas en `Assets/_Projet/Scenes/`.
- `AGENTS.md` y `README.md` en la raíz del repositorio.

ARCHIVOS MODIFICADOS:
- `.gitattributes` (desactivación de Git LFS para evitar límites de cuenta en GitHub).
- `.gitignore` (inclusión de carpetas temporales scratch).

ARCHIVOS ELIMINADOS:
- Ninguno.

PROBLEMAS CORREGIDOS:
- Bloqueo en `git push` por cuota excedida de Git LFS; resuelto normalizando sprites a Git estándar.

DECISIONES IMPORTANTES:
- No usar Git LFS para imágenes ni audio liviano (~6 MB total).
- Arquitectura dirigida por datos (Data-Driven) con ScriptableObjects.
- Event-driven desacoplado con `GameEvents`.

ESTADO:
COMPLETADO

PRÓXIMO PASO:
Auditoría arquitectónica completa y Fase 1 de Refactor Seguro (IInteractable, TableState Dirty, evitar teleport, validación de caminos, coincidencia exacta de platos entre mostrador y mesa).
------------------------------------------------------------
FECHA:
2026-09-22

VERSIÓN / FASE:
0.1.1 - Fase 1: Core Refactor Seguro & Limpieza de Sistemas

OBJETIVO:
Implementar refactorización segura y correcciones fundamentales de jugabilidad, interacción polimórfica, ciclo de suciedad de mesas, prioridades de camareros, eliminación de teletransportes y robustez de guardado.

CAMBIOS REALIZADOS:
- Creación de la interfaz `IInteractable` para interacción unificada e independiente en `TouchInputManager.cs`.
- Implementación de `TableState` (`Available`, `Reserved`, `Occupied`, `WaitingFood`, `Eating`, `Dirty`, `Cleaning`) en `Table.cs`.
- Vinculación directa `Table.currentCustomer` eliminando búsquedas ciegas `FindAnyObjectByType`.
- Los clientes ahora dejan la mesa en estado `Dirty` al levantarse tras comer.
- `WorkerController.cs` actualizado con prioridades: 1) Servir plato coincidente pedido por la mesa desde `DeliveryCounter`, 2) Limpiar mesa sucia, 3) Retornar a Idle.
- `DeliveryCounter.cs` ampliado con capacidad por niveles (1: 2 platos, 2: 4 platos, 3: 6 platos) y métodos de búsqueda y extracción exacta (`FindMatchingDish`, `TakeSpecificDish`).
- Eliminado completamente el teletransporte en pathfinding en `WorkerController.cs` y `CustomerController.cs`.
- Centralizado el crecimiento de cultivos en `FarmingManager.cs` (tick de 1s) eliminando el `Update()` por frame en `CropPlot.cs`.
- Zonificación ampliada en `GridManager.cs` (`Kitchen`, `Dining`, `Exterior`, `Farming`, `Market`, `Crafting`) y validación de `allowedZones` en `FurnitureSO.cs`.
- Validación de seguridad de rutas en `BuildManager.cs` antes de confirmar colocación de muebles, impidiendo que el restaurante quede bloqueado.
- Guardado atómico con archivos temporales y respaldo automático (`.bak`) en `SaveManager.cs` con `saveVersion = 1`.

ARCHIVOS CREADOS:
- `Assets/_Projet/Scripts/Interaction/IInteractable.cs`
- `Assets/_Projet/Documentation/PROJECT_HISTORY.md`
- `Assets/_Projet/Documentation/TECHNICAL_DECISIONS.md`
- `Assets/_Projet/Documentation/ROADMAP.md`
- `Assets/_Projet/Documentation/IDEAS_BACKLOG.md`
- `Assets/_Projet/Documentation/KNOWN_ISSUES.md`
- `Assets/_Projet/Documentation/AI_SESSION_LOG.md`

ARCHIVOS MODIFICADOS:
- `Assets/_Projet/Scripts/Building/BuildManager.cs`
- `Assets/_Projet/Scripts/Building/GridManager.cs`
- `Assets/_Projet/Scripts/Cooking/CookingStation.cs`
- `Assets/_Projet/Scripts/Customers/CustomerController.cs`
- `Assets/_Projet/Scripts/Economy/MerchantStall.cs`
- `Assets/_Projet/Scripts/Farming/CropPlot.cs`
- `Assets/_Projet/Scripts/Input/TouchInputManager.cs`
- `Assets/_Projet/Scripts/Managers/FarmingManager.cs`
- `Assets/_Projet/Scripts/Restaurant/DeliveryCounter.cs`
- `Assets/_Projet/Scripts/Restaurant/Table.cs`
- `Assets/_Projet/Scripts/Save/SaveData.cs`
- `Assets/_Projet/Scripts/Save/SaveManager.cs`
- `Assets/_Projet/Scripts/ScriptableObjects/FurnitureSO.cs`
- `Assets/_Projet/Scripts/Workers/WorkerController.cs`

ARCHIVOS ELIMINADOS:
- Ninguno.

PROBLEMAS CORREGIDOS:
- Issue #001 (Teletransporte en pathfinding).
- Issue #002 (Entrega ciega en DeliveryCounter).
- Issue #003 (Mesas sin ciclo Dirty/Cleaning).
- Issue #004 (Acoplamiento de interacción en TouchInputManager).
- Issue #005 (Tick de cultivos por frame).
- Issue #006 (Guardado no atómico).

DECISIONES IMPORTANTES:
- `IInteractable` como contrato único de interacción táctil.
- `TableState` y delegación del ciclo de suciedad a los camareros.
- Prioridad número 1 al servicio de comida caliente sobre la limpieza.

ESTADO:
COMPLETADO

PRÓXIMO PASO:
Fase 2: Sistema de Comerciantes & NPCs Especializados (`NPCSO`, `VendorSO`, `VendorController`, `VendorUI`, stock y restock UTC para Elena, Bruno, Tomás, Marina, Amelia, Lucas y Sofía).
------------------------------------------------------------

FECHA: 2026-09-22
VERSIÓN / TAG: 0.2.0 (Fase 2: Comerciantes y NPCs Especializados)
FASE: FASE 2 — Sistema de Comerciantes & NPCs Especializados
OBJETIVO:
Implementar el ecosistema data-driven de comerciantes de la villa con NPCs únicos, personalidades, catálogos ScriptableObject, stock persistente entre sesiones, cuenta regresiva de reabastecimiento en UTC y una interfaz modular táctil para compras de insumos e ítems.

CAMBIOS REALIZADOS:
- Creada clase de datos `NPCSO.cs` (identidad, nombre, rol, retrato, sprite de mundo, diálogo de bienvenida, catálogo de vendedor vinculado y nivel de desbloqueo).
- Creada clase de datos `VendorSO.cs` (`VendorItemEntry` con precio, stock máximo, nivel requerido, flag de inventario e intervalo de reabastecimiento en segundos).
- Creado componente `VendorController.cs` con cálculo de reabastecimiento automático basado en Epoch UTC (`DateTimeOffset.UtcNow.ToUnixTimeSeconds()`), reducción atómica de monedas con `EconomyManager`, acreditación a `InventoryManager`, emisión de eventos de misiones (`QuestType.CollectIngredients`) y guardado/carga persistente.
- Creado componente `NPCController.cs` con implementación de `IInteractable`, apertura de `VendorUI` y sincronización visual de sprites.
- Creada interfaz de usuario `VendorUI.cs` con cabecera de personaje (retrato, nombre, título, diálogo dinámico y cuenta regresiva de restock `MM:SS`), grilla con scroll de tarjetas de producto con botón de compra condicionado a saldo y stock.
- Integrado `SaveData.cs` con estructuras `VendorStockSaveEntry` y `VendorSaveData` para que el stock de cada comerciante se conserve sin reseteos al salir de la tienda o reiniciar el juego.
- Añadido generador procedimental de sprites de mundo (16x24) y retratos de medallón (32x32) para los 7 NPCs en `ArtAssetGenerator.cs` (Elena, Bruno, Tomás, Marina, Amelia, Lucas, Sofía) a 16 PPU sin dependencias externas.
- Configurada base de datos de los 7 especialistas en `AssetDatabasePopulator.cs`.
- Vinculado puesto físico `MerchantStall.cs` con soporte para `associatedNPC`, apertura directa de `VendorUI` y fallback a `MarketUI`.
- Añadido `VendorModal` en `RestaurantSceneSetupEditor.cs` y soporte en `RestaurantBootstrap.cs`.
- Corregida serialización en `DishInstance.cs` (`NonSerialized`), cálculo de sorting en `Table.cs` y enum `QuestType.CollectIngredients` en `QuestSO.cs`.

ARCHIVOS CREADOS:
- `Assets/_Projet/Scripts/ScriptableObjects/NPCSO.cs`
- `Assets/_Projet/Scripts/ScriptableObjects/VendorSO.cs`
- `Assets/_Projet/Scripts/NPC/NPCController.cs`
- `Assets/_Projet/Scripts/NPC/VendorController.cs`
- `Assets/_Projet/Scripts/UI/VendorUI.cs`

ARCHIVOS MODIFICADOS:
- `Assets/_Projet/Scripts/Core/Editor/ArtAssetGenerator.cs`
- `Assets/_Projet/Scripts/Core/Editor/AssetDatabasePopulator.cs`
- `Assets/_Projet/Scripts/Core/Editor/RestaurantSceneSetupEditor.cs`
- `Assets/_Projet/Scripts/Core/RestaurantBootstrap.cs`
- `Assets/_Projet/Scripts/Economy/MerchantStall.cs`
- `Assets/_Projet/Scripts/Save/SaveData.cs`
- `Assets/_Projet/Scripts/ScriptableObjects/QuestSO.cs`
- `Assets/_Projet/Scripts/Restaurant/Table.cs`
- `Assets/_Projet/Scripts/Cooking/DishInstance.cs`
- `Assets/_Projet/Documentation/TECHNICAL_DECISIONS.md`
- `Assets/_Projet/Documentation/ROADMAP.md`
- `Assembly-CSharp.csproj`

ESTADO:
COMPLETADO — Compilación limpia (0 errores, 0 advertencias)

PRÓXIMO PASO:
Fase 3: Sistema de Crafting (Procesamiento de Insumos: `CraftingRecipeSO`, `CraftingStation`, `CraftingManager`, `CraftingUI` y recetas intermedias trigo → harina → masa → pizza / frutas → mermeladas).
------------------------------------------------------------
FECHA:
2026-09-22

VERSIÓN / FASE:
0.3.0 (Fase 3: Sistema de Crafting y Procesamiento de Insumos)

RESUMEN:
- Implementación completa del sistema de procesamiento de insumos y elaboración intermedia desacoplado del menú de platos terminados (`RecipeSO`).
- ScriptableObject `CraftingRecipeSO` con definición de insumos requeridos, producto procesado resultante, estación de crafteo requerida, tiempo en segundos y XP otorgada.
- Componente interactuable `CraftingStation` con máquina de estados (`Idle`, `Crafting`, `ReadyToCollect`), indicador visual flotante de producto listo con animación sutil de rebote, integración con partículas y feedback sonoro.
- `CraftingManager` singleton con registro dinámico de recetas desde `Resources.LoadAll<CraftingRecipeSO>`, registro de estaciones en escena y soporte de guardado/carga atómico en `SaveData.craftingStations`.
- Interfaz táctil reactiva `CraftingUI` con selector de recetas navegable por ScrollRect, panel de proceso activo con barra de progreso, temporizador en tiempo real, botón de acelerar y botón de recolección ("¡Recolectar!").
- Añadida categoría `IngredientCategory.Procesado` en `IngredientSO` y categoría `FurnitureCategory.EstacionCrafting` en `FurnitureSO`.
- Eventos de juego en `GameEvents.cs`: `OnCraftStarted`, `OnCraftCompleted`, `OnCraftCollected`.
- Soporte para misiones de elaboración en `QuestSO.cs` (`QuestType.CraftItems`).
- Generación procedimental en `ArtAssetGenerator.cs` de sprites pixel art para estaciones (Molino de Grano, Mesa de Amasado, Marmita de Salsas, Paila Dulce) e ingredientes procesados (Harina Blanca, Masa de Pizza/Pan, Salsa de Tomate Casera, Mermelada de Fresa).
- Base de datos configurada en `AssetDatabasePopulator.cs` con 5 recetas de crafteo balanceadas.
- Actualización de `RestaurantBootstrap.cs` para inicializar automáticamente `CraftingManager`, spawnear el Molino inicial en `(13, 18)` y registrar el modal `CraftingUI`.
- Corrección de colisión de nombre de variable en `RestaurantSceneSetupEditor.cs` (`avRT` -> `actvRT`).

ARCHIVOS CREADOS:
- `Assets/_Projet/Scripts/ScriptableObjects/CraftingRecipeSO.cs`
- `Assets/_Projet/Scripts/Crafting/CraftingStation.cs`
- `Assets/_Projet/Scripts/Managers/CraftingManager.cs`
- `Assets/_Projet/Scripts/UI/CraftingUI.cs`

ARCHIVOS MODIFICADOS:
- `Assets/_Projet/Scripts/ScriptableObjects/IngredientSO.cs`
- `Assets/_Projet/Scripts/ScriptableObjects/FurnitureSO.cs`
- `Assets/_Projet/Scripts/ScriptableObjects/QuestSO.cs`
- `Assets/_Projet/Scripts/Core/GameEvents.cs`
- `Assets/_Projet/Scripts/Save/SaveData.cs`
- `Assets/_Projet/Scripts/Core/Editor/ArtAssetGenerator.cs`
- `Assets/_Projet/Scripts/Core/Editor/AssetDatabasePopulator.cs`
- `Assets/_Projet/Scripts/Core/Editor/RestaurantSceneSetupEditor.cs`
- `Assets/_Projet/Scripts/Core/RestaurantBootstrap.cs`
- `Assets/_Projet/Documentation/ROADMAP.md`
- `Assets/_Projet/Documentation/TECHNICAL_DECISIONS.md`
- `Assembly-CSharp.csproj`

ESTADO:
COMPLETADO — Compilación limpia (0 errores, 0 advertencias en runtime y editor)

PRÓXIMO PASO:
Fase 4: Expansiones de la Villa & Zonificación (`ExpansionSO`, desbloqueo progresivo de zonas: Restaurante → Terraza/Comedor Exterior → Huerto Extendido → Zona de Crafting → Plaza del Mercado).
------------------------------------------------------------
FECHA:
2026-09-22

VERSIÓN / FASE:
0.4.0 (Fase 4: Expansiones de la Villa y Zonificación)

RESUMEN:
- Creación del sistema de desbloqueo progresivo del mapa de la villa gastronómica mediante ScriptableObjects `ExpansionSO`.
- Cuatro zonas de expansión fundacionales configuradas: Terraza del Jardín (Nivel 2), Huerto Alto del Valle (Nivel 3), Taller de Molienda & Artesanía (Nivel 4) y Plaza del Mercado Gastronómico (Nivel 5).
- Integración de `ZoneType.Terrace` en `GridManager.cs` y habilitación de mesas y sillas para colocación en exteriores.
- Control de ocupación y límites en `GridCell` mediante `isUnlocked`; `BuildManager.cs` valida que la posición deseada esté dentro de un terreno ya adquirido antes de permitir la construcción (`isAreaUnlocked`).
- Marcadores de terreno interactuables en el mundo `ExpansionSign.cs` (`IInteractable`) con animación de rebote sutil, letrero de madera con estrella dorada y desaparición suave con partículas al concretar la compra.
- Interfaz táctil modular `ExpansionUI.cs` con feedback visual de requisitos de nivel y monedas, otorgamiento de experiencia e integración con audio y misiones.
- Singleton `ExpansionManager.cs` con persistencia atómica en `SaveData.unlockedExpansions` y sincronización bidireccional con `GridManager` y `SaveManager`.
- Generación de sprites pixel art procedimentales en `ArtAssetGenerator.cs` (letrero `sign_for_sale`, valla rústica `fence_rustic`, iconos de terraza, huerto, crafting y mercado a 16 PPU).
- Creación de assets en `AssetDatabasePopulator.cs`, integración de `ExpansionModal` en `RestaurantSceneSetupEditor.cs` y soporte automático en `RestaurantBootstrap.cs`.

ARCHIVOS CREADOS:
- `Assets/_Projet/Scripts/ScriptableObjects/ExpansionSO.cs`
- `Assets/_Projet/Scripts/Managers/ExpansionManager.cs`
- `Assets/_Projet/Scripts/Building/ExpansionSign.cs`
- `Assets/_Projet/Scripts/UI/ExpansionUI.cs`

ARCHIVOS MODIFICADOS:
- `Assets/_Projet/Scripts/Building/GridManager.cs`
- `Assets/_Projet/Scripts/Building/BuildManager.cs`
- `Assets/_Projet/Scripts/Save/SaveData.cs`
- `Assets/_Projet/Scripts/Save/SaveManager.cs`
- `Assets/_Projet/Scripts/Core/GameEvents.cs`
- `Assets/_Projet/Scripts/ScriptableObjects/QuestSO.cs`
- `Assets/_Projet/Scripts/Core/Editor/ArtAssetGenerator.cs`
- `Assets/_Projet/Scripts/Core/Editor/AssetDatabasePopulator.cs`
- `Assets/_Projet/Scripts/Core/Editor/RestaurantSceneSetupEditor.cs`
- `Assets/_Projet/Scripts/Core/RestaurantBootstrap.cs`
- `Assets/_Projet/Documentation/ROADMAP.md`
- `Assets/_Projet/Documentation/TECHNICAL_DECISIONS.md`
- `Assembly-CSharp.csproj`

ESTADO:
COMPLETADO — Compilación limpia (0 errores, 0 advertencias en runtime y editor)

PRÓXIMO PASO:
Fase 5: Progresión Profunda, Reputación y Misiones (Sistema de Reputación dinámico, arquetipos de clientes extendidos en `CustomerSO`: Impaciente, Generoso, Gourmet, Familiar, Turista, Crítico, VIP; progresión guiada y cadena de misiones narrativas con los especialistas).
------------------------------------------------------------
FECHA:
2026-09-22

VERSIÓN / FASE:
0.5.0 (Fase 5: Progresión Profunda, Reputación y Misiones de Especialistas)

RESUMEN:
- Implementación del sistema de Reputación Dinámica y Comportamiento de Clientes Reactivo.
- Ampliación de `CustomerSO.cs` con arquetipo `CustomerArchetype.Gourmet`, recompensas de reputación (+1 a +15), penalizaciones de reputación (-2 a -10) por impaciencia, y bonificación de experiencia (bonusXP).
- Integración en `CustomerController.cs` de recompensas y penalizaciones automáticas ligadas a la satisfacción del comensal.
- Selección estocástica ponderada en `CustomerManager.cs` (`SelectCustomerType`): a mayor nivel de reputación del restaurante, mayor probabilidad de atraer Críticos Gastronómicos, Clientes VIP, Turistas y Gourmets.
- Frecuencia dinámica de comensales en `CustomerManager.cs`: la tasa de llegada de clientes aumenta proporcionalmente a la reputación de la villa.
- Cadena de misiones con historia ligada a los 7 especialistas locales en `AssetDatabasePopulator.cs` (Elena, Bruno, Tomás, Marina, Amelia, Lucas, Sofía) con recompensas de oro, gemas, XP, reputación y desbloqueo de recetas exclusivas (`RecipeManager.UnlockRecipe`).
- Generación de sprites pixel art procedimentales para los 7 arquetipos de clientes en `ArtAssetGenerator.cs` (16x24 PPU 16 Point) en `Assets/_Projet/Art/Characters/Customers/`.
- Actualización de `QuestSO.cs` y `QuestManager.cs` para soportar `reputationReward` y `rewardRecipeID`.
- Actualización de `RecipeManager.cs` con método `UnlockRecipe(string recipeID)` para desbloqueo dinámico por progreso narrativo.

ARCHIVOS MODIFICADOS:
- `Assets/_Projet/Scripts/ScriptableObjects/CustomerSO.cs`
- `Assets/_Projet/Scripts/Customers/CustomerController.cs`
- `Assets/_Projet/Scripts/Managers/CustomerManager.cs`
- `Assets/_Projet/Scripts/ScriptableObjects/QuestSO.cs`
- `Assets/_Projet/Scripts/Managers/QuestManager.cs`
- `Assets/_Projet/Scripts/Managers/RecipeManager.cs`
- `Assets/_Projet/Scripts/Core/Editor/ArtAssetGenerator.cs`
- `Assets/_Projet/Scripts/Core/Editor/AssetDatabasePopulator.cs`
- `Assets/_Projet/Documentation/ROADMAP.md`
- `Assets/_Projet/Documentation/TECHNICAL_DECISIONS.md`

ESTADO:
COMPLETADO — Compilación limpia (0 errores, 0 advertencias en runtime y editor)

PRÓXIMO PASO:
Fase 6: Optimización Móvil & Pulido Audiovisual (Object Pooling para clientes y textos flotantes, configuración de Sprite Atlases para draw calls en móviles a 60 FPS, verificación de New/Old Input System y gestos táctiles).
------------------------------------------------------------
FECHA:
2026-09-22

VERSIÓN / FASE:
0.6.0 (Fase 6: Optimización Móvil a 60 FPS, Object Pooling y Sprite Atlases)

RESUMEN:
- Creación de la arquitectura de Object Pooling con la interfaz `IPoolable.cs` y el singleton central `ObjectPoolManager.cs`.
- Integración de `IPoolable` en `CustomerController.cs` con métodos de reciclaje `OnSpawnFromPool`, `OnReturnToPool` y `DespawnCustomer`, eliminando las llamadas destructivas a `Destroy` y el Garbage Collection recurrente durante la partida.
- Precalentamiento de comensales en `CustomerManager.cs` (`Prewarm("Customers", 8)`) y despacho reciclado mediante `ObjectPoolManager.Instance.Spawn`.
- Creación de `FloatingTextManager.cs` y refactorización de `FloatingText.cs` (`IPoolable`): indicadores flotantes de ganancias de oro (+oro), reputación (+/- rep), experiencia (+XP), recolección de cultivos y elaboración de recetas en estaciones de crafteo con 0 asignaciones de memoria.
- Soporte para Unity New Input System en `TouchInputManager.cs`: arquitectura híbrida resiliente que lee gestos táctiles y ratón clásicos y, ante configuración exclusiva de New Input System, conmuta de forma transparente a `UnityEngine.InputSystem.Touchscreen` y `Mouse`.
- Generación automatizada de Sprite Atlases V2 nativos en `Assets/_Projet/Art/Atlases/` (`Atlas_Characters`, `Atlas_Environment`, `Atlas_Exterior`, `Atlas_Food`, `Atlas_Furniture`, `Atlas_UI`) y herramienta de editor `SpriteAtlasSetupEditor.cs`, reduciendo drásticamente las llamadas de dibujo (draw calls) de más de 80 a menos de 10 en plataformas móviles.
- Integración de `ObjectPoolManager` y `FloatingTextManager` en el ciclo de auto-inicialización de `RestaurantBootstrap.cs`.

ARCHIVOS CREADOS:
- `Assets/_Projet/Scripts/Core/IPoolable.cs`
- `Assets/_Projet/Scripts/Core/ObjectPoolManager.cs`
- `Assets/_Projet/Scripts/UI/FloatingTextManager.cs`
- `Assets/_Projet/Scripts/Core/Editor/SpriteAtlasSetupEditor.cs`
- `Assets/_Projet/Art/Atlases/Atlas_Characters.spriteatlasv2`
- `Assets/_Projet/Art/Atlases/Atlas_Environment.spriteatlasv2`
- `Assets/_Projet/Art/Atlases/Atlas_Exterior.spriteatlasv2`
- `Assets/_Projet/Art/Atlases/Atlas_Food.spriteatlasv2`
- `Assets/_Projet/Art/Atlases/Atlas_Furniture.spriteatlasv2`
- `Assets/_Projet/Art/Atlases/Atlas_UI.spriteatlasv2`

ARCHIVOS MODIFICADOS:
- `Assets/_Projet/Scripts/UI/FloatingText.cs`
- `Assets/_Projet/Scripts/Customers/CustomerController.cs`
- `Assets/_Projet/Scripts/Managers/CustomerManager.cs`
- `Assets/_Projet/Scripts/Farming/CropPlot.cs`
- `Assets/_Projet/Scripts/Crafting/CraftingStation.cs`
- `Assets/_Projet/Scripts/Input/TouchInputManager.cs`
- `Assets/_Projet/Scripts/Core/RestaurantBootstrap.cs`
- `Assets/_Projet/Documentation/ROADMAP.md`
- `Assets/_Projet/Documentation/TECHNICAL_DECISIONS.md`
- `Assembly-CSharp.csproj`
- `Assembly-CSharp-Editor.csproj`

ESTADO:
COMPLETADO — Compilación limpia (0 errores, 0 advertencias en runtime y editor)

PRÓXIMO PASO:
Verificación en PlayMode, pruebas funcionales de la suite completa y entrega consolidada de la arquitectura de Villa del Chef.
------------------------------------------------------------

FECHA: 2026-09-22
VERSIÓN / TAG: 0.6.1 (Fase 6.1: Consolidación General & Integración Real Fases 1–6)
FASE: FASE 6.1 — Consolidación General

OBJETIVO:
Revisar, auditar y corregir la integración real de las Fases 1 a 6 antes de comenzar la Fase 7. Resolver el bug crítico de ObjectPool con mesas sucias, desacoplar Bootstrap de datos procedurales runtime, materializar y versionar en Git los ScriptableObjects y Sprites reales, implementar los locales físicos modulares de los 7 especialistas, centralizar el tick de crafteo con cálculo exacto de tiempo offline UTC, asegurar la restauración precisa de transitabilidad en construcción y limpiar el subsistema de input y PlayerSettings.

CAMBIOS REALIZADOS:
- Corregido bug crítico de mesas sucias en `CustomerController.cs` y `Table.cs`: implementación de `ReleaseTableReference()` que desacopla la partida del comensal de la limpieza de la mesa. `Table.ClearTable()` ya no limpia mesas en estado `Dirty` o `Cleaning`.
- Implementada reserva atómica de tareas en mozos (`Table.isCleaningReserved`, `DishInstance.isReserved` en `DeliveryCounter.cs` y `WorkerController.cs`), previniendo carreras de múltiples trabajadores y eliminando fallbacks aleatorios en entrega de platos.
- Refactorizado `RestaurantBootstrap.cs`: incorporada bandera `useDevelopmentFallbackData = false;`. En modo producción, no sobrescribe `allRecipes`, `allCrops`, `availableCustomerTypes` ni inventario, respetando las bases de datos de `Resources/`.
- Creado componente modular `VendorBuilding.cs` (`IInteractable`) para ubicar físicamente en el mapa exterior a los 7 especialistas de la villa (Elena, Bruno, Tomás, Marina, Amelia, Lucas, Sofía) con sus puestos, letreros flotantes y vinculación con `NPCController`.
- Materializados y versionados permanentemente en Git todos los ScriptableObjects en `Assets/_Projet/Resources/` (`NPC/`, `Vendors/`, `CraftingRecipes/`, `Expansions/`, `Customers/`, `Quests/`, `Ingredients/`, `Recipes/`, `Stations/`).
- Optimizado el sistema de Crafting: añadidas marcas de tiempo UTC (`craftStartTimestampSeconds`, `craftFinishTimestampSeconds`) en `SaveData.cs`, procesado de tiempo offline en `CraftingManager.cs`, y centralización del tick de crafteo (0.5s) eliminando el `Update()` por frame en `CraftingStation.cs`.
- Corregida la restauración de transitabilidad en `BuildManager.ValidateNavigationSafety()` mediante `Dictionary<GridCell, bool> previousWalkability`, evitando volver transitables celdas perimetrales o de diseño, con validación de 3 rutas críticas.
- Optimizado `TouchInputManager.cs`: eliminada la captura de excepciones por frame en `Update()`, detección única en `Awake()` y soporte completo de Build Mode para New Input System.
- Actualizado `ProjectSettings.asset`: Product Name establecido en "Villa del Chef" y rotación configurada estrictamente para Landscape.
- Actualizado `MainMenuController.cs`: soporte para botón Continuar (`SaveManager.HasSaveFile()`), Créditos, Salir (solo en desktop) y slot para `mainmenu_background.png`.

ARCHIVOS CREADOS:
- `Assets/_Projet/Scripts/NPC/VendorBuilding.cs`
- `Assets/_Projet/Scripts/NPC/VendorBuilding.cs.meta`
- Todos los assets `.asset` y `.meta` en `Assets/_Projet/Resources/` (`CraftingRecipes/`, `Customers/`, `Expansions/`, `NPC/`, `Vendors/`, `Quests/`).

ARCHIVOS MODIFICADOS:
- `Assets/_Projet/Scripts/Restaurant/Table.cs`
- `Assets/_Projet/Scripts/Customers/CustomerController.cs`
- `Assets/_Projet/Scripts/Cooking/DishInstance.cs`
- `Assets/_Projet/Scripts/Restaurant/DeliveryCounter.cs`
- `Assets/_Projet/Scripts/Workers/WorkerController.cs`
- `Assets/_Projet/Scripts/Save/SaveData.cs`
- `Assets/_Projet/Scripts/Crafting/CraftingStation.cs`
- `Assets/_Projet/Scripts/Managers/CraftingManager.cs`
- `Assets/_Projet/Scripts/Managers/FarmingManager.cs`
- `Assets/_Projet/Scripts/Inventory/InventoryManager.cs`
- `Assets/_Projet/Scripts/Building/BuildManager.cs`
- `Assets/_Projet/Scripts/UI/BuildUI.cs`
- `Assets/_Projet/Scripts/Input/TouchInputManager.cs`
- `Assets/_Projet/Scripts/Save/SaveManager.cs`
- `Assets/_Projet/Scripts/UI/MainMenuController.cs`
- `Assets/_Projet/Scripts/Core/RestaurantBootstrap.cs`
- `ProjectSettings/ProjectSettings.asset`
- `Assets/_Projet/Documentation/KNOWN_ISSUES.md`
- `Assets/_Projet/Documentation/TECHNICAL_DECISIONS.md`
- `Assets/_Projet/Documentation/ROADMAP.md`

ESTADO:
COMPLETADO — Compilación con 0 errores y 0 advertencias en runtime y editor.

PRÓXIMO PASO:
Fase 7: Contenido y Variedad (expansión de recetas del Valle del Elqui, personalización estética, nuevos muebles y progresión avanzada).
------------------------------------------------------------
