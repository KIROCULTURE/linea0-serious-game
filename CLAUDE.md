# CLAUDE.md — Línea 0 (serious game de matemática)

Lee esto completo y después `GDD_Linea0.md`. El GDD manda en diseño y contenido; este archivo manda en lo técnico y en cómo trabajar.

## Fase 2 (13 de noviembre): que el juego se entienda solo
**Lee esta sección primero.** La fase 1 ("presentar mañana", trabajo nocturno, hitos M0–M5) ya se entregó: release build-0801. Las reglas 1 a 8 de abajo siguen valiendo.

**Objetivo:** GDD sección 12. Un profe o un alumno con solo el `.exe` entiende en menos de 5 minutos de qué se trata y cómo se resuelve cada desafío, sin que nadie le explique. Referencia visual del paso guiado: `docs/mockup_guia.png`.

**Excepción a la regla 5:** se agrega el puzzle 0.2 de GDD 12.4 (cálculo ya verificado). Nada más nuevo en contenido matemático.

**Piezas nuevas**
- `Sala/Guia.cs`: foco (oscurece todo menos el objetivo), marco ámbar que late, mano de primitivas (toque o arrastre), globo con una línea, "Paso n de m", botón Seguir solo en pasos sin acción, "Ya sé jugar" y bloqueo de todo lo que no sea el objetivo. API en corrutina, por ejemplo `Paso(ancla, texto, hecho, gesto, conSeguir)`. Memoria con `PlayerPrefs` (`Visto(clave)`, `Marcar(clave)`), que respeta Opciones.
- `Vista/Anclas.cs`: registro de objetivos por nombre (`destino`, `boleto:0`, `ruta`, `partir`, `pista`, `ayuda`, `topera`, `palanca`, `dial:partes:+`, `dial:avanza:+`, `frenar`, `lupa`, `tramo:0`, `marca:7`, `compas:punta`, `rueda`, `tarjeta:0`, `vagon:×`, `alternativas`, `pasajero:0`…). Los objetos de la vía se convierten a rectángulo de pantalla.
- `Resources/guias.json`: todos los pasos y textos de GDD 12.4 a 12.7, editables sin tocar código.
- `Sesion.modo`: `Curso` o `Solo` (GDD 12.3). "Sin cuadrillas" sale de Opciones.
- Prueba automática: `-auto` completa las guías haciendo las acciones; nuevo `-auto -solo`; con `-capturas` guarda una captura por paso de guía (`guia_<puzzle>_<paso>.png`).

**Hitos fase 2** (cada uno: compila, tests, commit, tag, build, zip portátil, release `build-<hito>-HHMM`, copia al Escritorio, `AVANCE.md`, `git pull` y lectura de `MENSAJES_CHAT.md`):
- **F1 · Guía + Zona 0 + dos caminos:** sistema de guía, 0.1 con sus 5 pasos, 0.2 nuevo, menú con "Jugar con el curso" / "Jugar solo" y modo Solo funcionando de punta a punta (controles respondidos por el jugador, Apuesta de 3 s, sin cuadrillas). Build `F1_Guia`.
- **F2 · Herramientas:** guía de primera vez de cada herramienta (GDD 12.6) y avisos por evento (GDD 12.5). Build `F2_Herramientas`.
- **F3 · Ayudas:** botón "?" (GDD 12.7), pantalla "Para el profe" (GDD 12.8) con `Imprimibles/Linea0_Hoja_de_cuadrilla.pdf` copiado a `StreamingAssets` y abierto con `Application.OpenURL`, créditos del grupo en el menú, y en Opciones "Guías: Sí/No" y "Volver a mostrar las guías". Build `F3_Ayudas`.
- **F4 · Revisión:** correr `-auto`, `-auto -solo`, `-auto -fallar` y `-auto -demo` con capturas, sin excepciones, a 1366×768 y 1920×1080. Subir a `docs/capturas/` las capturas de cada paso de guía y una de cada pantalla nueva (máximo 40). Release final.

Si no alcanza el tiempo, el orden de prioridad es F1, F2, F3, F4.

