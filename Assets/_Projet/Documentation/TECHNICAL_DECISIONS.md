# TECHNICAL_DECISIONS.md — Villa del Chef

Registro permanente de decisiones arquitectónicas y técnicas tomadas en el proyecto. Ninguna decisión debe eliminarse; si queda obsoleta, marcar como OBSOLETA y referenciar la nueva decisión.

---

### DECISIÓN 001
- **Título**: No utilizar Git LFS para imágenes ni audios ligeros.
- **Problema**: GitHub tiene una cuota gratuita estricta de 1 GB de ancho de banda LFS por cuenta. Al intentar subir los PNGs y WAVs con LFS, el push fue rechazado por cuota excedida.
- **Decisión**: Desactivar reglas LFS para `*.png`, `*.wav`, `*.mp3`, etc. en `.gitattributes`. Todos los assets pixel art del juego pesan < 10 MB combinados y Git estándar maneja perfectamente binarios de ese tamaño.
- **Alternativas consideradas**:
  1. Comprar paquetes adicionales de datos LFS en GitHub.
  2. Alojar assets externamente.
  3. Desactivar LFS y usar Git nativo.
- **Elegida**: 3 (Desactivar LFS).
- **Estado**: ACTIVA.

---

### DECISIÓN 002
- **Título**: Desacoplamiento de subsistemas mediante eventos C# estáticos (`GameEvents.cs`).
- **Problema**: El restaurante combina economía, misiones, cocina, granja, clientes e interfaz. Si cada script tuviera referencias directas cruzadas a otros managers, se crearía espagueti y acoplamiento frágil.
- **Decisión**: Utilizar `GameEvents.cs` con eventos estáticos fuertemente tipados (`OnGoldChanged`, `OnDishPrepared`, `OnDishDelivered`, `OnCropHarvested`, etc.).
- **Alternativas consideradas**:
  1. Singletons referenciándose directamente entre sí.
  2. ScriptableObject GameEvents (Arquitectura Ryan Hipple).
  3. Eventos estáticos centralizados en C#.
- **Elegida**: 3 (Eventos estáticos centralizados por velocidad, simplicidad y cero overhead en garbage collection para móvil).
- **Estado**: ACTIVA.

---

### DECISIÓN 003
- **Título**: Interfaz unificada `IInteractable` para interacción táctil/ratón.
- **Problema**: `TouchInputManager.cs` utilizaba casts directos a `MerchantStall`, `CookingStation`, `CropPlot` y `DeliveryCounter`, requiriendo modificar el gestor de input cada vez que se agregaba un nuevo objeto interactivo.
- **Decisión**: Crear la interfaz `IInteractable` en `VillaDelChef.Interaction` con métodos claros (`Interact()`, `CanInteract()`, etc.). Cualquier entidad interactuable (NPC, puesto, estación, cultivo, mesa) implementará `IInteractable`.
- **Alternativas consideradas**:
  1. Mantener comprobaciones `GetComponentInParent<ClaseConcreta>()`.
  2. Interfaz unificada `IInteractable`.
  3. Sistema de mensajes por tags de Unity.
- **Elegida**: 2 (`IInteractable`).
- **Estado**: ACTIVA.

---

### DECISIÓN 004
- **Título**: Tiendas modulares con `NPCSO` y `VendorSO` en reemplazo de mercado universal estático.
- **Problema**: El mercado original cargaba todos los `IngredientSO` mediante `Resources.LoadAll`, sin stock individual, sin comerciantes con personalidad, ni control de reabastecimiento.
- **Decisión**: Cada comerciante en la villa tendrá su propio `NPCSO` y `VendorSO`, con catálogo individual, stock configurable, temporizadores de restock UTC y precios diferenciados.
- **Alternativas consideradas**:
  1. Tienda global única en el HUD.
  2. Listas fijas hardcodeadas en scripts.
  3. Datos orientados a ScriptableObjects (`NPCSO` + `VendorSO`) con `VendorUI` reutilizable.
- **Elegida**: 3 (`NPCSO` + `VendorSO`).
- **Estado**: ACTIVA.

---

