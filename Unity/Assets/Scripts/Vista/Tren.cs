using System;
using System.Collections;
using System.Collections.Generic;
using Linea0.Juego;
using Linea0.Matematica;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Vista
{
    /// <summary>Dos coches con ventanas y franja, cuatro ruedas que giran. La posición exacta es un Posicion; el double es solo para dibujar.</summary>
    public class Tren : MonoBehaviour
    {
        public Via via;
        public RectTransform rt, cuerpo;
        public Posicion posicion = Racional.Cero;
        public double x; // para dibujar
        readonly List<RectTransform> ruedas = new List<RectTransform>();
        readonly List<Image> puertas = new List<Image>();
        double xPrevio;
        public float sacudida;
        public const float RadioRueda = 13;

        public static Tren Crear(Via via, Color franja)
        {
            var go = UI.Nodo(via.capaTren, "Tren");
            var t = go.gameObject.AddComponent<Tren>();
            t.via = via;
            t.rt = go;
            UI.Ubicar(go, Vector2.zero, Vector2.zero, new Vector2(280, 100), new Vector2(0.5f, 0));
            t.cuerpo = UI.Llenar(UI.Nodo(go, "Cuerpo"));
            t.Construir(t.cuerpo, franja, false);
            return t;
        }

        void Construir(RectTransform padre, Color franja, bool silueta)
        {
            Color borde = silueta ? Paleta.ConAlfa(Paleta.Error, 0.4f) : Paleta.Texto;
            foreach (float cx in new[] { -70f, 70f })
            {
                var c = UI.Caja(padre, "Coche", borde);
                c.raycastTarget = false;
                UI.Ubicar(c.rectTransform, new Vector2(0.5f, 0), new Vector2(cx, 20), new Vector2(134, 78), new Vector2(0.5f, 0));
                if (silueta) continue;
                var f = UI.Caja(c.transform, "Blanco", Color.white); f.raycastTarget = false;
                UI.Llenar(f.rectTransform, 5, 5, 5, 5);
                var fr = UI.Caja(c.transform, "Franja", franja); fr.raycastTarget = false;
                UI.Ubicar(fr.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 14), new Vector2(124, 12), new Vector2(0.5f, 0));
                for (int i = 0; i < 3; i++)
                {
                    var v = UI.Caja(c.transform, "Ventana", Paleta.Hex("#A9C8EE")); v.raycastTarget = false;
                    UI.Ubicar(v.rectTransform, new Vector2(0, 1), new Vector2(14 + i * 30, -14), new Vector2(24, 22), new Vector2(0, 1));
                }
                var p = UI.Caja(c.transform, "Puerta", Paleta.Hex("#5B6470")); p.raycastTarget = false;
                UI.Ubicar(p.rectTransform, new Vector2(1, 1), new Vector2(-12, -12), new Vector2(18, 40), new Vector2(1, 1));
                puertas.Add(p);
            }
            var en = UI.Caja(padre, "Enganche", borde); en.raycastTarget = false;
            UI.Ubicar(en.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(14, 8), new Vector2(0.5f, 0));
            foreach (float wx in new[] { -112f, -28f, 28f, 112f })
            {
                var r = UI.Caja(padre, "Rueda", silueta ? borde : Paleta.Texto, UI.Circulo); r.raycastTarget = false;
                UI.Ubicar(r.rectTransform, new Vector2(0.5f, 0), new Vector2(wx, 6), new Vector2(RadioRueda * 2, RadioRueda * 2), new Vector2(0.5f, 0));
                if (silueta) continue;
                // eje con un punto para que se note el giro
                var pivote = UI.Nodo(r.transform, "Eje");
                UI.Ubicar(pivote, new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
                var d = UI.Caja(pivote, "Punto", Color.white, UI.Circulo); d.raycastTarget = false;
                UI.Ubicar(d.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 7), new Vector2(7, 7));
                ruedas.Add(pivote);
            }
            // marcador de posición exacta: punto sobre la vía en el centro
            var m = UI.Caja(padre, "Marcador", silueta ? borde : Paleta.Error, UI.Circulo); m.raycastTarget = false;
            UI.Ubicar(m.rectTransform, new Vector2(0.5f, 0), new Vector2(0, -8), new Vector2(18, 18), new Vector2(0.5f, 0.5f));
        }

        /// <summary>Silueta roja al 40 % en el destino (falla).</summary>
        public static RectTransform Silueta(Via via, double valor)
        {
            var go = UI.Nodo(via.capaTren, "Silueta");
            UI.Ubicar(go, Vector2.zero, new Vector2(via.X(valor), Via.YVia + 8), new Vector2(280, 100), new Vector2(0.5f, 0));
            go.SetAsFirstSibling();
            var t = new GameObject("tmp").AddComponent<Tren>();
            t.Construir(go, Color.clear, true);
            Destroy(t.gameObject);
            return go;
        }

        public void Poner(Posicion p)
        {
            posicion = p;
            x = xPrevio = p.ToDouble();
            cuerpo.localScale = Vector3.one;
            Actualizar();
        }

        void Update() => Actualizar();

        void Actualizar()
        {
            if (via == null) return;
            float dx = (float)(x - xPrevio) * via.PxPorUnidad;
            xPrevio = x;
            float sentido = Mathf.Sign(cuerpo.localScale.x == 0 ? 1 : cuerpo.localScale.x);
            float giro = -dx / RadioRueda * Mathf.Rad2Deg * sentido;
            foreach (var r in ruedas) r.Rotate(0, 0, giro);
            float sx = sacudida > 0 ? UnityEngine.Random.Range(-sacudida, sacudida) : 0;
            rt.anchoredPosition = new Vector2(via.X(x) + sx, Via.YVia + 8);
        }

        /// <summary>Mueve el tren con aceleración y frenado. progreso recibe 0..1.</summary>
        public IEnumerator Mover(Posicion hasta, float dur = -1, Action<float> progreso = null)
        {
            double a = x, b = hasta.ToDouble();
            if (dur < 0) dur = Mathf.Clamp(0.45f + (float)Math.Abs(b - a) * via.PxPorUnidad / 900f, 0.5f, 2.6f);
            yield return Ease.Durante(dur, t => { x = a + (b - a) * Ease.SuaveInOut(t); progreso?.Invoke(t); });
            posicion = hasta;
            x = b;
        }

        /// <summary>Espejo: el tren pasa por el 0 y se da vuelta.</summary>
        public IEnumerator Espejo(Posicion hasta, Action<float> progreso = null)
        {
            double a = x, b = hasta.ToDouble();
            float s0 = cuerpo.localScale.x;
            yield return Ease.Durante(1.4f, t =>
            {
                double e = Ease.SuaveInOut(t);
                x = a + (b - a) * e;
                float k = Mathf.Cos((float)e * Mathf.PI); // 1 → −1
                cuerpo.localScale = new Vector3(s0 * (Mathf.Abs(k) < 0.05f ? 0.05f * Mathf.Sign(k + 0.0001f) : k), 1, 1);
                progreso?.Invoke(t);
            });
            cuerpo.localScale = new Vector3(-s0, 1, 1);
            posicion = hasta;
            x = b;
        }

        public IEnumerator Choque()
        {
            sacudida = 14;
            yield return Ease.Pausa(0.45f);
            sacudida = 0;
        }

        public void Puertas(bool abiertas)
        {
            foreach (var p in puertas) p.color = abiertas ? Paleta.Sol : Paleta.Hex("#5B6470");
        }
    }
}
