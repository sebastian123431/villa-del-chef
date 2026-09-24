# PROJECT_ALIGNMENT.md — Villa del Chef
## Matriz de Alineación de Sistemas, Fuentes de Verdad y Estado de Implementación

> **Propósito**: Este documento establece el estado verificado de cada subsistema de Villa del Chef contrastando:
> 1. Decisiones explícitas del propietario (Prioridad 1)
> 2. Documento Maestro de Diseño, Modo de Juego y Metodología Técnica (Prioridad 2)
> 3. Game Design Document (GDD) (Prioridad 3)
> 4. Implementación física en código fuente (Prioridad 4)

---

### Estados Permitidos
- `[OK]`: Implementado, verificado físicamente y consistente con el diseño.
- `[PARTIAL]`: Implementado en su núcleo pero con detalles o pasos pendientes.
- `[MISSING]`: Diseñado formalmente pero no implementado en código.
- `[CONFLICT]`: Contradicción entre código y diseño que requiere alineación.
- `[FUTURE]`: Diseño aprobado para una fase futura planificada.
- `[OPEN]`: Decisión o balance aún no confirmado por el propietario.
- `[DEPRECATED]`: Mecánica descartada explícitamente (ej: energía, gemas premium obligatorias).
- `[BROKEN]`: Implementado pero presenta fallos o regresiones activas.

---

## 1. Matriz de Alineación Integral

