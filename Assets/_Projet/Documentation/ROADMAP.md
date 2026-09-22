# ROADMAP.md — Villa del Chef

Estado global del desarrollo y fases de evolución del proyecto.

Leyenda:
- `[x]` Terminado
- `[~]` Parcialmente terminado
- `[ ]` Pendiente
- `[!]` Bloqueado

---

## FASE 1 — Auditoría & Core Refactor Seguro
- [x] Grid 2D y sistema de colocación de muebles
- [x] Cocina y estaciones básicas
- [x] Farming básico con ciclos UTC
- [x] Clientes con spawn y consumo
- [x] Worker camarero básico
- [x] Guardado y carga local JSON
- [x] Implementar interfaz `IInteractable` para eliminar acoplamiento en `TouchInputManager`
- [x] Implementar `TableState` (`Available`, `Reserved`, `Occupied`, `WaitingFood`, `Eating`, `Dirty`, `Cleaning`)
- [x] Estado `Dirty` y ciclo de limpieza por parte del camarero
- [x] Referencia directa `Table.currentCustomer` (eliminar fallback `FindAnyObjectByType`)
- [x] Búsqueda exacta de plato pedido en mostrador (`FindMatchingDish` en `DeliveryCounter`)
- [x] Eliminar teleport como fallback de pathfinding en `CustomerController` y `WorkerController`
- [x] Validación de caminos bloqueados antes de confirmar construcción en `BuildManager`
- [x] Control de zonas (`ZoneType`: Kitchen, Dining, Exterior, Farming, Market, Crafting) en `FurnitureSO`
- [x] Optimizar ciclo de cultivos: FarmingManager centralizado (tick 1s) en vez de `Update()` individual por frame
- [x] Guardado atómico en `SaveManager` (`.tmp` -> `.bak` -> `.json`) y versionado (`saveVersion = 1`)


## FASE 2 — Sistema de Comerciantes & NPCs Especializados
- [x] ScriptableObject `NPCSO` (ID, nombre, retrato, sprite de mundo, diálogo, nivel de desbloqueo)
- [x] ScriptableObject `VendorSO` (inventario a la venta, precios, stock máximo, tiempo de restock)
- [x] `VendorController` con persistencia de stock y cuenta regresiva de restock UTC
- [x] `NPCController` interactuable con detección táctil / ratón (`IInteractable`)
- [x] `VendorUI` táctil y modular con catálogo dinámico, retrato, diálogo y temporizador
- [x] Los 7 NPCs fundacionales configurados en `AssetDatabasePopulator.cs`: Elena, Bruno, Tomás, Marina, Amelia, Lucas, Sofía
- [x] Generador de sprites y retratos pixel art procedimentales para los 7 NPCs en `ArtAssetGenerator.cs`
- [x] Integración de `MerchantStall.cs` y bootstrapping automático en escenas y Canvas

## FASE 3 — Sistema de Crafting (Procesamiento de Insumos)
- [x] ScriptableObject `CraftingRecipeSO` (craftID, insumos requeridos, resultado, tiempo, estación requerida)
- [x] Componente `CraftingStation` (molino, batidora, procesador, lácteos, etc.) con IInteractable y estados
- [x] `CraftingManager` singleton y registro dinámico de recetas y estaciones con persistencia
- [x] `CraftingUI` táctil para seleccionar recetas, acelerar y recolectar insumos con feedback visual
- [x] Integración con inventario (`IngredientCategory.Procesado`, harinas, masas, salsas, mermeladas)
- [x] Assets visuales generados procedimentalmente (estaciones e insumos procesados)
- [x] 5 recetas de crafting iniciales configuradas en `AssetDatabasePopulator.cs` y bootstrapping en escena

