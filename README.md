# 🍳 Villa del Chef

**Villa del Chef** es un videojuego 2D de gestión de restaurante, agricultura (farming) y construcción de villas en estilo Pixel Art, optimizado tanto para dispositivos móviles (Android/iOS) como PC.

---

## 🌟 Características Principales

- 🌾 **Granja y Cosecha:** Cultiva trigo, tomates, lechugas y otros ingredientes frescos en tus parcelas.
- 👨‍🍳 **Cocina Dinámica:** Prepara recetas deliciosas en hornos, parrillas y fogones con tiempos de cocción e ingredientes dinámicos.
- 🏪 **Gestión de Restaurante:** Atiende a comensales con diferentes personalidades, toma pedidos, sírvelos y recibe propinas en oro y gemas.
- 🔨 **Construcción y Decoración:** Sistema de grilla 2D para colocar mesas, sillas, decoraciones y ampliar tu villa culinaria.
- 🤝 **Trabajadores Automatizados:** Contrata camareros y cocineros para mantener el restaurante funcionando sin parar.
- 📜 **Misiones y Progresión:** Sube de nivel, desbloquea nuevas recetas, muebles y completa misiones diarias.
- 💾 **Persistencia Automática:** Guardado y carga local automático en formato JSON.
- 📱 **Soporte Móvil:** Controles táctiles nativos, soporte para gestos (pinch zoom y arrastre) y adaptación a pantallas con SafeArea (notches).

---

## 🏗️ Arquitectura del Proyecto

El proyecto está diseñado bajo una arquitectura modular y desacoplada mediante eventos en C#:

- **Ruta principal:** Todo el contenido del proyecto se encuentra en [`Assets/_Projet/`](file:///c:/Users/seba5/My%20project/Assets/_Projet).
- **Core:** `GameEvents.cs` conecta la economía, misiones, cocina y clientes sin acoplamiento rígido.
- **Data-Driven:** Recetas, ingredientes, clientes y muebles están modelados con `ScriptableObjects` en [`Assets/_Projet/Resources/`](file:///c:/Users/seba5/My%20project/Assets/_Projet/Resources).
- **Editor Tools:** Herramientas integradas en Unity bajo el menú `Tools > Villa del Chef` para configuración automática de escenas e importación de assets.

Para consultar detalles técnicos completos dirigidos a desarrolladores y agentes de IA, revisa el archivo [`AGENTS.md`](file:///c:/Users/seba5/My%20project/AGENTS.md).

---

## 🎮 Cómo Ejecutar en Unity

1. Abre el proyecto con **Unity 2022.3 LTS** o superior.
2. Abre la escena inicial:
   - [`Assets/_Projet/Scenes/00_Boot.unity`](file:///c:/Users/seba5/My%20project/Assets/_Projet/Scenes/00_Boot.unity)
3. Presiona el botón **Play**. El juego cargará los managers persistentes y te llevará al menú principal.
4. *(Opcional)* También puedes regenerar o reconfigurar las escenas en cualquier momento desde la barra superior:
   `Tools > Villa del Chef > Setup ALL Scenes (Boot, Menu, Restaurant)`.

---

## 🕹️ Controles

- **Móvil:**
  - **Tocar:** Seleccionar estaciones, parcelas, mesas o botones de UI.
  - **Deslizar con 1 dedo:** Mover la cámara por el restaurante.
  - **Pellizcar con 2 dedos:** Zoom in / Zoom out.
- **PC:**
  - **Clic izquierdo:** Interacción.
  - **Clic derecho / Arrastre central:** Desplazar la cámara.
  - **Rueda del ratón:** Zoom in / Zoom out.
