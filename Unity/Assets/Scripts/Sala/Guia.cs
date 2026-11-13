using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Linea0.Juego;
using Linea0.Vista;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Sala
{
    /// <summary>
    /// Guía paso a paso (GDD 12.2): foco sobre un objetivo, marco ámbar que late, mano dibujada, globo de una línea,
    /// "Paso n de m", Seguir solo si el paso no pide acción, "Ya sé jugar" y bloqueo de todo lo demás.
    /// Los textos vienen de Resources/guias.json. Lo visto se guarda con PlayerPrefs.
    /// </summary>
    public static class Guia
    {
        [Serializable] public class PasoDatos { public string ancla, texto, espera, gesto; public bool soloCurso; }
        [Serializable] public class GuiaDatos { public string clave; public PasoDatos[] pasos; }
        [Serializable] public class AvisoDatos { public string clave, ancla, texto; public bool soloCurso, soloSolo; }
        [Serializable] public class Archivo { public GuiaDatos[] guias; public AvisoDatos[] avisos; }

        static Archivo datos;
        public static Archivo Datos => datos ?? (datos = JsonUtility.FromJson<Archivo>(Resources.Load<TextAsset>("guias").text));

        static readonly HashSet<string> saltadas = new HashSet<string>();   // "Ya sé jugar" en esta partida
        static readonly HashSet<string> repetir = new HashSet<string>();    // "Ver cómo se hace"
        static readonly HashSet<string> avisosSesion = new HashSet<string>();
        static readonly HashSet<string> vistasSesion = new HashSet<string>();   // completas en esta partida
        public static bool EnPantalla;

        const string Prefijo = "linea0_guia_";
        public static bool Visto(string clave) => PlayerPrefs.GetInt(Prefijo + clave, 0) == 1;

        /// <summary>Guarda que la guía ya se vio. Las pruebas automáticas no guardan nada.</summary>
        public static void Marcar(GameManager g, string clave)
        {
            if (g.auto) return;
            PlayerPrefs.SetInt(Prefijo + clave, 1);
            PlayerPrefs.Save();
        }

        /// <summary>"Volver a mostrar las guías".</summary>
        public static void Olvidar()
        {
            foreach (var gd in Datos.guias) PlayerPrefs.DeleteKey(Prefijo + gd.clave);
            foreach (var a in Datos.avisos) PlayerPrefs.DeleteKey(Prefijo + "aviso_" + a.clave);
            PlayerPrefs.Save();
            saltadas.Clear(); avisosSesion.Clear(); vistasSesion.Clear();
        }

        public static void NuevaPartida() { saltadas.Clear(); avisosSesion.Clear(); vistasSesion.Clear(); }

        public static GuiaDatos Buscar(string clave) => Datos.guias.FirstOrDefault(x => x.clave == clave);

        /// <summary>Pide repetir la guía de una herramienta (botón "Ver cómo se hace").</summary>
        public static void Repetir(string clave) { repetir.Add(clave); saltadas.Remove(clave); }

        static bool Aplica(GameManager g, bool soloCurso, bool soloSolo) =>
            (!soloCurso || g.S.conCuadrillas) && (!soloSolo || !g.S.conCuadrillas);

        public static bool Debe(GameManager g, string clave) =>
            Buscar(clave) != null && !saltadas.Contains(clave) && (repetir.Contains(clave) || !vistasSesion.Contains(clave)) &&
            (repetir.Contains(clave) || (g.opciones.guias && (g.auto || !Visto(clave))));

        class Resultado { public bool saltada; }

        /// <summary>Un paso de la guía 'clave'. 'hecho' dice cuándo el jugador hizo la acción; 'auto' la hace en la prueba automática.</summary>
        public static IEnumerator Paso(GameManager g, string clave, int indice, Func<bool> hecho = null, Action auto = null)
        {
            var gd = Buscar(clave);
            if (gd == null || indice >= gd.pasos.Length || !Debe(g, clave)) yield break;
            var p = gd.pasos[indice];
            if (!Aplica(g, p.soloCurso, false)) yield break;
            var lista = gd.pasos.Where(x => Aplica(g, x.soloCurso, false)).ToList();
            int n = lista.IndexOf(p) + 1, m = lista.Count;
            var res = new Resultado();
            yield return Mostrar(g, p.ancla, p.texto, "PASO " + n + " DE " + m, p.espera, p.gesto, hecho, auto, "guia_" + clave + "_" + n, res);
            if (res.saltada) saltadas.Add(clave);
            if (res.saltada || n == m) { Marcar(g, clave); repetir.Remove(clave); vistasSesion.Add(clave); }
        }

        /// <summary>Todos los pasos de una guía cuyo ancla resuelve 'accion' (los demás los muestra quien corresponda).</summary>
        public static IEnumerator Pasos(GameManager g, string clave, Func<string, (Func<bool> hecho, Action auto)?> accion)
        {
            var gd = Buscar(clave);
            if (gd == null) yield break;
            for (int i = 0; i < gd.pasos.Length; i++)
            {
                var a = accion(gd.pasos[i].ancla);
                if (a.HasValue) yield return Paso(g, clave, i, a.Value.hecho, a.Value.auto);
            }
        }

        /// <summary>Aviso de primera vez (GDD 12.5): foco y botón Seguir.</summary>
        public static IEnumerator Aviso(GameManager g, string clave)
        {
            var a = Datos.avisos.FirstOrDefault(x => x.clave == clave);
            if (a == null || !Aplica(g, a.soloCurso, a.soloSolo) || !g.opciones.guias || avisosSesion.Contains(clave)) yield break;
            if (!g.auto && Visto("aviso_" + clave)) yield break;
            avisosSesion.Add(clave);
            yield return Mostrar(g, a.ancla, a.texto, "", "seguir", null, null, null, "aviso_" + clave, new Resultado());
            Marcar(g, "aviso_" + clave);
        }

        // ------------------------------------------------------------------ dibujo

        static RectTransform Rect(Transform padre, string n, Color c, bool bloquea, Sprite s = null)
        {
            var img = UI.Caja(padre, n, c, s);
            img.raycastTarget = bloquea;
            var rt = img.rectTransform;
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = Vector2.zero;
            return rt;
        }

        static void Poner(RectTransform rt, Rect r) { rt.anchoredPosition = r.min; rt.sizeDelta = r.size; }

        static Sprite manoSprite;

        /// <summary>Mano que apunta con el índice hacia arriba (punta del dedo = pivote).
        /// Imagen: "hand_point_n" del Cursor Pack de Kenney (CC0); si faltara, se dibuja con primitivas.</summary>
        static RectTransform Mano(Transform padre)
        {
            if (manoSprite == null)
            {
                var tex = Resources.Load<Texture2D>("Guia/mano");
                if (tex != null)
                {
                    tex.filterMode = FilterMode.Bilinear;
                    // punta del dedo en x = 28 px, y = 3 px desde arriba, en la imagen de 64 × 64 (variante con contorno)
                    manoSprite = Sprite.Create(tex, new UnityEngine.Rect(0, 0, tex.width, tex.height), new Vector2(28.5f / 64f, 1f - 3f / 64f), 100);
                }
            }
            if (manoSprite != null)
            {
                var img = UI.Caja(padre, "Mano", Color.white, manoSprite);
                img.raycastTarget = false;
                var rt = img.rectTransform;
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.pivot = manoSprite.pivot / manoSprite.rect.size;
                rt.sizeDelta = new Vector2(120, 120);
                return rt;
            }
            return ManoPrimitiva(padre);
        }

        static RectTransform ManoPrimitiva(Transform padre)
        {
            var raiz = UI.Nodo(padre, "Mano");
            raiz.anchorMin = raiz.anchorMax = new Vector2(0.5f, 0.5f);
            raiz.pivot = new Vector2(0.5f, 1f);
            raiz.sizeDelta = new Vector2(90, 150);
            void Parte(Color c, Vector2 pos, Vector2 tam, float giro, float borde)
            {
                var img = UI.Caja(raiz, "p", c, UI.Pildora);
                img.raycastTarget = false;
                UI.Ubicar(img.rectTransform, new Vector2(0.5f, 1f), pos, tam + Vector2.one * borde * 2, new Vector2(0.5f, 0.5f));
                img.rectTransform.localRotation = Quaternion.Euler(0, 0, giro);
            }
            foreach (var (c, b) in new[] { (Paleta.Texto, 4f), (Color.white, 0f) })
            {
                Parte(c, new Vector2(0, -36), new Vector2(30, 76), 0, b);            // índice
                Parte(c, new Vector2(-30, -92), new Vector2(26, 48), 28, b);         // pulgar
                Parte(c, new Vector2(6, -104), new Vector2(72, 76), 0, b);           // palma
            }
            return raiz;
        }

        static IEnumerator Mostrar(GameManager g, string ancla, string texto, string paso, string espera, string gesto,
                                   Func<bool> hecho, Action auto, string captura, Resultado res)
        {
            EnPantalla = true;
            var raiz = UI.Llenar(UI.Nodo(g.P.overlay, "Guia"));
            var velo = Paleta.ConAlfa(Paleta.Texto, 0.55f);
            var bloques = new[] { Rect(raiz, "FocoA", velo, true), Rect(raiz, "FocoB", velo, true), Rect(raiz, "FocoI", velo, true), Rect(raiz, "FocoD", velo, true) };
            var marco = new[] { Rect(raiz, "MarcoA", Paleta.Naturales, false), Rect(raiz, "MarcoB", Paleta.Naturales, false), Rect(raiz, "MarcoI", Paleta.Naturales, false), Rect(raiz, "MarcoD", Paleta.Naturales, false) };
            bool toque = false, seguir = false, saltar = false;
            var hueco = Rect(raiz, "Hueco", Color.clear, espera == "tocar");
            if (espera == "tocar") hueco.gameObject.AddComponent<Button>().onClick.AddListener(() => toque = true);
            var mano = Mano(raiz);

            // globo
            var globo = Rect(raiz, "Globo", Color.white, true, UI.Pildora);
            globo.GetComponent<Image>().type = Image.Type.Sliced;
            globo.pivot = new Vector2(0.5f, 0.5f);
            var tPaso = UI.Texto(globo, paso, 24, Paleta.Gris, TextAnchor.UpperLeft);
            UI.Llenar(tPaso.rectTransform, 44, 30, 22, 0);
            var tTexto = UI.Texto(globo, texto, 40, Paleta.Texto, TextAnchor.MiddleLeft);
            tTexto.resizeTextForBestFit = false;
            tTexto.horizontalOverflow = HorizontalWrapMode.Overflow;
            float ancho = Mathf.Clamp(tTexto.preferredWidth + 96, 520, 1500);
            bool conSeguir = espera == "seguir";
            float alto = (string.IsNullOrEmpty(paso) ? 110 : 150) + (conSeguir ? 90 : 0);
            globo.sizeDelta = new Vector2(ancho, alto);
            UI.Ubicar(tTexto.rectTransform, new Vector2(0, 1), new Vector2(44, string.IsNullOrEmpty(paso) ? -28 : -58), new Vector2(ancho - 80, 56), new Vector2(0, 1));
            if (conSeguir)
            {
                var b = UI.CrearBoton(globo, "Seguir", 36, Paleta.Texto, () => seguir = true);
                UI.Ubicar(b.rt, new Vector2(1, 0), new Vector2(-30, 24), new Vector2(220, 72), new Vector2(1, 0));
            }

            mano.SetAsLastSibling(); // la mano va encima del globo
            var ya = UI.BotonContorno(raiz, "Ya sé jugar", 34, () => saltar = true);
            UI.Ubicar(ya.rt, new Vector2(1, 1), new Vector2(-24, -Pantalla.AltoBarra - 20), new Vector2(260, 80), new Vector2(1, 1));

            float t = 0; bool capturado = false, autoHecho = false;
            while (true)
            {
                var R = raiz.rect;
                var r = Anclas.Rect(ancla, raiz);
                bool hay = r.HasValue;
                foreach (var bq in bloques) bq.gameObject.SetActive(hay);
                foreach (var mq in marco) mq.gameObject.SetActive(hay);
                hueco.gameObject.SetActive(hay);
                mano.gameObject.SetActive(hay);
                if (hay)
                {
                    var h = r.Value;
                    h = UnityEngine.Rect.MinMaxRect(Mathf.Max(R.xMin, h.xMin - 12), Mathf.Max(R.yMin, h.yMin - 12), Mathf.Min(R.xMax, h.xMax + 12), Mathf.Min(R.yMax, h.yMax + 12));
                    Poner(bloques[0], UnityEngine.Rect.MinMaxRect(R.xMin, h.yMax, R.xMax, R.yMax));
                    Poner(bloques[1], UnityEngine.Rect.MinMaxRect(R.xMin, R.yMin, R.xMax, h.yMin));
                    Poner(bloques[2], UnityEngine.Rect.MinMaxRect(R.xMin, h.yMin, h.xMin, h.yMax));
                    Poner(bloques[3], UnityEngine.Rect.MinMaxRect(h.xMax, h.yMin, R.xMax, h.yMax));
                    const float g4 = 4;
                    Poner(marco[0], UnityEngine.Rect.MinMaxRect(h.xMin - g4, h.yMax, h.xMax + g4, h.yMax + g4));
                    Poner(marco[1], UnityEngine.Rect.MinMaxRect(h.xMin - g4, h.yMin - g4, h.xMax + g4, h.yMin));
                    Poner(marco[2], UnityEngine.Rect.MinMaxRect(h.xMin - g4, h.yMin, h.xMin, h.yMax));
                    Poner(marco[3], UnityEngine.Rect.MinMaxRect(h.xMax, h.yMin, h.xMax + g4, h.yMax));
                    float a = 0.55f + 0.45f * Mathf.Sin(t * 5);
                    foreach (var mq in marco) mq.GetComponent<Image>().color = Paleta.ConAlfa(Paleta.Naturales, a);
                    Poner(hueco, h);

                    // mano: la punta del dedo toca el objetivo; si arrastra, recorre hasta el ancla "<ancla>:fin"
                    // objetivo en la mitad de abajo: la mano llega desde arriba apuntando hacia abajo (como el mockup); si no, desde abajo
                    bool desdeArriba = h.center.y < R.center.y;
                    float dir = desdeArriba ? 1 : -1;
                    Vector2 punta = new Vector2(h.center.x + h.width * 0.15f, desdeArriba ? h.yMax - Mathf.Min(24, h.height * 0.3f) : h.yMin + Mathf.Min(24, h.height * 0.3f));
                    var fin = gesto == "arrastrar" ? Anclas.Rect(ancla + ":fin", raiz) : null;
                    if (fin.HasValue) punta = Vector2.Lerp(h.center, fin.Value.center, Mathf.Repeat(t * 0.6f, 1f));
                    else punta.y += dir * Mathf.Abs(Mathf.Sin(t * 4)) * 16;
                    mano.localRotation = Quaternion.Euler(0, 0, desdeArriba ? 180 : 0);
                    mano.anchoredPosition = punta;

                    // globo abajo del objetivo si cabe; si no, arriba; nunca encima del hueco
                    // el globo va más allá de la mano (del lado por donde llega), para que no se tapen
                    float gx = Mathf.Clamp(h.center.x, R.xMin + ancho / 2 + 20, R.xMax - ancho / 2 - 20);
                    float lejos = desdeArriba ? h.yMax + 175 + alto / 2 : h.yMin - 175 - alto / 2;
                    float cerca = desdeArriba ? h.yMin - 30 - alto / 2 : h.yMax + 30 + alto / 2;
                    bool cabe(float y) => y - alto / 2 > R.yMin + 10 && y + alto / 2 < R.yMax - Pantalla.AltoBarra;
                    float gy = cabe(lejos) ? lejos : cabe(cerca) ? cerca : R.center.y;
                    globo.anchoredPosition = new Vector2(gx, gy);
                }
                else globo.anchoredPosition = new Vector2(0, R.yMin + 260);

                // prueba automática: captura y hace la acción del paso
                if (g.auto)
                {
                    if (!capturado && t > 0.25f) { g.Captura(captura); capturado = true; }
                    else if (capturado && !autoHecho && t > 0.45f)
                    {
                        autoHecho = true;
                        auto?.Invoke();
                        if (espera == "tocar") toque = true;
                        if (espera == "seguir") seguir = true;
                    }
                    if (t > 6f) break; // nunca trabar la prueba
                }

                bool listo = espera == "tocar" ? toque : espera == "seguir" ? seguir : hecho == null || hecho();
                if (listo || saltar) break;
                t += Time.deltaTime;
                yield return null;
            }
            UnityEngine.Object.Destroy(raiz.gameObject);
            EnPantalla = false;
            res.saltada = saltar;
        }
    }
}
