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

---

### DECISIÓN 011
- **Título**: Desacoplamiento de Desvinculación de Comensales vs. Limpieza de Mesa y Reserva Atómica de Camarero.
- **Problema**: Al terminar de comer, los clientes marcaban la mesa en `TableState.Dirty` y se retiraban. Sin embargo, cuando el GameObject del cliente retornaba al `ObjectPoolManager`, el callback `OnReturnToPool()` invocaba `assignedTable?.ClearTable()`, reseteando la mesa a `Available` de inmediato. Esto causaba una grave regresión en el ciclo de trabajo de los mozos/camareros. Además, en escenarios con múltiples camareros, varios podían competir por el mismo plato o la misma mesa sucia, y ante la falta de reservas recurrir a fallbacks no deterministas.
- **Decisión**:
  1. Se desacopla la desvinculación de referencias del comensal (`CustomerController.ReleaseTableReference()`) de la limpieza de la mesa (`Table.ClearTable()`).
  2. `Table.ClearTable()` solo limpia platos y comensales en mesas ocupadas normales, pero ignora y protege de forma estricta las mesas en estado `Dirty` o `Cleaning`.
  3. Se añade la propiedad `isCleaningReserved` en `Table` y `isReserved` en `DishInstance` para que los camareros reserven atómicamente la mesa sucia y el plato específico desde el momento en que inician su caminata, eliminando carreras entre trabajadores.
  4. Si un camarero no encuentra el plato específico asignado al llegar al mostrador, cancela la acción limpiamente a estado Idle sin recurrir a fallbacks aleatorios (`TakeNextDish`).
- **Alternativas consideradas**:
  1. No usar ObjectPool para comensales (inviable para rendimiento móvil a 60 FPS).
  2. Dejar que `OnReturnToPool()` limpie todo ciegamente (destruye el ciclo de mesas sucias).
  3. Desacoplamiento formal de métodos: el cliente desvincula su referencia sin tocar el estado físico del restaurante, el mozo limpia y finaliza el ciclo a `Available`.
- **Elegida**: 3 (Desacoplamiento formal con reservas atómicas).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.1).

---

### DECISIÓN 012
- **Título**: Consolidación Data-Driven en RestaurantBootstrap, Puestos Físicos Modulares (`VendorBuilding`) y Crafting Offline con Timestamps UTC.
- **Problema**: 
  1. `RestaurantBootstrap.cs` continuaba instanciando ScriptableObjects temporales procedurales y sobreescribiendo los catálogos reales de `RecipeManager`, `CustomerManager` y `FarmingManager`, anulando la base de datos real versionada en `Assets/_Projet/Resources/`.
  2. Los 7 especialistas comerciales de la villa (Elena, Bruno, Tomás, Marina, Amelia, Lucas, Sofía) no poseían locales físicos homogéneos en el mapa exterior.
  3. `CraftingStation` ejecutaba un `Update()` por frame y no procesaba el tiempo transcurrido con el juego cerrado (offline time).
- **Decisión**:
  1. Se añade la propiedad `useDevelopmentFallbackData = false` en `RestaurantBootstrap.cs`. En modo estándar, Bootstrap no sobrescribe ninguna base de datos, dejando que los managers carguen el catálogo completo desde `Resources/`.
  2. Se crea el componente modular `VendorBuilding.cs` (`IInteractable`) que configura puesto físico, sprite, colisionador, letrero flotante y vinculación al `NPCController` y `NPCSO` correspondiente para los 7 especialistas, colocados a lo largo del paseo comercial exterior.
  3. `CraftingStation` adopta timestamps UTC (`craftStartTimestampSeconds`, `craftFinishTimestampSeconds`) guardados en `SaveData`. `CraftingManager` centraliza el tick de actualización cada 0.5s y calcula el avance offline exacto al cargar el juego.
  4. `TouchInputManager.cs` elimina la captura de excepciones por frame en `Update()`, cacheando la disponibilidad del subsistema en `Awake()`.
