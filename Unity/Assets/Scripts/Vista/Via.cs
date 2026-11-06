using System;
using System.Collections;
using System.Collections.Generic;
using Linea0.Matematica;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Vista
{
    /// <summary>La recta numérica: fondo de día, túnel, topera, vía, durmientes y píldoras de estación.</summary>
    public class Via : MonoBehaviour
    {
        public RectTransform rt;
        RectTransform capaFondo, capaTunel, capaVia, capaMarcas, capaArcos;
        public RectTransform capaTren, capaEncima;

        public double min = -1, max = 6;
        public const float YVia = 150;

        // estado que se dibuja
        public int zona;
        public bool tunel;
        public bool topera = true;
        public bool toperaAgrietada;
        public int subdivision;
        public Color colorSubdivision = Paleta.Racionales;
        public string estilo = "";
        public Posicion? salida;
        public Posicion? destino;
        public string letreroDestino;
        public bool destinoIluminado;
        public bool mostrarPildoras = true;

        public class Marca
        {
            public double valor;
            public string texto;
            public Color color;
            public float alto = 46;
            public bool etiquetaArriba;
            public int tamTexto = 34;
        }
        public readonly List<Marca> marcas = new List<Marca>();

        public Action<RectTransform> alRedibujar; // para que un puzzle agregue lo suyo
        float anchoPrevio, altoPrevio;
        public RectTransform toperaRT;

        public static Via Crear(RectTransform contenedor)
        {
            var v = contenedor.gameObject.AddComponent<Via>();
            v.rt = contenedor;
            v.capaFondo = UI.Llenar(UI.Nodo(contenedor, "Fondo"));
            v.capaTunel = UI.Llenar(UI.Nodo(contenedor, "Tunel"));
            v.capaVia = UI.Llenar(UI.Nodo(contenedor, "Via"));
            v.capaMarcas = UI.Llenar(UI.Nodo(contenedor, "Marcas"));
            v.capaArcos = UI.Llenar(UI.Nodo(contenedor, "Arcos"));
            v.capaTren = UI.Llenar(UI.Nodo(contenedor, "Tren"));
            v.capaEncima = UI.Llenar(UI.Nodo(contenedor, "Encima"));
            return v;
        }

        public float Ancho => rt.rect.width;
        public float Alto => rt.rect.height;
        public float X(double v) => (float)((v - min) / (max - min) * Ancho);
        public double Valor(float x) => min + x / Ancho * (max - min);
        public float PxPorUnidad => (float)(Ancho / (max - min));

        /// <summary>Encuadra el rango del puzzle con un margen para que no se corten las píldoras.</summary>
        public static void RangoConMargen(double a, double b, out double nmin, out double nmax)
        {
            double pad = Math.Max(0.45, (b - a) * 0.09); // deja medio tren de margen
            nmin = a - pad; nmax = b + pad;
        }

        public double rangoA = -1, rangoB = 6; // rango pedido, sin margen

        public void Rango(double a, double b)
        {
            rangoA = a; rangoB = b;
            RangoConMargen(a, b, out min, out max);
            Redibujar();
        }

        public IEnumerator AnimarRango(double a, double b, float d = 0.7f)
        {
            rangoA = a; rangoB = b;
            RangoConMargen(a, b, out double nmin, out double nmax);
            double m0 = min, M0 = max;
            float t = 0;
            while (t < d)
            {
                float e = Linea0.Juego.Ease.SuaveInOut(t / d);
                min = m0 + (nmin - m0) * e;
                max = M0 + (nmax - M0) * e;
                Redibujar();
                t += Time.deltaTime;
                yield return null;
            }
            min = nmin; max = nmax;
            Redibujar();
        }

        /// <summary>Asegura que v quede a la vista; si no, amplía el rango.</summary>
        public IEnumerator Incluir(double v)
        {
            if (v >= rangoA && v <= rangoB) yield break;
            yield return AnimarRango(Math.Min(rangoA, Math.Floor(v)), Math.Max(rangoB, Math.Ceiling(v)), 0.6f);
        }

        void Update()
        {
            if (rt == null) return;
            if (Mathf.Abs(rt.rect.width - anchoPrevio) > 0.5f || Mathf.Abs(rt.rect.height - altoPrevio) > 0.5f) Redibujar();
        }

        // ---------------------------------------------------------------- dibujo

        public void Redibujar()
        {
            anchoPrevio = rt.rect.width; altoPrevio = rt.rect.height;
            if (Ancho < 10) return;
            UI.Limpiar(capaFondo); UI.Limpiar(capaTunel); UI.Limpiar(capaVia); UI.Limpiar(capaMarcas);
            DibujarFondo();
            DibujarTunel();
            DibujarVia();
            DibujarEstaciones();
            DibujarMarcas();
            alRedibujar?.Invoke(capaMarcas);
        }

        RectTransform Rect(Transform padre, string n, Color c, float x, float y, float w, float h, Sprite s = null, Vector2? pivote = null)
        {
            var img = UI.Caja(padre, n, c, s);
            img.raycastTarget = false;
            return UI.Ubicar(img.rectTransform, Vector2.zero, new Vector2(x, y), new Vector2(w, h), pivote ?? new Vector2(0.5f, 0.5f));
        }

        void DibujarFondo()
        {
            // sol y ciudad: decorado fijo en pantalla (no depende del rango)
            Rect(capaFondo, "Sol", Paleta.Sol, Ancho - 130, Alto - 110, 120, 120, UI.Circulo);
            float[] alturas = { 260, 380, 300, 440, 340, 500, 280, 420, 360, 470, 310, 390 };
            float x = 0; int i = 0;
            while (x < Ancho)
            {
                float w = 110 + (i * 37 % 60);
                float h = alturas[i % alturas.Length];
                Rect(capaFondo, "Edificio", Paleta.Ciudad, x + w / 2, YVia + h / 2 - 40, w, h);
                Rect(capaFondo, "Ventana", Paleta.ConAlfa(Paleta.Via, 0.12f), x + w / 2, YVia - 20, 26, 60);
                x += w + 18 + (i * 23 % 30);
                i++;
            }
        }

        void DibujarTunel()
        {
            if (!tunel || min >= 0) return;
            float x0 = Mathf.Clamp(X(0), 0, Ancho);
            if (x0 <= 0) return;
            float techo = Mathf.Min(Alto - 60, YVia + 330);
            Rect(capaTunel, "Tunel", Paleta.Tunel, x0 / 2, techo / 2, x0, techo);
            Rect(capaTunel, "Techo", Paleta.Hex("#B8C6D6"), x0 / 2, techo + 10, x0, 22);
            for (float lx = x0 - 120; lx > 30; lx -= 240)
                Rect(capaTunel, "Luz", Paleta.Hex("#FBE7A1"), lx, techo - 30, 70, 14, UI.Pildora);
            var t = UI.Texto(capaTunel, "BAJO CERO", 34, Paleta.Enteros, TextAnchor.MiddleLeft);
            UI.Ubicar(t.rectTransform, Vector2.zero, new Vector2(30, techo - 80), new Vector2(400, 50), new Vector2(0, 0.5f));
            // línea punteada vertical en el cero
            for (float y = 0; y < techo; y += 22) Rect(capaTunel, "Borde", Paleta.ConAlfa(Paleta.Via, 0.5f), x0, y, 3, 11);
        }

        void DibujarVia()
        {
            float xIni = 0;
            bool hayTopera = topera && !tunel && min < 0.3;
            if ((!tunel) && min < 0) xIni = Mathf.Max(0, X(0) - 40);
            Rect(capaVia, "Riel", Paleta.Via, (xIni + Ancho) / 2, YVia, Ancho - xIni, 10);
            // durmientes decorativos cada ~36 px
            for (float x = xIni + 10; x < Ancho; x += 36) Rect(capaVia, "Durmiente", Paleta.Via, x, YVia - 2, 5, 22);

            // divisiones de color (Divisor y vías partidas)
            if (subdivision > 1)
            {
                double paso = 1.0 / subdivision;
                double ini = Math.Floor(min);
                if (!tunel) ini = Math.Max(ini, 0);
                for (double v = ini; v <= max; v += 1)
                    for (int j = 1; j < subdivision; j++)
                    {
                        double p = v + j * paso;
                        if (p < min || p > max) continue;
                        Rect(capaVia, "Division", colorSubdivision, X(p), YVia + 6, 8, 40);
                    }
            }

            if (hayTopera)
            {
                float x0 = X(0) - 34;
                toperaRT = Rect(capaVia, "Topera", Paleta.Hex("#9AA3AD"), x0, YVia + 50, 40, 110);
                var franja = UI.Caja(toperaRT, "Franja", Paleta.Error); franja.raycastTarget = false;
                UI.Ubicar(franja.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -18), new Vector2(40, 18));
                var franja2 = UI.Caja(toperaRT, "Franja", Paleta.Error); franja2.raycastTarget = false;
                UI.Ubicar(franja2.rectTransform, new Vector2(0.5f, 1f), new Vector2(0, -58), new Vector2(40, 18));
                if (toperaAgrietada)
                {
                    for (int k = 0; k < 3; k++)
                    {
                        var g = UI.Caja(toperaRT, "Grieta", Paleta.Texto); g.raycastTarget = false;
                        UI.Ubicar(g.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(k * 6 - 6, 10 - k * 24), new Vector2(5, 46));
                        g.rectTransform.localRotation = Quaternion.Euler(0, 0, 25 - k * 30);
                    }
                }
            }
            else toperaRT = null;
        }

        static Color ColorEstacion(long k)
        {
            if (k > 0) return Paleta.Naturales;
            if (k == 0) return Paleta.Texto;
            return Paleta.Enteros;
        }

        string TextoEstacion(long k)
        {
            string n = UI.M(k.ToString());
            if (estilo == "grados") return n + " °C";
            if (estilo == "nota") return n + ",0";
            return n;
        }

        void DibujarEstaciones()
        {
            if (!mostrarPildoras) return;
            long a = (long)Math.Ceiling(min), b = (long)Math.Floor(max);
            float px = PxPorUnidad;
            long paso = px >= 120 ? 1 : px >= 60 ? 2 : px >= 26 ? 5 : 10;
            float ancho = estilo == "grados" ? 140 : estilo == "nota" ? 120 : 104;
            if (paso == 1 && px < ancho + 8) ancho = Mathf.Max(70, px - 10);
            for (long k = a; k <= b; k++)
            {
                if (k < 0 && !tunel) continue;
                bool conNumero = k % paso == 0;
                float x = X(k);
                // durmiente de estación (más alto) en cada entero
                Rect(capaVia, "Estacion", Paleta.Via, x, YVia + 4, 7, 34);
                if (!conNumero || x < ancho / 2 || x > Ancho - ancho / 2) continue; // no dibujar píldoras cortadas en el borde
                bool esDestino = destino.HasValue && destino.Value.EsRacional && destino.Value.Valor == new Racional(k);
                if (esDestino)
                {
                    if (destinoIluminado) Rect(capaVia, "Brillo", Paleta.Sol, x, YVia - 64, ancho + 60, 104, UI.Pildora);
                    Rect(capaVia, "Anillo", Paleta.Error, x, YVia - 64, ancho + 22, 84, UI.Pildora);
                    Rect(capaVia, "AnilloIn", Color.white, x, YVia - 64, ancho + 10, 72, UI.Pildora);
                }
                var col = ColorEstacion(k);
                var p = Rect(capaVia, "Pildora " + k, col, x, YVia - 64, ancho, 62, UI.Pildora);
                var t = UI.Texto(p, TextoEstacion(k), 44, Paleta.TextoSobre(col));
                UI.Llenar(t.rectTransform, 6, 6, 2, 2);
                if (estilo == "nota")
                {
                    var n = UI.Texto(capaVia, "nota", 26, Paleta.Gris);
                    UI.Ubicar(n.rectTransform, Vector2.zero, new Vector2(x, YVia - 112), new Vector2(120, 30));
                }
            }
            if (salida.HasValue) Etiqueta("SALIDA", salida.Value.ToDouble(), Paleta.Via);
            if (destino.HasValue) DibujarDestino();
        }

        void Etiqueta(string s, double v, Color c)
        {
            var t = UI.Texto(capaMarcas, s, 28, c);
            UI.Ubicar(t.rectTransform, Vector2.zero, new Vector2(X(v), YVia - 134), new Vector2(200, 34));
        }

        void DibujarDestino()
        {
            var d = destino.Value;
            double v = d.ToDouble();
            bool entero = d.EsRacional && d.Valor.EsEntero;
            if (!entero)
            {
                // banderín: palo rojo y letrero con el número
                if (destinoIluminado) Rect(capaMarcas, "Brillo", Paleta.Sol, X(v), YVia + 4, 46, 46, UI.Circulo);
                Rect(capaMarcas, "Punto", Paleta.Error, X(v), YVia + 4, 26, 26, UI.Circulo);
                Rect(capaMarcas, "Palo", Paleta.Error, X(v), YVia - 30, 6, 60);
                if (!string.IsNullOrEmpty(letreroDestino))
                {
                    var p = Rect(capaMarcas, "Letrero", Paleta.Error, X(v), YVia - 66, Mathf.Max(100, letreroDestino.Length * 26 + 30), 52, UI.Pildora);
                    var t = UI.Texto(p, letreroDestino, 38, Color.white);
                    UI.Llenar(t.rectTransform, 6, 6, 2, 2);
                }
            }
            var e = UI.Texto(capaMarcas, "DESTINO", 30, Paleta.Error);
            UI.Ubicar(e.rectTransform, Vector2.zero, new Vector2(X(v), YVia - 126), new Vector2(220, 36));
        }

        void DibujarMarcas()
        {
            foreach (var m in marcas)
            {
                if (m.valor < min - 0.0001 || m.valor > max + 0.0001) continue;
                float x = X(m.valor);
                Rect(capaMarcas, "Marca", m.color, x, YVia + m.alto / 2 - 4, 6, m.alto);
                if (!string.IsNullOrEmpty(m.texto))
                {
                    float y = m.etiquetaArriba ? YVia + m.alto + 24 : YVia - 40;
                    var t = UI.Texto(capaMarcas, m.texto, m.tamTexto, m.color);
                    UI.Ubicar(t.rectTransform, Vector2.zero, new Vector2(x, y), new Vector2(200, m.tamTexto + 8));
                }
            }
        }

        // ---------------------------------------------------------------- arcos

        public class Arco
        {
            public List<Image> puntos = new List<Image>();
            public RectTransform etiqueta, raiz;
            public void Progreso(float t)
            {
                int n = Mathf.CeilToInt(puntos.Count * Mathf.Clamp01(t));
                for (int i = 0; i < puntos.Count; i++) puntos[i].enabled = i < n;
                if (etiqueta != null) etiqueta.gameObject.SetActive(t > 0.3f);
            }
        }

        /// <summary>Arco punteado de a hasta b con su etiqueta en la cima.</summary>
        public Arco CrearArco(double a, double b, string texto, Color color, float alturaBase = 120)
        {
            var arco = new Arco();
            arco.raiz = UI.Llenar(UI.Nodo(capaArcos, "Arco"));
            float xa = X(a), xb = X(b);
            float y0 = YVia + alturaBase;
            float dist = Mathf.Abs(xb - xa);
            float h = Mathf.Clamp(dist * 0.35f, 50, Mathf.Max(60, Alto - y0 - 60));
            int n = Mathf.Max(4, Mathf.RoundToInt(dist / 22));
            Vector2 Punto(float t) => new Vector2(Mathf.Lerp(xa, xb, t), y0 + 4 * h * t * (1 - t));
            for (int i = 0; i <= n; i++)
            {
                var p = Punto(i / (float)n);
                var img = UI.Caja(arco.raiz, "p", color, UI.Circulo);
                img.raycastTarget = false;
                UI.Ubicar(img.rectTransform, Vector2.zero, p, new Vector2(11, 11));
                arco.puntos.Add(img);
            }
            // punta de flecha con dos barras
            var fin = Punto(1); var antes = Punto(0.94f);
            float ang = Mathf.Atan2(fin.y - antes.y, fin.x - antes.x) * Mathf.Rad2Deg;
            foreach (float s in new[] { 150f, -150f })
            {
                var bar = UI.Caja(arco.raiz, "flecha", color);
                bar.raycastTarget = false;
                UI.Ubicar(bar.rectTransform, Vector2.zero, fin, new Vector2(34, 8), new Vector2(0, 0.5f));
                bar.rectTransform.localRotation = Quaternion.Euler(0, 0, ang + s);
                arco.puntos.Add(bar);
            }
            if (!string.IsNullOrEmpty(texto))
            {
                var cima = Punto(0.5f);
                var pil = UI.Caja(arco.raiz, "Etiqueta", color, UI.Pildora);
                pil.raycastTarget = false;
                float w = Mathf.Max(96, texto.Length * 24 + 40);
                UI.Ubicar(pil.rectTransform, Vector2.zero, cima, new Vector2(w, 60));
                var t = UI.Texto(pil.transform, texto, 38, Paleta.TextoSobre(color));
                UI.Llenar(t.rectTransform, 6, 6, 2, 2);
                arco.etiqueta = pil.rectTransform;
            }
            arco.Progreso(0);
            return arco;
        }

        public void LimpiarArcos() => UI.Limpiar(capaArcos);

        public void LimpiarTodo()
        {
            LimpiarArcos();
            UI.Limpiar(capaEncima);
            marcas.Clear();
            alRedibujar = null;
            subdivision = 0;
            estilo = "";
            salida = null; destino = null; letreroDestino = null; destinoIluminado = false;
            mostrarPildoras = true;
        }
    }
}
