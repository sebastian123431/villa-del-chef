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
- [x] Exclusión mutua atómica en `DeliveryCounter.TakeNextDish()` evitando toma concurrente de platos ya reservados por otros mozos
- [x] Eliminación de fallbacks aleatorios en entrega de platos (camarero cancela a Idle de forma segura)
- [x] Refactor de `RestaurantBootstrap`: `useDevelopmentFallbackData = false` en producción; preserva catálogos completos en Resources (recetas, clientes, cultivos)
- [x] Generación y versionado en Git de todos los ScriptableObjects y Sprites en `Assets/_Projet/Resources/`
- [x] Materialización de assets físicos de muebles en `Assets/_Projet/Resources/Furniture/` (`table_wood`, `chair_wood`, `counter_delivery`, `stove_01`, `grill_01`, `crop_plot`) y enlace automático data-driven con `BuildUI.Instance.catalogItems`
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

---

## FASE 7 — Identidad, Prólogo, Elenco Social, Helper y Apertura del Restaurante [~] (95–98% — Verificación Técnica Completa / PlayMode E2E Pendiente)
- [x] Unificación del elenco social: Los 19 Friends de `Assets/_Projet/Art/Characters/Friends/` constituyen el pool dinámico único de personajes del juego.
- [x] Roles dinámicos por partida: 1 Friend como Protagonista (`selectedPlayerCharacterID`), 1 Friend como Ayudante (`selectedHelperCharacterID`) y el resto como Comensales elegibles.
- [x] Regla estricta de vestuario: Los Comensales (Customers) utilizan estrictamente ropa normal casual (`rnormal` y `movimientos_rnormal`). Se bloquea cualquier fallback cruzado hacia atuendos de chef (`allowCrossOutfitFallback: false`).
- [x] Helper reutiliza `WorkerController` con uniforme de chef (`rnchef` o `rbchef`).
- [x] Selector Real de Ayudante (`HelperIntroDialogUI.cs`): Grid interactivo de tarjetas reales con preview y nombre. Excluye al protagonista y a comensales activos dentro del restaurante. Permite elegir uniforme negro o blanco y confirmar antes de guardar.
- [x] Menú de Personal / Equipo (`StaffMenuUI.cs`): Botón "PERSONAL" en el HUD (`HUDController.cs`). Permite visualizar ayudante actual, alternar uniforme, contratar, relevar (el ayudante anterior reingresa al pool de clientes inmediatamente) o retirar ayudante.
- [x] Generación completa de 64 frames (16 filas): `CharacterPipelineEditor.cs` genera AnimationClips para las 16 filas de las hojas 4x16 (Idle, Walk, Cook en 4 direcciones, Think, Pickup, Carry_Serve, Celebrate). Pipeline 100% idempotente.
- [x] Animator direccional 2D con memoria: BlendTrees `SimpleDirectional2D` para Idle, Walk y Cook alimentados por `MoveX` y `MoveY`. Al frenar (`Speed = 0`), retiene el último vector de dirección.
- [x] Pathfinding de comensales sin teletransporte: `CustomerController.WalkToRoutine` evalúa si el comensal alcanzó la silla. Si el camino está bloqueado, libera la silla y reserva de mesa de inmediato, no teletransporta y sale/despawnea limpiamente.
- [x] Reciclaje limpio en Object Pool: `CustomerController.OnReturnToPool()` y `CharacterAppearanceController.ResetAppearance()` limpian triggers, parámetros direccionales y el sprite anterior (`spriteRenderer.sprite = null`).
- [x] Validadores de datos blindados: Falta de `normalPreview` o `normalAnimator` en `canAppearAsCustomer = true` es ERROR estricto. Comprobación de que Player y Helper tengan atuendos de chef completos. `ValidateAllGameData` y `ValidateCharacterDatabaseOnly` con 0 errores.
- [x] Exclusión mutua dinámica: El protagonista y el ayudante activo quedan excluidos automáticamente del pool de clientes. Al cambiar de ayudante, el anterior se reintegra de inmediato al pool de comensales.
- [x] Separación de responsabilidades: `CharacterSO` (identidad visual, previews, animators) desacoplado de `CustomerSO` (arquetipo, paciencia, propina, reputación, XP).
- [x] Prólogo reanudable (`PrologueController.cs`): Guardado incremental en cada hito (`prologueStep`: 1 a 5, `playerName`, `selectedPlayerCharacterID`, `selectedChefOutfit`). Al cerrar y reabrir la app, el jugador retoma el prólogo exactamente donde lo dejó.
- [x] Inicio de restaurante cerrado: Al completar el prólogo, `restaurantOpen = false` para permitir al jugador inspeccionar la cocina y comprar insumos antes de abrir las puertas al público.
- [x] Control defensivo de apertura: `CustomerManager.SpawnLoop` no genera comensales si el restaurante está cerrado o si `RestaurantOperatingManager.Instance == null`.
- [x] Protección de comerciantes oficiales: Los 7 comerciantes especialistas (Elena, Bruno, Tomás, Marina, Amelia, Lucas, Sofía) mantienen su estado independiente `[PENDIENTE ARTE NPC OFICIAL]` sin Friends asignados.
- [x] Blindaje de herramientas Editor: `ArtAssetGenerator`, `AssetDatabasePopulator` y `SpriteAtlasSetupEditor` respetan el arte original de los Friends sin regeneración ni sobrescritura.
- [x] Test de integración robusto (`SocialCastIntegrationTest.cs`): 9 suites automatizadas que validan exclusión, rotación, aislamiento SaveData, 19 personajes únicos, vestuario Normal estricto, reciclaje de pool y parámetros de 64 frames en AnimatorControllers. 100% PASS en Unity Batchmode.
- [ ] Validación final de PlayMode E2E en dispositivo / auditoría externa.

