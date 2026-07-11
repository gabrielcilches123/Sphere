# 🐧 Game Design Document — *(título provisional: "El Planeta Vacío")*

> Documento vivo. Estructura la visión del juego a partir del concepto original.
> Última actualización: 2026-07-10.

---

## 1. Logline

Un pingüino común vive en un asteroide diminuto. Su amigo construye un cohete y lo
invita a viajar por la galaxia... y él **dice que no**. El juego es la experiencia de
quedarse: el peso de la procrastinación vital y el miedo al cambio, hasta terminar
mirando el firmamento desde un planeta completamente vacío.

## 1b. Ficha técnica

| Campo | Valor |
|---|---|
| **Género** | Experiencia narrativa corta / clicker minimalista |
| **Duración objetivo** | 1–2 horas |
| **Plataformas** | Web (WebGL en itch.io) + EXE Windows |
| **Input** | Solo mouse: click y mantener click (compatible touch) |
| **Motor** | Unity 6 (6000.3.18f1), URP, Input System nuevo |
| **Cámara** | Ortográfica, vista lateral (mundo 3D que rota + personajes sprite 2D) |
| **Proyecto** | `C:\Users\G\Sphere\Sphere` — escena `Assets/Scenes/PlanetTest.unity` |

## 2. Concepto y experiencia buscada

No es un juego de vencer monstruos. El **"jefe final" es simplemente subirte a la nave —
y fallas**. Es una experiencia narrativa corta (≈1–2 h) donde el jugador *encarna
activamente* el auto-sabotaje: cada vez que algo está a punto de completarse, lo rompe o
lo pospone "para hacerlo mejor".

**Tema central:** miedo al cambio · procrastinación · arrepentimiento · zona de confort.

## 3. Pilares de diseño

1. **La comodidad como trampa.** Todo invita a quedarse: el iglú, la rutina, el "todavía no".
2. **Mecánicas decrementales.** En vez de crecer (RPG), el juego **te quita** cosas: se
   apaga la vida, se pierde el interés, las mecánicas desaparecen.
3. **Bucle del arrepentimiento.** El progreso es circular y frustrante a propósito:
   construir → auto-sabotear → volver a empezar.
4. **Scope pequeño y minimalista.** Pocas mecánicas, un asteroide, objetos-clave contados.

---

## 4. Narrativa — El Viaje del Héroe **a la inversa**

Usa la estructura clásica de Joseph Campbell, pero **subvierte los primeros pasos** para
justificar el declive:

| Etapa (Campbell) | En el juego |
|---|---|
| **Mundo Ordinario** | El planetita original. Hay vida, caen estrellas y junto a tu amigo las recogen como material para construir el cohete. |
| **Llamado a la Aventura** | El cohete está listo. Tu amigo: *"Nos vamos de este lugar, sube, hay un asiento para ti."* |
| **Rechazo del Llamado** *(núcleo del juego)* | Te da miedo el cambio, estás cómodo en tu iglú. Dejas ir a tu amigo y te quedas. Pones excusas: *"el cohete es de chatarra, hagamos uno mejor."* |
| **El Bucle del Arrepentimiento** | Intentos fallidos de cruzar el "Primer Umbral". Ves por el telescopio a tu amigo cumpliendo sus sueños. Con valor + celos/rabia empiezas tu propio cohete, más grande y mejor; cuando está casi listo te auto-saboteas ("no servirá, hay que hacerlo mejor"), lo rompes y vuelves a empezar. |

## 5. Protagonista y entorno

- **Protagonista:** un pingüino común y corriente.
- **Entorno:** un **asteroide esférico diminuto** — se da la vuelta completa en un par de
  segundos (mundo que rota bajo el personaje).
- **El amigo:** otro pingüino; construye el cohete y se va. Motor emocional del juego.
- **Objetos clave en escena:** iglú (dormir/pasar de día), telescopio (mirar al amigo),
  cohete (objeto que se craftea), estrellas caídas (recurso).
