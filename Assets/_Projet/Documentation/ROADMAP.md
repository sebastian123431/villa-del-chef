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

## FASE 6.2 — Cierre de Integración & Robustez de Sistemas
- [~] Estado actual de la fase: PARCIAL / EN CIERRE TÉCNICO (Compilación C# 100% limpia sin errores; Play Mode en runtime y Android Build físico pendientes de ejecución en el entorno del propietario).
- [x] New Input System táctil: Corregido bug donde `touch.press.isPressed == false` impedía procesar `wasReleasedThisFrame`.
- [x] Gestos móviles New Input: Tap, Drag de cámara, Drag de ghost en Build Mode, Pinch-to-Zoom con sensibilidad configurable (`pinchZoomSensitivity = 0.05f`), Long Press (`longPressDuration = 0.5f`) y rotación mediante UI (`RotateBuildSelection`).
- [x] Tiendas de NPCs: Gating de nivel real implementado (`CanInteract` evalúa `ProgressionManager.CurrentLevel >= unlockLevelRequirement`), feedback visual con color atenuado al estar bloqueadas y mensaje flotante informativo (`🔒 Se desbloquea en Nivel X`).
- [x] Cuadrícula & Footprints de tiendas: Los 7 locales (Marina, Bruno, Elena, Tomás, Amelia, Lucas, Sofía) reubicados en `y = 20` dentro del rango X: 1..27 (footprint 3x2). Validación defensiva `IsPlacementInsideGrid` antes de spawnear.
- [x] Zonificación & Expansiones: El bulevar comercial (`y >= 19`) es ahora zona pública abierta `ZoneType.Market`. Las expansiones (`exp_crops` y `exp_crafting`) se ajustaron a `height = 3` (y: 16..18) eliminando cualquier solapamiento con los comercios accesibles desde el inicio.
- [x] Bloqueo de construcción sobre tiendas: `GridManager.IsAreaAvailable` verifica `isUnlocked`, `isWalkable` y `occupyingObject`, evitando construir muebles encima de tiendas o áreas bloqueadas.
- [x] Prevención de excepciones en pathfinding: `BuildManager.ValidateNavigationSafety` protegido con bloque `try ... finally` para garantizar la restauración exacta de transitabilidad de celdas candidatas.
- [x] Explotación de inventario inicial eliminada: Bandera `starterItemsGranted` en `SaveData.cs` asegura que el paquete de inicio solo se entrega una vez por partida, incluso si el inventario queda completamente a cero.
- [x] Distinción de Nueva Partida vs Continuar: Bandera `hasStartedGame` en `SaveData.cs`. `SaveManager.CanContinueGame()` desacopla la existencia del archivo técnico de guardado del progreso real. Modal de confirmación para evitar sobreescritura accidental.
- [x] Entrega de platos atómica: Se eliminó la doble llamada `PlaceDish`. La entrega se delega exclusivamente a `CustomerController.ReceiveDish()`, quien valida que el plato coincida con el pedido antes de posicionarlo en la mesa.
- [x] Liberación de reservas en mozos: `WorkerController` libera atómicamente reservas de platos (`isReserved`) y mesas sucias (`isCleaningReserved`) en `OnDisable`, `OnDestroy` y cancelaciones de ruta.
- [x] Optimización de memoria en arranque: `RestaurantBootstrap.cs` migrado a evaluación perezosa (`GetOrCreateFallbackSprite` con fábrica lambda) para evitar la creación inútil de texturas procedimentales si los assets existen.
- [x] Compatibilidad con animación de NPCs: `NPCSO` extendido con soporte opcional para `RuntimeAnimatorController`, manteniendo fallback a `worldSprite` y `portrait`.
- [x] Corrección crítica de GameDataValidatorEditor: `cr.craftID` y ruta `CraftingRecipes/` validadas por Unity en batchmode con 0 errores y 0 advertencias.
- [x] Eliminación de bypass de nivel en NPCs: `NPCController` delega `CanInteract` y gating a su `VendorBuilding` padre; feedback de bloqueo unificado.
- [x] Footprint y persistencia de edificios estáticos: `GridObject.playerMovable = false`, `overrideSizeX/Y` (3x2) y `SetupStatic` eliminan desregistro parcial o movimiento de tiendas comerciales con long press.
- [x] Build Mode táctil móvil: Colocación fluida confirmada al soltar (`TouchPhase.Ended`), arrastre de ghost preview y discriminación de colocación de muebles nuevos vs recolocación de existentes.
- [x] Recuperación segura de mozos: Estados `ReturningDish` y `WaitingCounterSpace` en `WorkerController` con retornos seguros cuando el mostrador de entrega está lleno o el comensal no está disponible.
- [x] Migración centralizada SaveData v1 -> v2: `MigrateSaveIfNeeded()` infiere progreso en 14 dimensiones de juego tanto para el guardado primario como para el archivo de backup.
- [x] Versionado definitivo de `01_MainMenu.unity`: Escena completamente generada y serializada con botones, paneles modales de confirmación, opciones y créditos.
- [x] Blindaje de escenas en el Editor: `AutoSetupScenesOnEditorLoad` no destructivo; preserva arte y ajustes manuales en Git.
- [ ] Validación física en dispositivo Android / APK build.

