# KNOWN_ISSUES.md — Villa del Chef

Registro de bugs, fallos de arquitectura y deuda técnica detectados en el proyecto.

---

### ISSUE #001
- **Título**: Teletransporte como fallback de pathfinding en Worker y Customer.
- **Severidad**: ALTA.
- **Sistema**: Pathfinding / IA (`WorkerController.cs`, `CustomerController.cs`).
- **Descripción**: Cuando `GridPathfinding.FindPath()` no encuentra un camino válido hacia el destino (por ejemplo, si el camino está bloqueado por muebles o decoración), el código actual ejecuta un fallback que mueve instantáneamente la posición del personaje a la casilla destino (`transform.position = GridManager.Instance.GridToWorld(targetGrid)`), rompiendo la inmersión visual.
- **Solución Propuesta**: Cancelar la tarea o reintentar encontrar un nodo adyacente transitable. Si no hay ruta, emitir una advertencia, detener la acción y retornar a estado Idle sin teletransportar jamás.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se eliminó el `transform.position = ...` de fallback en `WalkToRoutine` en ambos controladores. Ahora se emite un `Debug.LogWarning`, se cancela la tarea de forma controlada y el personaje retorna a Idle o espera la próxima oportunidad.
- **Fecha**: 2026-09-22.

---

### ISSUE #002
- **Título**: Entrega de platos en `WorkerController` no verifica coincidencia con el pedido de la mesa.
- **Severidad**: ALTA.
- **Sistema**: Restaurante / Camareros (`WorkerController.cs`, `DeliveryCounter.cs`).
- **Descripción**: El trabajador simplemente toma el plato en el índice 0 del mostrador con `DeliveryCounter.Instance.TakeNextDish()`, y si la mesa solicitó otro plato diferente, puede entregar un plato incorrecto o no encontrar la mesa adecuada.
- **Solución Propuesta**: Implementar en `DeliveryCounter` un método `FindMatchingDishForOrder(RecipeSO order)` y permitir que el camarero busque mesas esperando platos específicos que estén disponibles en el mostrador.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se implementó `FindMatchingDish(RecipeSO recipe)` y `TakeSpecificDish(DishInstance targetDish)` en `DeliveryCounter.cs`. En `WorkerController.cs`, `FindMatchingOrderAndDish()` localiza mesas esperando pedidos que coincidan exactamente con un plato listo en el mostrador.
- **Fecha**: 2026-09-22.

---

### ISSUE #003
- **Título**: Mesas no tienen ciclo de suciedad ni limpieza (`TableState`).
- **Severidad**: MEDIA.
- **Sistema**: Restaurante (`Table.cs`, `CustomerController.cs`, `WorkerController.cs`).
- **Descripción**: Cuando el cliente termina de comer y se retira, `assignedTable.ClearTable()` elimina inmediatamente el plato y deja la mesa disponible instantáneamente, sin pasar por un estado `Dirty` que requiera ser limpiado por el camarero.
- **Solución Propuesta**: Agregar enum `TableState` (`Available`, `Reserved`, `Occupied`, `WaitingFood`, `Eating`, `Dirty`, `Cleaning`), asociar `Table.currentCustomer` directo, y hacer que el comensal deje la mesa `Dirty` para que el camarero deba limpiarla antes de que otro cliente pueda ocuparla.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se implementó `TableState` en `Table.cs` con métodos `MarkDirty()`, `StartCleaning()` y `FinishCleaning()`. El comensal deja la mesa en estado `Dirty` al levantarse tras comer. `WorkerController.cs` detecta mesas sucias como segunda prioridad, camina hacia ellas, ejecuta el temporizador de limpieza y las vuelve a dejar en `Available`.
- **Fecha**: 2026-09-22.

---

### ISSUE #004
- **Título**: Acoplamiento directo en `TouchInputManager.cs` sin interfaz de interacción.
- **Severidad**: MEDIA.
- **Sistema**: Input (`TouchInputManager.cs`, `IInteractable.cs`).
- **Descripción**: La función `HandleTap` realiza raycasts y ejecuta múltiples `GetComponentInParent<ClaseConcreta>` (para `MerchantStall`, `CookingStation`, `CropPlot`, etc.), impidiendo la adición de nuevos interactuables (NPCs, CraftingStations) sin tocar el código de input.
- **Solución Propuesta**: Crear la interfaz `IInteractable` y detectar simplemente `hit.collider.GetComponentInParent<IInteractable>()?.Interact()`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se creó `IInteractable.cs` y fue implementada en `CookingStation`, `CropPlot`, `MerchantStall`, `DeliveryCounter` y `Table`. `TouchInputManager.cs` delega los toques directamente a cualquier componente que implemente `IInteractable`.
- **Fecha**: 2026-09-22.

---

### ISSUE #005
- **Título**: Evaluación de cultivos por `Update()` individual por frame.
- **Severidad**: BAJA (Impacto en escalabilidad móvil).
- **Sistema**: Farming (`CropPlot.cs`, `FarmingManager.cs`).
- **Descripción**: Cada parcela ejecuta en su propio `Update()` cálculos de `DateTimeOffset.UtcNow.ToUnixTimeSeconds()`. Con decenas de parcelas, esto genera sobrecoste innecesario en móviles.
- **Solución Propuesta**: Desactivar el `Update()` por frame en `CropPlot` y controlarlo desde `FarmingManager` mediante un tick centralizado cada 0.5 - 1.0 segundo.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se removió el método `Update()` por frame en `CropPlot.cs`. Se añadió el método `TickGrowth(long now)`. `FarmingManager.cs` ejecuta la corrutina `CentralizedFarmingTickRoutine()` que evalúa todas las parcelas activas una vez por segundo.
- **Fecha**: 2026-09-22.

