# AGENTS.md — Villa del Chef

> **Contexto para Agentes de IA**: Este documento es la guía técnica oficial para cualquier agente (Antigravity, etc.) o desarrollador que trabaje en el repositorio **Villa del Chef**. Contiene la arquitectura del proyecto, la relación entre subsistemas, convenciones de código, flujos de trabajo en Unity y estado actual del desarrollo.

---

## 1. Resumen del Proyecto

- **Nombre**: Villa del Chef
- **Género**: Simulación de Gestión de Restaurante + Farming (Granja) + Construcción (Tycoon / Cozy 2D)
- **Estilo Visual**: Pixel Art 2D (PPU = 16, FilterMode = Point, No Compression)
- **Plataforma Objetivo**: Mobile (Android / iOS) y PC (Standalone)
- **Motor**: Unity 2022.3+ (o superior) con 2D Template
- **Repositorio**: `sebastian123431/villa-del-chef`

---

## 2. Reglas Críticas del Proyecto (IMPORTANTE)

1. **Git & LFS**:
   - **NO** usar Git LFS para imágenes (`.png`), audios (`.wav`, `.mp3`) ni assets estándar del proyecto.
   - La cuenta de GitHub tiene límite de cuota LFS alcanzado.
   - Todos los assets visuales son pixel art ligero (~6 MB en total), por lo que Git estándar los maneja perfectamente sin superar el límite de 100 MB por archivo.
2. **Estructura de Carpetas**:
   - Todo el código, assets y escenas de nuestro juego residen estrictamente dentro de **`Assets/_Projet/`**.
   - No crear scripts ni carpetas huérfanas en la raíz de `Assets/`.
   - La carpeta `Assets/_Drop/` es una zona de intercambio para nuevos packs de sprites descargados.
3. **Manejo de Pixel Art**:
   - Todo Sprite importado debe tener: `TextureType = Sprite (2D and UI)`, `SpriteMode = Single` o `Multiple`, `Pixels Per Unit = 16`, `Filter Mode = Point (no filter)`, `Compression = None`.
   - La herramienta automatizada `PixelArtAssetPostprocessor.cs` aplica esto automáticamente al importar.

---

## 3. Estructura de Directorios (`Assets/_Projet`)

```text
Assets/_Projet/
├── Art/
│   ├── Characters/         # Sprites de Jugador, Clientes, Trabajadores, NPCs
│   ├── Construction/       # Paredes, Suelos, Puertas, Ventanas
│   ├── Environment/        # Tilesets de terreno, césped, caminos
│   ├── Exterior/           # Vallas, huertos, cultivos
│   ├── Food/               # Ingredientes crudos y platos preparados
│   ├── Furniture/          # Mesas, Sillas, Mostradores, Estaciones de cocina
│   ├── UI/                 # Iconos, botones, paneles, fondos
│   └── _Unused_Library/    # Banco de iconos y assets listos para clasificar
├── Audio/
│   ├── Music/              # Música de fondo ambiental / cozy
│   └── SFX/                # Sonidos de monedas, cocción, level up, etc.
├── Prefabs/
│   ├── Characters/         # Prefabs de Clientes y Trabajadores con IA
│   ├── Construction/       # Prefabs de muros y suelos
│   ├── Crops/              # Prefabs de parcelas de cultivo
│   ├── Furniture/          # Prefabs de mesas, sillas, delivery counter
│   ├── Kitchen/            # Prefabs de estaciones de cocina (horno, parrilla, etc.)
│   └── UI/                 # Modales, HUDs, flotantes
├── Resources/              # ScriptableObjects cargables en tiempo de ejecución
│   ├── Crops/
│   ├── Furniture/
│   ├── Ingredients/
│   ├── Quests/
│   ├── Recipes/
│   └── Stations/
├── Scenes/
│   ├── 00_Boot.unity       # Inicializa managers persistentes y salta a MainMenu
│   ├── 01_MainMenu.unity   # Título, botón jugar, opciones
│   └── 02_Restaurant.unity # Escena principal del juego (Gameplay loop)
├── ScriptableObjects/      # Definiciones de datos (Data-Driven Architecture)
└── Scripts/                # Código fuente C#
    ├── Building/           # Sistema de cuadrícula (Grid) y modo construcción
    ├── Cooking/            # Lógica de estaciones de cocina y platos
    ├── Core/               # GameEvents, GameManager, Bootstrap, Boot
    │   └── Editor/         # Herramientas y generadores de Unity Editor
    ├── Customers/          # Máquina de estados de clientes
    ├── Economy/            # Monedas, gemas y mercado de compra/venta
    ├── Farming/            # Parcelas, siembra, crecimiento, cosecha
    ├── Input/              # Touch móvil, gestos de cámara (pinch/pan)
    ├── Inventory/          # Gestión de inventario e ítems
    ├── Managers/           # Audio, Misiones, Recetas, Tutorial, Trabajadores
    ├── Progression/        # Niveles del restaurante, experiencia y desbloqueos
    ├── Restaurant/         # Mesas, sillas y mostradores
    ├── Save/               # Sistema de persistencia en JSON
    ├── ScriptableObjects/  # Clases de definición SO (RecipeSO, CropSO, etc.)
    ├── UI/                 # Controladores de interfaz y Safe Area
    ├── Utilities/          # Pathfinding en grilla (A*)
    └── Workers/            # Controladores de IA de empleados
```

