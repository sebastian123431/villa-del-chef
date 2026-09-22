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

### ISSUE #012
- **Título**: BuildManager.ValidateNavigationSafety restauraba transitabilidad forzando true.
- **Severidad**: MEDIA.
- **Sistema**: Construcción / Pathfinding (`BuildManager.cs`).
- **Descripción**: Durante la validación temporal de colocación, las celdas se marcaban como no transitables (`isWalkable = false`) y al finalizar se restauraban con `isWalkable = true`, corrompiendo celdas que eran originalmente no transitables (como muros perimetrales).
- **Solución Propuesta**: Almacenar el estado original en un diccionario `previousWalkability` y restaurar con exactitud el valor previo de cada celda. Validar rutas críticas (DeliveryCounter a Mesas, Entrada a Mesas, Worker a DeliveryCounter).
- **Estado**: RESUELTO.
- **Solución Aplicada**: `BuildManager.ValidateNavigationSafety()` ahora almacena el estado previo en `previousWalkability` y restaura cada celda exactamente a su valor anterior. Se validan las 3 rutas esenciales y se muestra notificación al usuario si la colocación bloquearía el paso.
- **Fecha**: 2026-09-22 (Fase 6.1).