---

### ISSUE #006
- **Título**: Guardado JSON no atómico y sin versión de guardado.
- **Severidad**: MEDIA.
- **Sistema**: Persistencia (`SaveManager.cs`, `SaveData.cs`).
- **Descripción**: `SaveManager.cs` sobrescribe directamente el archivo `.json`. Si la app móvil se cierra intempestivamente durante la escritura, el archivo puede corromperse sin posibilidad de respaldo.
- **Solución Propuesta**: Implementar guardado atómico (escribir en `.tmp`, validar, reemplazar archivo principal) y mantener un `.bak` de seguridad, además de incluir `saveVersion = 1` en `SaveData`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `SaveData.cs` incluye `saveVersion = 1` y campo `isDirty` para muebles. `SaveManager.cs` escribe a `.tmp`, realiza copia de respaldo a `.bak` y reemplaza de forma atómica. `LoadOrCreateData()` intenta restaurar desde `.bak` si el archivo principal se corrompe.
---

### ISSUE #007
- **Título**: ObjectPool libera mesa Dirty al retornar CustomerController.
- **Severidad**: CRÍTICA.
- **Sistema**: Restaurante / Pool (`CustomerController.cs`, `Table.cs`, `WorkerController.cs`, `ObjectPoolManager.cs`).
- **Descripción**: Cuando un cliente terminaba de comer, marcaba la mesa en `TableState.Dirty` y caminaba a la salida. Al salir y retornar al ObjectPool, `OnReturnToPool()` llamaba `assignedTable?.ClearTable()`, reseteando la mesa a `Available` y destruyendo el ciclo de limpieza del Worker.
- **Solución Propuesta**: Desacoplar la desvinculación de referencias del comensal (`ReleaseTableReference()`) de la limpieza física de la mesa (`ClearTable()`). `ClearTable()` no debe alterar mesas en estado `Dirty` o `Cleaning`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se implementó `ReleaseTableReference()` en `CustomerController.cs` y se invoca antes de caminar a la salida. En `Table.cs`, `ClearTable()` valida que la mesa no esté sucia ni en limpieza. El retorno al pool limpia referencias internas del cliente sin alterar el estado de la mesa.
- **Fecha**: 2026-09-22 (Fase 6.1).

---

### ISSUE #008
- **Título**: RestaurantBootstrap sobrescribe bases de datos ScriptableObject reales.
- **Severidad**: ALTA.
- **Sistema**: Core / Bootstrap (`RestaurantBootstrap.cs`, `RecipeManager.cs`, `CustomerManager.cs`, `FarmingManager.cs`).
- **Descripción**: `RestaurantBootstrap.cs` instanciaba ScriptableObjects temporales en runtime y sobrescribía `allRecipes` (3 recetas), `availableCustomerTypes` (3 clientes) y `allCrops` (2 cultivos), ignorando las bases de datos de `Resources/`.
- **Solución Propuesta**: Agregar bandera `useDevelopmentFallbackData = false;`. En modo producción, no alterar los catálogos cargados por los managers y delegar la carga de ScriptableObjects a `Resources.LoadAll`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se añadió `useDevelopmentFallbackData = false`. Ahora `RecipeManager`, `CustomerManager` y `FarmingManager` mantienen sus catálogos data-driven completos de 12 recetas, 7 tipos de clientes y todos los cultivos.
- **Fecha**: 2026-09-22 (Fase 6.1).

---

### ISSUE #009
- **Título**: Crafting no procesaba tiempo transcurrido offline ni poseía tick centralizado.
- **Severidad**: ALTA.
- **Sistema**: Crafting / Persistencia (`CraftingManager.cs`, `CraftingStation.cs`, `SaveData.cs`).
- **Descripción**: Las estaciones de crafting usaban `Update()` individual por frame y solo guardaban `remainingTime`, perdiendo el progreso cuando el juego estaba cerrado.
- **Solución Propuesta**: Guardar marcas de tiempo UTC (`craftStartTimestampSeconds`, `craftFinishTimestampSeconds`), calcular tiempo transcurrido offline en `CraftingManager.LoadFromSave()`, y mover la simulación a una corrutina de tick cada 0.5s.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `SaveData.cs` almacena timestamps UTC. `CraftingManager.cs` procesa el tiempo offline al inicio y sincroniza las estaciones mediante `CentralizedCraftingTickRoutine()`. `CraftingStation.cs` ya no usa `Update()` por frame.
- **Fecha**: 2026-09-22 (Fase 6.1).

---

### ISSUE #010
- **Título**: Bases de ScriptableObjects y Sprites no estaban versionadas en Git.
- **Severidad**: ALTA.
- **Sistema**: Assets / Data-driven (`Assets/_Projet/Resources/`).
- **Descripción**: El proyecto dependía de scripts del editor para poblar ScriptableObjects (`AssetDatabasePopulator.cs`). Un clon limpio de Git carecía de los assets en `Resources/Vendors/`, `Resources/NPC/`, `Resources/CraftingRecipes/`, `Resources/Expansions/` y `Resources/Customers/`.
- **Solución Propuesta**: Ejecutar el generador y versionar permanentemente todos los `.asset` y `.asset.meta` en el repositorio Git.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se generaron y versionaron en Git todos los ScriptableObjects requeridos para las fases 1–6 (ingredientes, recetas, estaciones, puestos, NPCs, recetas de crafteo, expansiones y misiones).
- **Fecha**: 2026-09-22 (Fase 6.1).

