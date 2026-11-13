# Línea 0 — Game Design Document (prototipo)

Serious game de matemática para un bloque de clase de 45 minutos.
Autor: KIRO (Alejandro Gómez Valeria) · Diseño de Videojuegos UGM · noviembre 2026

---

## 0. Ficha

| | |
|---|---|
| Género | Puzzle 2D por turnos sobre una recta numérica (vista lateral) |
| Motor | Unity 2D (ver `CLAUDE.md`) |
| Plataforma | PC Windows con mouse, proyectado en la sala |
| Jugadores | Todo el curso en **4 cuadrillas** que comparten un solo PC (sección 4) |
| Duración | 45 min de clase: 6 de inicio y control de entrada, ~28 de juego, 4 de control de salida y 7 de cierre |
| Público | 1° medio. El juego **no supone conocimientos previos** de aritmética |
| Contenido | Naturales → enteros (negativos) → fracciones → decimales → reales (irracionales) → operaciones combinadas |
| Fuente | Programa de Estudio Matemática 1° medio, Mineduc 2016, Unidad 1, OA 1 (`docs/Programa_Matematica_1M.pdf`, pp. 59, 66–75) |
| Tono | Luminoso, de día, con humor cotidiano. Nada de terror ni ambiente lúgubre |

---

## 1. La idea

Eres maquinista de la **Línea 0**, la línea de Metro más nueva de Santiago, y hoy es el día de la inauguración. **La vía es la recta numérica y cada estación es un número.**

La línea se terminó a la rápida y quedó incompleta: solo existen las estaciones 0, 1, 2, 3… hacia la derecha. Durante el día los pasajeros piden bajarse en lugares que "no existen": antes del cero, a la mitad de un tramo, en un punto exacto que ninguna parada alcanza. En cada zona el curso choca con un **hueco** que sus números no pueden cubrir, y lo cierra creando el número nuevo.

| Hueco | Número que lo cierra |
|---|---|
| Contar pasajeros y avanzar estaciones | Naturales ℕ |
| 3 − 5 no tiene respuesta: el tren choca con el cero | Enteros ℤ (negativos) |
| Bajarse a la mitad de un tramo | Fracciones ℚ |
| Frenar exacto entre dos marcas | Decimales (otra forma de escribir fracciones) |
| Un punto que ninguna fracción alcanza | Irracionales → Reales ℝ |
| Muchos trenes a la vez | Orden de las operaciones (jerarquía) |

Así cada tipo de número llega con su razón de existir, que es como aparecieron en la historia de la matemática. Al final el plano de la Línea 0 se ve como cuatro líneas de Metro anidadas: ℕ ⊂ ℤ ⊂ ℚ ⊂ ℝ.

---

## 2. Objetivos de aprendizaje

**Fuente:** Programa de Estudio Matemática 1° medio (Mineduc, 2016), Unidad 1, **OA 1: calcular operaciones con números racionales en forma simbólica.** Indicadores de la p. 68 que trabaja el juego:
- identifican el tipo de número (racional, entero, natural) y las operaciones involucradas;
- realizan operaciones mixtas con números racionales respetando la jerarquía de las operaciones y los paréntesis;
- transforman expresiones del lenguaje natural a expresiones matemáticas (cada pedido de pasajero se traduce a boletos).

Actividades del programa que se adaptan: la **serpiente de operaciones** (actividad 2, p. 70) y los ejercicios mixtos con enteros, fracciones y decimales (actividades 3 a 5, pp. 71–72).

**Al terminar, el estudiante puede…**
| Zona | Objetivo medible |
|---|---|
| 1 Naturales | Usar números para contar y ordenar; sumar, multiplicar (saltos repetidos) y dividir (repartir) naturales |
| 2 Enteros | Ubicar negativos en la recta; explicar por qué hacen falta; sumar, restar y multiplicar enteros, incluido restar un negativo |
| 3 Fracciones | Ubicar una fracción como partes iguales de un tramo; reconocer equivalentes; sumar, multiplicar y dividir fracciones simples |
| 4 Decimales | Ubicar decimales como zoom de 10 en 10; pasar de fracción a decimal; reconocer un decimal periódico; operar decimales simples |
| 5 Reales | Reconocer que hay puntos de la recta que no son fracciones (√2, π) y clasificar un número en ℕ, ℤ, ℚ o ℝ |
| 6 Hora punta | Resolver expresiones mixtas respetando paréntesis y prioridad de × y : sobre + y − |

**Notas de precisión**
- En los textos escolares chilenos ℕ = {1, 2, 3, …} y ℕ₀ = {0, 1, 2, …}. El juego llama "Estación Cero" al 0 y **nunca dice que el 0 sea natural**.
- Los irracionales y los reales los pide el encargo, pero en el currículum aparecen en 2° medio. La Zona 5 es una **primera aproximación**: qué son, por qué existen y dos ejemplos. No opera con ellos.
- Los decimales se escriben con **coma** (0,35), como en Chile.
- El juego da una primera comprensión de cada conjunto y practica el OA 1. No reemplaza las clases del profe sobre cada tema: las abre.

---

## 3. Principios de diseño

1. **La matemática es la mecánica.** Sumar es avanzar, restar es retroceder, multiplicar es repetir un salto, dividir es contar cuántos saltos caben o partir un tramo. Para avanzar en el juego hay que hacer la cuenta: no hay preguntas que interrumpan el juego para dar permiso de seguir.
2. **Concreto → pictórico → simbólico** (el método COPISI que usa el currículum chileno). Cada zona parte con el tren y los pasajeros, pasa a la recta con arcos y fracciones escritas, y termina con un puzzle **sin vía**, donde solo se ven los números (sección 6.10).
3. **Primero se hace, después se nombra.** El curso lleva el tren a −2 y recién ahí aparece la palabra "negativo".
4. **Texto máximo: una línea** (≤ 90 caracteres), que se pueda leer en voz alta. Cero muros de texto.
5. **El error enseña.** Si el tren no llega, queda donde cayó, aparece una silueta en el destino y la **cuenta de lo que hiciste** junto a la que hacía falta: "Hiciste 3 + 8 = 11. El destino era −5." Reintentar es gratis, pero los puntos son solo para el primer intento.
6. **Predecir antes de mover.** Ninguna cuadrilla ve el resultado sin haberse comprometido antes con una predicción.
7. **Cada número dice para qué sirve.** Los pasajeros traen el uso real: el −2 del estacionamiento del mall, el 5,5 de promedio, el 1/4 de pan.

---

## 4. Diseño para la sala: un PC, todo el curso

En muchos colegios no alcanzan los computadores. El juego se diseñó para **un PC conectado al proyector**.