---

## FASE 7.1 — Customer Parties & Mesas por Grupo [DISEÑO APROBADO — PENDIENTE]
- [ ] Implementación de `CustomerPartyController` y gestión de grupos (1, 2, 3, 4, 5+ comensales).
- [ ] Mesas por capacidad y asignación de la mesa más pequeña compatible.
- [ ] Exclusión de comensales desconocidos en la misma mesa.
- [ ] Consumo y facturación consolidada por grupo.

---

## FASE 8 — Catálogo Gastronómico, Dominio de Recetas y Milagros [DISEÑO APROBADO / CONTENIDO FUTURO]
- [ ] Catálogo de 50 recetas chilenas por categorías (sándwiches, olla, horno, parrilla, costa, postres, estación fría).
- [ ] Sistema de dominio de recetas por repetición (`Principiante`, `Conocido`, `Experimentado`, `Especialista`, `Maestro`) con bonificaciones.
- [ ] Comerciante de recetas especiales y regionales.
- [ ] Sistema de Milagros del Chef (`MiracleSO` - Enfoque, Manos Rápidas, Tiempo Lento, Servicio Impecable) sin energía ni monedas premium.

---

## FASE 9 — Reloj del Restaurante, Franjas Horarias y Eventos Estacionales [FUTURO]
- [ ] `RestaurantClock`: Madrugada, Desayuno, Pre-almuerzo, Almuerzo, Once, Cena, Noche.
- [ ] Modificadores dinámicos de demanda y grupos según franja horaria.
- [ ] Sistema de eventos de calendario (`SeasonalEventSO` - Fiestas Patrias, Halloween, Navidad, Cumpleaños NPC) con mitigación de solapamiento.
- [ ] Sistema de música ambiental interna y diseño acústico.

---

## FASE 10 — Multijugador & Funciones Sociales [FUTURO]
- [ ] Modelo de autoridad de red y arquitectura de sincronización.
- [ ] Modos cooperativo, visitas de restaurantes, Chef VS Chef (competencia normalizada) e intercambio de recetas.


