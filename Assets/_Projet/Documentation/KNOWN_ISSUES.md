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