### 4.1 Cuadrillas por turnos
- Al empezar, el profe arma **4 cuadrillas** (A amarilla ▲, B azul ●, C verde ■, D roja ◆). Se ingresan con un clic en el menú y no hay que escribir nombres.
- Un alumno es el **maquinista**: maneja el mouse y hace lo que le dice la cuadrilla de turno. Cambia en cada zona.
- En cada puzzle hay una **cuadrilla de turno**, que rota A → B → C → D. Esa cuadrilla decide el viaje en voz alta.
- **Las otras tres cuadrillas predicen.** Durante la Apuesta del andén (6.4) cada una escribe en un papel o una pizarrita dónde va a parar el tren con lo que eligió la cuadrilla de turno, y lo levanta al terminar la cuenta regresiva.

### 4.2 Puntaje (marcador visible arriba)
| Quién | Cuándo | Puntos |
|---|---|---|
| Cuadrilla de turno | Llega al destino **al primer intento** | +2 |
| Cuadrilla de turno | Llega en el segundo intento | +1 |
| Cada cuadrilla que predijo | Su predicción coincide con donde paró el tren | +1 |

Después de cada viaje aparece "¿Quién predijo bien?" con los 4 botones de cuadrilla. El maquinista o el profe marca las que acertaron y presiona Listo. Son 2 clics y se pueden saltar.

### 4.3 Otras decisiones para la sala
- **Modo proyector** (por defecto). Números de la vía y botones con letra enorme (mínimo 48 px a 1080p), alto contraste, nada importante en los bordes.
- **Solo mouse.** No hay que escribir. Nada depende del sonido.
- **Color + forma + letra** en todo lo que identifica a una cuadrilla o una alternativa, para que funcione con daltonismo.
- **Botón del profe "Saltar"** en una esquina, con confirmación, para no trabar la clase.
- Si hay más computadores, el juego funciona igual con grupos de 3–4 por PC (1 cuadrilla por grupo y todas las predicciones internas).

---

## 5. Narrativa

Corta, para enmarcar. No compite con los puzzles.

- **Premisa.** Son las 7:00 del día de la inauguración. La Línea 0 se abrió a la rápida y le faltan estaciones. El turno del día es dejar a cada pasajero donde pide. Cada estación nueva queda en el plano.
- **La Central.** Voz por radio (solo texto con un ícono de radio). Da instrucciones en una línea y anuncia cada zona. Tono de jefa de turno con oficio y algo de talla: "Primer viaje de la Línea 0. Hay gente esperando en Estación Cero, y nadie leyó el plano."
- **Pasajeros.** Personajes simples y de colores, cada uno con una frase cotidiana. Son los que piden los números nuevos.
- **Reloj.** Arriba a la derecha avanza la hora del día zona por zona (7:00 → 18:00). La última zona es la hora punta de la tarde. Es ambientación, no hay tiempo límite.
- **Final.** A las 18:30 se corta la cinta de inauguración en Estación Cero, con papel picado de colores. Se dibuja el plano completo (ℕ ⊂ ℤ ⊂ ℚ ⊂ ℝ), gana la cuadrilla con más puntos y aparece el reporte para el profe.

---

## 6. Sistemas y controles

### 6.1 La vía (recta numérica)
- Línea horizontal gruesa con durmientes, sobre un viaducto al aire libre (de día, con cielo y ciudad simple de fondo). Cada estación entera es una "píldora" con su número, debajo de la vía.
- La cámara encuadra el rango del puzzle (por ejemplo −6 a 6). Zoom suave cuando se usa la lupa.
- A la izquierda del 0 hay una **topera** (muro) hasta la Zona 2, donde se abre el **túnel** de los negativos: la vía baja del nivel de la calle a una estación subterránea bien iluminada, en tonos azules.
- Cada estación muestra su número en el color de su conjunto (sección 10). El destino del puzzle lleva un anillo rojo y la palabra DESTINO; el punto de partida dice SALIDA.

### 6.2 El tren
- Dos coches rectangulares con ventanas y franja de color, cuatro ruedas circulares que giran al moverse (importa para π en la Zona 5).
- Su posición es un número **exacto** (fracción con numerador y denominador), nunca un float. Se convierte a float solo para dibujar.

### 6.3 Boletos (mecánica principal)
- Cada boleto es una operación: `+3`, `−5`, `+1/2`, `+0,75`, `× (−1)`, o un **expreso** `3 saltos de 4`.
- Clic en un boleto de la mano lo pone en la **ruta** (1 a 3 casillas, según el puzzle). Clic en un boleto de la ruta lo devuelve.
- Botón **Partir** → Apuesta del andén (6.4) → el tren ejecuta la ruta boleto por boleto. Cada salto se dibuja como un arco sobre la vía con su etiqueta.
- **Acierto** → se abren las puertas, baja el pasajero, la estación se ilumina y aparece la **cuenta escrita** del viaje (`0 + 1 + 3 = 4`) y la frase que nombra el concepto.
- **Falla** → el tren queda donde cayó, silueta en el destino y la cuenta de lo que pasó al lado de la meta ("Hiciste 3 + 8 = 11. El destino era −5."). Botón Reintentar.

### 6.4 Apuesta del andén (predicción)
- Al presionar Partir aparece "Cuadrillas: ¿dónde para el tren? ¡Escríbanlo!" con una cuenta regresiva de 8 s, en grande.
- Al llegar a 0 aparece "¡Arriba los papeles!" y el tren se mueve.
- Se salta con Espacio (para el profe). Se puede desactivar en Opciones, y en ese caso no hay puntos por predicción.

### 6.5 Palanca inversora (restar)
- Desde el puzzle 2.4 el panel tiene una palanca **Restar**. Si está activa, el próximo boleto que entra en la ruta **da vuelta su flecha**: `−3` pasa a apuntar a la derecha. Así se ve que `2 − (−3) = 5`.

### 6.6 Espejo (× (−1))
- Boleto que refleja el tren al otro lado del cero, a la misma distancia (de 4 a −4). Animación: el tren se voltea pasando por el 0.

### 6.7 Divisor de tramo (fracciones)
- Un dial ◀ ▶ elige **en cuántas partes iguales** se divide cada tramo entre enteros (1 a 12). Aparecen durmientes de color en cada división.
- Otro control ◀ ▶ elige **cuántas partes avanza** el tren. El tren se mueve pieza por pieza y arriba se escribe la fracción en vivo.
- Código de color: el denominador (abajo) en el color de los durmientes y el numerador (arriba) en el color del tren, igual en la fracción escrita.
- Botón **Frenar**. Se acepta cualquier fracción equivalente (2/4 vale para 1/2) y el juego muestra la igualdad (`2/4 = 1/2`).