- **Alternativas consideradas**:
  1. Crear scripts mono-comportamiento específicos para cada uno de los 7 especialistas (`BrunoController`, `MarinaController`, etc.) -> Rechazada por redundancia y deuda técnica.
  2. Mantener la generación procedural de ScriptableObjects en runtime -> Rechazada; el proyecto cuenta con bases de datos completas versionadas.
  3. Arquitectura modular (`VendorBuilding`), preservación estricta de Resources y temporizadores centralizados con UTC.
- **Elegida**: 3 (Arquitectura modular data-driven de alta fidelidad).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.1).

---

### DECISIÓN 013
- **Título**: Bulevar del Mercado (`ZoneType.Market`) Público y Accesible Independiente de Expansiones.
- **Problema**: Las expansiones bloqueadas `exp_crops` y `exp_crafting` cubrían las filas `y: 16..23` en el norte del mapa. Como los puestos comerciales de los especialistas estaban en `y = 18`, los jugadores no podían acceder a los comerciantes públicos sin haber comprado antes expansiones caras.
- **Decisión**: Restringir la altura de `exp_crops` y `exp_crafting` a 3 filas (`height = 3`, `y: 16..18`). Declarar todas las filas `y >= 19` como `ZoneType.Market`, configuradas en `GridManager.InitializeGrid()` como públicas, transitables y desbloqueadas desde el frame 1. Reubicar los 7 puestos comerciales en `y = 20` (footprint 3x2) con acera peatonal despejada en `y = 19`.
- **Alternativas consideradas**:
  1. Obligar al jugador a desbloquear expansiones para poder comprar insumos básicos (rompe el bucle de juego temprano).
  2. Mover las tiendas dentro del restaurante comedor (rompe la estética y reduce espacio de mesas).
  3. Establecer un bulevar comercial público en el sector norte (`y >= 19`) manteniendo las expansiones a sus costados.
- **Elegida**: 3 (Bulevar público en `y >= 19`).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 014
- **Título**: Desacoplamiento de Guardado Técnico vs Partida Jugada (`hasStartedGame` y `starterItemsGranted`).
- **Problema**: `BootManager` inicializa `SaveManager`, el cual crea un archivo `villadelchef_save.json` por defecto si no existe ninguno. Esto provocaba que `SaveManager.HasSaveFile()` retornara siempre `true`, haciendo que el botón "CONTINUAR" estuviera activo en la primera instalación. Además, comprobar `inventory.Count == 0` permitía al jugador reiniciar con inventario vacío y recibir el starter pack ilimitadamente.
- **Decisión**:
  1. Agregar `hasStartedGame = false` y `starterItemsGranted = false` a `SaveData.cs`.
  2. Implementar `SaveManager.CanContinueGame() => CurrentSave != null && CurrentSave.hasStartedGame`.
  3. El botón "CONTINUAR" en `MainMenuController` solo se habilita cuando `CanContinueGame()` es verdadero.
  4. Si el jugador pulsa "NUEVA PARTIDA" teniendo una partida previa en progreso, se muestra un modal de confirmación antes de resetear datos.
  5. El paquete de inicio se entrega una sola vez: `starterItemsGranted` pasa a `true` y se persiste.
  6. Migración retroactiva automática para partidas existentes que ya tuvieran progreso.
- **Alternativas consideradas**:
  1. No crear save en Boot y crearlo solo al jugar (puede causar NullReference en sistemas que consultan volumen o settings).
  2. Usar PlayerPrefs separado para la bandera (fragmenta la persistencia fuera del JSON).
  3. Banderas serializadas en `SaveData` con migración transparente.
- **Elegida**: 3 (Banderas en `SaveData` con migración transparente).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 015
- **Título**: Estrategia de New Input System con Evaluación Desacoplada de Liberación de Toque.
- **Problema**: En el New Input System de Unity, en el frame exacto en que un dedo se levanta de la pantalla, `touch.press.isPressed` es `false`, pero `touch.press.wasReleasedThisFrame` es `true`. Evaluar `isPressed` como condición externa impedía procesar el release, perdiendo taps y cancelaciones de arrastre.
- **Decisión**: Estructurar el New Input System en tres ramas independientes evaluadas secuencialmente: `wasPressedThisFrame` (inicio de gesto / pinch / hover), `isPressed` (arrastre continuo / pan / ghost preview) y `wasReleasedThisFrame` (finalización de gesto / tap / long press). Implementar además Pinch-to-Zoom y Long Press de forma nativa en New Input sin recurrir a excepciones.
- **Alternativas consideradas**:
  1. Regresar exclusivamente a Legacy Input (desaconsejado en Unity 6 y proyectos modernos).
  2. Mantener excepciones try/catch por frame (impacta rendimiento y ensucia perfiles de CPU).
  3. Flujo desacoplado en New Input System con sensibilidad de zoom y duraciones de tap/long press configurables.
