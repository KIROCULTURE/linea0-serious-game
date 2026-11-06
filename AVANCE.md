# AVANCE — Línea 0

## 1. Builds
| Hito | Qué se ve al jugarla | Duración | ¿Sirve para presentar? |
|---|---|---|---|
| M0_Base | Menú con título y "Próximamente" (solo prueba que el proyecto compila y arranca) | — | No |

## 2. Cómo abrir y jugar
- Abrir `Unity/` con Unity 6000.5.3f1 (Unity Hub → Add → carpeta `Unity`). La escena es `Assets/Scenes/Main.unity` (la crea `Tools/Línea 0/Generar escena`).
- Tests: Window → General → Test Runner → EditMode, o por consola (ver `CLAUDE.md`).
- Build: `Tools/Línea 0/Build Windows` o por consola con `-executeMethod BuildScript.BuildWindows -hito <nombre>`.

## 3. Pendiente
- M1 a M5.

## 4. Bloqueos y decisiones
- Input: el proyecto quedó con el manejador viejo (`activeInputHandler: 0`), así que se usa `StandaloneInputModule` e `Input.GetKeyDown(KeyCode.Space)`.
- Texto: se usa `UnityEngine.UI.Text` (uGUI) con fuente dinámica en vez de TextMeshPro. TMP viene dentro de `com.unity.ugui` 2.5, pero sus fuentes SDF necesitan generarse a mano; la fuente dinámica muestra `−`, `ℕ`, `√` y `π` sin pasos extra.
- `niveles.json`: los valores numéricos van con `-` ASCII y el juego los muestra con `−`. `correcta` es índice desde 0. `tipo` va como texto.
- Pistas (3 por puzzle): el GDD no las trae. Las escribí cortas; la tercera siempre es la solución del GDD.
- Anuncios de zona de la Central: el GDD solo trae los de la Zona 0 y la Zona 6. Escribí una línea corta para las Zonas 1 a 5.
- Pedidos que el GDD no trae: 1.6 ("Reparte parejo en los 2 vagones."), 4.8, 5.3 y los Sin vía ("Vía en mantención. Solo queda la cuenta.").
- Zona 5 no tiene Sin vía en el GDD: la cierra 5.3 (Clasificar). No se inventó uno.
- Jerarquía 6.1, 6.2, 6.3 y 6.5 no tienen alternativas en el GDD: se predice el resultado final y el test compara la meta con la evaluación.
- Commits sin línea de coautoría (regla 8 de `CLAUDE.md`).