### 6.8 Lupa (decimales)
- Clic en la lupa y luego en un tramo: zoom, y ese tramo se divide en **10** partes rotuladas (0,1 … 0,9). Se puede hacer zoom dentro de una parte (centésimas) y una tercera vez (milésimas).
- Arriba se arma el número: **cada zoom agrega una cifra después de la coma**.
- Clic en una marca y **Frenar**. Botón "Alejar" para salir del zoom.

### 6.9 Contar saltos (dividir)
- El juego muestra un tramo (por ejemplo de 0 a 2) y un salto (por ejemplo 1/4). La cuadrilla de turno elige entre 4 alternativas **cuántos saltos caben**, y las demás predicen.
- El tren salta de a uno con un contador grande (1, 2, 3…) hasta el final del tramo.
- Muestra la cuenta: `2 : 1/4 = 8`. "Dividir es contar cuántas veces cabe un salto."

### 6.10 Sin vía (cierre simbólico de cada zona)
- La vía queda tapada con un letrero "Vía en mantención". Solo se ve la cuenta, por ejemplo `−4 − 3 = ?`, y 4 alternativas grandes (▲ ● ■ ◆).
- La cuadrilla de turno elige y las otras predicen igual que siempre.
- Después se levanta el letrero y el tren hace el viaje para **comprobar con la recta**. El pictórico pasa a ser la corrección del simbólico.

### 6.11 Compás (√2) y rueda (π), solo Zona 5
- **Compás:** sobre el tramo 0–1 hay un cuadrado de lado 1 con la diagonal marcada. Se arrastra el extremo de la diagonal y se gira con centro en el 0 hasta acostarla sobre la vía. Donde toca, aparece una marca.
- **Rueda:** la rueda del tren mide 1 tramo de diámetro y tiene un punto rojo abajo, en el 0. "Rodar una vuelta": la rueda rueda sin resbalar y el punto vuelve a tocar la vía en π (≈ 3,14159).

### 6.12 Pistas (3 niveles)
- Botón "Pista" siempre disponible, parpadea tras 2 fallas. Usar pista no quita puntos ya ganados, pero el puzzle deja de contar como "al primer intento".
- Nivel 1: hacia dónde pensar. Nivel 2: qué herramienta usar. Nivel 3: la solución marcada.

### 6.13 Control de entrada y de salida
- Las **mismas 6 preguntas** (una por zona) al inicio y al final de la sesión. Cuatro alternativas grandes; el curso vota levantando 1 a 4 dedos o tarjetas de color; 10 s de cuenta regresiva.
- Después de cada pregunta el profe marca **cuántos acertaron** con 4 botones: Casi nadie · Menos de la mitad · Más de la mitad · Casi todos.
- Las respuestas no se muestran en el control de entrada (para no "enseñar" la prueba). En el de salida sí, una por una.

### 6.14 Ritmo adaptativo
- Cada puzzle está marcado como **esencial** o **refuerzo**.
- Si en el control de entrada la pregunta de una zona quedó en "Casi nadie" o "Menos de la mitad", esa zona incluye sus puzzles de refuerzo. Si no, se juegan solo los esenciales.
- Así el tiempo se gasta donde el curso partió más débil. Desde "Elegir zona" el profe puede forzar la versión con refuerzo.

---

## 7. Zonas y puzzles

Cada zona sigue el mismo ciclo: **choque** (el hueco) → **construir** (la herramienta nueva) → **practicar** → **usar** (un pasajero con un caso real) → **sin vía** (solo símbolos).
Formato: tipo · inicio → meta · lo que hay · solución · frase · qué se nombra. **[E]** esencial, **[R]** refuerzo.

### Zona 0 · Estación Cero (1 min, 7:00)
- **0.1 Tutorial [E]** · Boletos · 0 → 2 · mano `+2`, ruta de 1 · solución `+2`. No da puntos y sirve para explicar la apuesta.
  Central: "Primer viaje de la Línea 0. Deja a la pasajera en la estación 2."
  Nombra: "Cada estación es un número. Avanzar es sumar."

### Zona 1 · Tramo Conteo · Naturales ℕ (3–4 min, ámbar, 8:00)
- **1.1 Contar [E]** · Contar · 5 pasajeros en el andén. Se toca cada uno y aparece encima 1, 2, 3, 4, 5. Luego se elige entre 4, 5, 6, 7 cuántos suben → **5**.
  Pasajero: "Somos varios. ¿Caben todos?" · Nombra: "1, 2, 3, 4, 5… son los **números naturales**: sirven para contar."
- **1.2 Sumar [R]** · Boletos · 0 → 4 · mano `+1`, `+3`, `+5`, ruta de 2 · solución `+1` `+3`.
  Pasajero: "Me bajo 4 estaciones más allá." · Nombra: "`1 + 3 = 4`. Sumar es juntar avances."
- **1.3 Orden [R]** · Boletos · 0 → 7 · mano `+2`, `+4`, `+5`, ruta de 2 · solución `+2` `+5`.
  Pasajero: "Me bajo en la séptima, porfa." · Nombra: "Los naturales también ordenan: primera, segunda… séptima."
- **1.4 Expreso [E]** · Boletos · 0 → 12 · mano `3 saltos de 4`, `2 saltos de 5`, `+1`, ruta de 1 · solución `3 saltos de 4` (3 arcos de 4).
  Central: "El expreso no para en todas: salta de 4 en 4." · Nombra: "`3 × 4 = 12`. **Multiplicar** es repetir el mismo salto."
- **1.5 Repartir [E]** · Repartir · 12 pasajeros, 3 vagones. Cada clic manda un pasajero al siguiente vagón (1, 2, 3, 1, 2, 3…). Quedan 4 por vagón. Elegir entre 3, 4, 9, 15 → **4**.
  Central: "Reparte parejo en los 3 vagones." · Nombra: "`12 : 3 = 4`. **Dividir** es repartir en partes iguales."
- **1.6 Sobra uno [R]** · Repartir · 7 pasajeros, 2 vagones → 3 por vagón y **sobra 1**. Elegir entre 3, 4, 5, 14 → 3. Muestra `7 = 2 × 3 + 1`.
  Central: "Sobra uno. A una persona no la puedes partir… a la vía sí." (anuncia la Zona 3)
- **1.7 Sin vía [E]** · `12 : 4 = ?` · 1) **3** · 2) 8 · 3) 16 · 4) 48. Comprobación: 12 pasajeros en 4 vagones.

