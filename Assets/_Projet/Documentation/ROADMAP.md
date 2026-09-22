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
- [ ] ScriptableObject `CraftingRecipeSO` (craftID, insumos requeridos, resultado, tiempo, estación requerida)
- [ ] Componente `CraftingStation` (molino, batidora, procesador, lácteos, etc.)
- [ ] `CraftingManager` y cola de procesamiento
- [ ] `CraftingUI` táctil para seleccionar recetas y ver tiempo restante
- [ ] Integración con inventario (`RAW`, `CRAFTED`, `SPECIAL`)

## FASE 4 — Expansiones de la Villa & Zonificación
- [ ] ScriptableObject `ExpansionSO` (ID, costo, nivel requerido, reputación requerida, límites del terreno)
- [ ] Mesas exteriores y funcionamiento unificado para terrazas/patios
- [ ] Desbloqueo progresivo de zonas: Restaurante → Terraza → Huerto extendido → Zona de Crafting → Mercado
- [ ] Persistencia de expansiones desbloqueadas en `SaveData.cs`

## FASE 5 — Progresión Profunda, Reputación y Misiones
- [ ] Sistema de Reputación como recurso dinámico (afecta afluencia y clientes VIP)
- [ ] Tipos de clientes extendidos en `CustomerSO` (Impaciente, Generoso, Gourmet, Familiar, Turista, Crítico, VIP)
- [ ] Sistema de misiones ampliado (Tutorial, Historia, Diarias, Progresión)
- [ ] Progresión de aprendizaje: Tomás enseña la receta de masa tras completar misión "El secreto de la masa"

## FASE 6 — Optimización Móvil & Pulido Audiovisual
- [ ] Guardado atómico con archivo temporal y backup (`villadelchef_save_backup.json`)
- [ ] Input seguro con Unity New Input System
- [ ] Object pooling para comensales, partículas y textos flotantes
- [ ] Creación y asignación de Sprite Atlases para draw calls en 60 FPS móviles