### DECISIÓN 005
- **Título**: Sistema de Crafting intermedio desacoplado (`CraftingRecipeSO` + `CraftingStation`).
- **Problema**: En juegos clásicos tipo ChefVille, los ingredientes no solo se compran o cultivan, sino que se procesan (ej. trigo → harina → masa → pizza). Mezclar recetas de platos terminados con procesamiento de ingredientes dentro de `RecipeSO` crearía ambigüedad en los menús de cocina y en los pedidos de clientes.
- **Decisión**: Crear `CraftingRecipeSO` y `CraftingManager` separados de `RecipeSO`. Los clientes solo piden `RecipeSO` (platos terminados), mientras que las estaciones de procesamiento producen insumos intermedios (`CraftingRecipeSO`).
- **Alternativas consideradas**:
  1. Usar la misma clase `RecipeSO` para todo con una bandera `isCrafted`.
  2. Separar limpiamente `CraftingRecipeSO` y `CraftingStation`.
- **Elegida**: 2 (Separación limpia).
- **Estado**: ACTIVA.

---

### DECISIÓN 006
- **Título**: Arquitectura de Comerciantes Especializados, Restock Basado en Epoch UTC y Fallback Polimórfico en `MerchantStall`.
- **Problema**: El jugador necesita interactuar con NPCs específicos (agricultores, carniceros, panaderos, pescadores, carpinteros, ingenieros, decoradores). Al interactuar desde móviles o PC, el juego debe garantizar que el stock se conserve fielmente entre sesiones sin resetearse al cerrar la UI, que el reabastecimiento respete el paso del tiempo real (segundos UTC) y que cualquier puesto físico existente (`MerchantStall`) pueda redirigir suavemente tanto al nuevo `VendorUI` de un NPC asignado como al `MarketUI` tradicional si no hay ninguno.
- **Decisión**:
  1. `VendorController` calcula `nextRestockTimestampSeconds` como `DateTimeOffset.UtcNow.ToUnixTimeSeconds() + restockIntervalSeconds`.
  2. La estructura `VendorSaveData` serializa en `SaveData.json` los pares `itemID:currentStock` y el timestamp de restock, previniendo reseteos no deseados.
  3. `MerchantStall.cs` prioriza `associatedNPC.Interact()`, si no existe busca cualquier `NPCController` en escena con `VendorUI`, y como último recurso usa `MarketUI`.
  4. `ArtAssetGenerator.cs` genera procedimentalmente texturas de 16x24 (world sprites) y 32x32 (retratos con fondo medallón) para los 7 NPCs, garantizando coherencia de pixel art a 16 PPU sin dependencias de assets externos.
- **Alternativas consideradas**:
  1. Temporizadores basados en `Time.time` de Unity (se reiniciarían al cerrar el juego).
  2. Resetear el stock a tope cada vez que se abre la UI (rompe la economía y la gestión de recursos).
  3. Usar timestamp UTC persistente en `SaveData`.
- **Elegida**: 3 (Timestamp UTC persistente en `SaveData`).
- **Estado**: IMPLEMENTADA Y ACTIVA.

---

### DECISIÓN 007
- **Título**: Máquina de Estados de `CraftingStation`, Persistencia en `CraftingStationSaveEntry` y UI Reactiva.
- **Problema**: El procesamiento de insumos intermedios (ej. granos de trigo en molino para harina, harina en mesa para masa, tomates en marmita para salsa) requiere estaciones independientes que operen en segundo plano, muestren estados claros al jugador (espera, procesando con barra de progreso, listo para recolectar con indicador flotante), otorguen experiencia al recolectar y preserven el estado y tiempo restante exacto de cada estación al salir del juego.
- **Decisión**:
  1. Cada `CraftingStation` implementa `IInteractable` y una máquina de estados: `Idle`, `Crafting`, `ReadyToCollect`.
  2. Al completarse el tiempo de crafteo, la estación entra en `ReadyToCollect`, activa un indicador visual flotante (icono con rebote sutil) y un trigger de recolección táctil.
  3. Al recolectar, los productos van a `InventoryManager.AddIngredient()`, se otorgan puntos de experiencia (`ProgressionManager.AddXP()`), se disparan eventos de misiones `GameEvents.TriggerCraftCollected()`, y se limpian las partículas.
  4. La persistencia se realiza mediante `CraftingStationSaveEntry` en `SaveData.craftingStations`, guardando coordenadas de grilla `(gridX, gridY)`, el ID de la receta en curso, el tiempo restante en segundos y si está lista para recolección.