- **Elegida**: 3 (Flujo desacoplado en New Input System).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 016
- **Título**: Compatibilidad Polimórfica para Animaciones de NPCs (`NPCSO.animatorController`).
- **Problema**: El propietario está creando spritesheets y animaciones personalizadas en pixel art. El sistema de NPCs requería admitir animadores sin romper los NPCs existentes que solo disponen de sprites estáticos (`worldSprite` y `portrait`).
- **Decisión**: Agregar el campo opcional `public RuntimeAnimatorController animatorController;` a `NPCSO`. En `NPCController.UpdateVisuals()`, si existe un `animatorController`, se añade/asigna el componente `Animator` y se activa; si no existe, se mantiene el `SpriteRenderer` asignando `worldSprite` (o `portrait` como fallback).
- **Alternativas consideradas**:
  1. Forzar un AnimatorController para todos los NPCs (requeriría crear 7 controladores vacíos artificialmente).
  2. Mantener solo sprites estáticos sin soporte de Animator (bloquearía la incorporación del arte del usuario).
  3. Soporte opcional data-driven con fallback en cascada.
- **Elegida**: 3 (Soporte opcional data-driven con fallback en cascada).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 017
- **Título**: Evaluación Perezosa (Lazy Evaluation) de Fallbacks Procedimentales en `RestaurantBootstrap`.
- **Problema**: `GetOrFallbackSprite(path, CreatePixelSprite(...))` generaba llamadas a `CreatePixelSprite` antes de invocar el método debido a la evaluación ansiosa de argumentos en C#, creando decenas de texturas temporales en RAM incluso cuando los assets reales existían.
- **Decisión**: Reemplazar la firma por `GetOrCreateFallbackSprite(string subpath, System.Func<Sprite> fallbackFactory)`. La lambda procedural solo se evalúa si el asset de disco no existe.
- **Alternativas consideradas**:
  1. Mantener las asignaciones anticipadas (desperdicio de memoria y GC en móviles).
  2. Cargar todo síncronamente sin fallbacks (fallaría en escenas vacías o tests sin assets).
  3. Evaluación perezosa mediante delegados `Func<Sprite>`.
- **Elegida**: 3 (Evaluación perezosa).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 018
- **Título**: Entrega de Platos Atómica y Single-Sourced en `CustomerController.ReceiveDish`.
- **Problema**: `WorkerController` llamaba a `targetTable.PlaceDish(carryingDish)` y luego llamaba a `currentCustomer.ReceiveDish(carryingDish)` que volvía a llamar a `assignedTable.PlaceDish(dish)`, duplicando la colocación física y el parenting del plato, sin comprobar si el plato coincidía con el pedido real.
- **Decisión**: El trabajador entrega el plato al cliente mediante `currentCustomer.ReceiveDish(carryingDish)`. El cliente valida que `dish.recipeData.recipeID == orderedDish.recipeID`. Si coincide, el cliente es la única fuente que llama a `assignedTable.PlaceDish(dish)` y pasa a `Eating`. Si no coincide, el mozo recupera el plato y lo regresa al mostrador sin romper el estado del cliente.
- **Alternativas consideradas**:
  1. Que la mesa sea la que orqueste la colocación y notificación (acopla la mesa a la lógica de pedidos).
  2. Que el mozo llame a ambos métodos (causa el bug de doble invocación).
  3. Delegación al cliente con validación estricta y rollback seguro para el mozo.