## Contexto (fase 1)
- KIRO (estudiante de Diseño de Videojuegos, UGM) necesita un **prototipo funcional para presentar mañana**. No es el producto final: el gameloop tiene que funcionar de inicio a fin y verse presentable.
- Es un serious game de **bajo presupuesto**: primitivas, código simple, nada fuera del GDD.
- Se juega con **un solo PC y proyector**, con el curso en 4 cuadrillas por turnos (GDD sección 4). Eso es parte central del diseño, no un extra.
- KIRO se va a dormir apenas termine M0. **Trabaja de corrido toda la noche** sin esperar aprobación entre hitos.
- Referencias: `docs/Programa_Matematica_1M.pdf` (Unidad 1, pp. 59 y 66–75) y `docs/mockup_zona2.png` (estilo visual de una pantalla de puzzle).

## Reglas de trabajo nocturno
1. No hagas preguntas. Si algo bloquea, anótalo en `AVANCE.md` (sección "Bloqueos"), toma la decisión más simple que respete el GDD y sigue.
2. No modifiques nada fuera de esta carpeta. No instales programas. Paquetes de Unity Package Manager sí.
3. Al cerrar cada hito: compila sin errores, corre los tests, commit, tag `hito-Mx`, build de Windows y actualiza `AVANCE.md`.
4. Nunca dejes `main` roto. Lo que no alcance a quedar bien aparece como "Próximamente" en el menú, nunca como un error.
5. El contenido matemático del GDD (metas, boletos, soluciones, alternativas correctas) **ya está verificado**. Cópialo tal cual al JSON. No inventes puzzles.
6. Texto en pantalla en español de Chile, decimales con coma (`0,35`), signo menos real `−` en pantalla.
7. **Nada de assets generativos.** No uses imágenes, sonidos, música, voces ni fuentes generados con IA, ni herramientas que los generen. El arte se dibuja con primitivas de Unity (sprites de cuadrado y círculo, LineRenderer, formas de UI). Todo lo que no sea primitiva se descarga de bibliotecas gratis con licencia libre (CC0 de preferencia): Kenney (kenney.nl), OpenGameArt (filtrar CC0), Google Fonts. Cada asset descargado se anota en `CREDITOS.md` con nombre, autor, URL y licencia.
8. No dejes menciones a IA en el código, los comentarios, los textos del juego, los `LEEME.txt` ni los mensajes de commit.

## Unity
- Busca el editor en `C:\Program Files\Unity\Hub\Editor\*\Editor\Unity.exe` y usa la versión más nueva instalada. No preguntes la ruta.
- Proyecto Unity en la subcarpeta `Unity/` (la raíz queda para docs, builds y git). Créalo por línea de comandos con `-createProject` si no existe.
- Plantilla 2D (Built-in Render Pipeline). Paquetes: TextMeshPro (uGUI) y Unity Test Framework.
- Input: solo mouse a través de uGUI (EventSystem + botones). Espacio para saltar la Apuesta del andén, con el Input System si viene en la plantilla o con el manejador viejo si no.
- Resolución de referencia 1920×1080, `CanvasScaler` en Scale With Screen Size. Pantalla completa por defecto, con opción en ventana.
- Build por línea de comandos:
  `Unity.exe -batchmode -quit -projectPath Unity -executeMethod BuildScript.BuildWindows -logFile build.log`
  `BuildScript` recibe el nombre del hito y deja el `.exe` en `Builds/<hito>/Linea0.exe`. También en el menú `Tools/Línea 0/Build Windows`.
- Tests por línea de comandos:
  `Unity.exe -batchmode -projectPath Unity -runTests -testPlatform EditMode -testResults tests.xml -logFile tests.log`