- **Alternativas consideradas**:
  1. Producción instantánea sin temporizador (anula la mecánica de gestión del tiempo).
  2. Procesamiento global en un manager sin representación en el mundo físico (rompe la inmersión del restaurante y la villa).
  3. Estaciones físicas en la grilla con estados, feedback visual, recolección interactiva y persistencia atómica.
- **Elegida**: 3 (Estaciones físicas reactivas con persistencia atómica).
- **Estado**: IMPLEMENTADA Y ACTIVA.

---

### DECISIÓN 008
- **Título**: Sistema de Expansiones de Terreno Modular (`ExpansionSO`), Marcadores Físicos `ExpansionSign` y Zonificación Integrada en `GridManager`.
- **Problema**: Para recrear el progreso satisfactorio de juegos como ChefVille, el jugador no debe tener acceso libre a todo el terreno desde el inicio. El terreno debe expandirse gradualmente por zonas (Terraza Exterior, Huerto Extendido, Taller de Crafting, Plaza del Mercado) al cumplir requisitos de nivel de restaurante y monedas de oro. Además, `BuildManager` no debe permitir colocar objetos fuera del terreno adquirido ni en zonas incompatibles (ej. mesas en huertos o cocinas en la terraza sin permiso).
- **Decisión**:
  1. Cada expansión es un ScriptableObject `ExpansionSO` con `expansionID`, `gridBounds` (`RectInt`), `targetZone` (`ZoneType`), nivel requerido, costo en oro, recompensa de XP y coordenadas de su letrero físico.
  2. `GridCell` incluye la propiedad `isUnlocked`. Al iniciar el juego, las celdas de las áreas de expansión permanecen bloqueadas (`isUnlocked = false`).
  3. `BuildManager.UpdateGhostPosition()` valida `isAreaUnlocked` antes de permitir la colocación de cualquier mueble.
  4. Los límites de expansión se señalan en el mundo con `ExpansionSign` (`IInteractable`). Al interactuar, abre un modal táctil `ExpansionUI` que evalúa los requisitos en tiempo real.
  5. Al comprar una expansión, `ExpansionManager.UnlockExpansion()` gasta las monedas, otorga XP, desbloquea las celdas en `GridManager.UnlockZoneArea()`, reproduce efectos audiovisuales, destruye suavemente el marcador y persiste el ID en `SaveData.unlockedExpansions`.
  6. Se añade `ZoneType.Terrace`, permitiendo que mesas y sillas se ubiquen en exteriores y los clientes las ocupen de forma transparente mediante el pathfinding A*.
- **Alternativas consideradas**:
  1. Desbloqueo pasivo en el menú de pausa sin representación en el mundo físico.
  2. Bloqueo de cámara rígido.
  3. Marcadores interactivos en el mundo (`ExpansionSign`) + validación de celdas en `GridManager` + modal táctil `ExpansionUI`.
- **Elegida**: 3 (Marcadores interactivos en el mundo con zonificación dinámica y persistencia atómica).
- **Estado**: IMPLEMENTADA Y ACTIVA.

---

### DECISIÓN 009
- **Título**: Sistema de Reputación Dinámico, Selección Estocástica Ponderada de Arquetipos de Clientes y Desbloqueo de Recetas por Misiones Narrativas.
- **Problema**: En simuladores de restaurante de calidad como ChefVille, los clientes no deben comportarse de forma homogénea ni estática. La reputación del restaurante debe impactar directamente en el ritmo de llegada y en el perfil de comensales que visitan la villa. Asimismo, las misiones principales encomendadas por los especialistas locales deben desbloquear recetas exclusivas y prestigio que impulsen la economía y la satisfacción general.
- **Decisión**:
  1. Se amplía `CustomerSO` integrando `reputationReward` (+1 a +15), `reputationPenalty` (-2 a -10), `bonusXP` y soporte para el arquetipo `CustomerArchetype.Gourmet`.
  2. `CustomerController` premia la reputación y otorga XP al entregar platos a tiempo, y aplica penalizaciones de reputación si el cliente se marcha enojado por sobrepasar la paciencia de espera.
  3. `CustomerManager` calcula dinámicamente el tiempo de spawn mediante un multiplicador de reputación (`repMultiplier = Mathf.Clamp(1f - (currentRep * 0.004f), 0.55f, 1.15f)`), atrayendo mayor afluencia de clientes a medida que la villa gana prestigio.
  4. La selección de clientes en `CustomerManager.SelectCustomerType()` emplea ruleta estocástica ponderada por reputación: los arquetipos exigentes y de alto rendimiento (Críticos Gastronómicos con paciencia estricta pero +15 de reputación, VIPs con generosas propinas, Gourmets y Turistas) solo aparecen o incrementan su probabilidad al alcanzar umbrales altos de reputación (>= 50 a 75).
  5. `QuestSO` añade `reputationReward` y `rewardRecipeID`. `QuestManager.CompleteQuest()` otorga reputación mediante `EconomyManager.ModifyReputation()` y desbloquea recetas culinarias específicas invocando `RecipeManager.UnlockRecipe()`.