### Zona 2 · Túnel Bajo Cero · Enteros ℤ (5 min, azul, 10:00)
- **2.1 El choque [E]** · Boletos con guion · 3 → −2 · mano `−5`, ruta de 1.
  Primer intento obligado (no cuenta para puntos): el tren retrocede 3, 2, 1, 0 y **choca con la topera**. Central: "3 − 5 no se puede… con los números que conoces."
  La topera se agrieta. Clic en ella → se abre el túnel y aparecen −1, −2, −3… en azul. Se reintenta y el tren llega a −2 (este intento sí cuenta).
  Pasajera: "Voy al −2. Dejé el auto en el estacionamiento del mall." · Nombra: "`3 − 5 = −2`. Es un **número negativo**: está 2 estaciones antes del cero."
- **2.2 Temperatura [R]** · Boletos · 3 → −5 · mano `−8`, `+8`, `−3`, ruta de 1 · solución `−8`.
  Pasajera: "En Punta Arenas había 3 °C y en la noche bajó 8 grados." (los letreros muestran °C) · Nombra: "`3 − 8 = −5`. Los negativos miden lo que está bajo cero."
- **2.3 Opuestos [E]** · Boletos · −3 → 0 · mano `+3`, `−3`, `+6`, ruta de 1 · solución `+3`.
  Pasajero: "Debía 3 lucas y las pagué. Quedé en cero." · Nombra: "`−3 + 3 = 0`. −3 y 3 son **opuestos**: juntos dan cero."
- **2.4 Palanca inversora [E]** · Boletos + palanca Restar · 2 → 5 · mano `−3`, ruta de 1 · solución: activar Restar y poner `−3`. Sin la palanca el tren va a −1.
  Pasajera: "Tenía 2 lucas contando una deuda de 3. Me perdonaron la deuda." · Nombra: "`2 − (−3) = 5`. Quitar algo negativo es como sumar."
- **2.5 Espejo [E]** · Boletos · 4 → −4 · mano `× (−1)`, `−4`, `+4`, ruta de 1 · solución `× (−1)`.
  Central: "El espejo manda el tren al otro lado del cero, a la misma distancia." · Nombra: "`4 × (−1) = −4`. Multiplicar por un negativo da vuelta el signo."
- **2.6 Menos por menos [R]** · Boletos · 0 → 6 · mano `3 saltos de (−2)`, `× (−1)`, `+3`, ruta de 2 · solución `3 saltos de (−2)` y luego `× (−1)` (0 → −6 → 6).
  Central: "Tres saltos hacia atrás, y después el espejo." · Nombra: "`(−6) × (−1) = 6`. Negativo por negativo da positivo."
- **2.7 Sin vía [E]** · `−4 − 3 = ?` · 1) **−7** · 2) −1 · 3) 1 · 4) 7. Comprobación: el tren parte en −4 y retrocede 3.

### Zona 3 · Tramo Entremedio · Fracciones ℚ (6 min, verde, 12:00)
- **3.1 La mitad [E]** · Divisor · 0 → 1/2 · dial libre (1–12) · solución: 2 partes, avanzar 1 (o equivalente).
  Primero solo hay boletos `+1`: el tren se pasa. La Central entrega el Divisor.
  Pasajero: "Me bajo justo a la mitad entre el 0 y el 1." · Nombra: "**1/2**. Abajo: en cuántas partes iguales partiste el tramo. Arriba: cuántas avanzaste. Es una **fracción**."
- **3.2 Tres cuartos [R]** · Divisor · 0 → 3/4 · solución: 4 partes, avanzar 3.
  Pasajera: "Me bajo en 3/4 del tramo. Como la receta: 3/4 de taza." · Nombra: "`3/4`: 4 partes, avanzas 3."
- **3.3 Equivalentes [E]** · Divisor con dial bloqueado en 6 · 0 → 1/2 · solución: avanzar 3.
  Central: "La parada de la mitad sigue ahí. Llega con el tramo partido en 6." · Nombra: "`3/6 = 2/4 = 1/2`. **Fracciones equivalentes**: el mismo lugar con otro nombre." (Se iluminan las tres marcas en el mismo punto.)
- **3.4 Más de un tramo [R]** · Divisor · 0 → 5/4 (vía visible 0–2) · solución: 4 partes, avanzar 5.
  Pasajero: "El viaje dura una hora y cuarto. O sea, 5/4 de hora." · Nombra: "`5/4 = 1 + 1/4`. Una fracción puede pasar del 1."
- **3.5 Sumar fracciones [E]** · Boletos · 1/4 → 3/4 · mano `+1/2`, `+1/3`, `+3/4`, ruta de 1 · solución `+1/2`.
  La vía se muestra dividida en cuartos; al aplicar `+1/2` el arco cubre 2 cuartos.
  Pasajera: "Ya hice un cuarto del camino. Me falta llegar a 3/4." · Nombra: "`1/4 + 2/4 = 3/4`. Para sumar, las partes tienen que ser del mismo tamaño."
- **3.6 Expreso de fracciones [E]** · Boletos · 0 → 3/2 · mano `3 saltos de 1/2`, `2 saltos de 1/2`, `3 saltos de 1/3`, ruta de 1 · solución `3 saltos de 1/2`.
  Central: "El expreso chico salta de a medio tramo." · Nombra: "`3 × 1/2 = 3/2`. Multiplicar una fracción también es repetir el salto."
- **3.7 ¿Cuántos caben? [E]** · Contar saltos · tramo 0 → 2, salto 1/4 · alternativas 1) 4 · 2) 6 · 3) **8** · 4) 1/2.
  Pasajero: "Cada parada de buses está a 1/4 de tramo. ¿Cuántas hay hasta el 2?" · Nombra: "`2 : 1/4 = 8`. Dividir por una fracción es contar cuántas veces cabe."
- **3.8 Sin vía [E]** · `3 × 1/4 = ?` · 1) 3/12 · 2) **3/4** · 3) 4/3 · 4) 1/12. Comprobación: 3 saltos de 1/4.

### Zona 4 · La Lupa · Decimales (5 min, verde claro, 14:00)
Los decimales son racionales, así que esta zona usa el color de ℚ en un tono más claro.
- **4.1 Zoom [E]** · Lupa · 0 → 0,7 · solución: zoom en 0–1, marca 7.
  Central: "Prueba la lupa sobre el primer tramo." · Nombra: "**0,7** = 7/10. La cifra después de la coma dice en cuál de las 10 partes estás."
- **4.2 Dos zooms [E]** · Lupa · 0 → 0,35 · solución: zoom, marca 3, zoom, marca 5.
  Pasajero: "Me bajo en 0,35. Ni un poquito antes." · Nombra: "`0,35 = 35/100`. Cada zoom agrega una cifra."