---

### ISSUE #011
- **Título**: Especialistas de la villa carecían de locales físicos en el mundo 2D.
- **Severidad**: MEDIA.
- **Sistema**: NPC / Mundo Exterior (`VendorBuilding.cs`, `RestaurantBootstrap.cs`).
- **Descripción**: Únicamente Elena tenía un puesto físico en el restaurante exterior. Los otros 6 comerciantes (Bruno, Tomás, Marina, Amelia, Lucas, Sofía) existían solo como datos o UI.
- **Solución Propuesta**: Crear el componente reutilizable `VendorBuilding.cs` y ubicar puestos modulares con sus NPCs y puntos de interacción en la zona exterior.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se creó `VendorBuilding.cs` implementando `IInteractable`. En `RestaurantBootstrap.cs`, `SpawnSpecialistVendorBuildings()` ubica y configura físicamente los 7 locales especializados en la calle comercial exterior.
- **Fecha**: 2026-09-22 (Fase 6.1).

---

### ISSUE #013
- **Título**: New Input System touch release ignorado por condición exterior `isPressed == false`.
- **Severidad**: CRÍTICA.
- **Sistema**: Input táctil (`TouchInputManager.cs`).
- **Descripción**: La rama New Input System evaluaba `touch.press.isPressed` como guarda externa previa, impidiendo capturar el evento `wasReleasedThisFrame` ya que en el frame del release `isPressed` ya es falso. Esto causaba taps colgados o gestos no completados.
- **Solución Propuesta**: Desacoplar la evaluación de estados (`wasPressedThisFrame`, `isPressed`, `wasReleasedThisFrame`) para que el release se procese independientemente del estado actual de presión.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se desacoplaron las tres fases en `TouchInputManager.HandleNewInputSystem()`. `HandleTouchBegan` se ejecuta al presionar, `HandleTouchMoved` mientras se mantiene presionado, y `HandleTouchEnded` incondicionalmente en el frame de soltado.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #014
- **Título**: Tiendas de NPCs permitían interacción a pesar de no cumplir `unlockLevelRequirement`.
- **Severidad**: ALTA.
- **Sistema**: Progresión / Tiendas NPC (`VendorBuilding.cs`, `VendorUI.cs`).
- **Descripción**: `VendorBuilding.CanInteract` no validaba el nivel actual del jugador contra `unlockLevelRequirement`, permitiendo abrir la tienda y comprar artículos de especialistas bloqueados.
- **Solución Propuesta**: Conectar `CanInteract` con `ProgressionManager.Instance.CurrentLevel >= unlockLevelRequirement`, proveer feedback flotante y atenuar visualmente el puesto.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `VendorBuilding.CanInteract` comprueba `ProgressionManager.CurrentLevel`. Si está bloqueada, se invoca `ShowLockedFeedback()` mostrando un texto flotante `🔒 Se desbloquea en Nivel X`, y el SpriteRenderer del puesto y NPC se atenúa a gris sutil mediante `RefreshUnlockState()`, respondiendo a `GameEvents.OnLevelUp`.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #015
- **Título**: Coordenadas de puestos de Lucas y Sofía fuera de los límites del Grid.
- **Severidad**: ALTA.
- **Sistema**: Grilla / Bootstrap (`RestaurantBootstrap.cs`, `GridManager.cs`).
- **Descripción**: `SpawnSpecialistVendorBuildings()` ubicaba a Lucas en `X=32` y Sofía en `X=36`. Con `gridWidth = 32` (rango 0..31), ambos edificios quedaban completamente fuera de la grilla lógica.
- **Solución Propuesta**: Reubicar los 7 puestos en `y = 20` dentro del rango X: 1..27 con separación de 1 casilla, y añadir validación defensiva `IsPlacementInsideGrid`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se implementó `GridManager.IsPlacementInsideGrid(origin, sizeX, sizeY)` y se reubicaron los 7 puestos en `y = 20`: Marina (1), Bruno (5), Elena (9), Tomás (13), Amelia (17), Lucas (21), Sofía (25). Todos quedan 100% dentro del grid con una acera peatonal transitable en `y = 19`.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #016
- **Título**: Locales comerciales de NPCs caían dentro de expansiones inicialmente bloqueadas.
- **Severidad**: ALTA.
- **Sistema**: Expansiones / Zonificación (`exp_crops.asset`, `exp_crafting.asset`, `GridManager.cs`).
- **Descripción**: Las expansiones `exp_crops` (Y: 16..23) y `exp_crafting` (Y: 16..23) cubrían la zona norte del mapa, provocando que los comercios públicos quedaran inaccesibles hasta comprar dichas expansiones de alto costo.
- **Solución Propuesta**: Reducir la altura de las expansiones a 3 filas (`height = 3`, Y: 16..18) y declarar las filas superiores (`y >= 19`) como bulevar comercial público permanente (`ZoneType.Market`).
- **Estado**: RESUELTO.
- **Solución Aplicada**: `exp_crops.asset` y `exp_crafting.asset` fueron ajustadas a `height = 3` (Y: 16..18). `GridManager.InitializeGrid()` asigna `ZoneType.Market` a todas las celdas con `y >= 19`, desbloqueadas y transitables por defecto.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #017
- **Título**: Exploit de paquete de inventario inicial al reiniciar con inventario en cero.
- **Severidad**: MEDIA.
- **Sistema**: Economía / Inventario (`RestaurantBootstrap.cs`, `SaveData.cs`).
- **Descripción**: La lógica de inicio comprobaba `InventoryManager.GetAllItems().Count == 0` para regalar 25 ingredientes. Si un jugador consumía todos sus ingredientes y recargaba la escena, recibía el paquete una y otra vez.
- **Solución Propuesta**: Agregar una bandera persistente `starterItemsGranted` en `SaveData.cs`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se agregó `bool starterItemsGranted` a `SaveData.cs`. `RestaurantBootstrap.cs` entrega el paquete únicamente si `!starterItemsGranted`, marcándolo en `true` y guardando la partida. Se incluyó migración automática para partidas previas.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #018
- **Título**: Botón Continuar en MainMenu activo en primera instalación por guardado técnico default.
- **Severidad**: MEDIA.
- **Sistema**: Menú Principal / Persistencia (`SaveManager.cs`, `MainMenuController.cs`).
- **Descripción**: `BootManager` invoca `SaveManager` antes del menú principal. Si no había guardado previo, `CreateDefaultSave()` creaba un archivo físico, haciendo que `File.Exists` retornara `true` y el botón "Continuar" apareciera activo en una instalación limpia.
- **Solución Propuesta**: Agregar bandera `hasStartedGame` en `SaveData.cs` y centralizar en `SaveManager.CanContinueGame()`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se agregó `bool hasStartedGame = false` a `SaveData.cs` y el método `SaveManager.CanContinueGame()`. `MainMenuController.continueButton.interactable` solo se activa cuando `CanContinueGame()` es verdadero. Si el jugador pulsa "Nueva Partida" existiendo progreso, se despliega un modal de confirmación para evitar pérdidas accidentales.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #019
- **Título**: Doble ejecución de `PlaceDish` en entrega de comida.
- **Severidad**: MEDIA.
- **Sistema**: Mozos / Clientes / Mesas (`WorkerController.cs`, `CustomerController.cs`, `Table.cs`).
- **Descripción**: `WorkerController` ejecutaba `targetTable.PlaceDish(carryingDish)` y luego `currentCustomer.ReceiveDish(carryingDish)`, el cual volvía a ejecutar `assignedTable.PlaceDish(dish)`. Además, no se validaba si el plato coincidía con el pedido del comensal.
- **Solución Propuesta**: Delegar la colocación exclusivamente a `CustomerController.ReceiveDish()`, validando coincidencia de receta.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `WorkerController` invoca únicamente `currentCustomer.ReceiveDish(carryingDish)`. `CustomerController` valida que `dish.recipeData.recipeID == orderedDish.recipeID` antes de ordenar a la mesa `assignedTable.PlaceDish(dish)` y pasar al estado `Eating`. Si no coincide, el mozo devuelve el plato al mostrador de forma segura.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #020
- **Título**: Eager evaluation de texturas procedimentales en `RestaurantBootstrap.cs`.
- **Severidad**: BAJA (Optimización de memoria / arranque).
- **Sistema**: Core / Bootstrap (`RestaurantBootstrap.cs`).
- **Descripción**: La llamada `GetOrFallbackSprite(path, CreatePixelSprite(...))` evaluaba los argumentos en C# de forma anticipada, generando decenas de texturas y sprites procedurales en memoria incluso cuando el asset real existía en disco.
- **Solución Propuesta**: Migrar a evaluación perezosa `GetOrCreateFallbackSprite(path, Func<Sprite>)`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se implementó `GetOrCreateFallbackSprite(string path, System.Func<Sprite> factory)`. La fábrica procedural solo se ejecuta si el asset real falla al cargarse, eliminando las asignaciones superfluas de texturas.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #021
- **Título**: Celdas ocupadas por tiendas NPC permitían construcción de muebles encima.
- **Severidad**: MEDIA.
- **Sistema**: Construcción (`GridManager.cs`, `VendorBuilding.cs`).
- **Descripción**: `VendorBuilding` registraba ocupación en el grid pasando `occupyingObject = null`. `GridManager.IsAreaAvailable` solo comprobaba `occupyingObject != null`, permitiendo colocar muebles directamente encima de los puestos comerciales.
- **Solución Propuesta**: Validar `!cell.isUnlocked` y `!cell.isWalkable` en `IsAreaAvailable`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `GridManager.IsAreaAvailable` ahora rechaza cualquier celda donde `!cell.isUnlocked || !cell.isWalkable || cell.occupyingObject != null`. Adicionalmente, `VendorBuilding` registra un `GridObject` representativo en su footprint.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #022
- **Título**: Reservas de mozos no se liberaban al desactivar o destruir el componente.
- **Severidad**: MEDIA.
- **Sistema**: Mozos (`WorkerController.cs`).
- **Descripción**: Si un trabajador se destruía, desactivaba o perdía su ruta mientras tenía un plato (`isReserved`) o una mesa sucia (`isCleaningReserved`) reservada, dichos elementos quedaban bloqueados permanentemente para otros trabajadores.
- **Solución Propuesta**: Rastrear `currentlyReservedDish` y `currentlyReservedTable` y liberarlos en `OnDisable`, `OnDestroy` y abortos de tarea.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se crearon referencias internas `currentlyReservedDish` y `currentlyReservedTable`, gestionadas con el método `ReleaseReservations()` invocado automáticamente en `OnDisable()` y `OnDestroy()`, liberando las reservas y devolviendo platos al mostrador.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #023
- **Título**: GameDataValidatorEditor fallaba con `cr.recipeID` y ruta `Crafting` inexistente.
- **Severidad**: ALTA.
- **Sistema**: Editor / Herramientas de Validación (`GameDataValidatorEditor.cs`).
- **Descripción**: `GameDataValidatorEditor.cs` intentaba acceder a la propiedad inexistente `cr.recipeID` en `CraftingRecipeSO` (la propiedad real es `craftID`) y cargaba recetas desde `Resources.LoadAll<CraftingRecipeSO>("Crafting")` en lugar de la carpeta física real `CraftingRecipes/`. Esto impedía la compilación y validación de datos en Unity.
- **Solución Propuesta**: Corregir a `cr.craftID` y actualizar la ruta a `CraftingRecipes`. Añadir guarda `!Application.isBatchMode` para permitir ejecución headless/CI.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se corrigieron todas las referencias a `cr.craftID` y la ruta a `"CraftingRecipes"`. Se validó mediante Unity batchmode con 0 errores y 0 advertencias.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #024
- **Título**: Bypass de nivel en NPCController mediante interacción táctil directa sobre el NPC hijo.
- **Severidad**: ALTA.
- **Sistema**: NPC / Progresión (`NPCController.cs`, `VendorBuilding.cs`).
- **Descripción**: `VendorBuilding` validaba `unlockLevelRequirement`, pero el `NPCController` hijo poseía su propio `BoxCollider2D` e implementaba `IInteractable` con `CanInteract => npcData != null`, permitiendo saltarse la restricción de nivel al tocar directamente al NPC en vez del edificio.
- **Solución Propuesta**: `NPCController` debe consultar a su `VendorBuilding` padre si existe, delegando `CanInteract` y mostrando feedback bloqueado en `Interact()` sin abrir la tienda.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se centralizó la lógica en `VendorBuilding`. `NPCController` obtiene `GetComponentInParent<VendorBuilding>()`. Si el edificio está bloqueado, `CanInteract` es falso e `Interact()` invoca `ShowLockedFeedback()`, imposibilitando cualquier bypass.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #025
- **Título**: Long press en Build Mode permitía seleccionar tiendas comerciales y objetos estáticos.
- **Severidad**: MEDIA.
- **Sistema**: Construcción / Grilla (`GridObject.cs`, `BuildManager.cs`, `TouchInputManager.cs`, `VendorBuilding.cs`).
- **Descripción**: Los puestos comerciales registraban ocupación con `GridObject` genérico sin datos de mueble (`furnitureData == null`). Un long press sobre el puesto invocaba `StartMovingObject`, intentando mover o destruir el edificio comercial y dejando un footprint huérfano de 1x1.
- **Solución Propuesta**: Añadir `playerMovable` y `overrideSizeX/Y` a `GridObject`. Filtrar en `BuildManager.StartMovingObject` y `TouchInputManager.HandleLongPress` para rechazar objetos no movibles.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se añadió `public bool playerMovable = true;` y soporte de footprint `overrideSizeX/Y` con método `SetupStatic`. En `BuildManager` y `TouchInputManager`, sólo los objetos con `playerMovable == true && furnitureData != null` pueden seleccionarse o moverse. Los locales comerciales quedan blindados contra desplazamientos.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #026
- **Título**: Build Mode táctil en móvil no confirmaba colocación al soltar el dedo (`TouchPhase.Ended`).
- **Severidad**: ALTA.
- **Sistema**: Input Táctil / Construcción (`TouchInputManager.cs`).
- **Descripción**: Al arrastrar o posicionar un mueble con touch en Build Mode, el evento `TouchPhase.Ended` solo evaluaba `HandleTap` bajo el umbral de arrastre. Si el jugador arrastraba el ghost para posicionarlo, al levantar el dedo el mueble quedaba como ghost sin colocarse nunca.
- **Solución Propuesta**: Al soltar el dedo (`TouchPhase.Ended`) en Build Mode con un mueble seleccionado, llamar a `BuildManager.Instance.TryPlaceObject()`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: En `TouchInputManager` (tanto en Legacy como en New Input System), `TouchPhase.Ended` con `isBuildMode && selectedFurniture != null` actualiza la posición del hover y ejecuta `TryPlaceObject()`, permitiendo colocación táctil intuitiva de catálogo o recolocación.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #027
- **Título**: Trabajador congelado indefinidamente con plato en mano si el mostrador estaba lleno.
- **Severidad**: ALTA.
- **Sistema**: Mozos (`WorkerController.cs`, `DeliveryCounter.cs`).
- **Descripción**: Si un trabajador llevaba un plato pero la mesa era inalcanzable, o el cliente se marchaba antes de llegar, intentaba devolverlo al mostrador. Si el mostrador estaba lleno, el mozo pasaba a `WorkerState.Idle`, pero `WorkerThinkRoutine` solo busca trabajo si `carryingDish == null`. Como resultado, el trabajador quedaba congelado de por vida.
- **Solución Propuesta**: Añadir estados seguros `WorkerState.ReturningDish` y `WorkerState.WaitingCounterSpace`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se implementó la corrutina `ReturnDishRoutine()` con los estados `ReturningDish` y `WaitingCounterSpace`. Si el mostrador está lleno, el mozo espera de forma segura y reintenta periódicamente depositar el plato. Si el cliente desaparece o cambia de pedido, el mozo nunca deja el plato en una mesa vacía y lo retorna al mostrador.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #028
- **Título**: Migración de partidas antiguas a SaveData v2 incompleta y dispersa.
- **Severidad**: MEDIA.
- **Sistema**: Persistencia (`SaveData.cs`, `SaveManager.cs`).
- **Descripción**: El esquema de guardado incorporó `hasStartedGame` y `starterItemsGranted`, pero la versión continuaba en `saveVersion = 1` y la migración solo evaluaba 4 variables básicas, ignorando progreso en cultivos, crafteo, expansiones, misiones, reputación y monedas.
- **Solución Propuesta**: Incrementar a `saveVersion = 2`, crear `MigrateSaveIfNeeded()` evaluando las 14 dimensiones de progreso del jugador y aplicarlo tanto al archivo principal como a backups.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `SaveData.saveVersion` se elevó a 2. Se implementó `MigrateSaveIfNeeded(SaveData data)` en `SaveManager.cs`, el cual detecta cualquier progreso previo (nivel, XP, inventario, muebles, parcelas, misiones, tiendas, estaciones de crafteo, expansiones, recetas, tutorial, monedas y reputación) para inferir `hasStartedGame = true` y `starterItemsGranted = true` sin pérdida de datos.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #029
- **Título**: AutoSetup de escenas en `RestaurantSceneSetupEditor` podía sobreescribir arte personalizado.
- **Severidad**: MEDIA.
- **Sistema**: Editor Tools (`RestaurantSceneSetupEditor.cs`).
- **Descripción**: El atributo `[InitializeOnLoadMethod]` regeneraba automáticamente todas las escenas mediante una clave de EditorPrefs, con el riesgo de borrar personalizaciones manuales en escenas o assets del proyecto al abrir Unity.
- **Solución Propuesta**: Condicionar la auto-generación únicamente a la ausencia física total de las escenas en disco, manteniendo el menú manual como herramienta de desarrollo.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `AutoSetupScenesOnEditorLoad` verifica si alguna de las 3 escenas (`00_Boot.unity`, `01_MainMenu.unity`, `02_Restaurant.unity`) no existe en disco antes de ejecutar `SetupAllScenes()`. Las escenas ya versionadas en Git se preservan intactas.
- **Fecha**: 2026-09-22 (Fase 6.2).

