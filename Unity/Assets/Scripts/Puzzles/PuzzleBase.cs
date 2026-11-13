using System.Collections;
using System.Collections.Generic;
using Linea0.Datos;
using Linea0.Juego;
using Linea0.Matematica;
using Linea0.Sala;
using Linea0.Vista;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Puzzles
{
    public abstract class PuzzleBase
    {
        public GameManager G;
        public Puzzle P;
        public Sesion.RegistroPuzzle R;
        public int intento;       // intentos que cuentan
        public int fallas;
        protected Pantalla Pa => G.P;
        protected Via V => G.P.via;
        protected Tren T => G.P.tren;
        protected bool Dec => P.UsaDecimales;

        public abstract IEnumerator Jugar();

        /// <summary>Corrutinas lanzadas desde clics; se detienen si el puzzle se salta.</summary>
        public readonly List<Coroutine> extras = new List<Coroutine>();
        protected void Lanzar(IEnumerator e) => extras.Add(G.StartCoroutine(e));

        /// <summary>Prueba automática con -fallar: el primer intento que cuenta va mal.</summary>
        protected bool AutoFalla => G.auto && G.fallar && intento == 0;

        public static readonly Color[] ColoresPasajero =
        {
            Paleta.Hex("#9B6CD9"), Paleta.Hex("#F28C38"), Paleta.Hex("#3BB6C4"), Paleta.Hex("#E06B9A"), Paleta.Hex("#7A9E3B"), Paleta.Hex("#C9A227")
        };

        protected Color ColorPasajero => ColoresPasajero[Mathf.Abs(P.id.GetHashCode()) % ColoresPasajero.Length];

        public string Num(Racional r) => r.ToTexto(Dec);
        public string Num(Posicion p) => p.ToTexto(Dec);
        protected string MetaTexto => !string.IsNullOrEmpty(P.letreroMeta) ? P.letreroMeta : UI.M(Num(P.Meta));

        /// <summary>Vía, tren, salida, destino y pedido según el JSON.</summary>
        protected void PrepararVia(bool conDestino = true)
        {
            V.zona = P.zona;
            V.tunel = P.zona >= 3 || (P.zona == 2 && G.tunelAbierto);
            V.topera = true;
            V.toperaAgrietada = false;
            V.subdivision = P.subdivision;
            V.colorSubdivision = Dec ? Paleta.Decimales : Paleta.Racionales;
            V.estilo = P.estiloEstacion ?? "";
            V.salida = P.Inicio;
            V.destino = conDestino && !string.IsNullOrEmpty(P.meta) ? P.Meta : (Posicion?)null;
            if (conDestino && !string.IsNullOrEmpty(P.meta))
            {
                var m = P.Meta;
                if (!(m.EsRacional && m.Valor.EsEntero)) V.letreroDestino = MetaTexto;
            }
            V.Rango(P.RangoMin.ToDouble(), P.RangoMax.ToDouble());
            if (V.destino.HasValue) Anclas.Poner("destino", () => V.destino.HasValue ? V.Esquinas(V.X(V.destino.Value.ToDouble()), Via.YVia - 64, 170, 96) : null);
            T.Poner(P.Inicio);
        }

        protected void Pedido() => Pa.Dialogo(P.hablante, P.frasePedido, ColorPasajero);
        protected void Central(string s) => Pa.Dialogo("Central", s, Color.white);

        protected RectTransform Etiqueta(Transform padre, string s, int tam, Color c, Vector2 ancla, Vector2 pos, Vector2 tamRect)
        {
            var t = UI.Texto(padre, s, tam, c);
            return UI.Ubicar(t.rectTransform, ancla, pos, tamRect);
        }

        protected RectTransform EtiquetaPanel(string s, float x)
        {
            var r = Etiqueta(Pa.panel, s, 26, Paleta.Via, new Vector2(0, 1), new Vector2(x, -28), new Vector2(400, 34));
            r.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            return r;
        }

        protected UI.Boton BotonPrincipal(string texto, UnityEngine.Events.UnityAction a)
        {
            var b = UI.CrearBoton(Pa.panel, texto, 52, Paleta.Texto, a);
            b.fondo.sprite = UI.Cuadrado; b.fondo.type = Image.Type.Simple;
            UI.Ubicar(b.rt, new Vector2(1, 0.5f), new Vector2(-40, -6), new Vector2(360, 124), new Vector2(1, 0.5f));
            return b;
        }

        // ------------------------------------------------------------------ resultado

        /// <summary>Después de un viaje: ¿quién predijo bien?, puntos, y el cartel de acierto o falla.
        /// Devuelve en logrado si se llegó.</summary>
        protected IEnumerator Resultado(bool acierto, string cuentaFalla, bool cuenta = true)
        {
            if (cuenta) { intento++; R.intentos++; }
            if (!P.sinPuntos && cuenta) yield return QuienPredijo.Correr(G, R);
            if (acierto)
            {
                int pts = 0;
                if (!P.sinPuntos && cuenta)
                {
                    pts = Sesion.PuntosPorLlegar(intento, R.pistas > 0);
                    G.S.Sumar(G.S.turno, pts);
                    Marcador.Refrescar(Pa, G.S);
                }
                R.logrado = true;
                R.primerIntento = intento <= 1 && R.pistas == 0;
                G.fallasPuzzle = 0;
                string puntos = G.S.conCuadrillas && pts > 0 ? "+" + pts + " para " + Paleta.Letras[G.S.turno] : "";
                yield return Cartel(true, UI.M(P.cuentaEscrita), P.fraseNombre, puntos, "Siguiente");
            }
            else
            {
                fallas++;
                G.fallasPuzzle = fallas;
                yield return Cartel(false, cuentaFalla, "", "", "Reintentar");
            }
        }

        /// <summary>Cartel grande sobre la vía con una cuenta, una frase y un botón.</summary>
        protected IEnumerator Cartel(bool bien, string linea1, string linea2, string extra, string boton)
        {
            var raiz = UI.Nodo(Pa.capaPuzzle, "Cartel");
            UI.Ubicar(raiz, new Vector2(0.5f, 1), new Vector2(0, -170), new Vector2(1760, 230), new Vector2(0.5f, 1));
            var caja = UI.CajaConBorde(raiz, "Caja", Color.white, bien ? Paleta.Racionales : Paleta.Error, 6);
            UI.Llenar(caja.rectTransform);
            var t1 = UI.Texto(caja.transform, linea1, 58, bien ? Paleta.Texto : Paleta.Error);
            UI.Ubicar(t1.rectTransform, new Vector2(0, 1), new Vector2(40, -20), new Vector2(1320, 100), new Vector2(0, 1));
            t1.alignment = TextAnchor.MiddleLeft;
            if (!string.IsNullOrEmpty(linea2))
            {
                var t2 = UI.Texto(caja.transform, linea2, 38, Paleta.Texto, TextAnchor.MiddleLeft, false);
                UI.Ubicar(t2.rectTransform, new Vector2(0, 0), new Vector2(40, 18), new Vector2(1340, 100), new Vector2(0, 0));
            }
            if (!string.IsNullOrEmpty(extra))
            {
                var t3 = UI.Texto(caja.transform, extra, 40, Paleta.Racionales);
                UI.Ubicar(t3.rectTransform, new Vector2(1, 1), new Vector2(-30, -24), new Vector2(330, 50), new Vector2(1, 1));
            }
            var s = new Senal();
            var b = UI.CrearBoton(caja.transform, boton, 46, bien ? Paleta.Racionales : Paleta.Error, () => s.Dar());
            UI.Ubicar(b.rt, new Vector2(1, 0), new Vector2(-30, 26), new Vector2(330, 100), new Vector2(1, 0));
            Anclas.Poner(bien ? "cuenta" : "cartel", raiz);
            G.Captura(P.id + (bien ? "_acierto" : "_falla"));
            if (bien) yield return Guia.Pasos(G, P.id, a => a == "cuenta" ? ((System.Func<bool>, System.Action)?)(null, null) : null);
            if (G.auto) { yield return Ease.Pausa(0.4f); s.Dar(); }
            yield return s.Esperar();
            Object.Destroy(raiz.gameObject);
        }

        /// <summary>Muestra la silueta del tren en el destino (falla).</summary>
        protected void Silueta()
        {
            if (V.destino.HasValue) Tren.Silueta(V, V.destino.Value.ToDouble());
        }

        protected void QuitarSiluetas()
        {
            for (int i = V.capaTren.childCount - 1; i >= 0; i--)
                if (V.capaTren.GetChild(i).name == "Silueta") Object.Destroy(V.capaTren.GetChild(i).gameObject);
        }

        protected IEnumerator Llegada()
        {
            T.Puertas(true);
            V.destinoIluminado = true;
            V.Redibujar();
            // baja el pasajero y camina al andén
            if (T.gameObject.activeSelf)
            {
                var per = Pantalla.Persona(V.capaEncima, ColorPasajero, 0.7f);
                float x0 = V.X(T.x) + 60;
                UI.Ubicar(per, Vector2.zero, new Vector2(x0, Via.YVia + 30), per.sizeDelta, new Vector2(0.5f, 0));
                yield return Ease.Durante(0.7f, t => per.anchoredPosition = new Vector2(x0 + 90 * Ease.Salida(t), Via.YVia + 30 + 40 * Mathf.Sin(t * Mathf.PI)));
            }
            yield return Ease.Pausa(0.3f);
        }

        /// <summary>Color de un número según su conjunto.</summary>
        public static Color ColorNumero(Racional v, bool dec)
        {
            if (!v.EsEntero) return dec ? Paleta.Decimales : Paleta.Racionales;
            if (v.Signo < 0) return Paleta.Enteros;
            return Paleta.Naturales;
        }
    }

    /// <summary>Puzzles que se responden con 4 alternativas: elegir → apuesta → comprobar.</summary>
    public abstract class PuzzleAlternativas : PuzzleBase
    {
        protected List<UI.Boton> botones;
        protected readonly HashSet<int> descartadas = new HashSet<int>();

        protected IEnumerator Elegir(Senal s, Vector2 centro)
        {
            s.Reset();
            botones = Alternativas.Crear(Pa.panel, P.alternativas, k => { if (!descartadas.Contains(k)) s.Dar(k); }, new Vector2(0.5f, 0.5f), centro, 390, 130);
            foreach (int d in descartadas) UI.Atenuar(botones[d], Paleta.Cuadrillas[d], true);
            G.Captura(P.id + "_elegir");
            if (G.auto) s.Dar(AutoFalla ? (P.correcta + 1) % 4 : P.correcta);
            yield return s.Esperar();
            for (int i = 0; i < botones.Count; i++)
            {
                botones[i].Activo = false;
                UI.Atenuar(botones[i], Paleta.Cuadrillas[i], i != s.valor);
                if (i == s.valor) botones[i].rt.localScale = Vector3.one * 1.06f;
            }
        }

        protected void BorrarBotones()
        {
            if (botones == null) return;
            foreach (var b in botones) if (b.button != null) Object.Destroy(b.button.gameObject);
            botones = null;
        }

        protected string TextoEleccion(int k) => "Elegiste " + UI.M(P.alternativas[k]) + ". Mira la comprobación y vuelve a intentar.";
    }
}
