# Convención Oficial de Personajes — Villa del Chef

Este documento establece la convención estricta y obligatoria para todos los assets de personajes ubicados en `Assets/_Projet/Art/Characters/Friends/`.

---

## 1. Nomenclatura Oficial de Vestuarios (Outfits)

| Clave | Significado Oficial | Descripción |
| :--- | :--- | :--- |
| **`rnormal`** | Ropa Normal | Vestimenta casual de calle para prólogo, exploración y vida diaria. |
| **`rnchef`** | Ropa Negra de Chef | Uniforme gastronómico negro de alta cocina para trabajo en restaurante. |
| **`rbchef`** | Ropa Blanca de Chef | Uniforme gastronómico blanco clásico para trabajo en restaurante. |

> [!IMPORTANT]
> - `rnchef` = Ropa **NEGRA** de Chef.
> - `rbchef` = Ropa **BLANCA** de Chef.
> - Nunca alterar, renombrar ni reinterpretar estas claves.

---

## 2. Tipos de Archivos y Emparejamiento Obligatorio

Cada personaje se compone de pares exactos entre **preview estático** y **spritesheet animada**:

1. **Ropa Normal**:
   - Preview: `<personaje>_rnormal.png`
   - Spritesheet: `movimientos_rnormal.png`
2. **Uniforme Negro de Chef**:
   - Preview: `<personaje>_rnchef.png`
   - Spritesheet: `movimientos_rnchef.png`
3. **Uniforme Blanco de Chef**:
   - Preview: `<personaje>_rbchef.png`
   - Spritesheet: `movimientos_rbchef.png`

> [!WARNING]
> NUNCA mezclar variantes (por ejemplo, preview de `rbchef` con spritesheet de `movimientos_rnchef`). Cada variante debe usar estrictamente su propio par.

---

## 3. Formato de Spritesheets de Movimiento

- **Estructura**: 4 columnas × 16 filas = 64 frames.
- **Cálculo de celda**:
  - `cellWidth = texture.width / 4`
  - `cellHeight = texture.height / 16`
  - Requiere: `texture.width % 4 == 0` y `texture.height % 16 == 0`.
- **Mapeo de Filas (4 frames por fila)**:
  - Fila 1: Idle Down
  - Fila 2: Walk Down
  - Fila 3: Idle Up
  - Fila 4: Walk Up
  - Fila 5: Idle Left
  - Fila 6: Walk Left
  - Fila 7: Idle Right
  - Fila 8: Walk Right
  - Fila 9: Cook Down
  - Fila 10: Cook Up
  - Fila 11: Cook Left
  - Fila 12: Cook Right
  - Fila 13: Think / Wait
  - Fila 14: Pickup
  - Fila 15: Carry / Serve
  - Fila 16: Celebrate

---

## 4. Reglas de Conservación de Arte Original

1. **Intocabilidad de los PNG**: Los archivos PNG en esta carpeta son fuentes de arte originales proporcionadas por el propietario.
   - NO editar, escalar, recortar, recolorear ni sobrescribir los PNG.
   - Unity genera `.meta`, `AnimationClip`, `AnimatorController`, prefabs y `ScriptableObject`, pero los PNG físicos se preservan 100% intactos.
2. **Identidad de Personajes**:
   - Nombres con espacios, ciudades o apodos son intencionales (ej. `diego serena` y `diego_vallenar` son personas distintas).
   - NO fusionar carpetas ni renombrar directorios físicos.
3. **Casos Especiales Registrados**:
   - `andres_arica/andres_rbnormal.png`: Nomenclatura no estándar (`rbnormal`). Se mantiene intacto sin asumir `rbchef`.
   - `juan/movimientos.png`: Movimientos sin clave explícita de outfit. Se mantiene intacto como ambiguo hasta confirmación.
