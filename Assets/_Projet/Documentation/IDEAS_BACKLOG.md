# IDEAS_BACKLOG.md — Villa del Chef

Registro de ideas, propuestas y posibles mejoras que surgen durante el desarrollo pero que NO forman parte del alcance de la fase activa inmediata.

---

### IDEA #001
- **Nombre**: Mercader Ambulante Misterioso
- **Descripción**: Un NPC nómada que aparece únicamente en horarios específicos del día o días de la semana con un carromato en las afueras de la villa. Vende semillas exóticas, recetas de edición limitada y decoraciones temáticas.
- **Beneficio**: Aumenta la retención diaria y ofrece eventos sorpresa para el jugador.
- **Complejidad**: MEDIA.
- **Prioridad**: FUTURO.
- **Dependencias**: Sistema de VendorSO y ciclo horario en `SaveManager`.
- **Estado**: BACKLOG.

---

### IDEA #002
- **Nombre**: Especial del Chef (Plato del Día)
- **Descripción**: El jugador puede designar un plato desbloqueado como el "Especial del Chef" cada día del juego. Los clientes tienen un 40% más de probabilidad de pedirlo y otorga un bono del +15% de propina y reputación.
- **Beneficio**: Estimula al jugador a alternar recetas y planificar su cultivo/crafting según el especial activo.
- **Complejidad**: BAJA.
- **Prioridad**: MEJORA.
- **Dependencias**: `RecipeManager`, `CustomerController`.
- **Estado**: BACKLOG.

---

### IDEA #003
- **Nombre**: Minijuego QTE táctil opcional de cocina ("Calidad Estrella Dorada")
- **Descripción**: Al cocinar en una estación, el jugador puede tocar la pantalla en el momento justo (círculo de sincronización) para otorgar al plato una calidad "Estrella Dorada", vendiéndose a 1.5x de precio. Si no lo toca, el plato se cocina normalmente al 100% de calidad base (sin penalización).
- **Beneficio**: Añade dinamismo táctil muy satisfactorio para sesiones activas sin perjudicar a los jugadores casuales.
- **Complejidad**: MEDIA.
- **Prioridad**: MEJORA / UX.
---

### IDEA #004
- **Nombre**: Pool de Tarjetas Reutilizables en VendorUI
- **Descripción**: En lugar de destruir e instanciar tarjetas UI para cada producto al abrir la tienda de un comerciante, implementar un pool o reciclador de 6 a 8 tarjetas UI fijas para minimizar asignaciones de memoria en móviles.
- **Beneficio**: Garantiza 0 bytes de recolección de basura (GC) al abrir y navegar los menús comerciales.
- **Complejidad**: BAJA.
- **Clasificación**: OPTIMIZACIÓN.
- **Dependencias**: `VendorUI.cs`.
- **Estado**: BACKLOG.

---

### IDEA #005
- **Nombre**: Animaciones de Respiración e Interacción para Especialistas
- **Descripción**: Incorporar animaciones sutiles de respiración ("idle breathe") y reacción de saludo visual cuando el jugador toca a un comerciante antes de desplegar el modal.
- **Beneficio**: Aumenta enormemente la calidez y el estilo cozy de la villa chilena en el Valle del Elqui.
- **Complejidad**: MEDIA.
- **Clasificación**: CONTENIDO / UX.
- **Dependencias**: `NPCSO.cs`, `VendorBuilding.cs`, `NPCController.cs`.
- **Estado**: BACKLOG.

---

### IDEA #006
- **Nombre**: Destello Sutil en Mesa Reservada al Ingreso del Comensal
- **Descripción**: Cuando un cliente entra al restaurante y reserva una mesa disponible, proyectar un destello sutil de color dorado o contorno blanco sobre la mesa durante 1 segundo.
- **Beneficio**: Claridad visual instantánea para el jugador al monitorear el flujo del salón.
- **Complejidad**: BAJA.
- **Clasificación**: UX.
- **Dependencias**: `CustomerController.cs`, `Table.cs`.
- **Estado**: BACKLOG.

