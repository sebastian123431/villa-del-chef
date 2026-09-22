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
- **Fecha**: 2026-09-22.