- **Elegida**: 3 (Delegación al cliente con validación estricta).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 019
- **Título**: Estrategia de Input System Híbrido: Transición Segura con Backend Dual (`activeInputHandler = Both`).
- **Problema**: Forzar New Input System al 100% como único módulo en Android/iOS puede ocasionar problemas con EventSystem, componentes UI heredados o gestos complejos. Sin embargo, depender exclusivamente del Legacy Input bloquea la modernización hacia el New Input System.
- **Decisión**: Se adopta la **ESTRATEGIA A — TRANSICIÓN SEGURA**:
  1. `ProjectSettings.asset` mantiene `activeInputHandler: 2` (Both).
  2. `TouchInputManager` detecta dinámicamente si Legacy Input está operativo (`isLegacyInputAvailable`). Si está disponible, procesa toques nativos y mouse con alta estabilidad para Canvas y EventSystem.
  3. La rama `#if ENABLE_INPUT_SYSTEM` procesa `Touchscreen.current` y `Mouse.current` con gestos completos (tap, drag, pinch, release y long press), lista como backend secundario resiliente sin riesgo de eventos duplicados.
- **Alternativas consideradas**:
  1. Migración destructiva obligatoria a New Input System (alto riesgo de regresiones en UI y toques móviles).
  2. Quedarse anclado en Legacy Input sin soporte para las nuevas APIs de Unity 6.
  3. Estrategia híbrida resiliente con Both en PlayerSettings y detección dinámica en tiempo de ejecución.
- **Elegida**: 3 (Estrategia híbrida resiliente con transición segura).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 020
- **Título**: Centralización de Gating de Nivel en `VendorBuilding` y Bloqueo de Bypass en `NPCController`.
- **Problema**: Al implementar un GameObject hijo para el personaje con su propio `BoxCollider2D` y `NPCController`, los toques directos sobre el avatar del NPC se saltaban la verificación de nivel contenida en `VendorBuilding`, permitiendo compras en puestos comerciales bloqueados.
- **Decisión**: `VendorBuilding` es la única fuente de verdad sobre el estado de desbloqueo del puesto. `NPCController` obtiene `GetComponentInParent<VendorBuilding>()`. En `CanInteract` y en `Interact()`, delega directamente a las propiedades y métodos del edificio padre. Si el puesto está bloqueado, se muestra el feedback informativo de bloqueo y no se despliega la interfaz de compra.
- **Alternativas consideradas**:
  1. Duplicar la variable `unlockLevelRequirement` y la lógica de nivel en `NPCController` (violación del principio DRY y riesgo de desincronización).
  2. Eliminar el collider del NPC hijo (limitaría animaciones o feedback local sobre el personaje).
  3. Delegación jerárquica padre-hijo donde el hijo consulta al componente raíz.
- **Elegida**: 3 (Delegación jerárquica padre-hijo).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 021
- **Título**: Blindaje de Objetos Estáticos contra Movimiento y Footprint Personalizado (`GridObject.playerMovable = false`).
- **Problema**: Los puestos de especialistas (`VendorBuilding`) usaban `GridObject` para ocupar celdas, pero al no poseer `FurnitureSO`, reportaban tamaño 1x1 en lugar de 3x2, y podían ser seleccionados por Long Press o arrastre en Build Mode como si fueran mesas o sillas, desregistrando celdas erróneas.
- **Decisión**: Se extiende `GridObject` con `playerMovable` (por defecto `true`), `overrideSizeX/Y` y el método `SetupStatic(pos, sizeX, sizeY, blocks)`. `BuildManager.StartMovingObject` y `TouchInputManager.HandleLongPress` verifican explícitamente `obj.playerMovable == true && obj.furnitureData != null`. Al destruir o desregistrar un puesto estático, se limpian las 6 celdas exactas sin dejar celdas fantasma.
- **Alternativas consideradas**:
  1. Crear una jerarquía de clases separada (`StaticGridObject`) que obligaría a reescribir `GridManager` y los arrays de ocupación.
  2. Ignorar el problema y confiar en que el jugador no mantenga presionado el puesto.
  3. Incorporar banderas `playerMovable` y overrides de tamaño directamente en `GridObject`.