---

### ISSUE #030
- **Título**: Ausencia de ScriptableObjects físicos de muebles en `Resources/Furniture/` causaba fallback en memoria y sobreescritura de catálogo en BuildUI.
- **Severidad**: MEDIA.
- **Sistema**: Construcción / Data-Driven (`RestaurantBootstrap.cs`, `AssetDatabasePopulator.cs`, `BuildUI.cs`).
- **Descripción**: La carpeta `Assets/_Projet/Resources/Furniture/` no contenía archivos `.asset` serializados físicamente. `RestaurantBootstrap.cs` creaba instancias en memoria con `ScriptableObject.CreateInstance<FurnitureSO>()` y sobrescribía `BuildUI.Instance.catalogItems` con una lista fija hardcodeada de 6 items, ignorando estaciones de crafteo y otros assets data-driven.
- **Solución Propuesta**: Crear método `CreateOrUpdateFurniture` en `AssetDatabasePopulator.cs`, materializar los `.asset` y `.meta` físicos de `table_wood`, `chair_wood`, `counter_delivery`, `stove_01`, `grill_01`, `crop_plot` en `Resources/Furniture/`, y hacer que `RestaurantBootstrap` cargue desde `Resources.LoadAll<FurnitureSO>("Furniture")`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se crearon los 6 assets físicos `.asset` con sus respectivos `.meta` en `Assets/_Projet/Resources/Furniture/`. Se implementó `CreateOrUpdateFurniture` en `AssetDatabasePopulator.cs`. En `RestaurantBootstrap.cs`, la inicialización consulta `Resources.Load<FurnitureSO>` con fallback defensivo, y `BuildUI.Instance.catalogItems` prioriza `Resources.LoadAll<FurnitureSO>("Furniture")` respetando la arquitectura data-driven.
- **Fecha**: 2026-09-24.