---

## 4. Arquitectura y Patrones de Software

### 4.1. Desacoplamiento por Eventos (`GameEvents.cs`)
En lugar de acoplar referencias directas entre scripts, los sistemas emiten y escuchan eventos estáticos de C#:
- `OnGoldChanged(int newGold)`, `OnGemsChanged(int newGems)`
- `OnDishPrepared(DishInstance dish)`, `OnDishDelivered(DishInstance dish)`
- `OnCropHarvested(CropSO crop, int amount)`
- `OnCustomerServed(CustomerController customer)`
- `OnLevelUp(int newLevel)`
- `OnQuestCompleted(QuestSO quest)`

### 4.2. Inicialización y Bootstrap (`RestaurantBootstrap.cs`)
Si una escena de gameplay se ejecuta sola en el editor sin pasar por `00_Boot`:
- `RestaurantBootstrap.cs` detecta los managers faltantes (`SaveManager`, `GridManager`, `BuildManager`, `EconomyManager`, `FarmingManager`, `CustomerManager`, `WorkerManager`, `RecipeManager`, `InventoryManager`, etc.) y los instancia dinámicamente con configuración MVP funcional por defecto.

### 4.3. Persistencia de Datos (`SaveManager.cs` y `SaveData.cs`)
- Almacenamiento local mediante serialización JSON en:
  `Application.persistentDataPath/villadelchef_save.json`
- Guarda: oro, gemas, nivel, XP, inventario de ingredientes y platos, estado de parcelas (tipo de cultivo, tiempo restante), objetos colocados en la grilla y misiones completadas.
- Soporta guardado automático periódico y guardado en `OnApplicationPause`/`OnApplicationQuit`.

### 4.4. Entrada y Soporte Móvil (`TouchInputManager.cs` y `CameraController2D.cs`)
- Diseñado para pantallas táctiles móviles con soporte retroactivo para ratón en PC.
- Arrastre con un dedo/clic para paneo de cámara.
- Gesto de pellizco (Pinch-to-zoom) con dos dedos o rueda del ratón para zoom orthographic con límites suaves.
- `SafeAreaFitter.cs` ajusta los Canvas a notches e islas dinámicas en dispositivos iOS y Android.

### 4.5. Elenco Social Dinámico (Friends)
- Todos los personajes de `Assets/_Projet/Art/Characters/Friends/` forman el elenco social dinámico del juego.
- En cada partida:
  - 1 Friend es elegido como **Protagonista (Player)** (Uniforme de chef negro o blanco).
  - 1 Friend es contratado como **Ayudante (Helper/Worker)** (Uniforme de chef negro o blanco).
  - **Todos los Friends restantes** forman el pool principal de **Clientes** que visitan el restaurante como comensales con ropa normal casual (`rnormal` + `movimientos_rnormal`).