## Arquitectura (mínima)
```
Unity/Assets/
  Scripts/
    Matematica/   Racional.cs, Posicion.cs, Expresion.cs     (lógica pura, sin MonoBehaviour)
    Datos/        DatosNiveles.cs ([Serializable] para JsonUtility), CargadorNiveles.cs
    Juego/        GameManager.cs (máquina de estados), Sesion.cs (cuadrillas, puntos, controles, reporte)
    Vista/        Via.cs, Tren.cs, CamaraVia.cs, Paleta.cs, UIFactory.cs
    Puzzles/      PuzzleBase.cs + un script por tipo
    Sala/         ApuestaAnden.cs, QuienPredijo.cs, Marcador.cs, ControlCurso.cs, CambioMaquinista.cs
  Editor/         BuildScript.cs, GeneradorEscena.cs
  Resources/      niveles.json, Fuentes/
  Tests/EditMode/ RacionalTests.cs, ExpresionTests.cs, NivelesTests.cs
```
- **Una sola escena** `Main.unity` con un objeto `Bootstrap`. Vía, tren y UI se construyen por código al iniciar. `GeneradorEscena` (menú `Tools/Línea 0/Generar escena`, también en batch) crea la escena y la agrega a Build Settings. No edites YAML de escenas a mano.
- **`Racional`**: struct inmutable con `long num, den`, siempre reducido y `den > 0`. Operadores `+ − × ÷`, comparación, `Parse` para `"3"`, `"-2"`, `"3/4"`, `"0,35"`, `"1,5"`, y `ToFraccion()` / `ToDecimal(maxCifras)` con coma. **Nunca uses float para la lógica.**
- **`Posicion`**: un `Racional` o un irracional marcado (`Raiz2`, `Pi`) con su `double` solo para dibujar.
- **`Expresion`**: parsea y evalúa las expresiones de la Zona 6 y de los "Sin vía" (`2 + 3 × 4`, `(−2) · 3 + 10 : 2`) con jerarquía y paréntesis, y también paso a paso en el orden que elija el jugador (para mostrar el "choque de horarios").
- **Tipos de puzzle** (enum en JSON): `Contar`, `Boletos`, `Repartir`, `Divisor`, `Lupa`, `ContarSaltos`, `SinVia`, `Compas`, `Rueda`, `Clasificar`, `Jerarquia`, `Serpiente`.
- Cada puzzle en JSON: `id`, `zona`, `tipo`, `esencial` (bool; false = refuerzo), `inicio`, `meta`, `mano`, `casillas`, `rangoVia`, `frasePedido`, `fraseNombre`, `cuentaEscrita`, `pistas[3]`, `alternativas[4]` + `correcta` cuando aplica, y campos propios del tipo (`dialFijo` en 3.3, `guion` en 2.1, 4.5 y 5.1, `salto` y `tramo` en ContarSaltos, `expresion` en SinVia/Jerarquia).
- **Boleto**: `operacion` (`suma`, `resta`, `mult`, `div`, `expreso`, `espejo`) + `valor` (string Racional) + `repeticiones` para el expreso.
- **GameManager**, estados: `Menu → Cuadrillas → ControlEntrada → AnuncioZona → Puzzle → (Apuesta → Viaje → Resultado → QuienPredijo) → … → SinVia → CambioMaquinista → … → ControlSalida → Final`.
- **Sesion**: cuadrilla de turno (rota A→B→C→D en cada puzzle), puntaje según la tabla 4.2 del GDD, nivel de acierto de cada pregunta de control (entrada y salida, 4 niveles), intentos y pistas por puzzle, tiempo. Decide los refuerzos con la regla 6.14 del GDD.

## Tests obligatorios (EditMode)
- `RacionalTests`: aritmética, reducción, signos, parse con coma, `1/2 == 2/4`, `2 − (−3) == 5`, `ToDecimal(1/3)` = `"0,333…"`.
- `ExpresionTests`: las 6 expresiones de la Zona 6, los "Sin vía" y la pregunta 6 del control dan lo que dice el GDD; el orden equivocado de 6.1 da 20.
- `NivelesTests`: carga `niveles.json` y comprueba, para cada puzzle:
  - Boletos: la solución del GDD llega exacto a la meta y, probando con fuerza bruta todas las secuencias posibles de la mano, **todas las que llegan usan los mismos boletos** (no hay soluciones accidentales).
  - Jerarquia, Serpiente, SinVia y ContarSaltos: la alternativa marcada como correcta coincide con el cálculo.
  - Divisor y Lupa: la meta es alcanzable con los controles del puzzle.
  - Controles de entrada/salida: la correcta es la del GDD (9.4).

## Hitos (en este orden)
La prioridad es **el loop completo de punta a punta lo antes posible**, con las cuadrillas funcionando, y después rellenar.