- **Alternativas consideradas**:
  1. Reputación como número estético sin impacto en la simulación.
  2. Spawn aleatorio plano sin ponderación (los clientes VIP y críticos aparecen igual con 0 reputación, restando coherencia y progresión).
  3. Selección estocástica ponderada ligada a reputación + cadencia dinámica de clientes + recompensas de recetas en misiones narrativas.
- **Elegida**: 3 (Simulación viva reactiva a la reputación con progresión guiada por misiones).
- **Estado**: IMPLEMENTADA Y ACTIVA.

---

### DECISIÓN 010
- **Título**: Arquitectura de Optimización Móvil a 60 FPS: Object Pooling Centralizado (`ObjectPoolManager`), Sprite Atlases V2 e Input Híbrido Resiliente.
- **Problema**: En plataformas móviles (Android e iOS), el rendimiento puede degradarse severamente por dos motivos críticos:
  1. *GC Spikes (Micro-tirones)*: El ciclo continuo de `Instantiate` y `Destroy` de entidades frecuentes (clientes, textos flotantes de ganancia, partículas) genera fragmentación y activa el Garbage Collector durante el gameplay activo.
  2. *Exceso de Draw Calls*: Dibujar cientos de sprites individuales sin empaquetar ahoga la GPU móvil.
  3. *Incompatibilidad de Input*: Unity 6 permite configurar el Player con Input Clásico, New Input System o Ambos; usar directamente una sola API sin fallback arroja excepciones fatales si el proyecto o la plataforma altera la configuración.
- **Decisión**:
  1. Se crea la interfaz `IPoolable` y el singleton `ObjectPoolManager.cs`. Los clientes (`CustomerController`) y textos de feedback (`FloatingText`) implementan `IPoolable` (`OnSpawnFromPool`, `OnReturnToPool`), reciclando GameObjects en memoria sin llamadas a `Destroy`.
  2. Se crea `FloatingTextManager.cs` con precalentamiento (prewarm) de pool para indicadores numéricos dinámicos (+oro, +XP, +/- reputación, avisos de paciencia o recolección).
  3. Se diseñan y generan Sprite Atlases V2 nativos en `Assets/_Projet/Art/Atlases/` (`Atlas_Characters`, `Atlas_Environment`, `Atlas_Exterior`, `Atlas_Food`, `Atlas_Furniture`, `Atlas_UI`) que agrupan los sprites pixel art reduciendo drásticamente los draw calls móviles a menos de 10.
  4. Se dota a `TouchInputManager.cs` de compatibilidad híbrida: gestiona toques y gestos clásicos, y ante `InvalidOperationException` delega de forma transparente a `UnityEngine.InputSystem` (`Touchscreen.current` / `Mouse.current`).
- **Alternativas consideradas**:
  1. Mantener `Instantiate`/`Destroy` estándar (inviable para 60 FPS estables en móviles gama media/baja).
  2. Usar un solo gran atlas desorganizado (inconveniente para streaming y clasificación de assets).
  3. Object Pooling desacoplado + Sprite Atlases V2 estructurados por categorías + Manejador de Input Híbrido resiliente.
- **Elegida**: 3 (Arquitectura integral de alto rendimiento móvil con 0 GC allocations durante la simulación y draw calls consolidados).
- **Estado**: IMPLEMENTADA Y ACTIVA.