- **Fondo:** siempre oscuro/espacial. **Ya no hay ciclo día/noche**, solo el paso de días.

---

## 6. Estructura de juego — Días / Rondas

Los **días funcionan como rondas** y marcan el progreso. Cada día **configura qué
"eventos" deben ocurrir**.

- **Pasar de día:** el jugador va al iglú y **mantiene click izquierdo sobre el iglú para
  dormir**. Al despertar, avanza el día y se activan los eventos de ese día.
- Ejemplos de eventos por día:
  - **Día 5:** al mirar por el telescopio ves una pequeña **cinemática/animación** del
    amigo viajando en el cohete.
  - **Día 10:** **pierdes el interés** por recoger estrellas → se **elimina temporalmente**
    la mecánica de recolección por click.

> Nota de diseño: los días son el "reloj narrativo". La dificultad no sube; lo que cambia
> es **qué puedes hacer** (mecánicas que se activan o se apagan).

---

## 7. Sistemas y mecánicas (scope pequeño)

### 7.1 Recolectar estrellas
- Cada día caen **X estrellas** (número definido por día).
- Se recogen **haciendo click** sobre ellas.
- Sirven como **recurso** para construir el cohete.
- **Se acumulan:** NO se pierden ni se reinician al pasar de día.

### 7.2 Banco de crafteo (la "tienda")
- Funciona más como un **banco de crafteo** que como tienda.
- Flujo: **eliges un cohete** → el cohete **spawnea en la escena** (posición predefinida)
  → luego **lo vas llenando de estrellas** hasta terminar de craftearlo.
- Los cohetes son **meramente visuales**: no tienen estadísticas reales, porque al final
  **ninguno va a funcionar** (refuerza el tema del auto-sabotaje).

### 7.3 Telescopio
- Objeto en escena; el jugador puede **mirar por él**.
- **Día con evento de telescopio** → **animación/cinemática** (ej: ver al amigo lejos, en
  medio de su aventura).
- **Día sin evento** → el personaje dice un **diálogo**: *"No parece que haya algo
  interesante."*

### 7.4 Mecánicas decrementales
- A medida que avanzan los días, **se quitan** capacidades. Ejemplo eje: a partir del
  Día 12 se **elimina la mecánica de recoger estrellas** y estas **se acumulan sin
  recogerse** en la superficie del planeta.

---

## 8. Secuencia de días (prototipo)

### Fase A — Construir y rechazar (Días 1–5)
| Día | Acción / evento |
|---|---|
| **1** | Recolectar estrellas para terminar de construir el **primer cohete** junto a tu amigo. |
| **2** | **Rechazar** la invitación a viajar en el cohete con el amigo. |
| **3** | Construir **otro cohete** (porque "tú lo puedes hacer mejor"). |
| **4** | **Seleccionar** qué cohete construir (panfleto / banco de crafteo). |
| **5** | Recolectar **más estrellas** para construir el cohete. |
| — | *(pasan X días)* |

### Fase B — El bucle del arrepentimiento (Días 6–13)
| Día | Acción / evento |
|---|---|
| **6** | Nuevas acciones (mirar el cielo, mirar por el telescopio). **Evento:** ver al amigo a lo lejos, en medio de su aventura. *(Cada X días; si no es día de evento → diálogo "no parece que haya algo interesante".)* |
| **7–8** | Continúan los días sin evento → diálogos de "nada interesante". |
| **9** | **Mantener el interés** por recoger estrellas (para poder construir el cohete). |
| **10** | Este cohete "no funciona" / lo vas **reemplazando** por otro que parece mejor. |
| **11** | El plan **no está yendo bien**. |
| **12** | **Pierdes el interés** por recoger estrellas → se **quita** la mecánica de recolección. |
| **13** | Las estrellas se **acumulan** solas en la luna/planeta (ya no las recoges). |