| Subsistema / Requisito | Fuente de Verdad | Código / Asset Actual | Estado | Problema Identificado | Acción Correctiva / Estado |
| :--- | :--- | :--- | :--- | :--- | :--- |
| **Elenco Social Dinámico (Friends)** | Regla Propietario / GDD / Doc Maestro | `Assets/_Projet/Art/Characters/Friends/` (19 Friends) | `[OK]` | Anteriormente se asumió erróneamente que Friends eran solo personal. | Los 19 Friends forman Player, Helper y Customers. Exclusión mutua implementada en `CustomerManager`. |
| **Outfits por Rol (rnormal / rnchef / rbchef)** | Convención Oficial Propietario / CHARACTERS.md | `CharacterSO.cs`, `CharacterOutfit` | `[OK]` | Riesgo de mezclar ropa casual en servicio o chef en comensales. | Player/Helper usan `rnchef` / `rbchef`. Customers usan estrictamente `rnormal` sin fallback a chef. |
| **Fallback Estricto de Customer Normal** | Doc Maestro / Instrucción Directa | `CharacterSO.GetPreviewSprite()` y `GetAnimator()` | `[CONFLICT]` | Si falta `Normal`, hacía fallback a variantes de chef (`ChefBlack`/`ChefWhite`). | Agregar sobrecarga `allowCrossOutfitFallback = false` para Customers. |
| **Prólogo Reanudable (Resume)** | Doc Maestro / GDD Prólogo | `PrologueController.cs` | `[PARTIAL]` | `Start()` forzaba `ShowStep(1)`, ignorando `prologueStep` y datos previos en SaveData. | Cargar `prologueStep` guardado (1..5), restaurar nombre y selecciones previas al reanudar. |
| **Persistencia Incremental del Prólogo** | Doc Maestro / Requisito Persistencia | `PrologueController.cs` | `[PARTIAL]` | Solo guardaba al pulsar "Entrar al Restaurante" en el paso 5. | Guardar en cada paso completado (`OnSubmitName`, `OnConfirmCharacter`, `OnOutfitChosen`). |
| **Restaurante Inicia Cerrado** | Regla Propietario / Doc Maestro | `PrologueController.cs` (línea 259) | `[CONFLICT]` | Al completar el prólogo forzaba `data.restaurantOpen = true;`. | Corregir a `data.restaurantOpen = false;` para que el jugador abra manualmente el restaurante. |
| **Control de Apertura / Cierre (Operating)** | GDD / Doc Maestro | `RestaurantOperatingManager.cs`, `HUDController.cs` | `[PARTIAL]` | En `CustomerManager`, si el manager era nulo asumía `isOpen = true`. | Cambiar a validación defensiva estricta: `isOpen = Instance != null && Instance.IsOpen`. |
| **Exclusión de Clientes y Rotación de Helper** | Regla Propietario | `CustomerManager.SelectEligibleFriendAppearance()` | `[OK]` | Al rotar Helper (A -> B), A debe regresar al pool y B salir. | Implementado y comprobado en `CustomerManager.cs` y suite de tests. |
| **Comerciantes Oficiales (7 NPCs)** | GDD / Doc Maestro | `Assets/_Projet/Resources/NPC/` (Elena, Bruno, Tomás, Marina, Amelia, Lucas, Sofía) | `[OK]` | Riesgo de reemplazarlos por Friends aleatorios. | Catalogados `[PENDIENTE ARTE NPC OFICIAL]`, puestos físicos en `y=20`, sin mezclar con Friends. |
| **Generación Procedural Detenida** | Instrucción Propietario | `ArtAssetGenerator.cs`, `AssetDatabasePopulator.cs` | `[OK]` | Sobreescritura de arte real y generación de sprites genéricos de 16x24. | Generación de personajes desactivada. Guardián de seguridad `if (File.Exists) return;` implementado. |
| **Ciclo de Mesas y Suciedad (Dirty Tables)** | GDD Actualizado / Doc Maestro | `Table.cs`, `WorkerController.cs`, `CustomerController.cs` | `[OK]` | GDD anterior decía "limpieza pendiente". | Implementado en Fase 6.1: `Available` -> `Reserved` -> `Occupied` -> `WaitingFood` -> `Eating` -> `Dirty` -> `Cleaning` -> `Available`. |
| **Entrega de Platos Atómica (Delivery)** | Doc Maestro | `WorkerController.cs`, `DeliveryCounter.cs`, `CustomerController.cs` | `[OK]` | Posible carrera o duplicación en `PlaceDish`. | Responsabilidad única en `CustomerController.ReceiveDish`. Reserva atómica de plato y mesa sucia. |
| **Customer Parties / Grupos en Mesas** | GDD Sección Grupos | N/A (Actualmente comensales individuales) | `[FUTURE]` | No se deben implementar de golpe en Fase 7 rompiendo el flujo base. | Asignado formalmente a **FASE 7.1 — CUSTOMER PARTIES Y MESAS POR GRUPO**. |
| **Construcción y Zonificación (Grid / Build)** | Doc Maestro / GDD | `BuildManager.cs`, `GridManager.cs`, `FurnitureSO.cs` | `[OK]` | Validación de caminos y protección de tiendas físicas. | Footprints 3x2, zonificación (`Kitchen`, `Dining`, `Exterior`, `Farming`, `Market`, `Crafting`), restauración exacta de transitabilidad con `try/finally`. |
| **Farming Centralizado (Huerto)** | GDD / Doc Maestro | `FarmingManager.cs`, `CropPlot.cs` | `[OK]` | Timestamps UTC y cálculo offline. | Tick central de 1s en `FarmingManager`. Persistencia de estado de crecimiento en JSON. |
| **Crafting de Insumos Intermedios** | GDD / Doc Maestro | `CraftingManager.cs`, `CraftingStation.cs`, `CraftingRecipeSO.cs` | `[OK]` | Distinción plato final vs insumo procesado. | 5 recetas procesadas, cálculo offline UTC y estaciones interactuables en grid. |
| **Economía (Monedas, XP, Reputación)** | GDD / Doc Maestro | `EconomyManager.cs`, `ProgressionManager.cs` | `[OK]` | Riesgo de añadir monedas premium o barras de energía. | Economía limpia: Oro, Experiencia/Nivel (1-50) y Reputación. Sin gemas obligatorias ni energía. |
| **Persistencia Atómica (Save/Load)** | Doc Maestro | `SaveManager.cs`, `SaveData.cs` | `[OK]` | Corrupción en cortes abruptos en móvil. | Versionado `saveVersion = 3`, archivo `.tmp`, backup `.bak`, validación de integridad y migración automática. |
| **Distinción New Game vs Continue** | Doc Maestro | `MainMenuController.cs`, `SaveData.hasStartedGame` | `[OK]` | Continuar activo sin progreso real. | `CanContinueGame()` desacopla existencia de archivo técnico del avance jugable real. |
| **Entrada Táctil Móvil & PC (New Input)** | Doc Maestro | `TouchInputManager.cs`, `CameraController2D.cs` | `[OK]` | Conflictos entre tap, drag y pinch. | Estados mutuamente excluyentes, pinch-to-zoom suave, drag de cámara y ghost preview en build mode. |
| **Optimización Móvil 60 FPS** | Doc Maestro | `ObjectPoolManager.cs`, `SpriteAtlasSetupEditor.cs` | `[OK]` | Draw calls altos por sprites individuales. | Atlases V2 configurados. `Friends/` excluido explícitamente para proteger hojas 4x16. |
| **Suite de Tests de Elenco Social** | Doc Maestro / Protocolo QA | `SocialCastIntegrationTest.cs` | `[PARTIAL]` | Hardcode de 19 personajes fijos y mutación directa de SaveData en producción. | Generalizar conteo contra assets cargados y aislar SaveData con snapshot/restauración. |
| **Catálogo de 50 Recetas Chilenas** | GDD Catálogo Gastronómico | N/A (12 recetas base actuales) | `[FUTURE]` | No implementar de golpe sin insumos, balance ni estaciones. | Asignado formalmente a **FASE 8 — RECETAS, DOMINIO Y MILAGROS** (subfase 8A en lotes temáticos). |
| **Dominio de Recetas (Mastery)** | GDD | N/A | `[FUTURE]` | Niveles de preparación por plato con bonos progresivos. | Asignado a **FASE 8**. |
| **Milagros del Chef (Miracles)** | GDD | N/A | `[FUTURE]` | Habilidades activas (no consumibles premium). | Asignado a **FASE 8**. |
| **Reloj de Restaurante y Franjas Horarias** | GDD | N/A | `[FUTURE]` | Desayuno, Almuerzo, Once, Cena con demanda dinámica. | Asignado a **FASE 9 — HORARIOS Y EVENTOS ESTACIONALES**. |
| **Eventos Estacionales & Calendario** | GDD | N/A | `[FUTURE]` | Fiestas Patrias, Halloween, Navidad, cumpleaños. | Asignado a **FASE 9**. |
| **Multiplayer Cooperativo / Versus** | GDD | N/A | `[FUTURE]` | Red local / Online Chef vs Chef. | Asignado a **FASE 10 — MULTIPLAYER & FUNCIONES SOCIALES**. |

