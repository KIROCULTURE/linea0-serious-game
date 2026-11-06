using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Linea0.Datos;
using Linea0.Puzzles;
using Linea0.Sala;
using Linea0.Vista;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Juego
{
    public enum Estado { Menu, Cuadrillas, ControlEntrada, AnuncioZona, Puzzle, CambioMaquinista, ControlSalida, Final }

    public class Opciones
    {
        public bool apuesta = true;
        public bool proyector = true;
    }

    /// <summary>Máquina de estados del juego: Menú → Cuadrillas → Control de entrada → zonas → Control de salida → Final.</summary>
    public class GameManager : MonoBehaviour
    {
        public Pantalla P;
        public Sesion S = new Sesion();
        public Opciones opciones = new Opciones();
        public Estado estado;
        public bool auto;
        public bool tunelAbierto;
        public int fallasPuzzle;
        bool saltar;
        string dirCapturas;
        PuzzleBase actual;
        bool sinCuadrillasOpcion;

        static readonly HashSet<TipoPuzzle> Implementados = new HashSet<TipoPuzzle>
        {
            TipoPuzzle.Boletos, TipoPuzzle.Contar, TipoPuzzle.Repartir, TipoPuzzle.SinVia, TipoPuzzle.Divisor,
            TipoPuzzle.Lupa, TipoPuzzle.Compas, TipoPuzzle.Jerarquia, TipoPuzzle.Serpiente, TipoPuzzle.ContarSaltos
        };

        void Start()
        {
            var args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-auto") auto = true;
                if (args[i] == "-capturas" && i + 1 < args.Length) { dirCapturas = args[i + 1]; Directory.CreateDirectory(dirCapturas); }
            }
            UI.AsegurarEventSystem();
            P = Pantalla.Crear();
            P.pista.button.onClick.AddListener(Pista);
            P.saltar.button.onClick.AddListener(() => StartCoroutine(ConfirmarSaltar()));
            P.MostrarJuego(false);
            if (auto) StartCoroutine(Partida(Enumerable.Range(0, 7).ToList(), true, true));
            else StartCoroutine(Menu());
        }

        void Update()
        {
            // la pista parpadea tras 2 fallas
            if (P != null && P.pista != null)
            {
                bool parpadea = fallasPuzzle >= 2 && estado == Estado.Puzzle;
                P.pista.fondo.color = parpadea && Mathf.Repeat(Time.time, 0.8f) < 0.4f ? Paleta.Sol : Color.white;
            }
        }

        public void Captura(string nombre)
        {
            if (dirCapturas == null) return;
            ScreenCapture.CaptureScreenshot(Path.Combine(dirCapturas, nombre.Replace('.', '_') + ".png"));
        }

        // ------------------------------------------------------------------ pantallas de menú

        RectTransform PantallaCompleta(string nombre)
        {
            var raiz = UI.Llenar(UI.Nodo(P.overlay, nombre));
            var f = UI.Caja(raiz, "Fondo", Paleta.Cielo); UI.Llenar(f.rectTransform);
            return raiz;
        }

        UI.Boton BotonMenu(Transform padre, string texto, float y, Action a, bool proximamente = false)
        {
            var b = UI.CrearBoton(padre, proximamente ? texto + " · Próximamente" : texto, 50, proximamente ? Paleta.Borde : Paleta.Texto, () => { if (!proximamente) a(); });
            UI.Ubicar(b.rt, new Vector2(0.5f, 0.5f), new Vector2(0, y), new Vector2(720, 104));
            if (proximamente) b.texto.color = Paleta.Gris;
            return b;
        }

        IEnumerator Menu()
        {
            estado = Estado.Menu;
            P.MostrarJuego(false);
            UI.Limpiar(P.overlay);
            var raiz = PantallaCompleta("Menu");
            // vía decorativa
            for (int i = 0; i < 4; i++)
            {
                var l = UI.Caja(raiz, "Linea", new[] { Paleta.Naturales, Paleta.Enteros, Paleta.Racionales, Paleta.Irracionales }[i], UI.Pildora);
                UI.Ubicar(l.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -250 - i * 20), new Vector2(900 - i * 120, 12));
            }
            var t = UI.Texto(raiz, "Línea 0", 150, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(1200, 180));
            var st = UI.Texto(raiz, "Inauguración de la línea más nueva de Santiago", 40, Paleta.Via, TextAnchor.MiddleCenter, false);
            UI.Ubicar(st.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -360), new Vector2(1400, 60));
            string elegido = null;
            BotonMenu(raiz, "Jugar", 40, () => elegido = "jugar");
            BotonMenu(raiz, "Elegir zona", -80, () => elegido = "zona");
            BotonMenu(raiz, "Opciones", -200, () => elegido = "opciones");
            BotonMenu(raiz, "Modo demo", -320, () => elegido = "demo");
            BotonMenu(raiz, "Salir", -440, () => Application.Quit());
            Captura("menu");
            while (elegido == null) yield return null;
            Destroy(raiz.gameObject);
            if (elegido == "jugar") yield return Partida(Enumerable.Range(0, 7).ToList(), true, false);
            else if (elegido == "zona") yield return ElegirZona();
            else if (elegido == "opciones") yield return MenuOpciones();
            else if (elegido == "demo") yield return Partida(new List<int> { 2 }, false, true);
        }

        IEnumerator MenuOpciones()
        {
            var raiz = PantallaCompleta("Opciones");
            var t = UI.Texto(raiz, "Opciones", 90, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -110), new Vector2(1000, 120));
            bool listo = false;
            UI.Boton bA = null, bC = null, bP = null, bF = null;
            void Refrescar()
            {
                bA.texto.text = "Apuesta del andén: " + (opciones.apuesta ? "Sí" : "No");
                bC.texto.text = "Sin cuadrillas: " + (sinCuadrillasOpcion ? "Sí" : "No");
                bP.texto.text = "Modo proyector: " + (opciones.proyector ? "Sí" : "No");
                bF.texto.text = "Pantalla completa: " + (Screen.fullScreen ? "Sí" : "No") + "  (Alt+Enter)";
                P.escalador.referenceResolution = opciones.proyector ? new Vector2(1920, 1080) : new Vector2(2200, 1238);
            }
            bA = BotonMenu(raiz, "", 160, () => { opciones.apuesta = !opciones.apuesta; Refrescar(); });
            bC = BotonMenu(raiz, "", 40, () => { sinCuadrillasOpcion = !sinCuadrillasOpcion; Refrescar(); });
            bP = BotonMenu(raiz, "", -80, () => { opciones.proyector = !opciones.proyector; Refrescar(); });
            bF = BotonMenu(raiz, "", -200, () => { Screen.fullScreen = !Screen.fullScreen; StartCoroutine(RefrescarLuego(Refrescar)); });
            foreach (var b in new[] { bA, bC, bP, bF }) b.rt.sizeDelta = new Vector2(1000, 104);
            BotonMenu(raiz, "Volver", -360, () => listo = true);
            Refrescar();
            while (!listo) yield return null;
            Destroy(raiz.gameObject);
            yield return Menu();
        }

        IEnumerator RefrescarLuego(Action a) { yield return null; yield return null; a(); }

        List<Puzzle> PuzzlesDe(int zona, bool refuerzo) =>
            CargadorNiveles.PuzzlesDeZona(zona, refuerzo).Where(p => Implementados.Contains(p.Tipo)).ToList();

        IEnumerator ElegirZona()
        {
            var raiz = PantallaCompleta("ElegirZona");
            var t = UI.Texto(raiz, "Elegir zona", 80, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(1000, 110));
            int elegida = -1; bool volver = false; bool refuerzo = false;
            UI.Boton bR = null;
            for (int z = 0; z < 7; z++)
            {
                int zz = z;
                var zona = CargadorNiveles.Zona(z);
                bool hay = PuzzlesDe(z, true).Count > 0;
                var col = Paleta.Hex(zona.color);
                string txt = "Zona " + z + " · " + zona.nombre + (string.IsNullOrEmpty(zona.conjunto) ? "" : " · " + zona.conjunto);
                var b = UI.CrearBoton(raiz, txt, 40, hay ? col : Paleta.Borde, () => { if (hay) elegida = zz; });
                UI.Ubicar(b.rt, new Vector2(0.5f, 1), new Vector2(0, -230 - z * 98), new Vector2(1100, 84));
                if (!hay) b.texto.text = txt + " · Próximamente";
            }
            bR = UI.BotonContorno(raiz, "Con refuerzo: No", 40, () => { refuerzo = !refuerzo; bR.texto.text = "Con refuerzo: " + (refuerzo ? "Sí" : "No"); });
            UI.Ubicar(bR.rt, new Vector2(0.5f, 0), new Vector2(-300, 70), new Vector2(520, 96));
            var bv = UI.BotonContorno(raiz, "Volver", 40, () => volver = true);
            UI.Ubicar(bv.rt, new Vector2(0.5f, 0), new Vector2(300, 70), new Vector2(400, 96));
            while (elegida < 0 && !volver) yield return null;
            Destroy(raiz.gameObject);
            if (volver) { yield return Menu(); yield break; }
            if (refuerzo) S.refuerzoForzado.Add(elegida);
            yield return Partida(new List<int> { elegida }, false, false);
        }

        // ------------------------------------------------------------------ partida

        IEnumerator Partida(List<int> zonas, bool conControles, bool demoAuto)
        {
            var refuerzoForzado = S.refuerzoForzado;
            S = new Sesion { conCuadrillas = !sinCuadrillasOpcion };
            S.refuerzoForzado = refuerzoForzado;
            tunelAbierto = zonas[0] > 2;
            if (demoAuto && !auto) { S.conCuadrillas = true; S.CargarEjemplo(); S.refuerzoForzado.Add(2); }
            else if (S.conCuadrillas) yield return Cuadrillas();
            S.PrimerTurno();
            if (conControles) { estado = Estado.ControlEntrada; yield return Pantallas.Control(this, false); }
            for (int i = 0; i < zonas.Count; i++)
            {
                int z = zonas[i];
                bool refuerzo = S.ConRefuerzo(z);
                var lista = PuzzlesDe(z, refuerzo);
                if (lista.Count == 0) continue;
                if (refuerzo && lista.Any(p => !p.esencial)) S.zonasJugadasConRefuerzo.Add(z);
                yield return AnuncioZona(z);
                foreach (var p in lista)
                {
                    yield return JugarPuzzle(p);
                    S.SiguienteTurno();
                }
                if (i < zonas.Count - 1 && S.conCuadrillas)
                {
                    estado = Estado.CambioMaquinista;
                    yield return Pantallas.CambioMaquinista(this, "Zona " + zonas[i + 1]);
                }
            }
            if (conControles) { estado = Estado.ControlSalida; P.MostrarJuego(false); yield return Pantallas.Control(this, true); }
            yield return Final();
            if (auto) { Application.Quit(); yield break; }
            yield return Menu();
        }

        IEnumerator Cuadrillas()
        {
            estado = Estado.Cuadrillas;
            var raiz = PantallaCompleta("Cuadrillas");
            var t = UI.Texto(raiz, "Armen 4 cuadrillas", 80, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -110), new Vector2(1400, 110));
            var d = UI.Texto(raiz, "Clic en una cuadrilla para sacarla o ponerla. Cada zona cambia el maquinista.", 38, Paleta.Via, TextAnchor.MiddleCenter, false);
            UI.Ubicar(d.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -210), new Vector2(1600, 60));
            var botones = new List<UI.Boton>();
            for (int i = 0; i < 4; i++)
            {
                int k = i;
                var b = UI.CrearBoton(raiz, "", 60, Color.white, null);
                UI.Ubicar(b.rt, new Vector2(0.5f, 0.5f), new Vector2(-570 + i * 380, 40), new Vector2(340, 300));
                b.fondo.sprite = UI.Cuadrado; b.fondo.type = Image.Type.Simple;
                var f = UI.Forma(b.rt, i, Paleta.Cuadrillas[i], 120);
                UI.Ubicar(f.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 40), f.rectTransform.sizeDelta);
                b.texto.text = "Cuadrilla " + Paleta.Letras[i];
                b.texto.color = Paleta.Texto;
                UI.Ubicar(b.texto.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(320, 70));
                b.button.onClick.AddListener(() =>
                {
                    if (S.activas[k] && S.CuantasActivas <= 2) return;
                    S.activas[k] = !S.activas[k];
                    b.fondo.color = S.activas[k] ? Color.white : Paleta.ConAlfa(Color.white, 0.3f);
                    f.color = S.activas[k] ? Paleta.Cuadrillas[k] : Paleta.Borde;
                });
                botones.Add(b);
            }
            bool listo = false;
            var e = UI.CrearBoton(raiz, "Empezar", 56, Paleta.Texto, () => listo = true);
            UI.Ubicar(e.rt, new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(460, 120));
            Captura("cuadrillas");
            if (auto) listo = true;
            while (!listo) yield return null;
            Destroy(raiz.gameObject);
        }

        IEnumerator AnuncioZona(int z)
        {
            estado = Estado.AnuncioZona;
            var zona = CargadorNiveles.Zona(z);
            var col = Paleta.Hex(zona.color);
            P.MostrarJuego(true);
            P.LimpiarPuzzle();
            P.Zona("Zona " + z + " · " + zona.nombre, col, zona.hora);
            Marcador.Refrescar(P, S);
            MostrarPrioridad(z == 6);
            P.Dialogo("Central", zona.anuncio, Color.white);
            P.via.zona = z;
            P.via.tunel = z >= 3 || (z == 2 && tunelAbierto);
            P.via.Rango(z >= 2 ? -3 : 0, 6);
            P.tren.Poner(Linea0.Matematica.Racional.Cero);
            var v = P.Ventana(new Vector2(1300, 520));
            var caja = v.GetChild(1);
            var pill = UI.Caja(caja, "Pill", col, UI.Pildora);
            UI.Ubicar(pill.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(980, 100));
            var t = UI.Texto(pill.transform, ("Zona " + z + " · " + zona.nombre).ToUpperInvariant(), 52, Paleta.TextoSobre(col));
            UI.Llenar(t.rectTransform, 20, 20, 4, 4);
            var c = UI.Texto(caja, (string.IsNullOrEmpty(zona.conjunto) ? "" : zona.conjunto + " · ") + zona.hora, 46, Paleta.Via);
            UI.Ubicar(c.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -200), new Vector2(1200, 70));
            var a = UI.Texto(caja, zona.anuncio, 40, Paleta.Texto, TextAnchor.MiddleCenter, false);
            UI.Ubicar(a.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -50), new Vector2(1200, 110));
            var s = new Senal();
            var b = UI.CrearBoton(caja, "Empezar", 50, Paleta.Texto, () => s.Dar());
            UI.Ubicar(b.rt, new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(360, 100));
            Captura("zona" + z);
            if (auto) s.Dar();
            yield return s.Esperar();
            Destroy(v.gameObject);
        }

        void MostrarPrioridad(bool v)
        {
            var r = P.letreroPrioridad;
            r.gameObject.SetActive(v);
            if (!v || r.childCount > 0) return;
            UI.Ubicar(r, new Vector2(1, 1), new Vector2(-20, -Pantalla.AltoBarra - 14), new Vector2(270, 136), new Vector2(1, 1));
            var c = UI.Caja(r, "Fondo", Paleta.Texto); UI.Llenar(c.rectTransform);
            var t = UI.Texto(r, "PRIORIDAD\n1° ( )\n2° × :   3° + −", 30, Color.white);
            UI.Llenar(t.rectTransform, 8, 8, 6, 6);
        }

        PuzzleBase Crear(Puzzle p)
        {
            switch (p.Tipo)
            {
                case TipoPuzzle.Boletos: return new PuzzleBoletos();
                case TipoPuzzle.Contar: return new PuzzleContar();
                case TipoPuzzle.Repartir: return new PuzzleRepartir();
                case TipoPuzzle.SinVia: return new PuzzleSinVia();
                case TipoPuzzle.Divisor: return new PuzzleDivisor();
                case TipoPuzzle.Lupa: return new PuzzleLupa();
                case TipoPuzzle.Compas: return new PuzzleCompas();
                case TipoPuzzle.Jerarquia: return new PuzzleJerarquia();
                case TipoPuzzle.Serpiente: return new PuzzleSerpiente();
                case TipoPuzzle.ContarSaltos: return new PuzzleContarSaltos();
            }
            return null;
        }

        IEnumerator JugarPuzzle(Puzzle p)
        {
            estado = Estado.Puzzle;
            P.LimpiarPuzzle();
            Marcador.Refrescar(P, S);
            var pz = Crear(p);
            pz.G = this; pz.P = p; pz.R = S.Registro(p.id, p.zona);
            actual = pz;
            fallasPuzzle = 0;
            pistaNivel = 0;
            saltar = false;
            bool fin = false;
            Coroutine c = null;
            IEnumerator Envolver() { yield return pz.Jugar(); fin = true; }
            c = StartCoroutine(Envolver());
            while (!fin && !saltar) yield return null;
            if (!fin)
            {
                StopCoroutine(c);
                pz.R.saltado = true;
                for (int i = P.overlay.childCount - 1; i >= 0; i--) Destroy(P.overlay.GetChild(i).gameObject);
                if (p.guion == "choque") tunelAbierto = true;
            }
            actual = null;
            P.LimpiarPuzzle();
        }

        int pistaNivel;

        void Pista()
        {
            if (actual == null || actual.P.pistas == null || actual.P.pistas.Length == 0) return;
            pistaNivel = Mathf.Min(pistaNivel + 1, actual.P.pistas.Length);
            actual.R.pistas = Mathf.Max(actual.R.pistas, pistaNivel);
            P.Dialogo("Central", "Pista " + pistaNivel + ": " + actual.P.pistas[pistaNivel - 1], Color.white);
        }

        IEnumerator ConfirmarSaltar()
        {
            if (actual == null) yield break;
            var v = P.Ventana(new Vector2(1000, 360));
            var caja = v.GetChild(1);
            var t = UI.Texto(caja, "¿Saltar este puzzle?", 60, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(900, 90));
            int r = -1;
            var si = UI.CrearBoton(caja, "Sí, saltar", 44, Paleta.Error, () => r = 1);
            UI.Ubicar(si.rt, new Vector2(0.5f, 0), new Vector2(-220, 60), new Vector2(380, 100));
            var no = UI.BotonContorno(caja, "No", 44, () => r = 0);
            UI.Ubicar(no.rt, new Vector2(0.5f, 0), new Vector2(220, 60), new Vector2(380, 100));
            while (r < 0) yield return null;
            Destroy(v.gameObject);
            if (r == 1) saltar = true;
        }

        // ------------------------------------------------------------------ final y reporte

        IEnumerator Final()
        {
            estado = Estado.Final;
            P.MostrarJuego(false);
            UI.Limpiar(P.overlay);
            var raiz = PantallaCompleta("Final");
            var t = UI.Texto(raiz, "18:30 · ¡Inauguración de la Línea 0!", 74, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(1800, 100));
            // cinta cortada
            var cintaI = UI.Caja(raiz, "Cinta", Paleta.Error); UI.Ubicar(cintaI.rectTransform, new Vector2(0.5f, 1), new Vector2(-560, -170), new Vector2(700, 26));
            cintaI.rectTransform.localRotation = Quaternion.Euler(0, 0, -4);
            var cintaD = UI.Caja(raiz, "Cinta", Paleta.Error); UI.Ubicar(cintaD.rectTransform, new Vector2(0.5f, 1), new Vector2(560, -170), new Vector2(700, 26));
            cintaD.rectTransform.localRotation = Quaternion.Euler(0, 0, 4);
            Plano(raiz, new Vector2(-380, -40));
            // ganadora
            if (S.conCuadrillas)
            {
                var g = S.Ganadoras();
                var gt = UI.Texto(raiz, g.Count > 1 ? "¡Empate!" : "Gana la cuadrilla", 50, Paleta.Texto);
                UI.Ubicar(gt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(560, 250), new Vector2(700, 70));
                for (int i = 0; i < g.Count; i++)
                    Marcador.Pildora(raiz, g[i], S.puntos[g[i]] + " pts", new Vector2(560, 160 - i * 100) + new Vector2(0, 0), new Vector2(330, 88), 44).anchorMin =
                        new Vector2(0.5f, 0.5f);
                float y = -120;
                for (int i = 0; i < 4; i++)
                {
                    if (!S.activas[i]) continue;
                    Marcador.Pildora(raiz, i, S.puntos[i] + " pts", new Vector2(560, y), new Vector2(260, 64), 34);
                    y -= 76;
                }
            }
            var conf = UI.Llenar(UI.Nodo(raiz, "Confeti"));
            StartCoroutine(Confeti(conf));
            bool listo = false;
            var b = UI.CrearBoton(raiz, "Ver reporte", 50, Paleta.Texto, () => listo = true);
            UI.Ubicar(b.rt, new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(460, 104));
            Captura("final");
            if (auto) { yield return Ease.Pausa(1f); listo = true; }
            while (!listo) yield return null;
            Destroy(raiz.gameObject);
            yield return Reporte();
        }

        IEnumerator Confeti(RectTransform capa)
        {
            var piezas = new List<(RectTransform rt, float v, float giro)>();
            var cols = new[] { Paleta.Naturales, Paleta.Enteros, Paleta.Racionales, Paleta.Irracionales, Paleta.Error, Paleta.Decimales };
            for (int i = 0; i < 90; i++)
            {
                var c = UI.Caja(capa, "p", cols[i % cols.Length]); c.raycastTarget = false;
                UI.Ubicar(c.rectTransform, new Vector2(UnityEngine.Random.value, 1), new Vector2(0, UnityEngine.Random.Range(0, 900)), new Vector2(18, 30));
                piezas.Add((c.rectTransform, UnityEngine.Random.Range(120, 260), UnityEngine.Random.Range(-200, 200)));
            }
            float alto = ((RectTransform)P.canvas.transform).rect.height + 100;
            while (capa != null)
            {
                foreach (var (rt, v, g) in piezas)
                {
                    var p = rt.anchoredPosition; p.y -= v * Time.deltaTime;
                    if (p.y < -alto) p.y = 60;
                    rt.anchoredPosition = p;
                    rt.Rotate(0, 0, g * Time.deltaTime);
                }
                yield return null;
            }
        }

        /// <summary>Plano de la red: cuatro líneas anidadas ℕ ⊂ ℤ ⊂ ℚ ⊂ ℝ.</summary>
        public static RectTransform Plano(RectTransform padre, Vector2 pos, float escala = 1)
        {
            var raiz = UI.Nodo(padre, "Plano");
            UI.Ubicar(raiz, new Vector2(0.5f, 0.5f), pos, new Vector2(1000, 680) * escala);
            var conj = new[] { ("ℝ Reales · Irracionales √2  π", Paleta.Irracionales), ("ℚ Racionales  3/4  0,35", Paleta.Racionales), ("ℤ Enteros  −2", Paleta.Enteros), ("ℕ Naturales  7", Paleta.Naturales) };
            for (int i = 0; i < 4; i++)
            {
                var (n, c) = conj[i];
                float w = (1000 - i * 190) * escala, h = (680 - i * 150) * escala;
                var b = UI.CajaConBorde(raiz, n, Paleta.ConAlfa(c, 0.10f), c, 10 * escala);
                b.raycastTarget = false;
                UI.Ubicar(b.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -i * 55 * escala), new Vector2(w, h));
                b.transform.GetChild(0).GetComponent<Image>().color = i == 0 ? Color.white : Paleta.Hex("#FAFCFE");
                var t = UI.Texto(b.transform, n, (int)(36 * escala), c == Paleta.Naturales ? Paleta.Hex("#B07A00") : c, TextAnchor.UpperCenter);
                UI.Llenar(t.rectTransform, 10, 10, 14 * escala, 0);
            }
            return raiz;
        }

        static string Barra(int nivel) => nivel < 0 ? "—" : Sesion.Niveles[nivel];

        IEnumerator Reporte()
        {
            var raiz = PantallaCompleta("Reporte");
            var t = UI.Texto(raiz, "Reporte para el profe", 64, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -50), new Vector2(1600, 80));

            // columna izquierda: controles
            var izq = UI.Nodo(raiz, "Controles");
            UI.Ubicar(izq, new Vector2(0, 1), new Vector2(60, -130), new Vector2(880, 560), new Vector2(0, 1));
            Linea(izq, "Control      Entrada → Salida", 0, 34, Paleta.Via);
            for (int q = 0; q < 6; q++)
            {
                Linea(izq, "Pregunta " + (q + 1), 60 + q * 76, 32, Paleta.Texto);
                Barras(izq, S.entrada[q], new Vector2(260, -(60 + q * 76) - 4), Paleta.Gris);
                Barras(izq, S.salida[q], new Vector2(560, -(60 + q * 76) - 4), Paleta.Racionales);
            }

            // columna derecha: zonas
            var der = UI.Nodo(raiz, "Zonas");
            UI.Ubicar(der, new Vector2(1, 1), new Vector2(-60, -130), new Vector2(880, 560), new Vector2(1, 1));
            Linea(der, "Zona    1er intento    Pistas    Refuerzo", 0, 34, Paleta.Via);
            int fila = 1;
            for (int z = 0; z <= 6; z++)
            {
                var regs = S.registros.Where(r => r.zona == z).ToList();
                if (regs.Count == 0) continue;
                int primer = regs.Count(r => r.primerIntento);
                int pct = Mathf.RoundToInt(100f * primer / regs.Count);
                Linea(der, "Zona " + z + "      " + pct + " %  (" + primer + "/" + regs.Count + ")      " + regs.Sum(r => r.pistas) + "      " + (S.zonasJugadasConRefuerzo.Contains(z) ? "Sí" : "No"), fila * 64, 32, Paleta.Texto);
                fila++;
            }

            var todas = S.registros.SelectMany(r => r.aciertosPrediccion).ToList();
            string pred = todas.Count > 0 ? (todas.Average()).ToString("0.0").Replace('.', ',') : "—";
            string pts = S.conCuadrillas ? string.Join("   ", Enumerable.Range(0, 4).Where(i => S.activas[i]).Select(i => Paleta.Letras[i] + ": " + S.puntos[i])) : "Sin cuadrillas";
            var resumen = UI.Texto(raiz, "Predicciones correctas por viaje: " + pred + "   ·   Puntos  " + pts + "   ·   Tiempo: " + Mathf.RoundToInt(S.MinutosJugados) + " min", 34, Paleta.Texto, TextAnchor.MiddleCenter, false);
            UI.Ubicar(resumen.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 330), new Vector2(1800, 60));

            var cierre = UI.Texto(raiz, "Para conversar:  1. " + CargadorNiveles.Datos.preguntasCierre[0] + "\n2. " + CargadorNiveles.Datos.preguntasCierre[1] + "   3. " + CargadorNiveles.Datos.preguntasCierre[2], 34, Paleta.Enteros, TextAnchor.MiddleCenter, false);
            UI.Ubicar(cierre.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 210), new Vector2(1800, 110));

            bool listo = false;
            var b = UI.CrearBoton(raiz, "Menú", 46, Paleta.Texto, () => listo = true);
            UI.Ubicar(b.rt, new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(360, 96));
            Captura("reporte");
            if (auto) { yield return Ease.Pausa(1f); listo = true; }
            while (!listo) yield return null;
            Destroy(raiz.gameObject);
        }

        static void Linea(RectTransform padre, string s, float y, int tam, Color c)
        {
            var t = UI.Texto(padre, s, tam, c, TextAnchor.MiddleLeft, false);
            UI.Ubicar(t.rectTransform, new Vector2(0, 1), new Vector2(0, -y), new Vector2(880, tam + 16), new Vector2(0, 1));
        }

        /// <summary>Barra de 4 niveles (Casi nadie → Casi todos).</summary>
        static void Barras(RectTransform padre, int nivel, Vector2 pos, Color c)
        {
            for (int i = 0; i < 4; i++)
            {
                var b = UI.Caja(padre, "Nivel", i <= nivel ? c : Paleta.GrisClaro); b.raycastTarget = false;
                UI.Ubicar(b.rectTransform, new Vector2(0, 1), pos + new Vector2(i * 62, 0), new Vector2(54, 40), new Vector2(0, 1));
            }
        }
    }
}