---

### ISSUE #031
- **Título**: `DeliveryCounter.TakeNextDish()` permitía que dos mozos compitieran por un plato reservado.
- **Severidad**: ALTA.
- **Sistema**: Mozos y Mostrador (`DeliveryCounter.cs`, `WorkerController.cs`).
- **Descripción**: Si múltiples trabajadores buscaban tareas simultáneamente, `DeliveryCounter.TakeNextDish()` tomaba el primer plato en el mostrador sin validar si `isReserved == true`, provocando que un trabajador retirara el plato que otro ya tenía asignado para una comanda.
- **Solución Propuesta**: Modificar `TakeNextDish()` para ignorar cualquier plato con `isReserved == true`, retornando únicamente platos disponibles sin reserva activa.
- **Estado**: RESUELTO.
- **Solución Aplicada**: En `DeliveryCounter.cs`, `TakeNextDish()` itera sobre la lista y solo extrae un plato si `!dish.isReserved`, garantizando exclusión mutua atómica entre trabajadores.
- **Fecha**: 2026-09-24.

---

### ISSUE #032
- **Título**: Fallback indeseado a atuendos de chef en comensales de la villa.
- **Severidad**: ALTA (Violación visual de diseño).
- **Sistema**: Personajes / Clientes (`CharacterSO.cs`, `CharacterAppearanceController.cs`, `CustomerController.cs`).
- **Descripción**: `CharacterSO.GetPreviewSprite()` y `GetAnimator()` utilizaban un fallback universal donde si `normalPreview` o `normalAnimator` no estaban presentes, se devolvían las variantes de chef (`blackChefPreview` / `whiteChefPreview`). Esto causaba que comensales en el restaurante aparecieran vestidos con uniformes de cocina en lugar de ropa casual (`rnormal`).
- **Solución Propuesta**: Incorporar el parámetro `allowCrossOutfitFallback` en `GetPreviewSprite` y `GetAnimator`, con valor forzado en `false` para todo comensal instanciado o configurado en `CustomerController`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se implementó `allowCrossOutfitFallback = true` (por defecto) y `false` estricto en comensales. Si falta el atuendo normal, se retorna `null` y se registra un error explícito en consola (`Debug.LogError`), prohibiendo rotundamente vestir comensales como chefs.
- **Fecha**: 2026-09-24 (Fase 7).

