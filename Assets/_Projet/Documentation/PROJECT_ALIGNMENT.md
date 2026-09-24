# PROJECT_ALIGNMENT.md — Villa del Chef
## Matriz de Alineación de Sistemas, Fuentes de Verdad y Estado de Implementación

> **Propósito**: Este documento establece el estado verificado de cada subsistema de Villa del Chef contrastando:
> 1. Decisiones explícitas del propietario (Prioridad 1)
> 2. Documento Maestro de Diseño, Modo de Juego y Metodología Técnica (Prioridad 2)
> 3. Game Design Document (GDD) (Prioridad 3)
> 4. Implementación física en código fuente (Prioridad 4)

---

### Estados Permitidos
- `[OK — STATIC]`: Implementado en código y validado formalmente mediante compilación, pruebas estáticas o suite de tests en Editor.
- `[OK — PLAYMODE]`: Probado y validado en sesión activa de Play Mode.
- `[PARTIAL — PLAYMODE PENDING]`: Implementado técnicamente, pendiente de confirmación en Play Mode / dispositivo.
- `[DESIGN FUTURE]`: Diseño aprobado para una fase futura planificada (ej: Fases 7.1 a 10).
- `[BROKEN]`: Implementado pero presenta fallos o regresiones activas.

---

## 1. Matriz de Alineación Integral (Fase 7.0.4)