- **4.3 Notas [R]** · Lupa · vía 1–7 con letreros "nota 1,0" … "nota 7,0" · meta 5,5 · solución: zoom en 5–6, marca 5.
  Pasajera: "Saqué 5,5 de promedio. Llévame a esa estación." · Nombra: "Los decimales miden lo que queda entre dos enteros: notas, estatura, temperatura."
- **4.4 Fracción a decimal [E]** · Lupa · 0 → letrero "1/4" · solución: zoom, marca 2, zoom, marca 5 → 0,25.
  Pasajero: "Compré 1/4 de kilo de pan. ¿Cuánto marca la pesa?" · Nombra: "`1/4 = 0,25`. Fracción y decimal: dos nombres del mismo lugar."
- **4.5 El que no termina [E]** · Lupa con guion · 0 → letrero "1/3".
  Zoom: la meta cae entre 0,3 y 0,4 → entre 0,33 y 0,34 → entre 0,333 y 0,334. Tras el tercer zoom la Central dice "Siempre otro 3. Para siempre." y aparece el Divisor con dial en 3: avanzar 1 → llega exacto.
  Pasajera: "Somos 3 y compramos 1 bebida para compartir." · Nombra: "`1/3 = 0,333…`: un **decimal periódico**. El 3 se repite sin fin, pero la fracción lo nombra exacto."
- **4.6 Sumar decimales [R]** · Boletos · 1,5 → 2,25 · mano `+0,75`, `−0,2`, `+2,1`, ruta de 1 · solución `+0,75`.
  Pasajero: "Llevo 1,5 km caminando y me faltan 0,75." · Nombra: "`1,5 + 0,75 = 2,25`. Se suman las cifras que están en el mismo zoom."
- **4.7 Expreso decimal [E]** · Boletos · 0 → 1 · mano `4 saltos de 0,25`, `4 saltos de 0,2`, `2 saltos de 0,25`, ruta de 1 · solución `4 saltos de 0,25`.
  Central: "Cuatro saltos de 0,25. ¿Hasta dónde llega?" · Nombra: "`4 × 0,25 = 1`. Igual que 4 × 1/4."
- **4.8 ¿Cuántos caben? [R]** · Contar saltos · tramo 0 → 3, salto 0,5 · alternativas 1) **6** · 2) 1,5 · 3) 15 · 4) 3,5.
  Nombra: "`3 : 0,5 = 6`. En 3 tramos caben 6 medios."
- **4.9 Sin vía [E]** · `0,5 + 0,25 = ?` · 1) 0,30 · 2) **0,75** · 3) 0,525 · 4) 7,5. Comprobación: el tren parte en 0,5 y avanza 0,25 con la lupa.

### Zona 5 · Estación Sin Nombre · Reales ℝ (4 min, magenta, 16:00)
- **5.1 La diagonal [E]** · Compás con guion · 0 → √2.
  Sobre el tramo 0–1 hay un cuadrado de lado 1. Pasajero: "Mi casa queda donde cae la diagonal del cuadrado si la acuestas en la vía."
  Paso 1: se gira la diagonal con el compás y aparece la marca entre 1 y 2.
  Paso 2: Central: "Intenta llegar con el divisor." Nunca calza: se muestra la distancia (con 7/5 falta 0,014…; la más cercana con dial ≤ 12 es 17/12, que se pasa por 0,002…).
  Paso 3: Central: "Prueba con la lupa." Zoom: 1,4 → 1,41 → 1,414 → 1,4142… las cifras siguen y no se repiten.
  Paso 4: se habilita "Frenar en la marca del compás" → llega.
  Nombra: "**√2 = 1,41421356…** Ninguna fracción cae justo ahí y sus cifras nunca se repiten. Es un **número irracional**."
- **5.2 La rueda [E]** · Rueda · 0 → π.
  Pasajera: "Me bajo donde la rueda del tren completa una vuelta."
  "Rodar una vuelta" → el punto rojo toca la vía en 3,14159… La lupa muestra 3,1 → 3,14 → 3,141 → 3,1415… sin patrón. "Frenar en la marca."
  Nombra: "**π = 3,14159…** Lo que avanza una rueda de diámetro 1 en una vuelta. También es irracional."
- **5.3 El plano de la red [E]** · Clasificar · arrastrar 6 tarjetas al plano de 4 líneas anidadas: `7`, `−2`, `3/4`, `0,35`, `√2`, `π`. Cada cuadrilla de turno clasifica una o dos tarjetas.
  Regiones: ℕ Naturales (adentro) ⊂ ℤ Enteros ⊂ ℚ Racionales ⊂ ℝ Reales (afuera). Entre ℚ y el borde de ℝ está la franja **Irracionales**.
  Respuestas (región más chica que contiene al número): 7 → ℕ · −2 → ℤ · 3/4 → ℚ · 0,35 → ℚ · √2 → Irracionales · π → Irracionales.
  Al soltar bien una tarjeta se encienden también los anillos de afuera ("7 también es entero, racional y real").
  Nombra: "Todos son **números reales**. Juntos llenan la vía completa, sin un solo hueco."

### Zona 6 · Hora Punta · Operaciones combinadas (5 min, todos los colores, 18:00)
Letrero fijo arriba durante toda la zona: **PRIORIDAD → 1° ( ) · 2° × : · 3° + −**
Central: "Hora punta. Los expresos (× y :) pasan primero. Lo que va entre paréntesis tiene vía reservada."
Mecánica: la expresión aparece como una fila de vagones, cada operación es un vagón con su signo. La cuadrilla de turno indica qué vagón **parte primero**, luego el siguiente, hasta que queda un solo número. Cada paso mueve el tren y la expresión se reduce en pantalla. Las demás cuadrillas predicen el resultado final.
Si el orden es incorrecto, el juego lo ejecuta igual, muestra el resultado distinto y una señal roja: "Choque de horarios: el expreso tenía prioridad". Reintentar.
- **6.1 [E]** `2 + 3 × 4` · `3 × 4 = 12`, luego `2 + 12` → **14** (el orden equivocado da 20).
- **6.2 [E]** `(2 + 3) × 4` · paréntesis primero: `5 × 4` → **20**.
- **6.3 [R]** `−8 + 12 : 4` · `12 : 4 = 3`, luego `−8 + 3` → **−5**.
- **6.4 Serpiente [E]** (actividad 2a del programa, p. 70) · en orden, sin prioridad: parte en `−10` y sigue `: 5`, `− 2`, `· (−3)`, `+ 0,5`.
  Antes de partir todas las cuadrillas predicen la estación final; la de turno elige entre 1) **12,5** · 2) −12,5 · 3) 11,5 · 4) 6,5. El tren recorre −10 → −2 → −4 → 12 → 12,5.