---

### ISSUE #033
- **Título**: Pérdida de progreso del prólogo al salir de la aplicación antes de finalizar.
- **Severidad**: MEDIA.
- **Sistema**: Prólogo / Persistencia (`PrologueController.cs`, `SaveData.cs`).
- **Descripción**: Si el usuario introducía su nombre o seleccionaba a su personaje en el prólogo y cerraba la app antes de completar el último paso, la siguiente sesión reiniciaba el prólogo desde el paso 1 sin recordar el nombre ni la elección.
- **Solución Propuesta**: Persistencia incremental en cada transición de paso (`prologueStep`: 1 a 5, `playerName`, `selectedPlayerCharacterID`, `selectedChefOutfit`) y detección de reanudación automática en `Start()`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se implementaron llamadas a `SaveGame()` en `OnSubmitName()` (paso 2), `OnConfirmCharacter()` (paso 4) y `OnOutfitChosen()` (paso 5). `Start()` detecta si `prologueStep > 1 && !prologueCompleted` para reanudar la UI en el paso guardado restaurando nombre, personaje seleccionado y uniforme.
- **Fecha**: 2026-09-24 (Fase 7).

---

### ISSUE #034
- **Título**: El restaurante iniciaba en estado abierto (`restaurantOpen = true`) tras concluir el prólogo.
- **Severidad**: ALTA (Diseño de bucle jugable).
- **Sistema**: Flujo de Juego / Ciclo de Operación (`PrologueController.cs`).
- **Descripción**: Al hacer clic en "Comenzar Aventura" en el paso 5 del prólogo, el código asignaba `data.restaurantOpen = true`. Esto violaba la regla de diseño oficial según la cual el jugador debe ingresar con el restaurante cerrado para familiarizarse con la cocina, sembrar o abastecerse antes de abrir manualmente las puertas.
- **Solución Propuesta**: Cambiar a `data.restaurantOpen = false` al concluir el prólogo.
- **Estado**: RESUELTO.
- **Solución Aplicada**: En `PrologueController.OnEnterRestaurant()`, se asigna de manera explícita `data.restaurantOpen = false;`.
- **Fecha**: 2026-09-24 (Fase 7).

