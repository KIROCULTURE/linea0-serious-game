using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Linea0.Datos;
using Linea0.Juego;
using Linea0.Matematica;
using Linea0.Sala;
using Linea0.Vista;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Puzzles
{
    /// <summary>Boletos: armar la ruta con la mano, Partir, apuesta, viaje con arcos. Incluye expreso, espejo, palanca y el choque de 2.1.</summary>
    public class PuzzleBoletos : PuzzleBase
    {
        readonly List<int> ruta = new List<int>();
        readonly List<bool> inv = new List<bool>();
        bool palancaActiva;
        readonly List<UI.Boton> mano = new List<UI.Boton>();
        readonly List<RectTransform> casillas = new List<RectTransform>();
        UI.Boton partir, palanca;
        Senal sPartir;

        public static Color ColorBoleto(Boleto b, bool dec)
        {
            if (b.operacion == "espejo") return Paleta.Enteros;
            var v = b.Valor;
            if (b.operacion == "resta") v = -v;
            return ColorNumero(v, dec);
        }

        public static string EtiquetaInvertida(Boleto b) => Racional.Menos + "(" + b.Etiqueta() + ")";

        public override IEnumerator Jugar()
        {
            PrepararVia();
            Pedido();
            ArmarPanel();
            bool choque = P.guion == "choque" && !G.tunelAbierto;
            while (true)
            {
                sPartir = new Senal();
                Refrescar();
                G.Captura(P.id + "_ruta");
                if (intento == 0 && fallas == 0) yield return Guia.Pasos(G, P.id, AccionGuia);
                if (G.auto) AutoRuta();
                yield return sPartir.Esperar();
                partir.Activo = false;
                for (int i = 0; i < mano.Count; i++) { mano[i].Activo = false; UI.Atenuar(mano[i], ColorBoleto(P.mano[i], Dec), true); }
                if (palanca != null) palanca.Activo = false;

                if (choque)
                {
                    yield return Choque();
                    choque = false;
                    Reiniciar();
                    continue;
                }

                yield return Linea0.Sala.ApuestaAnden.Correr(G);
                string cuenta = UI.M(Num(P.Inicio));
                var pos = P.Inicio;
                bool estrello = false;
                for (int k = 0; k < ruta.Count; k++)
                {
                    var b = P.mano[ruta[k]];
                    bool iv = inv[k];
                    if ((b.operacion == "mult" || b.operacion == "div" || b.operacion == "espejo") && k > 0) cuenta = "(" + cuenta + ")";
                    cuenta += b.Cuenta(iv);
                    var nueva = b.Aplicar(pos, iv);
                    yield return Viajar(b, iv, pos, nueva, r => estrello = r);
                    if (estrello) { pos = Racional.Cero; break; }
                    pos = nueva;
                }
                string hiciste = "Hiciste " + cuenta + " = " + UI.M(Num(estrello ? P.mano[ruta[0]].Aplicar(P.Inicio, inv[0]) : pos)) + ". El destino era " + MetaTexto + ".";
                if (estrello) hiciste = "Hiciste " + cuenta + ". La topera no deja pasar del cero.";
                bool ok = !estrello && P.Meta.EsRacional && pos == P.Meta.Valor;
                if (ok) yield return Llegada(); else Silueta();
                yield return Resultado(ok, hiciste);
                if (ok) yield break;
                Reiniciar();
            }
        }

        /// <summary>Qué acción espera cada objetivo de la guía y cómo la hace la prueba automática.</summary>
        (System.Func<bool>, System.Action)? AccionGuia(string ancla)
        {
            if (ancla == "destino") return (null, null);
            if (ancla == "ruta") return (() => ruta.Count == P.casillas, () => { foreach (int k in P.solucion) AMano(k); });
            if (ancla == "partir") return (() => sPartir.listo, () => sPartir.Dar());
            if (ancla == "palanca") return (() => palancaActiva, () => { palancaActiva = true; Refrescar(); });
            if (ancla.StartsWith("boleto:"))
            {
                int k = int.Parse(ancla.Substring(7));
                return (() => ruta.Contains(k), () => AMano(k));
            }
            return null;
        }

        void AutoRuta()
        {
            ruta.Clear(); inv.Clear();
            int malo = System.Array.FindIndex(P.mano, b => System.Array.IndexOf(P.solucion, System.Array.IndexOf(P.mano, b)) < 0);
            if (AutoFalla && malo >= 0) { ruta.Add(malo); inv.Add(false); sPartir.Dar(); return; }
            if (AutoFalla && P.palanca) { ruta.Add(P.solucion[0]); inv.Add(false); sPartir.Dar(); return; }
            for (int k = 0; k < P.solucion.Length; k++) { ruta.Add(P.solucion[k]); inv.Add(P.invertidos != null && P.invertidos.Contains(k)); }
            sPartir.Dar();
        }

        void Reiniciar()
        {
            V.LimpiarArcos();
            QuitarSiluetas();
            T.Puertas(false);
            V.destinoIluminado = false;
            V.Redibujar();
            T.Poner(P.Inicio);
            foreach (var b in mano) b.Activo = true;
            if (palanca != null) palanca.Activo = true;
        }

        /// <summary>Un boleto: arco(s) y movimiento. Si no hay túnel y pasa del cero, choca.</summary>
        IEnumerator Viajar(Boleto b, bool iv, Racional desde, Racional hasta, System.Action<bool> estrellado)
        {
            Color col = ColorBoleto(b, Dec);
            if (iv) col = Paleta.Racionales;
            if (b.operacion == "espejo")
            {
                yield return V.Incluir(hasta.ToDouble());
                var a = V.CrearArco(desde.ToDouble(), hasta.ToDouble(), b.Etiqueta(), col, 130);
                yield return T.Espejo(hasta, a.Progreso);
                yield break;
            }
            var saltos = new List<(Racional, Racional, string)>();
            if (b.operacion == "expreso")
            {
                var paso = iv ? -b.Valor : b.Valor;
                var p = desde;
                string v = UI.M(b.valor);
                for (int i = 0; i < b.repeticiones; i++) { saltos.Add((p, p + paso, (iv ? Racional.Menos.ToString() : (paso.Signo < 0 ? "" : "+")) + v)); p += paso; }
            }
            else saltos.Add((desde, hasta, iv ? EtiquetaInvertida(b) : b.Etiqueta()));

            foreach (var (a0, a1, et) in saltos)
            {
                yield return V.Incluir(a1.ToDouble());
                if (!V.tunel && a1.Signo < 0)
                {
                    // retrocede hasta el cero y choca con la topera
                    var arco = V.CrearArco(a0.ToDouble(), 0, et, col, 130);
                    yield return T.Mover(Racional.Cero, -1, arco.Progreso);
                    yield return T.Choque();
                    estrellado(true);
                    yield break;
                }
                var ar = V.CrearArco(a0.ToDouble(), a1.ToDouble(), et, col, 130);
                yield return T.Mover(a1, -1, ar.Progreso);
            }
        }

        /// <summary>2.1: el tren choca con la topera, la topera se agrieta y al hacer clic se abre el túnel.</summary>
        IEnumerator Choque()
        {
            var b = P.mano[ruta[0]];
            var arco = V.CrearArco(P.Inicio.ToDouble(), 0, b.Etiqueta(), ColorBoleto(b, Dec), 130);
            yield return T.Mover(Racional.Cero, -1, arco.Progreso);
            yield return T.Choque();
            Central(P.lineasGuion[0]);
            yield return Ease.Pausa(G.auto ? 0.2f : 2.2f);
            V.toperaAgrietada = true;
            V.Redibujar();
            Central(P.lineasGuion[1]);
            var s = new Senal();
            if (V.toperaRT != null)
            {
                var img = V.toperaRT.GetComponent<Image>();
                img.raycastTarget = true;
                var bt = V.toperaRT.gameObject.AddComponent<Button>();
                bt.onClick.AddListener(() => s.Dar());
            }
            else s.Dar();
            G.Captura(P.id + "_topera");
            if (G.auto) s.Dar();
            float t = 0;
            while (!s.listo)
            {
                t += Time.deltaTime;
                if (V.toperaRT != null) V.toperaRT.localScale = Vector3.one * (1 + 0.06f * Mathf.Sin(t * 6));
                yield return null;
            }
            G.tunelAbierto = true;
            V.tunel = true;
            V.LimpiarArcos();
            V.Redibujar();
            yield return Ease.Pausa(0.5f);
            Pedido();
        }

        // ------------------------------------------------------------------ panel

        void ArmarPanel()
        {
            EtiquetaPanel("TU RUTA", 40);
            for (int i = 0; i < P.casillas; i++)
            {
                var c = UI.CajaConBorde(Pa.panel, "Casilla", Paleta.GrisClaro, Paleta.Borde, 4);
                UI.Ubicar(c.rectTransform, new Vector2(0, 0.5f), new Vector2(40 + i * 250, -16), new Vector2(236, 120), new Vector2(0, 0.5f));
                casillas.Add(c.rectTransform);
            }
            float x0 = 40 + P.casillas * 250 + 60;
            float xm = x0;
            Etiqueta(Pa.panel, "BOLETOS", 26, Paleta.Via, new Vector2(0, 1), new Vector2(x0, -28), new Vector2(400, 34)).GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            for (int i = 0; i < P.mano.Length; i++)
            {
                int k = i;
                var b = P.mano[i];
                var bt = Boleto(Pa.panel, b.Etiqueta(), ColorBoleto(b, Dec), () => AMano(k));
                float w = Mathf.Clamp(b.Etiqueta().Length * 26 + 60, 200, 330); // el texto se achica solo
                UI.Ubicar(bt.rt, new Vector2(0, 0.5f), new Vector2(xm, -16), new Vector2(w, 110), new Vector2(0, 0.5f));
                xm += w + 24;
                mano.Add(bt);
            }
            if (P.palanca)
            {
                palanca = UI.BotonContorno(Pa.panel, "Palanca: Restar", 34, () => { palancaActiva = !palancaActiva; Refrescar(); });
                UI.Ubicar(palanca.rt, new Vector2(1, 0.5f), new Vector2(-430, -16), new Vector2(330, 110), new Vector2(1, 0.5f));
            }
            partir = BotonPrincipal("Partir", () => { if (ruta.Count > 0) sPartir?.Dar(); });
            for (int i = 0; i < mano.Count; i++) Anclas.Poner("boleto:" + i, mano[i].rt);
            Anclas.Poner("ruta", casillas.ToArray());
            Anclas.Poner("partir", partir.rt);
            if (palanca != null) Anclas.Poner("palanca", palanca.rt);
        }

        public static UI.Boton Boleto(Transform padre, string texto, Color c, UnityEngine.Events.UnityAction a)
        {
            var bt = UI.CrearBoton(padre, texto, 50, c, a);
            bt.fondo.sprite = UI.Cuadrado; bt.fondo.type = Image.Type.Simple;
            // línea punteada del talón
            for (int y = -40; y <= 40; y += 16)
            {
                var d = UI.Caja(bt.rt, "Talon", Paleta.ConAlfa(Paleta.TextoSobre(c), 0.5f)); d.raycastTarget = false;
                UI.Ubicar(d.rectTransform, new Vector2(0, 0.5f), new Vector2(28, y), new Vector2(4, 9));
            }
            UI.Llenar(bt.texto.rectTransform, 40, 12, 4, 4);
            return bt;
        }

        void AMano(int k)
        {
            if (ruta.Contains(k) || ruta.Count >= P.casillas) return;
            ruta.Add(k);
            inv.Add(palancaActiva && P.mano[k].Invertible);
            palancaActiva = false;
            Refrescar();
        }

        void Refrescar()
        {
            for (int i = 0; i < mano.Count; i++)
            {
                bool usado = ruta.Contains(i);
                UI.Atenuar(mano[i], ColorBoleto(P.mano[i], Dec), usado);
            }
            foreach (var c in casillas) for (int j = c.childCount - 1; j >= 1; j--) Object.Destroy(c.GetChild(j).gameObject);
            for (int k = 0; k < ruta.Count; k++)
            {
                int pos = k;
                var b = P.mano[ruta[k]];
                var bt = Boleto(casillas[k], inv[k] ? EtiquetaInvertida(b) : b.Etiqueta(), inv[k] ? Paleta.Racionales : ColorBoleto(b, Dec), () => Quitar(pos));
                UI.Llenar(bt.rt, 10, 10, 8, 8);
            }
            if (palanca != null)
            {
                palanca.fondo.color = palancaActiva ? Paleta.Racionales : Color.white;
                palanca.texto.color = palancaActiva ? Color.white : Paleta.Texto;
                palanca.texto.text = palancaActiva ? "Restar: ACTIVA" : "Palanca: Restar";
            }
            partir.Activo = ruta.Count > 0;
        }

        void Quitar(int pos)
        {
            if (sPartir == null || sPartir.listo || pos >= ruta.Count) return;
            ruta.RemoveAt(pos); inv.RemoveAt(pos);
            Refrescar();
        }
    }
}