- **Elegida**: 3 (Extensión modular de `GridObject`).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 022
- **Título**: Máquina de Estados de Retorno Seguro de Platos para Mozos (`WorkerState.ReturningDish` y `WaitingCounterSpace`).
- **Problema**: Si un mozo no podía alcanzar una mesa o el comensal se marchaba, intentaba devolver el plato al mostrador. Si el mostrador de entrega estaba saturado, el mozo pasaba a `Idle` con el plato en mano, quedando congelado para siempre al no poder recibir nuevas tareas.
- **Decisión**: Se introducen los estados `WorkerState.ReturningDish` y `WorkerState.WaitingCounterSpace` junto a la corrutina `ReturnDishRoutine()`. El mozo retiene el plato de forma segura, acude al mostrador y si este está lleno, entra en espera activa y reintenta periódicamente. Cuando se libera un espacio, deposita el plato, limpia la reserva y regresa a su puesto inactivo.
- **Alternativas consideradas**:
  1. Destruir el plato si el mostrador está lleno (pérdida injusta de recursos e ingredientes para el jugador).
  2. Dejar al mozo en Idle bloqueado (bug crítico original).
  3. Espera activa no bloqueante con máquina de estados finita hasta disponibilidad de espacio.
- **Elegida**: 3 (Espera activa no bloqueante con estados explícitos).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 023
- **Título**: Migración Centralizada de Datos de Guardado (SaveData v1 a v2) con Detección Multi-dimensional.
- **Problema**: Partidas antiguas guardadas con `saveVersion = 1` carecían de `hasStartedGame` y `starterItemsGranted`. Si solo se evaluaban nivel y monedas, un jugador con parcelas sembradas o crafteo avanzado podía ser detectado como partida nueva y perder su estado.
- **Decisión**: `SaveData.saveVersion` se eleva a `2`. Se implementa `MigrateSaveIfNeeded(SaveData data)` en `SaveManager.cs`. Se evalúan 14 campos representativos de progreso (nivel, experiencia, inventario, muebles colocados, parcelas de cultivo, misiones activas/completadas, tiendas visitadas, estaciones de crafteo, expansiones desbloqueadas, recetas aprendidas, tutorial, monedas y reputación). Si se detecta avance, se activan las banderas correspondientes y se guarda la versión migrada tanto en cargas primarias como en backups.
- **Alternativas consideradas**:
  1. Forzar wipe o reseteo de guardados al cambiar el esquema (inaceptable para usuarios en fase de pruebas).
  2. Verificación simplificada de solo 4 campos (riesgo de clasificar partidas válidas como inactivas).
  3. Método centralizado idempotente con comprobación exhaustiva de progreso.
- **Elegida**: 3 (Método centralizado idempotente con comprobación exhaustiva).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.2).

---

### DECISIÓN 024
- **Título**: Desacoplamiento de Ciclo de Vida de Mesas Sucias y Reserva Atómica de Platos en Mostrador.
- **Problema**:
  1. Al reciclar comensales mediante `ObjectPoolManager.Instance.Despawn()`, si `CustomerController` invocaba `ClearTable()`, la mesa sucia (`Dirty`) se reiniciaba inmediatamente a `TableState.Available`, cancelando la necesidad de limpieza por parte del camarero y rompiendo el bucle de simulación de restaurante.
  2. Si dos mozos consultaban tareas simultáneamente en `DeliveryCounter`, `TakeNextDish()` extraía el primer plato sin comprobar si `isReserved == true`, provocando que un trabajador tomara el plato que ya estaba reservado para otra mesa.
- **Decisión**:
  1. En `CustomerController.OnReturnToPool()`, se ejecuta exclusivamente `ReleaseTableReference()`. La mesa retiene su estado `TableState.Dirty` y solo el trabajador que ejecute la rutina de limpieza con `FinishCleaning()` puede restaurarla a `Available`. `Table.ClearTable()` cuenta además con guardas de seguridad que impiden restablecer `Available` si la mesa requiere aseo.
  2. En `DeliveryCounter.TakeNextDish()`, se añade validación estricta descartando platos con `isReserved == true`, garantizando exclusión mutua entre trabajadores.
  3. Los muebles se materializan como assets ScriptableObject físicos en `Assets/_Projet/Resources/Furniture/`, y `BuildUI` se alimenta dinámicamente mediante `Resources.LoadAll<FurnitureSO>("Furniture")`, eliminando dependencias de instancias volátiles creadas en memoria.