---

### ISSUE #035
- **Título**: `CustomerManager.SpawnLoop()` generaba clientes cuando `RestaurantOperatingManager.Instance == null`.
- **Severidad**: ALTA.
- **Sistema**: Clientes / Spawner (`CustomerManager.cs`).
- **Descripción**: La evaluación booleana `isOpen` utilizaba `RestaurantOperatingManager.Instance == null || RestaurantOperatingManager.Instance.IsOpen`. En entornos de test o si el manager no se había inicializado, asumía por defecto que el restaurante estaba abierto y generaba comensales descontroladamente.
- **Solución Propuesta**: Aplicar evaluación defensiva: solo generar comensales si la instancia existe y `IsOpen == true`. Registrar advertencia si el manager está ausente.
- **Estado**: RESUELTO.
- **Solución Aplicada**: En `CustomerManager.cs`, se modificó a `bool isOpen = RestaurantOperatingManager.Instance != null && RestaurantOperatingManager.Instance.IsOpen;` con advertencia preventiva si la instancia es nula, asegurando que el restaurante permanezca cerrado por seguridad.
- **Fecha**: 2026-09-24 (Fase 7).

---

### ISSUE #036
- **Título**: Test de integración de elenco social mutaba permanentemente el archivo de guardado del usuario y hardcodeaba conteo de personajes.
- **Severidad**: MEDIA (QA y testing).
- **Sistema**: Tests de Editor (`SocialCastIntegrationTest.cs`).
- **Descripción**: El test modificaba `SaveManager.Instance.SaveData` para asignar player y helper pero no restauraba los valores previos del jugador al terminar. Además, fallaba si el número de Friends en disco era distinto de exactamente 19.
- **Solución Propuesta**: Utilizar `try ... finally` con snapshot de `SaveData` mediante `JsonUtility.ToJson` / `FromJsonOverwrite` para aislar el test y evaluar conteo dinámico (`allCharacters.Length >= 2`).
- **Estado**: RESUELTO.
- **Solución Aplicada**: `SocialCastIntegrationTest.cs` encapsula toda la prueba en un bloque `try ... finally`, restaura el JSON original íntegramente al terminar, destruye los GameObjects temporales y valida dinámicamente la exclusión y rotación de personajes sin hardcodear el total de amigos.
- **Fecha**: 2026-09-24 (Fase 7).

---