- **6.5 Último servicio [E]** · `1/2 · (−4) + 0,5 · 6` · primero las dos multiplicaciones (en cualquier orden): `1/2 · (−4) = −2` y `0,5 · 6 = 3`, después `−2 + 3` → **1**.
  Central: "Último pasajero antes de la inauguración."
- **6.6 Sin vía [E]** · `(−2) · 3 + 10 : 2 = ?` · 1) **−1** · 2) 2 · 3) 7 · 4) −11. (2 es el error de ir de izquierda a derecha.)

### Final
Corte de cinta, papel picado, plano completo, cuadrilla ganadora y reporte.

---

## 8. Plan de clase y gameloop

### 8.1 Bloque de 45 minutos
| Min | Qué pasa | Quién |
|---|---|---|
| 0–3 | Arman 4 cuadrillas, eligen al primer maquinista, el profe cuenta la premisa en una frase | Profe |
| 3–6 | **Control de entrada** (6 preguntas) | Curso |
| 6–34 | Juego: Zonas 0 a 6 | Cuadrillas |
| 34–38 | **Control de salida** (las mismas 6) | Curso |
| 38–45 | Cierre: reporte en pantalla y 3 preguntas para conversar | Profe |

Preguntas de cierre (en la pantalla final):
1. ¿Por qué hicieron falta los números negativos?
2. ¿Qué diferencia hay entre 0,333… y 1,4142…?
3. ¿Dónde vieron hoy decimales o fracciones fuera del colegio?

### 8.2 Gameloop
```
Menú ─► Cuadrillas ─► Control de entrada ─► Zona 0
  ─► [ Zona n: anuncio de la Central (1 línea)
        ─► por cada puzzle (esenciales + refuerzos si el curso partió débil):
             cuadrilla de turno decide → maquinista arma → Partir
             → Apuesta del andén (todas predicen) → el tren se mueve
             → acierto: concepto nombrado + cuenta escrita
               falla: silueta + cuenta de lo que pasó → reintentar
             → ¿Quién predijo bien? → puntos → siguiente cuadrilla
        ─► Sin vía (cierre simbólico)
        ─► Cambio de maquinista ] ×6
  ─► Control de salida ─► Final: plano, ganadora, reporte, preguntas de cierre
```

### 8.3 Tiempos objetivo del juego
| Zona | Solo esenciales | Con refuerzo |
|---|---|---|
| 0 | 1 | 1 |
| 1 Naturales | 3 | 5 |
| 2 Enteros | 4 | 5,5 |
| 3 Fracciones | 5 | 6,5 |
| 4 Decimales | 5 | 7 |
| 5 Reales | 4 | 4 |
| 6 Hora punta | 4 | 5 |
| **Total** | **26** | 34 (nunca pasa porque no todas las zonas parten débiles) |

Si las pruebas pasan de 28 min con un curso típico, se acortan los textos de la Central y la animación de la Apuesta (8 → 6 s).

### 8.4 Menú principal
- Jugar (cuadrillas → control de entrada → Zona 0)
- Elegir zona (todas desbloqueadas, con interruptor "con refuerzo"; sirve para el profe y para presentar el prototipo)
- Opciones: Apuesta del andén on/off, modo proyector on/off, **Sin cuadrillas** (juega una sola persona: sin marcador, sin turnos ni "¿Quién predijo bien?"; todo lo demás igual), volumen
- Salir

---

## 9. Evaluación

### 9.1 Reporte final (pantalla para que el profe le saque foto)
- **Entrada vs. salida** por pregunta: barra de 4 niveles (Casi nadie → Casi todos) antes y después.
- Por zona: % de puzzles logrados al primer intento, pistas usadas, si se jugó con refuerzo.
- Predicciones: cuántas cuadrillas acertaron en promedio por puzzle.
- Puntaje de cada cuadrilla y tiempo total.

### 9.2 Qué prueba este prototipo
| Hipótesis | Cómo se mide en el reporte |
|---|---|
| H1. Con predicción obligatoria y puntos al primer intento, el curso piensa la cuenta en vez de probar boletos al azar | % de puzzles logrados al primer intento por zona |
| H2. Todo el curso participa aunque haya un solo PC | Promedio de cuadrillas que predicen bien por puzzle (y observación del profe) |
| H3. El curso entiende más al final que al principio | Diferencia entre control de entrada y de salida |
| H4. La experiencia cabe en un bloque de 45 minutos | Tiempo total jugado |

### 9.3 Cobertura de indicadores
| Indicador (p. 68) | Dónde |
|---|---|
| Identifican el tipo de número | 5.3, control 5 |
| Operaciones mixtas con jerarquía y paréntesis | Zona 6, control 6 |
| Lenguaje natural → expresión matemática | Todos los pedidos de pasajeros se traducen a boletos |
| Operaciones con racionales (suma, multiplicación, división) | 3.5–3.8, 4.6–4.9, 6.5 |

### 9.4 Preguntas del control (entrada y salida)
1. "Un expreso da 5 saltos de 3 desde el 0. ¿En qué estación queda?" · 1) 8 · 2) **15** · 3) 53 · 4) 2
2. "Estás en el piso −1 del mall y subes 3 pisos. ¿En qué piso quedas?" · 1) −4 · 2) **2** · 3) 4 · 4) −2
3. "Divides el tramo en 8 partes y avanzas 4. ¿Qué fracción es igual?" · 1) 1/4 · 2) 4/4 · 3) **1/2** · 4) 8/4
4. "¿Cuál es igual a 3/10?" · 1) 3,10 · 2) **0,3** · 3) 0,03 · 4) 30
5. "¿Cuál NO se puede escribir como fracción?" · 1) 0,5 · 2) −3 · 3) **√2** · 4) 0,333…
6. "¿Cuánto es 6 − 2 × 3?" · 1) 12 · 2) **0** · 3) −12 · 4) 6

---

## 10. Estilo visual y audio

**Referente:** señalética de transporte público (letreros planos, píldoras de color, tipografía clara). **Sin** logos, nombres ni colores oficiales de Metro de Santiago. La Línea 0 es ficticia. Hay un mockup de referencia en `docs/mockup_zona2.png`.

**Evitar:** degradados, brillos neón, glassmorphism, bordes redondeados en todo, íconos genéricos. Plano y legible desde el fondo de la sala.