- **Alternativas consideradas**:
  1. Mantener la limpieza automática de mesa al despawnear comensal (anula el rol de los mozos y el ciclo de servicio).
  2. Recorrer todas las mesas en cada frame con comparaciones dinámicas (costoso en CPU móvil).
  3. Desacoplamiento estricto entre pooling de clientes y ciclo de vida de la mesa con exclusión mutua de platos.
- **Elegida**: 3 (Desacoplamiento estricto con exclusión mutua y persistencia data-driven).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 6.1/6.2).

---

### DECISIÓN 025
- **Título**: Elenco Social Dinámico y Prohibición de Fallback Cruzado a Uniformes de Chef en Clientes.
- **Problema**: Los Friends representan personajes con personalidad que pueden ser seleccionados como Jugador, contratados como Ayudantes o visitar el restaurante como Comensales. Si se permitía que `CustomerController` vistiera atuendos de chef por fallback cuando faltara `normalPreview`, los comensales aparecían vestidos de chefs dentro del comedor, arruinando la narrativa y el aspecto visual.
- **Decisión**:
  1. Un único `CharacterSO` por personaje Friend (en `Assets/_Projet/Art/Characters/Friends/`).
  2. Los roles se definen dinámicamente en runtime: 1 Player, 1 Helper, y el resto Customers.
  3. `CustomerSO` define únicamente el arquetipo de comportamiento (paciencia, propina, bonus XP, etc.).
  4. Los Comensales usan estrictamente `rnormal` + `movimientos_rnormal` con `allowCrossOutfitFallback: false`. Si falta, se registra un `Debug.LogError` y jamás se recurre a atuendos de cocina.
- **Alternativas consideradas**:
  1. Crear ScriptableObjects triplicados (`dafne_player.asset`, `dafne_helper.asset`, `dafne_customer.asset`). Descartada por redundancia masiva y desincronización de identidad.
  2. Permitir fallback a chef si no hay ropa normal. Descartada por violar la regla de vestuario.
  3. Separación data-driven con `CharacterSO` (identidad visual) y asignación estricta de `CharacterOutfit.Normal` sin fallback cruzado.
- **Elegida**: 3 (Separación data-driven con asignación estricta y sin fallback cruzado).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 7).

---

### DECISIÓN 026
- **Título**: Persistencia Incremental de Hitos del Prólogo y Restaurante Inicialmente Cerrado.
- **Problema**:
  1. Si el jugador cerraba el juego a mitad del prólogo (después de escribir su nombre o elegir a su protagonista), debía empezar desde cero al volver a abrir.
  2. Al entrar al restaurante, este iniciaba abierto (`restaurantOpen = true`), impidiendo que el jugador inspeccionara el establecimiento o gestionara sus recursos con calma antes de recibir comensales.
- **Decisión**:
  1. `PrologueController` guarda incrementalmente cada hito en `SaveData` (`playerName`, `selectedPlayerCharacterID`, `selectedChefOutfit`, `prologueStep`). Al reiniciar, `Start()` reanuda directamente en el paso pendiente.
  2. Al completar el prólogo, se asigna `restaurantOpen = false`. El restaurante inicia cerrado y el jugador debe pulsar el botón de apertura conscientemente cuando esté listo.
  3. `CustomerManager.SpawnLoop()` adopta comprobación defensiva: no genera clientes si `RestaurantOperatingManager.Instance == null` o si `IsOpen == false`.
- **Alternativas consideradas**:
  1. Guardar todo únicamente al presionar el último botón del prólogo (riesgo de frustración y abandono si la app se suspende).
  2. Abrir el restaurante inmediatamente (abruma al jugador novato con clientes antes de entender la cocina).
  3. Persistencia incremental con arranque cerrado y apertura voluntaria.
- **Elegida**: 3 (Persistencia incremental con arranque cerrado).
- **Estado**: IMPLEMENTADA Y ACTIVA (Fase 7).



