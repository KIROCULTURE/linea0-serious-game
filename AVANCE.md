# AVANCE — Línea 0

## 1. Builds
| Hito | Qué se ve al jugarla | Duración | ¿Sirve para presentar? |
|---|---|---|---|
| M0_Base | Menú con título y "Próximamente" (solo prueba que compila y arranca) | — | No |
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
- 5.2 Rueda (π) y 5.3 Clasificar: aún no están. En la Zona 5 se juega solo 5.1 (Compás), y Elegir zona muestra la zona igual.
- 5.1 Compás: la diagonal se gira con un botón ("Girar compás") y no arrastrándola. El divisor y la lupa del paso 2 y 3 se muestran como texto (7/5, 17/12; 1,4 → 1,41 → 1,414…).
- Pulido M4: animación de puertas y pasajero bajando, zoom con más easing, revisión de textos largos (algunas frases de "Nombra" del GDD pasan de 90 caracteres y se achican solas).
- Sonido (ver Bloqueos).
- `Imprimibles/Hoja_Cuadrilla.html` (M5).

## 4. Bloqueos y decisiones
- **Sonido:** kenney.nl rechaza descargas automáticas (403), así que el juego no trae sonido. El GDD lo marca como opcional y ningún contenido depende de él.
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
- Se trabajó con la prioridad que pidió KIRO por chat: M2 primero y mínimo, y recién después M4, M3 y M5.

## 5. Para presentar mañana
- **Copiar al pendrive:** `Escritorio\Linea0_M2_LoopCompleto.zip` (o el último `Linea0_M*.zip` que haya en el Escritorio). En el PC del proyector, descomprimir y abrir `Linea0.exe`. No pide internet, instalación ni administrador.
- **Modo demo:** Menú → *Modo demo*. Parte en la Zona 2 con 4 cuadrillas y puntos de ejemplo, juega toda la zona y termina en el final y el reporte.
- **Si hay 5 minutos:**
  1. Modo demo y una ronda de cuadrillas: muestra el marcador, la cuadrilla DE TURNO, la Apuesta del andén y "¿Quién predijo bien?".
  2. 2.1: el choque con la topera. Clic en la topera agrietada abre el túnel y el tren llega a −2.
  3. Saltar (arriba a la derecha) hasta 2.7 Sin vía.
  4. El reporte final.
- **Hipótesis del prototipo (GDD 9.2):**
  - H1: con predicción obligatoria y puntos al primer intento, el curso piensa la cuenta. Se mide con el % de logros al primer intento por zona.
  - H2: todo el curso participa con un solo PC. Se mide con el promedio de cuadrillas que predicen bien por viaje.
  - H3: el curso entiende más al final. Se mide con la diferencia entre el control de entrada y el de salida.
  - H4: cabe en 45 minutos. Se mide con el tiempo total del reporte.