- **M0 · Base** (KIRO está despierto mirando): crea `Unity/`, `.gitignore` de Unity (incluye `Builds/` salvo los `LEEME.txt`), `Racional`, `Expresion`, sus tests, `niveles.json` con **todo** el contenido del GDD, `NivelesTests`, `BuildScript`, `GeneradorEscena`. Corre una compilación en batch y los tests. Primer commit. **Hazlo primero y rápido**: así KIRO aprueba los permisos de Unity y git antes de dormirse.
- **M1 · Núcleo + sala**: vía, tren, cámara, boletos, ruta, Partir, Apuesta del andén, acierto/falla con silueta y cuenta de lo que pasó, cuenta escrita, pistas, Contar y Repartir, **cuadrillas, marcador, "¿Quién predijo bien?"**, Cambio de maquinista, menú. Zona 0 y Zona 1 completas. Build `M1_Nucleo`.
- **M2 · Loop completo**: Control de entrada y de salida, el **primer puzzle esencial de cada zona** (2.1, 3.1, 4.1, 5.1, 6.1) más el **Sin vía** de cada zona, y la pantalla Final con plano, ganadora, reporte y preguntas de cierre. Con esto se juega de inicio a fin. Elegir zona en el menú. Build `M2_LoopCompleto`. **Es la build mínima presentable.**
- **M3 · Contenido esencial**: todos los puzzles **[E]** de las zonas 2 a 6 (topera y túnel, palanca inversora, espejo, divisor, contar saltos, lupa con 3 zooms, compás, rueda, clasificar, jerarquía, serpiente). Build `M3_Esencial`.
- **M4 · Pulido para presentar**: animaciones con easing (tren acelera y frena, ruedas giran, arcos de salto), modo proyector, opciones, botón Saltar del profe, sonidos de Kenney CC0 ("Interface Sounds" e "Impact Sounds"; si no hay red, el juego queda sin sonido y lo anotas en AVANCE.md), fuente Atkinson Hyperlegible desde Google Fonts (si no se puede, la de TextMeshPro), `CREDITOS.md` al día. Revisa que ningún texto pase de una línea y que todo se lea en 1080p desde lejos. Build `M4_Presentable`.
- **M5 · Refuerzos y ritmo adaptativo**: puzzles **[R]** y la regla 6.14 del GDD. Un archivo `Imprimibles/Hoja_Cuadrilla.html` (A4, para imprimir) con 30 casillas "Predicción ___" y una franja de recta numérica en blanco. Build `M5_Completo`.

Si no alcanza el tiempo: M2 tiene que existir sí o sí. Después M4 sobre lo que haya, y recién ahí M3 y M5.

## Builds
- `Builds/<hito>/Linea0.exe` + `LEEME.txt` de 5–10 líneas: qué incluye, cómo se juega, qué falta, cuánto dura.
- Cada build arranca en el menú y es presentable. Lo que no está hecho aparece como "Próximamente".
- **Portátil:** se va a jugar en un PC ajeno conectado a un proyector, desde un pendrive. Comprime la carpeta completa de cada build en `Builds/<hito>.zip` (el `.exe` solo no funciona sin `Linea0_Data` y las DLL). No debe necesitar internet, instalación ni permisos de administrador.
- **Cualquier proyector:** la UI se adapta a 1366×768, 1280×720, 1920×1080 y 16:10 sin cortar nada. Alt+Enter alterna pantalla completa y ventana.
- **Opción "Sin cuadrillas"** (desde M2): oculta marcador, turnos y "¿Quién predijo bien?" y deja el juego para una persona. Todo lo demás funciona igual. Sirve de respaldo si en la presentación no hay público para armar cuadrillas.
- **Modo demo** (botón en el menú, desde M4): 4 cuadrillas ya creadas con puntos de ejemplo, salta el control de entrada (con datos de ejemplo), juega la Zona 2 completa y termina en el reporte final con datos de ejemplo. Dura ~5 minutos y sirve para presentar el prototipo.

## AVANCE.md (mantenlo al día)
1. Tabla de builds: hito · qué se ve al jugarla · duración aproximada · ¿sirve para presentar?
2. Cómo abrir el proyecto en Unity y cómo jugar.
3. Qué quedó pendiente.
4. Bloqueos y decisiones que tomaste sin preguntar.
5. Al final, una sección **"Para presentar mañana"**: qué `.zip` copiar al pendrive; cómo usar el Modo demo; qué mostrar si hay 5 minutos (cuadrillas + Zona 2 desde el choque con la topera hasta el Sin vía, y el reporte final); las 4 hipótesis del prototipo (GDD 9.2) en una línea cada una.