---

## 2. Acciones Inmediatas de Corrección y Cierre (Fase 7)

1. **`CharacterSO.cs`**:
   - Sobrecarga de `GetPreviewSprite(outfit, allowCrossOutfitFallback)` y `GetAnimator(outfit, allowCrossOutfitFallback)`.
   - Garantizar que para `CharacterOutfit.Normal` cuando `allowCrossOutfitFallback == false`, **nunca** retorne trajes de chef.
2. **`PrologueController.cs`**:
   - Reanudar en el paso exacto (`ShowStep(save.prologueStep)`) si `prologueStep > 1` y `!prologueCompleted`.
   - Persistir hitos en cada paso (`OnSubmitName` guarda paso 2; `OnConfirmCharacter` guarda paso 4; `OnOutfitChosen` guarda paso 5).
   - En `OnEnterRestaurant`, fijar `data.restaurantOpen = false;` (el restaurante debe iniciar CERRADO tras el prólogo).
3. **`CustomerManager.cs`**:
   - Validar defensivamente `isOpen = RestaurantOperatingManager.Instance != null && RestaurantOperatingManager.Instance.IsOpen;`.
   - Usar `allowCrossOutfitFallback = false` al configurar la apariencia normal del comensal.
4. **`SocialCastIntegrationTest.cs`**:
   - Aislar el SaveData: crear snapshot antes de la prueba y restaurarlo exactamente al finalizar.
   - Generalizar el conteo de personajes: validar que existan al menos 2 Friends y comparar el pool dinámicamente sin hardcodear el número 19.