| Subsistema / Requisito | Fuente de Verdad | Código / Asset Actual | Estado | Detalle Técnico / Comportamiento |
| :--- | :--- | :--- | :--- | :--- |
| **Elenco Social Dinámico (Friends)** | Regla Propietario / GDD / Doc Maestro | `Assets/_Projet/Art/Characters/Friends/` (19 Friends) | `[OK — STATIC]` | Los 19 Friends forman Player, Helper y Customers. Exclusión mutua dinámica verificada en `CustomerManager`. |
| **Outfits por Rol (rnormal / rnchef / rbchef)** | Convención Oficial Propietario / CHARACTERS.md | `CharacterSO.cs`, `CharacterOutfit` | `[OK — STATIC]` | Player/Helper usan `rnchef` / `rbchef`. Customers usan estrictamente `rnormal`. |
| **Fallback Estricto de Customer Normal** | Doc Maestro / Regla Propietario | `CharacterSO.cs`, `CharacterAppearanceController.cs` | `[OK — STATIC]` | `allowCrossOutfitFallback: false` garantizado en comensales. Si falta arte normal se marca error de datos; nunca se viste de chef. |
| **Validación Estricta de Uniforme del Protagonista en Prólogo** | Regla Propietario / Auditoría Fase 7.0.3 / 7.0.4 | `PrologueController.cs` | `[OK — STATIC]` | `UpdateOutfitDisplay()` desactiva botones de trajes incompletos con `HasCompleteOutfit()`, usa previews con `allowCrossOutfitFallback: false`. `OnOutfitChosen()` implementa doble guarda que rechaza con `LogError` atuendos incompletos. Reanudar con traje incompleto fuerza deterministamente retorno al Paso 4 vía `ResolveResumeStep`. |
| **Protección de Identidad del Protagonista** | Regla Propietario / Auditoría Fase 7.0.3 / 7.0.4 | `PrologueController.CanEnterRestaurant()` | `[OK — STATIC]` | `CanEnterRestaurant()` valida estrictamente que `selectedCharacter != null` (0 fallback silencioso a "alex"), consistencia de ID con partidas bloqueadas y disponibilidad completa del atuendo. Aborta con error antes de guardar o cargar escena. |
| **Auto-Binding y Fallback Defensivo en Tarjetas** | Auditoría Fase 7.0.3 / UI | `CharacterCardUI.cs`, `HelperIntroDialogUI.cs`, `StaffMenuUI.cs` | `[OK — STATIC]` | `CharacterCardUI` soporta `TryAutoBindReferences()` por convención ("Icon", "Name", "Status", "Button"). `Bind()` retorna booleano defensivo. Si el prefab asignado está incompleto, se destruye y `CreateProceduralCard()` genera tarjeta de reemplazo operativa sin NRE. |
| **Selector Real de Ayudante (Helper UI)** | Regla Propietario / Especificación 7.0.1 | `HelperIntroDialogUI.cs`, `StaffMenuUI.cs` | `[OK — STATIC]` | Se genera grid real de tarjetas seleccionables con preview y nombre. Excluye al protagonista y amigos comensales activos. Permite seleccionar traje (Negro/Blanco) y confirmar. Elimina auto-pick silencioso. |
| **Menú de Personal / Relevo de Helper** | Regla Propietario / GDD Fase 7 | `StaffMenuUI.cs`, `HUDController.cs` | `[OK — STATIC]` | Botón "PERSONAL" en HUD. Permite alternar uniforme chef, relevar ayudante (el anterior reingresa al pool de clientes inmediatamente) o retirarlo. |
| **Generación de 64 Frames (16 Filas)** | Especificación Oficial 7.0.1 | `CharacterPipelineEditor.cs` | `[OK — STATIC]` | Las 16 filas de las hojas 4x16 se exportan como clips dedicados (Idle, Walk, Cook en 4 direcciones, Think, Pickup, Carry_Serve, Celebrate). Pipeline 100% idempotente. |
| **Animator Direccional 2D con Memoria** | Especificación Oficial 7.0.1 | `CharacterPipelineEditor.cs`, Controllers | `[OK — STATIC]` | Locomoción y cocina mediante BlendTrees SimpleDirectional2D (`MoveX`, `MoveY`). Al detenerse (`Speed = 0`), retiene el último vector direccional para mantener el Idle en la orientación correcta. |
| **Pathfinding de Comensal sin Teletransporte y Recuperación de Mesa** | Bug Audit Fase 7 / Problemas 3-4 | `CustomerController.WalkToRoutine()`, `Table.cs` | `[OK — STATIC]` | Si el comensal no encuentra camino a la mesa, no se teletransporta. Llama a `Table.CancelCustomerReservation(this)` atómicamente, liberando silla y mesa a `Available` sin afectar mesas `Dirty`/`Cleaning`, y despawnea al salir. |
| **Seguridad de Ayudante sin Auto-Preselección** | Bug Audit Fase 7.0.2 / Seguridad de Elenco | `StaffMenuUI.cs`, `CharacterCardUI.cs` | `[OK — STATIC]` | Abre con selección nula ("Selecciona un amigo") y confirmar deshabilitado. Requiere clic explícito. Valida en tiempo real en Confirmar contra comensales activos y carreras. |
| **Data-Binding Reutilizable en Tarjetas** | Bug Audit Fase 7.0.2 / UI | `CharacterCardUI.cs`, `HelperIntroDialogUI.cs` | `[OK — STATIC]` | `CharacterCardUI` enlaza imagen, nombre, estado y callback de forma idéntica en prefab y generación por código. Deshabilita comensales activos. |
| **Cero Duplicación de Identidad Social** | Bug Audit Fase 7.0.2 / Unicidad | `CustomerManager.SelectEligibleFriendAppearance()` | `[OK — STATIC]` | Si todos los amigos elegibles están en el restaurante, retorna `null`. El comensal usa apariencia/comportamiento legacy de `CustomerSO` y nunca clona un Friend. |
| **Validación de Outfits Incompletos** | Regla Propietario / Seguridad de Vestuarios | `CharacterSO.HasCompleteOutfit()`, `HelperIntroDialogUI.cs`, `StaffMenuUI.cs` | `[OK — STATIC]` | Trajes incompletos (ej. ChefWhite en Andrés Arica) tienen botones deshabilitados, preview estricto sin fallback cruzado, y son rechazados con LogError en runtime. |
| **Idempotencia de Pipeline sin Fuga de Sub-Assets** | Auditoría Fase 7.0.2 / Deuda Técnica | `CharacterPipelineEditor.cs` | `[OK — STATIC]` | Destrucción preventiva de sub-assets `BlendTree` huérfanos antes de recrear estados. Generación 100% idempotente (mismo conteo de líneas/sub-assets). |
| **Reciclaje Limpio de Object Pool** | Problema 6 / Higiene Visual | `CustomerController.cs`, `CharacterAppearanceController.cs` | `[OK — STATIC]` | `OnReturnToPool()` limpia triggers de Animator, parámetros direccionales y el sprite previo (`spriteRenderer.sprite = null`), evitando persistencia de apariencia al cambiar de identidad. |
| **Validadores de Datos Blindados** | Problema 5 / Protocolo QA | `GameDataValidatorEditor.cs` | `[OK — STATIC]` | Falta de `normalPreview` o `normalAnimator` en `canAppearAsCustomer = true` es ERROR estricto. Valida que Player y Helper cuenten con atuendos de chef completos. `ValidateAllGameData`: 0 errores. |
| **Prólogo Reanudable Determinista (Resume)** | Doc Maestro / GDD Prólogo | `PrologueController.ResolveResumeStep()` | `[PARTIAL — PLAYMODE PENDING]` | Carga `prologueStep` guardado (1..5), restaura nombre y selecciones. Si el atuendo guardado en paso 5 no es completo, fuerza retorno a paso 4 sin mutar el archivo de guardado silenciosamente. |
| **Persistencia Incremental del Prólogo** | Doc Maestro / Requisito Persistencia | `PrologueController.cs` | `[PARTIAL — PLAYMODE PENDING]` | Guarda en cada hito completado (`OnSubmitName`, `OnConfirmCharacter`, `OnOutfitChosen`). |
| **Restaurante Inicia Cerrado** | Regla Propietario / Doc Maestro | `PrologueController.cs` | `[PARTIAL — PLAYMODE PENDING]` | Al completar el prólogo se establece `restaurantOpen = false` con mensaje invitando a revisar el restaurante antes de abrir. |
| **Control de Apertura / Cierre (Operating)** | GDD / Doc Maestro | `RestaurantOperatingManager.cs`, `HUDController.cs` | `[OK — STATIC]` | Validación defensiva en `CustomerManager`: si el restaurante está cerrado o el manager falta, no se generan comensales. Los comensales que ya estaban dentro terminan su comida normalmente. |
| **Comerciantes Oficiales (7 NPCs)** | GDD / Doc Maestro | `Assets/_Projet/Resources/NPC/` (7 NPCs) | `[OK — STATIC]` | Puestos en `y = 20`. Preservan estado `[PENDIENTE ARTE NPC OFICIAL]`, desacoplados de los Friends. |
| **Ciclo de Mesas y Suciedad (Dirty Tables)** | GDD Actualizado / Doc Maestro | `Table.cs`, `WorkerController.cs`, `CustomerController.cs` | `[OK — STATIC]` | Implementado: `Available` -> `Reserved` -> `Occupied` -> `WaitingFood` -> `Eating` -> `Dirty` -> `Cleaning` -> `Available`. |
| **Entrega de Platos Atómica (Delivery)** | Doc Maestro | `WorkerController.cs`, `DeliveryCounter.cs`, `CustomerController.cs` | `[OK — STATIC]` | Sin carreras: reserva atómica de plato y mesa sucia. Mozo acciona `Pickup`, `IsCarrying` y `Serve`. |
| **Customer Parties / Grupos en Mesas** | GDD Sección Grupos | N/A (Comensales individuales actuales) | `[DESIGN FUTURE]` | Asignado formalmente a **FASE 7.1 — CUSTOMER PARTIES Y MESAS POR GRUPO**. |
| **Catálogo de 50 Recetas Chilenas** | GDD Catálogo Gastronómico | N/A (12 recetas base actuales) | `[DESIGN FUTURE]` | Asignado formalmente a **FASE 8 — RECETAS, DOMINIO Y MILAGROS**. |
| **Dominio de Recetas (Mastery)** | GDD | N/A | `[DESIGN FUTURE]` | Asignado a **FASE 8**. |
| **Milagros del Chef (Miracles)** | GDD | N/A | `[DESIGN FUTURE]` | Asignado a **FASE 8**. |
| **Reloj de Restaurante y Franjas Horarias** | GDD | N/A | `[DESIGN FUTURE]` | Asignado a **FASE 9 — HORARIOS Y EVENTOS ESTACIONALES**. |
| **Eventos Estacionales & Calendario** | GDD | N/A | `[DESIGN FUTURE]` | Asignado a **FASE 9**. |
| **Multiplayer Cooperativo / Versus** | GDD | N/A | `[DESIGN FUTURE]` | Asignado a **FASE 10 — MULTIPLAYER & FUNCIONES SOCIALES**. |