### ISSUE #037
- **Título**: Selector de ayudante (`HelperIntroDialogUI`) incompleto con auto-selección silenciosa.
- **Severidad**: ALTA (Experiencia de usuario y narrativa).
- **Sistema**: Diálogo / UI de Ayudante (`HelperIntroDialogUI.cs`, `StaffMenuUI.cs`).
- **Descripción**: Al dispararse la invitación del primer ayudante, `LoadAvailableFriends()` ejecutaba `SelectCharacter(availableFriends[0])` sin instanciar tarjetas interactivas visibles, y el fallback seleccionaba silenciosamente al primer amigo. Además, si el jugador pulsaba "Ahora no", no existía un menú para contratar o relevar al ayudante con posterioridad.
- **Solución Propuesta**: Poblar un grid real de tarjetas seleccionables excluyendo al protagonista y comensales activos; requerir confirmación explícita con selección de vestuario (ChefBlack / ChefWhite); crear el menú `StaffMenuUI` ("PERSONAL") accesible desde el HUD para relevar o contratar ayudantes más adelante.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `HelperIntroDialogUI.cs` genera tarjetas dinámicas completas, deshabilita comensales activos, requiere selección activa antes de habilitar "Confirmar", y delega el fallback a `StaffMenuUI`. Se implementó `StaffMenuUI.cs` y su botón en el HUD (`HUDController.cs`).
- **Fecha**: 2026-09-24 (Fase 7.0.1).

---

### ISSUE #038
- **Título**: Teletransporte indirecto del comensal en caso de camino bloqueado hacia la mesa.
- **Severidad**: CRÍTICA (Fidelidad física y visual).
- **Sistema**: Pathfinding e IA de Comensal (`CustomerController.cs`).
- **Descripción**: En `CustomerController.WalkToRoutine()`, si el pathfinding fallaba por bloqueo o no existía camino a la mesa, la corutina finalizaba con `yield break`, pero el ciclo posterior ejecutaba `transform.position = assignedChair.GetSitPosition()`, sentando mágicamente al comensal en la mesa inaccesible.
- **Solución Propuesta**: Hacer que `WalkToRoutine` reporte éxito/fracaso mediante callback booleano. Si falla, liberar de inmediato la silla y la mesa reservada, cancelar la atención, caminar a la salida y despawnear de forma segura sin teletransporte.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `CustomerController.CustomerLifecycleRoutine` evalúa `reachedChair`. Si es falso, libera la mesa y silla, registra advertencia, no teletransporta y despawnea al comensal limpiamente.
- **Fecha**: 2026-09-24 (Fase 7.0.1).

---

### ISSUE #039
- **Título**: 64 frames (16 filas) desaprovechados y Animator direccional sin memoria de orientación.
- **Severidad**: ALTA (Animación y calidad visual).
- **Sistema**: Pipeline de Animación (`CharacterPipelineEditor.cs`).
- **Descripción**: Las hojas 4x16 poseían 16 filas de acciones pero el editor solo generaba clips para Down, dejando Up/Left/Right y acciones especializadas (Cocina en 4 direcciones, Think, Pickup, Carry/Serve, Celebrate) incompletas. Además, el Animator no retenía la última dirección al detenerse (`Speed == 0`), regresando siempre a `Idle_Down`.
- **Solución Propuesta**: Generar las 16 filas como AnimationClips dedicados por cada vestuario; configurar BlendTrees `SimpleDirectional2D` para Idle, Walk y Cook; alimentar `MoveX` y `MoveY` en locomoción preservando el último vector al detenerse.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `CharacterPipelineEditor.cs` genera los 16 clips por vestuario e instala BlendTrees direccionales y transiciones completas. `CustomerController.cs` y `WorkerController.cs` preservan la última orientación al frenar.
- **Fecha**: 2026-09-24 (Fase 7.0.1).

---

### ISSUE #040
- **Título**: Contaminación visual en reciclaje de ObjectPool y validadores excesivamente permisivos.
- **Severidad**: MEDIA.
- **Sistema**: Object Pooling y Validación (`CustomerController.cs`, `GameDataValidatorEditor.cs`).
- **Descripción**: Si un comensal devuelto al pool era reasignado a otra identidad con assets pendientes, podía conservar el sprite o controller del comensal anterior. Asimismo, `GameDataValidatorEditor` catalogaba la falta de preview normal como advertencia y no como error.
- **Solución Propuesta**: Implementar `ResetAppearance()` en `CharacterAppearanceController` y llamarlo en `CustomerController.OnReturnToPool()`. Endurecer validadores para catalogar la falta de normalPreview/Animator en `canAppearAsCustomer` como ERROR estricto.
- **Estado**: RESUELTO.
- **Solución Aplicada**: `CustomerController.OnReturnToPool()` y `CharacterAppearanceController.ResetAppearance()` limpian totalmente sprites y runtime controllers al reciclar. `GameDataValidatorEditor.cs` marca errores estrictos ante omisión de arte normal o atuendos de chef requeridos.
- **Fecha**: 2026-09-24 (Fase 7.0.1).

---

### ISSUE #041
- **Título**: Expansiones `exp_crafting.asset` y `exp_crops.asset` con `height = 8` invadían la zona Market pública (`y >= 19`).
- **Severidad**: ALTA.
- **Sistema**: Expansiones / Zonificación (`exp_crafting.asset`, `exp_crops.asset`).
- **Descripción**: Aunque la decisión técnica había estipulado reducir la altura de estas expansiones a 3 filas (`height = 3`, `y: 16..18`), los archivos `.asset` en disco aún mantenían `height: 8`, invadiendo las filas del bulevar comercial público y disparando errores en `GameDataValidatorEditor`.
- **Solución Propuesta**: Ajustar `height: 3` en `exp_crafting.asset` y `exp_crops.asset`.
- **Estado**: RESUELTO.
- **Solución Aplicada**: Se corrigió `height: 3` en ambos assets YAML, eliminando todo solapamiento con la zona comercial y logrando 0 errores en `GameDataValidatorEditor`.
- **Fecha**: 2026-09-24 (Fase 7.0.1).