**Paleta** (un color por conjunto, como las líneas de un plano de Metro). Fondo claro y de día: números oscuros sobre fondo claro se leen mejor con proyector en una sala iluminada.
| Uso | Hex |
|---|---|
| Cielo (fondo) | `#EAF4FB` |
| Ciudad de fondo (siluetas planas) | `#CFDCE6` |
| Túnel negativo (izquierda del 0, iluminado) | `#D6E6FA` |
| Vía y durmientes | `#5B6470` |
| Texto | `#1E2228` |
| Paneles de UI | `#FFFFFF` con borde `#D3DDE6` |
| ℕ Naturales | `#F2A900` |
| ℤ Enteros | `#2F80ED` |
| ℚ Racionales | `#27AE60` (decimales: `#6FCF97`) |
| ℝ Irracionales | `#D6336C` |
| Error / silueta | `#E5484D` (silueta del tren al 40 % de opacidad) |
| Cuadrillas A ▲ · B ● · C ■ · D ◆ | `#F2C94C` · `#2F80ED` · `#27AE60` · `#E5484D` |

Píldoras de estación: fondo del color del conjunto. Número en blanco sobre azul, verde y magenta; en `#1E2228` sobre ámbar y verde claro (el blanco no se lee ahí).

**Tipografía:** Atkinson Hyperlegible (Google Fonts, licencia OFL, gratis), hecha para máxima legibilidad. Si no se puede descargar, la fuente por defecto de TextMeshPro.

**Arte:** todo con primitivas (rectángulos, círculos, líneas). Los pasajeros son siluetas de cápsula con un color distinto cada uno.

**Assets:** nada generado con IA. Arte con primitivas; todo lo demás de bibliotecas gratis con licencia libre (Kenney, OpenGameArt CC0, Google Fonts), con créditos en `CREDITOS.md`.

**Audio (opcional):** sonidos CC0 de Kenney (kenney.nl, "Interface Sounds" e "Impact Sounds"). Campana al abrir puertas, golpe sordo en la topera, chirrido de frenos, "tic" en la cuenta regresiva. Ningún contenido depende del sonido.

---

## 11. Alcance del prototipo

**Sí:** gameloop completo de inicio a fin con cuadrillas, controles de entrada y salida, las 6 zonas jugables, ritmo adaptativo, reporte final y menú con selección de zona.
**No:** guardado entre sesiones, cuentas, idiomas, cinemáticas, personajes animados, voces, 3D, tienda, multijugador en red, dispositivos de los alumnos.

---

## 12. Aprender a jugar sin que nadie explique (fase 2, 13 de noviembre)

**Objetivo:** que un profe o un alumno reciba solo el `.exe` y, sin leer nada aparte, entienda en menos de 5 minutos de qué se trata el juego, cómo se mueve el tren y cómo se resuelve cada desafío.

**Problema de la versión actual (build-0801):** el "Cómo jugar" explica la dinámica de la sala (cuadrillas, hoja, puntos), pero no cómo se juega un puzzle. Cada herramienta nueva aparece con una sola frase, y la opción para jugar solo está escondida en Opciones.

### 12.1 Principios
1. **Se aprende haciendo.** Cada paso de la guía espera la acción real: el jugador toca el boleto de verdad, no una imagen de un boleto.
2. **Una cosa por paso.** Una línea de texto (máximo 70 caracteres) y un solo objetivo señalado.
3. **Se muestra una vez.** Cada guía aparece la primera vez que hace falta. Se puede volver a ver con el botón "?".
4. **Nunca encierra.** Siempre está "Ya sé jugar" para saltarse la guía.

### 12.2 La guía (sistema)
- **Foco:** la pantalla se oscurece (`#1E2228` al 55 %) salvo un hueco alrededor del objetivo. Ese hueco tiene un marco ámbar (`#F2A900`, 4 px) que late suave.
- **Mano:** un puntero dibujado con primitivas (blanco con borde oscuro) que toca el objetivo una y otra vez. Para lo que se arrastra (compás, tarjetas), la mano hace el recorrido.
- **Globo:** tarjeta blanca junto al objetivo con la línea de texto (40 px a 1080p), "Paso 2 de 4" en gris y, solo si el paso no pide una acción, un botón "Seguir".
- **"Ya sé jugar":** arriba a la derecha, siempre visible durante la guía.
- **Bloqueo:** mientras dura un paso, solo responde el objetivo. Lo demás no se puede tocar.
- **Memoria:** cada guía vista se guarda en el PC (`PlayerPrefs`). Es la única excepción a "sin guardado" de la sección 11. En Opciones: "Guías: Sí/No" y "Volver a mostrar las guías".
- Referencia visual: `docs/mockup_guia.png`.

### 12.3 Entrada al juego: dos caminos
El menú parte con dos botones grandes:
- **"Jugar con el curso"** · "Profe con proyector · 4 cuadrillas · 45 min". Es el modo actual: cuadrillas, hoja, controles votados por el curso.
- **"Jugar solo"** · "A tu ritmo · unos 30 min". Sin cuadrillas, sin marcador por equipos y sin "¿Quién predijo bien?". Los controles de entrada y salida los responde el mismo jugador (el juego registra si acertó). La Apuesta del andén dice "Piensa dónde va a parar" y dura 3 segundos.

Abajo, más chicos: "Para el profe", "Modo demo", "Elegir zona", "Opciones". La opción "Sin cuadrillas" sale de Opciones porque ahora la decide la entrada.

El "Cómo jugar" de 4 láminas queda solo para "Jugar con el curso" (explica la sala). En "Jugar solo" no se muestra: la guía de la Zona 0 enseña directamente.

### 12.4 Zona 0 como tutorial guiado (~2 min)
**0.1** (ya existe: 0 → 2 con `+2`):
1. Objetivo: la estación DESTINO · "La pasajera va a la estación 2: la del anillo rojo." · Acción: tocar el destino.
2. Objetivo: el boleto `+2` · "Los boletos mueven el tren. Toca el +2." · Acción: el boleto entra a la ruta.
3. Objetivo: Partir · "Ahora parte." · Acción: tocar Partir.
4. Durante la Apuesta (solo modo curso), objetivo: la cuenta regresiva · "Antes de partir, el curso anota dónde va a parar el tren." · Avanza solo.
5. Al llegar, objetivo: la cuenta escrita · "Llegaste. Así se escribe lo que hiciste: 0 + 2 = 2." · Botón Seguir.

