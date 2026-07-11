# Plan de implementación — Sphere (solo programación)

> Compañero del [GDD](GDD.md). Ordena el trabajo de código en fases pequeñas y
> verificables. El arte queda para después: todo con placeholders.
> Flujo de trabajo: Claude implementa → **Gabriel verifica en Play** con la checklist → siguiente fase.

## Cómo leer este plan

- Cada fase termina en un estado **jugable y verificable** (nada queda a medias entre fases).
- El orden respeta dependencias: los **Días** son la columna vertebral; telescopio, crafteo
  y guion narrativo se cuelgan de ellos.
- ✅ = ya existe en el prototipo · 🔧 = existe pero se adapta · 🔲 = nuevo.

---

## Fase 0 — Refactor de alineación con el GDD

*Objetivo: eliminar las divergencias entre el prototipo actual y el GDD antes de construir encima.*

| # | Tarea | Tipo |
|---|---|---|
| 0.1 | Eliminar el **botón Día/Noche** y el `DayNightManager` (el fade `TransitionManager` se conserva). | 🔧 |
| 0.2 | Fondo **siempre oscuro** (espacio): color fijo de cámara + starfield visible. | 🔧 |
| 0.3 | `GameManager`: quitar la meta "20 → misión completa". El contador pasa a ser **recurso acumulativo** sin meta (`* 47`). | 🔧 |
| 0.4 | `StarSpawner`: cambiar de spawn infinito por intervalo a **modo tanda** — método `SpawnBatch(int n)` que deja caer n estrellas (se invocará al empezar cada día). Mantener un goteo opcional para testear. | 🔧 |
| 0.5 | Limpieza: borrar scripts sin uso (`PlanetRotator`, `SolidBlackBackground`, `TintColor`) y el objeto `PlanetSurface_Deprecated`. | 🔧 |

**Verificar en Play:** el juego se ve como espacio permanente, estrellas caen en tanda, contador acumula sin "misión completa", nada de día/noche.

---

## Fase 1 — Sistema de Días (columna vertebral)

*Objetivo: los días como rondas, avanzados durmiendo en el iglú.*

| # | Tarea | Tipo |
|---|---|---|
| 1.1 | **`DayManager`**: día actual (int), evento `OnDayStarted(int)`, avance con fade (reutiliza `TransitionManager`). Config por día: cuántas estrellas caen, flags de eventos (lista editable en inspector). | 🔲 |
| 1.2 | **Interacción "mantener click"** genérica (`HoldInteractable`): mantener el click sobre un objeto N segundos dispara una acción; con **indicador de progreso** placeholder (anillo/barra world-space). Integrada a la prioridad de clicks del `PlanetController`. | 🔲 |
| 1.3 | **Iglú** (placeholder visual) colgado del planeta con `HoldInteractable` → dormir: fade → día+1 → tanda de estrellas del nuevo día. | 🔲 |
| 1.4 | **UI de día**: texto simple "Día N" (TMP existente o segundo texto). Mostrarlo unos segundos al despertar. | 🔲 |

**Verificar en Play:** mantener click en el iglú duerme (con indicador), pasa el día con fade, caen las estrellas del nuevo día, el contador de estrellas **no** se resetea.

---

## Fase 2 — Telescopio

*Objetivo: primer objeto que consulta los eventos del día.*

| # | Tarea | Tipo |
|---|---|---|
| 2.1 | **Telescopio** (placeholder) colgado del planeta, interactuable con click. | 🔲 |
| 2.2 | Día **sin** evento: el **player** dice *"No parece que haya algo interesante."* (burbuja `SpeechBubble` sobre el player — hoy solo habla el NPC, generalizar). | 🔧 |
| 2.3 | Día **con** evento: "cinemática" placeholder — panel fullscreen simple (fade in → imagen/animación mínima del amigo en cohete → fade out). Sistema `TelescopeEvent` por día en el `DayManager`. | 🔲 |

**Verificar en Play:** mirar el telescopio en día normal → diálogo; en día de evento (ej. día 2 para probar) → cinemática placeholder.

---

## Fase 3 — Banco de crafteo + cohete

*Objetivo: el sumidero del recurso estrellas.*