- **Regla de Exclusión Dinámica**: El Player y el Helper activo están excluidos del pool de clientes. Si el helper cambia, el helper anterior vuelve a ser cliente elegible y el nuevo pasa a ser helper.
- **Separación de Responsabilidades**: `CharacterSO` define la apariencia e identidad visual. `CustomerSO` define el comportamiento de juego (arquetipo, paciencia, propina, reputación, XP).

---

## 5. Herramientas del Editor (Menú Superior en Unity)

Dentro de la barra de menú de Unity, bajo **`Tools > Villa del Chef/`**:

1. **`Setup ALL Scenes (Boot, Menu, Restaurant)`**:
   - Genera automáticamente los assets pixel art procedimentales.
   - Configura los ScriptableObjects en `Assets/_Projet/Resources`.
   - Construye y configura las escenas con toda la jerarquía de cámaras, canvas y managers.
2. **`Import & Organize Assets from _Drop`**:
   - Descomprime archivos `.zip` en `Assets/_Drop/` y clasifica imágenes en las carpetas de arte adecuadas según palabras clave.
3. **`Populate ScriptableObjects Database`**:
   - Crea o actualiza todos los ScriptableObjects (recetas, ingredientes, cultivos, muebles, etc.) con balance de precios y tiempos.

---

## 6. Estado Actual de Implementación

| Sistema | Estado | Detalles |
| :--- | :--- | :--- |
| **Núcleo & Escenas** | ✅ Completo | 3 escenas operativas (`00_Boot`, `01_MainMenu`, `02_Restaurant`). |
| **Economía & Monedas** | ✅ Completo | Monedas de oro, gemas, mercado de compras y venta (`EconomyManager`, `MarketUI`). |
| **Cocina (Cooking)** | ✅ Completo | Estaciones de cocina configurables, temporizadores, verificación de ingredientes, output de platos (`CookingStation`, `DishInstance`, `CookStationUI`). |
| **Granja (Farming)** | ✅ Completo | Siembra, etapas visuales de maduración, recolección al inventario (`CropPlot`, `FarmingManager`). |
| **Clientes (Customers)** | ✅ Completo | Spawn periódico, búsqueda de mesa/silla libre, pedido, consumo, propina y retirada (`CustomerController`, `CustomerManager`). |
| **Trabajadores (Workers)** | ✅ Completo | Empleados contratables que transportan platos y asisten en tareas (`WorkerController`, `WorkerManager`). |
| **Construcción & Grid** | ✅ Completo | Cuadrícula 2D, colocación de muebles, validación de ocupación (`GridManager`, `BuildManager`, `BuildUI`). |
| **Progresión & Misiones**| ✅ Completo | Niveles 1-50, curva de XP, misiones principales y secundarias (`ProgressionManager`, `QuestManager`, `QuestUI`). |
| **Tutorial Onboarding** | ✅ Completo | Pasos guiados interactivos para nuevos jugadores (`TutorialManager`, `TutorialUI`). |
| **Guardado (Save/Load)** | ✅ Completo | Guardado y carga robusta en JSON con fallback de datos (`SaveManager`, `SaveData`). |

---

## 7. Próximos Pasos Recomendados para Futuros Agentes

1. **Expansión de Arte y Animaciones**:
   - Reemplazar sprites generados procedimentalmente con hojas de sprites animadas en `Assets/_Projet/Art/Animations/`.
2. **Audio & Música**:
   - Añadir más pistas de audio ambientales y efectos de sonido en `Assets/_Projet/Audio/`.
3. **Mini-juegos de Cocina**:
   - Implementar QTEs (Quick Time Events) táctiles opcionales al cocinar para obtener platos con calidad "Estrella Dorada".
4. **Optimización Móvil**:
   - Probar build en Android (APK) y verificar rendimiento a 60 FPS con Sprite Atlases (`.spriteatlasv2`).