**0.2 (nuevo):** Boletos · 0 → 5 · mano `+2`, `+3`, `+4`, ruta de 2 · solución `+2` `+3` · sin puntos.
- Pasajero: "Voy a la estación 5. ¿Me llevas?"
- Guía, objetivo: las dos casillas de la ruta · "Ahora caben dos boletos. Elige los que juntos lleguen a 5."
- Al acertar, nombra: "`2 + 3 = 5`. Puedes juntar varios boletos en un viaje."
- Cálculo verificado: con dos boletos de {+2, +3, +4} solo `+2 +3` llega a 5 (2 + 4 = 6, 3 + 4 = 7).

### 12.5 Avisos de primera vez (por evento)
Aparecen solos la primera vez que pasa algo, con foco en lo que corresponde y botón Seguir:
| Evento | Texto |
|---|---|
| Primera falla | "El tren quedó donde cayó y te dice cuánto faltó. Reintentar es gratis." |
| La Pista empieza a parpadear | "¿Atascado? Pide una pista. Hay tres por puzzle." |
| Primer "¿Quién predijo bien?" (curso) | "Marca las cuadrillas que anotaron bien dónde paró el tren." |
| Primer control de entrada (curso) | "Voten con los dedos: 1, 2, 3 o 4. El profe marca cuántos acertaron." |
| Primer control de entrada (solo) | "Elige tu respuesta. La misma pregunta vuelve al final del juego." |
| Primer Sin vía | "Aquí la vía está tapada. Resuelve con números y después el tren comprueba." |
| Primera Serpiente (6.4) | "Aquí no hay prioridad: el tren hace cada paso en orden." |

### 12.6 Guía de cada herramienta (primera vez que aparece)
| Puzzle | Pasos (objetivo · texto · acción) |
|---|---|
| 1.1 Contar | primer pasajero · "Toca a cada pasajero para contarlo." · tocar uno |
| 1.4 Expreso | boleto "3 saltos de 4" · "Este boleto repite el mismo salto: 3 saltos de 4." · ponerlo en la ruta |
| 1.5 Repartir | primer pasajero · "Toca un pasajero: sube al vagón que sigue." · tocar uno |
| 2.1 Topera | el muro agrietado (después del choque) · "Toca el muro del cero." · tocarlo |
| 2.4 Palanca | 1) la palanca · "Activa Restar: el próximo boleto se da vuelta." · activarla. 2) el `−3` · "Pon el −3 y mira cómo gira su flecha." · ponerlo |
| 2.5 Espejo | boleto `× (−1)` · "El espejo lleva el tren al otro lado del cero." · ponerlo |
| 3.1 Divisor | 1) ▶ de "Partes del tramo" · "Corta el tramo en 2 partes iguales." · valor 2. 2) ▶ de "Partes que avanza" · "Avanza 1 parte." · valor 1. 3) Frenar · "Frena aquí." · tocar |
| 3.7 Contar saltos | las alternativas · "Elige cuántos saltos caben. El tren salta y los cuenta." · elegir |
| 4.1 Lupa | 1) la lupa · "La lupa hace zoom en un tramo." · tocarla. 2) tramo 0–1 · "Toca el tramo entre 0 y 1." · zoom. 3) marca 7 · "Cada marca es una décima. Toca la 7." · tocarla. 4) Frenar |
| 5.1 Compás | punta de la diagonal (gesto arrastrar) · "Arrastra la punta de la diagonal hasta la vía." · diagonal acostada |
| 5.2 Rueda | "Rodar una vuelta" · "Haz rodar la rueda una vuelta completa." · tocar |
| 5.3 Clasificar | primera tarjeta (gesto arrastrar a su línea) · "Lleva cada número a la línea más chica donde cabe." · una tarjeta ubicada |
| 6.1 Jerarquía | el vagón `×` · "Los expresos (× y :) parten primero. Toca el ×." · tocarlo |

### 12.7 Botón "?" (siempre visible, junto a Pista)
Abre una tarjeta con:
- **Lo que hay que lograr:** la frase del pasajero y el destino.
- **Cómo se usa:** una o dos líneas según el tipo de puzzle:
  | Tipo | Texto |
  |---|---|
  | Boletos | "Pon boletos en la ruta y presiona Partir. Cada boleto mueve el tren." |
  | Contar | "Toca a cada pasajero. El número que aparece es cuántos van." |
  | Repartir | "Toca los pasajeros: suben por turno a cada vagón." |
  | Divisor | "Elige en cuántas partes iguales se corta el tramo y cuántas avanza el tren. Después frena." |
  | Lupa | "Toca la lupa y un tramo para hacer zoom. Cada zoom agrega una cifra después de la coma." |
  | ContarSaltos | "Elige cuántos saltos del tamaño indicado caben en el tramo." |
  | SinVia | "Resuelve la cuenta y elige la alternativa. Después el tren comprueba." |
  | Compas | "Arrastra la punta de la diagonal hasta acostarla sobre la vía." |
  | Rueda | "Haz rodar la rueda una vuelta y frena donde quedó el punto rojo." |
  | Clasificar | "Arrastra cada número a la línea más chica que lo contiene." |
  | Jerarquia | "Toca los vagones en el orden en que parten: ( ), después × :, después + −." |
  | Serpiente | "Predice dónde termina. El tren hace cada paso en orden." |
- Botón **"Ver cómo se hace"**: repite la guía de esa herramienta (12.6).
- No cuenta como pista ni quita puntos.

### 12.8 Pantalla "Para el profe" (desde el menú)
Una sola pantalla, sin scroll:
- **Qué enseña:** números naturales, enteros, fracciones, decimales, una primera mirada a los reales y operaciones combinadas. Matemática 1° medio, Unidad 1, OA 1.
- **Cuánto dura:** unos 30 minutos de juego en un bloque de 45.
- **Cómo se juega en sala:** un PC con proyector y el curso en 4 cuadrillas que se turnan. La de turno decide el viaje y las otras anotan en su hoja dónde va a parar el tren.
- **Qué mide:** las mismas 6 preguntas al inicio y al final, más el reporte por zona.
- **Si algo se traba:** "Saltar" (arriba a la derecha) pasa al siguiente puzzle y "?" explica el actual.
- Botón **"Abrir la hoja para imprimir"**: abre `Linea0_Hoja_de_cuadrilla.pdf`, incluido en la build.
- Pie: "Grupo BulletHelp: Darío Valdebenito, Bastián Allende y Alejandro Gómez".

### 12.9 Cómo sabremos que funciona
- **Prueba de los 5 minutos:** una persona que nunca vio el juego, solo con el `.exe`, completa la Zona 0 y la Zona 1 sin preguntar nada. Se prueba con alguien de verdad antes de entregar.
- Cada herramienta nueva tiene su guía la primera vez.
- Ningún texto de guía pasa de 70 caracteres ni de una línea a 1080p.