## FASE 4 — Expansiones de la Villa & Zonificación
- [x] ScriptableObject `ExpansionSO` (ID, costo, nivel requerido, límites en grilla `gridBounds`, XP de recompensa, zona objetivo)
- [x] Mesas exteriores y funcionamiento unificado para terrazas/patios (`ZoneType.Terrace` en `Table` y `Chair`)
- [x] Desbloqueo progresivo de zonas: Restaurante → Terraza → Huerto extendido → Zona de Crafting → Plaza del Mercado
- [x] Marcadores interactivos en el mundo `ExpansionSign` (`IInteractable`) con feedback de animación flotante y compra
- [x] Interfaz modal táctil `ExpansionUI` con verificación de nivel y monedas en tiempo real
- [x] `ExpansionManager` singleton con registro dinámico, desbloqueo en `GridManager` y eventos
- [x] Persistencia de expansiones desbloqueadas en `SaveData.unlockedExpansions`
- [x] Generación procedimental de sprites de expansión (letreros `sign_for_sale`, vallas y 4 iconos de zona)
- [x] 4 expansiones de la villa configuradas en `AssetDatabasePopulator.cs` y auto-setup en bootstrap y editor

## FASE 5 — Progresión Profunda, Reputación y Misiones
- [x] Sistema de Reputación como recurso dinámico en `EconomyManager` (afecta afluencia, reduce tiempos de spawn y atrae clientes VIP y Críticos)
- [x] Tipos de clientes extendidos en `CustomerSO` (Normal, Impaciente, Generoso, Gourmet, Turista, Crítico Gastronómico, VIP) con multiplicadores de paciencia, propinas, bonus XP y penalizaciones
- [x] Generación procedimental de sprites pixel art para los 7 clientes a 16 PPU (`ArtAssetGenerator.cs`)
- [x] Spawning ponderado estocástico por nivel y reputación en `CustomerManager.cs`
- [x] Sistema de misiones ampliado con historia y reputación (`QuestSO.cs`, `QuestManager.cs`)
- [x] Cadena narrativa con los 7 especialistas (Elena, Bruno, Tomás, Marina, Amelia, Lucas, Sofía) con desbloqueo de recetas y prestigio

## FASE 6 — Optimización Móvil & Pulido Audiovisual
- [x] Guardado atómico con archivo temporal y backup (`villadelchef_save.json.bak`)
- [x] Input seguro híbrido compatible con Unity New Input System y Legacy Touch/Mouse
- [x] Object pooling (`ObjectPoolManager.cs`, `IPoolable.cs`) para comensales y textos flotantes
- [x] Creación y asignación de Sprite Atlases V2 para draw calls en 60 FPS móviles (`SpriteAtlasSetupEditor.cs` y `.spriteatlasv2`)
- [x] Feedback visual flotante dinámico con `FloatingTextManager.cs` (+oro, +reputación, +XP, éxito, advertencias) sin GC spikes

## FASE 6.1 — Consolidación General & Integración Real (Fases 1–6)
- [x] Bug crítico corregido: Retorno de cliente al ObjectPool ya no libera mesas en estado `Dirty` o `Cleaning` (`ReleaseTableReference()` desacoplado de `ClearTable()`)
- [x] Prevención de carreras en mozos: Reserva atómica de plato (`isReserved`) y de mesa sucia (`isCleaningReserved`)
- [x] Eliminación de fallbacks aleatorios en entrega de platos (camarero cancela a Idle de forma segura)
- [x] Refactor de `RestaurantBootstrap`: `useDevelopmentFallbackData = false` en producción; preserva catálogos completos en Resources (recetas, clientes, cultivos)
- [x] Generación y versionado en Git de todos los ScriptableObjects y Sprites en `Assets/_Projet/Resources/`
- [x] Locales y puestos físicos de los 7 especialistas (Elena, Bruno, Tomás, Marina, Amelia, Lucas, Sofía) implementados en el mapa exterior con `VendorBuilding.cs` (`IInteractable`)
- [x] Crafting offline con marcas de tiempo UTC (`craftStartTimestampSeconds`, `craftFinishTimestampSeconds`), cálculo de segundos transcurridos y tick centralizado en `CraftingManager` (0.5s)
- [x] Restauración exacta de transitabilidad (`previousWalkability`) en `BuildManager.ValidateNavigationSafety` con validación de 3 rutas críticas
- [x] Eliminación de excepciones por frame en `TouchInputManager.cs` y soporte para hover/colocación en New Input System
- [x] Configuración de PlayerSettings: Nombre de producto "Villa del Chef" y orientación forzada en Landscape (desactivado Portrait)
- [x] MainMenu responsivo con soporte para Jugar, Continuar (según `SaveManager.HasSaveFile`), Opciones, Créditos, Salir (solo PC) y slot para `mainmenu_background.png`

