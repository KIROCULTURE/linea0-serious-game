# AVANCE — Línea 0

## Último release bueno: build-0801

`Escritorio\Linea0_Presentacion_0801.zip`, también en GitHub como release [build-0801](https://github.com/KIROCULTURE/linea0-serious-game/releases/tag/build-0801). Trae `Juego/` (la build completa), `COMO_ABRIR.txt`, `PARA_PRESENTAR.md`, el GDD y el mockup. Se descomprime y se abre `Juego/Linea0.exe`.

Lo que trae:
- El loop completo con todos los puzzles del GDD, esenciales y de refuerzo, y el ritmo adaptativo.
- Modo demo, Sin cuadrillas y "Cómo jugar" (4 pantallas).
- 5.1: la diagonal del compás se arrastra.
- 5.3: las tarjetas se arrastran al plano.

Pasa los 52 tests y las pruebas automáticas (respuestas correctas, fallas, Saltar y Modo demo) sin excepciones, a 1366×768 y 1920×1080. No tiene sonido.

Releases anteriores, por si algo falla:
- build-0751: sin el arrastre de tarjetas.
- build-0730: sin los arrastres.
- build-0720: la versión segura, sin "Cómo jugar".

## Fase 2 · F0 Arreglos visuales (13 de noviembre)

Revisé las 100 capturas de `-auto` a 1366×768 y 1920×1080, y probé además 1280×720 y 1024×768 en ventana. Cada arreglo se hizo en el origen (pivotes, anclas, tamaño del texto). Las capturas de antes y después están en `docs/capturas/antes_despues/` (`<arreglo>_antes.png` / `_despues.png`).

| # | Defecto | Causa | Arreglo |
|---|---|---|---|
| 1 | La forma (▲ ● ■ ◆) se montaba sobre la letra y el ◆ quedaba más arriba (marcador y alternativas) | La forma usaba como pivote su borde izquierdo, y el rombo giraba en torno a ese borde | Pivote al centro. En la píldora van la forma y la letra a la izquierda, y el número a la derecha |
| 2 | "DE TURNO" pegado al borde de la barra | Píldoras muy bajas | Píldoras más arriba y más bajas (58 px), con la etiqueta debajo y aire hasta el borde |
| 3 | Reloj y "SALIDA" cortados en ventana | La ventana podía quedar más grande que el monitor (al pasar a ventana con Alt+Enter u Opciones) | La ventana se ajusta al 80 % del monitor. SALIDA y DESTINO no se salen del borde de la vía |
| 4 | "−6 °C" partido en dos líneas y fuera de la píldora | La píldora tenía ancho fijo y el texto saltaba de línea | La píldora toma el ancho de su texto, que no salta de línea; si no cabe, se achica la letra. El anillo de DESTINO sigue a la píldora |
| 5 | Estaciones cortadas abajo | Con la ventana más grande que el monitor, la parte de abajo quedaba fuera de pantalla (mismo caso del 3) | Igual que el 3. A 1024×768, 1280×720 y 1366×768 se ve la fila completa |
| 6 | Punta de flecha chueca o despegada | Dos barras giradas según la cuerda del final del arco | Un triángulo con la punta en el final exacto del arco, girado según la tangente |
| 7 | Boletos apagados ilegibles | Se bajaba la opacidad de todo el boleto | `UI.Atenuar`: fondo gris y texto oscuro, a opacidad completa. Se usa en boletos, alternativas descartadas, vagones que todavía no pueden partir y control de salida |
| 8 | Marcas de la lupa cortadas abajo | Iban 150 px bajo la vía | Van en la fila de las estaciones, y los enteros conservan su píldora |
| 9 | "nota" encimado con SALIDA (4.3) | Etiqueta suelta bajo la píldora | El letrero dice "nota 5,0" dentro de la píldora, como pide el GDD |
| 10 | Rueda (5.2) cortada en el borde izquierdo | El margen de la vía era menor que el radio de la rueda | La vía parte en −0,6 en ese puzzle |
| 11 | Repartir: pasajeros encima de la topera y botón sobre la vía | | El andén oculta la topera y el botón queda bajo la vía |

Textos: con el skill de edición corregí tres frases de pantalla con dos puntos de más ("El destino está 12 estaciones más allá.", "1/4 = 25/100, así que necesitas dos zooms.", "Otra persona toma el mouse para la Zona 3."). Los textos del GDD no se tocaron.

## Fase 2 · F1 Guía, Zona 0 y dos caminos (13 de noviembre)

- **Guía (`Sala/Guia.cs`):** todos sus pasos esperan la acción real del jugador. Cada paso tiene:
  - foco que oscurece todo menos el objetivo y marco ámbar que late;
  - mano dibujada con primitivas: llega desde arriba si el objetivo está abajo y desde abajo si está arriba;
  - globo de una línea con "PASO n DE m", y Seguir solo en los pasos sin acción;
  - "Ya sé jugar" y bloqueo del resto de la pantalla.
- **Textos y anclas:**
  - Los textos están en `Resources/guias.json`, que ya trae también las guías y avisos de F2.
  - Los objetivos se registran por nombre en `Vista/Anclas.cs` (`destino`, `boleto:0`, `ruta`, `partir`, `apuesta`, `cuenta`, `pista`…).
- **Zona 0:** 0.1 con sus 5 pasos (el paso 4 solo en modo curso) y el puzzle nuevo 0.2 (0 → 5 con `+2 +3`), con su paso de guía. El test de boletos confirma que solo `+2 +3` llega.
- **Menú:**
  - Arriba están "Jugar con el curso" y "Jugar solo".
  - Abajo, "Para el profe", "Modo demo", "Elegir zona" y "Opciones", más los créditos del grupo.
  - "Cómo jugar" se muestra solo en el camino del curso.
- **Modo Solo (`Sesion.modo`):**
  - Sin cuadrillas, marcador ni "¿Quién predijo bien?".
  - La Apuesta dura 3 s y dice "Piensa dónde va a parar".
  - El jugador responde los controles y el juego guarda si acertó (barra llena o vacía en el reporte).
  - "Sin cuadrillas" salió de Opciones. "Elegir zona" tiene un botón de modo.
- **Adelantado de F3** (era poco y el menú nuevo lo necesitaba):
  - Pantalla "Para el profe", con la hoja PDF en `StreamingAssets`.
  - Créditos en el menú.
  - En Opciones, "Guías: Sí/No" y "Volver a mostrar las guías".
- **Memoria:** `PlayerPrefs` con prefijo `linea0_guia_`. Las pruebas automáticas no guardan nada, para no esconder las guías en el PC de KIRO.
- **Pruebas:**
  - Tests: 54, todos pasan, con `GuiasTests` nuevo (textos de guía de 70 caracteres o menos y una guía por cada puzzle del GDD).
  - `-auto` (curso) y `-auto -solo` corren de punta a punta sin excepciones y guardan `guia_<puzzle>_<paso>.png`.

## 1. Builds
| Hito | Qué se ve al jugarla | Duración | ¿Sirve para presentar? |
|---|---|---|---|
| M0_Base | Menú con título y "Próximamente" (solo prueba que compila y arranca) | — | No |
| M5_Completo | Todo el GDD: las 7 zonas con esenciales y refuerzos, ritmo adaptativo (regla 6.14) y hoja imprimible | 26–34 min completo · ~5 min el Modo demo | **Sí (la recomendada)** |
| M3_Esencial | Todos los esenciales del GDD, incluidas 5.2 Rueda y 5.3 Clasificar | ~30 min completo | Sí |
| M4_Presentable | Igual que M2 más pulido: el pasajero baja al llegar, margen para el tren, UI revisada en 1366×768 y 16:10 | ~30 min completo · ~5 min el Modo demo | Sí |
| M2_LoopCompleto | Juego de punta a punta: menú → cuadrillas → control de entrada → Zonas 0 a 6 (esenciales y Sin vía; refuerzos según la regla 6.14) → control de salida → final con plano, ganadora y reporte. Modo demo, Elegir zona y Opciones | ~30 min completo · ~5 min el Modo demo | **Sí** |

La build M1 no salió por separado: el núcleo (vía, tren, boletos, cuadrillas, marcador, "¿Quién predijo bien?") quedó dentro de M2, porque se pidió priorizar M2.

Cada build está en `Builds/<hito>/` y comprimida en `Builds/<hito>.zip`. Las presentables también se copiaron a `Escritorio\Linea0_<hito>.zip`.

## 2. Cómo abrir y jugar
- Abrir `Unity/` con Unity 6000.5.3f1 (Unity Hub → Add → carpeta `Unity`). La escena es `Assets/Scenes/Main.unity` (la crea `Tools/Línea 0/Generar escena`).
- Tests: Window → General → Test Runner → EditMode (52 tests en verde), o por consola (ver `CLAUDE.md`).
- Build: `Tools/Línea 0/Build Windows` o por consola con `-executeMethod BuildScript.BuildWindows -hito <nombre>`.
- Jugar: descomprimir el zip y abrir `Linea0.exe`. Solo mouse; Espacio salta las cuentas regresivas; Alt+Enter alterna ventana y pantalla completa.
- Prueba automática (para desarrollo): `Linea0.exe -auto -capturas <carpeta>` juega todo solo con las respuestas correctas y guarda capturas de cada pantalla.

## 3. Pendiente
- Sonido (ver Bloqueos).
- 5.1 Compás: la diagonal ya se arrastra (el botón "Girar compás" queda de respaldo). El divisor y la lupa de los pasos 2 y 3 se muestran como texto (7/5, 17/12; 1,4 → 1,41 → 1,414…), sin usar las herramientas.
- 5.2 Rueda: la lupa sobre π también se muestra como texto.
- 5.3 Clasificar: las tarjetas se arrastran a su línea (tocar tarjeta y luego línea también funciona). Da +2 o +1 por tarjeta a la cuadrilla de turno y no tiene Apuesta.
- Los arrastres se probaron con eventos simulados en la prueba automática, no con un mouse real.
- Algunas frases de "Nombra" del GDD pasan de 90 caracteres. El texto se achica solo para caber en el cartel, pero a veces ocupa dos líneas.
- No se probó a mano con mouse. Todo se verificó con pruebas automáticas sobre la build, todas sin excepciones:
  - `-auto`: juega todo con las respuestas correctas.
  - `-auto -fallar`: se equivoca al primer intento en cada puzzle que lo permite, pide pista y usa Saltar en 1.3 y 4.2.
  - `-auto -demo`: recorre el Modo demo real.
  - Quedan sin prueba automática las pantallas de Opciones y Elegir zona. Conviene abrirlas una vez antes de presentar.
- Corregido: el refuerzo forzado (Modo demo o "Con refuerzo" en Elegir zona) ya no queda pegado para la siguiente partida.

## 4. Bloqueos y decisiones
- Sonido: kenney.nl rechaza descargas automáticas (403), así que el juego no trae sonido. El GDD lo marca como opcional y ningún contenido depende de él.
- Input: el proyecto quedó con el manejador viejo (`activeInputHandler: 0`), así que se usa `StandaloneInputModule` e `Input.GetKeyDown(KeyCode.Space)`.
- Texto: se usa `UnityEngine.UI.Text` (uGUI) con Atkinson Hyperlegible como fuente dinámica, en vez de TextMeshPro. Las fuentes SDF de TMP habría que generarlas a mano; la fuente dinámica muestra `−`, `ℕ ℤ ℚ ℝ`, `√` y `π` (con respaldo de fuentes del sistema).
- Las formas de cuadrilla y alternativa (▲ ● ■ ◆) se dibujan con primitivas, sin depender de la fuente.
- `niveles.json`: los valores numéricos van con `-` ASCII y en pantalla se muestran con `−`. `correcta` es un índice desde 0 y `tipo` va como texto.
- Pistas (3 por puzzle): el GDD no las trae. Las escribí cortas y la tercera siempre es la solución del GDD.
- Anuncios de zona de la Central: el GDD solo trae los de las Zonas 0 y 6. Escribí una línea corta para las Zonas 1 a 5.
- Pedidos que el GDD no trae: 1.6 ("Reparte parejo en los 2 vagones."), 4.8, 5.3 y los Sin vía ("Vía en mantención. Solo queda la cuenta.").
- Zona 5 no tiene Sin vía en el GDD (la cierra 5.3). No se inventó uno.
- Jerarquía (6.1, 6.2, 6.3, 6.5): no tiene alternativas en el GDD.
  - Se predice el resultado final.
  - La cuadrilla de turno toca los vagones en el orden que quiere, después de la Apuesta.
  - Si el resultado no coincide, sale "Choque de horarios".
- Puntos: si se usó una pista antes de llegar, se da +1 aunque sea el primer intento (GDD 6.12: deja de contar "al primer intento").
- "¿Quién predijo bien?" aparece después de cada viaje que cuenta, también en las fallas. El 0.1 y el primer intento obligado de 2.1 no dan puntos.
- Alternativas falladas quedan descoloridas y no se pueden volver a elegir.
- Tras una falla en un Sin vía, la comprobación muestra la respuesta, así que el segundo intento vale +1.
- Modo demo: entra directo a la Zona 2 con refuerzo, con 4 cuadrillas y datos de ejemplo en puntos y controles.
- Commits sin línea de coautoría (regla 8 de `CLAUDE.md`).
- Se trabajó con la prioridad que pidió KIRO por chat: M2 primero y mínimo, y después M4, M3 y M5.
- Hoja imprimible: KIRO ya la tiene en PDF. Queda además `Imprimibles/Hoja_Cuadrilla.html`, hecha antes de que lo avisara.
- `MENSAJES_CHAT.md`: no apareció en ningún `git pull` (M2, M3, M4 y M5), así que no hubo instrucciones del diseñador que aplicar.

## 5. Para presentar mañana
- Qué abrir: `Escritorio\Linea0_Presentacion_0801.zip` (respaldo seguro: `Linea0_Presentacion_0720.zip`). Cópialo al pendrive. En el PC del proyector, descomprímelo completo y abre `Juego/Linea0.exe`: el `.exe` solo no funciona. Si Windows dice "Windows protegió su PC", haz clic en Más información → Ejecutar de todas formas. Si abre en ventana, Alt+Enter. Las instrucciones están en `COMO_ABRIR.txt`, y el guion y las hipótesis en `PARA_PRESENTAR.md`, ambos dentro del zip.
- **Verificado antes de presentar:**
  - Prueba de humo con `-logFile`: 0 excepciones y 0 errores.
  - Partida automática completa a 1366×768 y 1920×1080 (con fallas, pistas y Saltar): 0 excepciones.
  - Los textos no se salen de sus recuadros.
- **Modo demo:** Menú → *Modo demo*. Parte en la Zona 2 con 4 cuadrillas y puntos de ejemplo, juega toda la zona (con refuerzos) y termina en el final y el reporte con datos de ejemplo.
- **Guion de 5 minutos (todo dentro del Modo demo):**
  1. *(1 min)* Menú → Modo demo. Pasa rápido "Cómo jugar" y muestra el marcador con la cuadrilla DE TURNO.
  2. *(1,5 min)* 2.1: elige el boleto −5 y presiona Partir. El tren choca con la topera. Clic en la topera agrietada: se abre el túnel. Vuelve a Partir: Apuesta del andén (Espacio la salta), el tren llega a −2 y aparece "¿Quién predijo bien?".
  3. *(1,5 min)* Saltar (arriba a la derecha) hasta 2.7 Sin vía: la cuenta sola, 4 alternativas, y el tren comprueba en la recta.
  4. *(1 min)* El final: plano ℕ ⊂ ℤ ⊂ ℚ ⊂ ℝ, ganadora y "Ver reporte". Cierra con las 4 hipótesis.
- **Hipótesis del prototipo (GDD 9.2):**
  - H1: con predicción obligatoria y puntos al primer intento, el curso piensa la cuenta. Se mide con el % de logros al primer intento por zona.
  - H2: todo el curso participa con un solo PC. Se mide con el promedio de cuadrillas que predicen bien por viaje.
  - H3: el curso entiende más al final. Se mide con la diferencia entre el control de entrada y el de salida.
  - H4: cabe en 45 minutos. Se mide con el tiempo total del reporte.