| # | Tarea | Tipo |
|---|---|---|
| 3.1 | **Catálogo de cohetes** (`RocketData`: nombre, sprite placeholder, costo en estrellas). ScriptableObjects o lista en inspector. | 🔲 |
| 3.2 | **Banco de crafteo** (objeto en escena, click) → abre **panfleto UI** (panel con 2–3 cohetes, elegir uno). | 🔲 |
| 3.3 | Al elegir: el cohete **spawnea en construcción** en su posición predefinida del planeta (`RocketBuildSite`). | 🔲 |
| 3.4 | **Depositar estrellas**: click (o mantener) sobre el cohete en construcción transfiere estrellas acumuladas → progreso `n/costo` visible → al completarse, cohete terminado (cambio visual placeholder). | 🔲 |
| 3.5 | API para el guion: `DemolishRocket()` (auto-sabotaje) y `LaunchRocket()` (partida del amigo). | 🔲 |

**Verificar en Play:** elegir cohete en el panfleto → aparece → llenarlo con estrellas → se completa. Las estrellas se descuentan del contador.

---

## Fase 4 — Guion de días (eventos narrativos)

*Objetivo: la curva narrativa del GDD §8 corriendo sobre los sistemas anteriores.*

| # | Tarea | Tipo |
|---|---|---|
| 4.1 | **Sistema de eventos por día** (`DayEvent`): acciones componibles — mostrar diálogo, activar/desactivar mecánica, spawn/despawn de NPC, disparar cinemática, lanzar cohete. Configurable por día sin tocar código. | 🔲 |
| 4.2 | **Días 1–2 guiados**: día 1 construir el primer cohete con el amigo (diálogos + crafteo); día 2 diálogo de invitación → rechazo → **el amigo se va** (cohete despega con fade; reutiliza `MakeNpcsLeave`). | 🔧 |
| 4.3 | **Mecánica decremental**: flag por día que **desactiva la recolección** de estrellas (el click las ignora y se acumulan en el planeta). | 🔲 |
| 4.4 | **Auto-sabotaje guiado** (día ~10): diálogo + `DemolishRocket()` + reelección en el panfleto. | 🔲 |
| 4.5 | Rellenar el **timeline completo** (días 1–13 del GDD) en la config del `DayManager`. | 🔲 |

**Verificar en Play:** jugar de día 1 a 13 de corrido y sentir la curva: construir juntos → rechazo → bucle → declive.

---

## Fase 5 — Finales y cierre

| # | Tarea | Tipo |
|---|---|---|
| 5.1 | Trigger de **finales** (al llegar al último día): elegir 1 de los 3 del GDD para el MVP (recomendado: Final 3 — mirar el telescopio). | 🔲 |
| 5.2 | Pantalla/secuencia final placeholder + créditos mínimos + opción de reiniciar. | 🔲 |

**Verificar en Play:** partida completa de principio a fin sin errores.

---

## Fase 6 — Builds y publicación (itch.io)

| # | Tarea | Tipo |
|---|---|---|
| 6.1 | **Build WebGL**: configurar player settings (compresión, template, resolución), probar en navegador local. Revisar que el input touch/mouse funcione. | 🔲 |
| 6.2 | **Build EXE** Windows. | 🔲 |
| 6.3 | Página de itch.io (draft privado): subir ambas builds, probar embed web. | 🔲 |
| 6.4 | Ronda de fixes de lo que se rompa en WebGL (fuentes TMP, resolución, rendimiento). | 🔲 |

> Consejo: hacer una **build WebGL de prueba al terminar la Fase 1** (no esperar al final) para
> detectar temprano problemas específicos de WebGL.

---

## Resumen de dependencias

```
Fase 0 (refactor)
   └─→ Fase 1 (Días) ──→ build WebGL de prueba temprana
          ├─→ Fase 2 (Telescopio)
          ├─→ Fase 3 (Crafteo/Cohete)
          │      └─→ Fase 4 (Guion de días) ─→ Fase 5 (Finales) ─→ Fase 6 (Publicación)
          └────────┘
```

## Fuera de este plan

- **Arte final** (sprites propios, música, SFX) — fase posterior.
- **Baúl de ideas** (clímax del rechazo con tecla mantenida, cartero) — GDD §11.
- Guardado/persistencia entre sesiones (el juego dura 1–2 h; se decide más adelante si hace falta).