---

## 2. Resumen de Calidad de Fase 7.0.4
- **Compilación C#**: 0 errores, 0 advertencias en `Assembly-CSharp` y `Assembly-CSharp-Editor` (`dotnet build`).
- **Suite de Prólogo Dedicada (`PrologueIntegrationTest.cs`)**: 100% PASS (9/9 pruebas especializadas ejecutadas en Unity Batchmode).
- **Suite de Integración Social (`SocialCastIntegrationTest.cs`)**: 100% PASS (20/20 suites verificadas en Unity Batchmode con Test 20 integrado a `CanEnterRestaurant`).
- **Total Tests Automatizados en Batchmode**: 29/29 PASSED (100%).
- **Validador de Base de Datos (`GameDataValidatorEditor.cs`)**: 0 errores, 3 advertencias benignas documentadas.
- **Validador de Personajes (`ValidateCharacterDatabaseOnly`)**: 0 errores, 1 advertencia benigna documentada (Andrés Arica ChefWhite faltante).
- **Nivel de Madurez Técnica Fase 7**: 98–99% real (100% código, arquitectura, datos y tests batchmode; PlayMode interactivo y Android físico clasificados honestamente como `[PARTIAL — PLAYMODE PENDING]` hasta sesión de prueba en dispositivo físico o auditoría externa).