> *(La numeración fina de los días 6–13 es una reconstrucción del texto original, que venía
> cortado. Ajustar libremente al montar el prototipo.)*

## 9. Finales

Tres desenlaces posibles, todos coherentes con el tema del rechazo:

1. **Final 1 — Rendición:** pierdes toda motivación y te vas a dormir.
2. **Final 2 — Un intento más:** lo intentas una vez más (bucle que se reabre).
3. **Final 3 — El observador:** pierdes toda motivación y solo miras por el telescopio.

**Imagen final del juego:** mirando el firmamento estrellado desde la ventana de un
**planeta completamente vacío**.

---

## 10. Alcance / MVP

**Objetivo del prototipo:** un bucle diario jugable de principio a fin, publicable en
itch.io (build .exe y web), con **pocas mecánicas pero con la emoción central funcionando**.

**Dentro del MVP:**
- Asteroide que rota + pingüino protagonista (✔ ya prototipado).
- Estrellas que caen y se recogen con click, **acumulativas** (✔ base ya prototipada; falta acumulación entre días).
- Iglú para **dormir → pasar de día**.
- Banco de crafteo: elegir cohete → spawnea → llenar de estrellas.
- Telescopio con evento (cinemática) vs diálogo por defecto.
- Sistema de **días como rondas** que activan/desactivan eventos y mecánicas.
- Al menos **1 final**.

**Fuera del MVP (por ahora):** ver "Baúl de ideas".

---

## 11. Baúl de ideas (backlog — no tocar aún)

### 🎬 Clímax del Rechazo (mecánica de input)
Cuando aparece la oportunidad de irte (p. ej. **un pingüino cartero** que viene a buscar
las últimas cajas), el juego te **exige interactuar**:
- Debes **mantener presionada una tecla** (W o Espacio) para caminar hacia la nave
  mientras la **pantalla tiembla y se llena de ruido visual** (ansiedad/miedo).
- Si **sueltas la tecla un segundo**, el pingüino **se da vuelta** automáticamente y corre
  a esconderse a su casa → pierdes la oportunidad.
- Refleja que "en el momento de la verdad siempre se echaba atrás".

*(Otras ideas que surjan van aquí hasta decidir integrarlas.)*

---

## 12. Tono y conclusión

Experiencia narrativa corta y melancólica. No hay enemigos ni victoria: el reto era
**subirse a la nave**, y no lo lograste. El juego cierra contigo, solo, mirando las
estrellas desde un planeta vacío.

---

## Apéndice — Estado actual del prototipo (vs. este documento)

Lo ya construido en la escena `PlanetTest` y cómo se relaciona con el GDD:

| Ya existe | Encaja / cambia según el GDD |
|---|---|
| Planeta esférico que rota con click (mitades de pantalla) | ✔ Base del "mundo ordinario". |
| Estrellas 2D que caen y se recogen con click + contador | ✔ Mecánica 7.1. **Falta:** acumular entre días (no resetear). |
| NPC con diálogo estilo *A Short Hike* | ✔ Reutilizable para **el amigo** y para el diálogo del telescopio. |
| Sistema día/noche + fade + botón | ⚠️ **Se reemplaza:** el GDD dice *"ya no hay día/noche, solo días; fondo siempre oscuro"*. El **fade** se reaprovecha para pasar de día; el **botón** se sustituye por **dormir en el iglú**. |
| Misión "recoge 20 → NPC se va" | 🔁 Se reencuadra como **Día 2: el amigo se va en el cohete**. |

**Próximos pasos sugeridos hacia el GDD:**
1. Convertir el ciclo día/noche en **sistema de Días** (dormir en iglú, fondo oscuro fijo).
2. Hacer las estrellas **acumulativas** (recurso persistente entre días).
3. Añadir el **iglú** y el **telescopio** como objetos interactuables.
4. Prototipar el **banco de crafteo** (elegir cohete → spawnea → llenar de estrellas).
