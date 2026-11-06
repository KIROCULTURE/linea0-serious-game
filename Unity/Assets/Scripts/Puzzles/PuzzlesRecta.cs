using System.Collections;
using System.Collections.Generic;
using Linea0.Juego;
using Linea0.Matematica;
using Linea0.Vista;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Puzzles
{
    /// <summary>Divisor de tramo: dial de partes (1–12) y de avance; Frenar. Acepta equivalentes.</summary>
    public class PuzzleDivisor : PuzzleBase
    {
        int partes = 1, avance;
        Text tPartes, tAvance, fraccion;
        Senal sFrenar;
        UI.Boton frenar;

        public override IEnumerator Jugar()
        {
            PrepararVia();
            Pedido();
            if (P.guion == "pasarse") yield return Pasarse();
            if (P.dialFijo > 0) partes = P.dialFijo;
            ArmarPanel();
            while (true)
            {
                sFrenar = new Senal();
                frenar.Activo = true;
                Refrescar();
                G.Captura(P.id + "_divisor");
                if (G.auto) { partes = P.partesSolucion; avance = P.avanceSolucion + (AutoFalla ? 1 : 0); Refrescar(); sFrenar.Dar(); }
                yield return sFrenar.Esperar();
                frenar.Activo = false;
                yield return Linea0.Sala.ApuestaAnden.Correr(G);
                var destino = P.Inicio + new Racional(avance, partes);
                var arco = V.CrearArco(P.Inicio.ToDouble(), destino.ToDouble(), avance + "/" + partes, Paleta.Racionales, 130);
                yield return T.Mover(destino, -1, arco.Progreso);
                bool ok = P.Meta.EsRacional && destino == P.Meta.Valor;
                string eq = ok && (avance != P.Meta.Valor.Num || partes != P.Meta.Valor.Den) ? "  (" + avance + "/" + partes + " = " + P.Meta.Valor.ToFraccion() + ")" : "";
                if (ok) { yield return Llegada(); if (eq != "") Central("Fracciones equivalentes:" + eq); }
                else Silueta();
                yield return Resultado(ok, "Hiciste " + avance + "/" + partes + " = " + UI.M(destino.ToDecimal(3)) + ". El destino era " + MetaTexto + ".");
                if (ok) yield break;
                V.LimpiarArcos(); QuitarSiluetas(); T.Poner(P.Inicio);
            }
        }

        IEnumerator Pasarse()
        {
            var s = new Senal();
            EtiquetaPanel("BOLETOS", 40);
            var b = PuzzleBoletos.Boleto(Pa.panel, "+1", Paleta.Naturales, () => s.Dar());
            UI.Ubicar(b.rt, new Vector2(0, 0.5f), new Vector2(40, -16), new Vector2(200, 110), new Vector2(0, 0.5f));
            Etiqueta(Pa.panel, "Solo hay boletos de +1. Prueba uno.", 40, Paleta.Via, new Vector2(0.5f, 0.5f), new Vector2(160, -6), new Vector2(1100, 80));
            if (G.auto) s.Dar();
            yield return s.Esperar();
            var a = V.CrearArco(0, 1, "+1", Paleta.Naturales, 130);
            yield return T.Mover(Racional.Uno, -1, a.Progreso);
            Silueta();
            Central(P.lineasGuion[0]);
            yield return Ease.Pausa(G.auto ? 0.2f : 2.5f);
            V.LimpiarArcos(); QuitarSiluetas();
            yield return T.Mover(P.Inicio, 0.8f);
            UI.Limpiar(Pa.panel);
            Pedido();
        }

        Text Dial(string titulo, float x, System.Action menos, System.Action mas, bool bloqueado)
        {
            Etiqueta(Pa.panel, titulo, 26, Paleta.Via, new Vector2(0, 1), new Vector2(x, -28), new Vector2(380, 34)).GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            var bm = UI.CrearBoton(Pa.panel, "◀", 50, bloqueado ? Paleta.Borde : Paleta.Texto, () => { menos(); Refrescar(); });
            UI.Ubicar(bm.rt, new Vector2(0, 0.5f), new Vector2(x, -16), new Vector2(110, 110), new Vector2(0, 0.5f));
            var t = UI.Texto(Pa.panel, "1", 72, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0, 0.5f), new Vector2(x + 115, -16), new Vector2(150, 110));
            var bp = UI.CrearBoton(Pa.panel, "▶", 50, bloqueado ? Paleta.Borde : Paleta.Texto, () => { mas(); Refrescar(); });
            UI.Ubicar(bp.rt, new Vector2(0, 0.5f), new Vector2(x + 270, -16), new Vector2(110, 110), new Vector2(0, 0.5f));
            if (bloqueado) { bm.Activo = false; bp.Activo = false; }
            return t;
        }

        void ArmarPanel()
        {
            bool fijo = P.dialFijo > 0;
            tPartes = Dial(fijo ? "PARTES (BLOQUEADO)" : "PARTES DEL TRAMO", 40, () => partes = Mathf.Max(1, partes - 1), () => partes = Mathf.Min(12, partes + 1), fijo);
            tAvance = Dial("PARTES QUE AVANZA", 480, () => avance = Mathf.Max(0, avance - 1), () => avance++, false);
            fraccion = UI.Texto(Pa.panel, "", 80, Paleta.Racionales);
            UI.Ubicar(fraccion.rectTransform, new Vector2(0, 0.5f), new Vector2(1080, -10), new Vector2(260, 130));
            frenar = BotonPrincipal("Frenar", () => sFrenar?.Dar());
        }

        void Refrescar()
        {
            int max = (int)((P.RangoMax - P.Inicio) * partes).ToDouble();
            avance = Mathf.Clamp(avance, 0, max);
            tPartes.text = partes.ToString();
            tAvance.text = avance.ToString();
            fraccion.text = "<color=#2F80ED>" + avance + "</color>/<color=#27AE60>" + partes + "</color>";
            V.subdivision = partes;
            V.marcas.Clear();
            V.marcas.Add(new Via.Marca { valor = (P.Inicio + new Racional(avance, partes)).ToDouble(), texto = avance + "/" + partes, color = Paleta.Enteros, alto = 70, etiquetaArriba = true, tamTexto = 40 });
            V.Redibujar();
        }
    }

    /// <summary>Lupa: zoom de 10 en 10 (hasta 3 veces), elegir marca y Frenar.</summary>
    public class PuzzleLupa : PuzzleBase
    {
        readonly Stack<(Racional a, int k)> pila = new Stack<(Racional, int)>();
        Racional baseA; int nivel; // nivel 0 = vista normal; n = tramo de 10^-(n-1)
        bool lupaActiva;
        Racional? elegido;
        Text numero;
        UI.Boton bLupa, bAlejar, frenar;
        Senal sFrenar;

        Racional Paso(int k) { var p = Racional.Uno; for (int i = 0; i < k; i++) p /= 10; return p; }

        public override IEnumerator Jugar()
        {
            PrepararVia();
            V.subdivision = 0;
            Pedido();
            ArmarPanel();
            V.alRedibujar = DibujarMarcas;
            V.Redibujar();
            while (true)
            {
                sFrenar = new Senal();
                Refrescar();
                G.Captura(P.id + "_lupa");
                if (G.auto) yield return AutoLupa();
                yield return sFrenar.Esperar();
                frenar.Activo = false;
                yield return Linea0.Sala.ApuestaAnden.Correr(G);
                var d = elegido.Value;
                yield return V.Incluir(d.ToDouble());
                var arco = V.CrearArco(T.x, d.ToDouble(), "", Paleta.Decimales, 110);
                yield return T.Mover(d, -1, arco.Progreso);
                bool ok = P.Meta.EsRacional && d == P.Meta.Valor;
                if (ok) yield return Llegada(); else Silueta();
                yield return Resultado(ok, "Frenaste en " + UI.M(d.ToDecimal(3)) + ". El destino era " + MetaTexto + ".");
                if (ok) yield break;
                V.LimpiarArcos(); QuitarSiluetas(); T.Poner(P.Inicio); elegido = null;
            }
        }

        /// <summary>Prueba automática: hace zoom cifra por cifra hasta la meta (o hasta que el guion la marque).</summary>
        IEnumerator AutoLupa()
        {
            var m = P.Meta.Valor;
            if (AutoFalla) { elegido = m + new Racional(1, 10); Refrescar(); sFrenar.Dar(); yield break; }
            if (nivel == 0) yield return Zoom(new Racional((long)System.Math.Floor(m.ToDouble())));
            while (!elegido.HasValue && nivel <= 3)
            {
                var paso = Paso(nivel);
                var q = (m - baseA) / paso;
                if (q.EsEntero) { elegido = m; break; }
                if (nivel == 3) { float t = 0; while (!elegido.HasValue && t < 5) { t += Time.deltaTime; yield return null; } break; }
                yield return Zoom(baseA + new Racional(q.Num / q.Den) * paso);
            }
            Refrescar();
            sFrenar.Dar();
        }

        void ArmarPanel()
        {
            bLupa = UI.BotonContorno(Pa.panel, "Lupa", 48, () => { lupaActiva = !lupaActiva && nivel < 3; Refrescar(); });
            UI.Ubicar(bLupa.rt, new Vector2(0, 0.5f), new Vector2(40, -6), new Vector2(260, 120), new Vector2(0, 0.5f));
            bAlejar = UI.BotonContorno(Pa.panel, "Alejar", 48, () => Lanzar(Alejar()));
            UI.Ubicar(bAlejar.rt, new Vector2(0, 0.5f), new Vector2(330, -6), new Vector2(260, 120), new Vector2(0, 0.5f));
            numero = UI.Texto(Pa.panel, "", 80, Paleta.Texto);
            UI.Ubicar(numero.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(120, -6), new Vector2(700, 130));
            frenar = BotonPrincipal("Frenar", () => { if (elegido.HasValue) sFrenar?.Dar(); });
        }

        void Refrescar()
        {
            bLupa.fondo.color = lupaActiva ? Paleta.Decimales : Color.white;
            bLupa.texto.text = lupaActiva ? "Toca un tramo" : "Lupa";
            bLupa.Activo = nivel < 3;
            bAlejar.Activo = nivel > 0;
            frenar.Activo = elegido.HasValue;
            numero.text = elegido.HasValue ? UI.M(elegido.Value.ToDecimal(3)) : "";
            V.Redibujar();
        }

        IEnumerator Zoom(Racional a)
        {
            pila.Push((baseA, nivel));
            baseA = a; nivel++;
            lupaActiva = false;
            Refrescar();
            yield return V.AnimarRango(a.ToDouble(), (a + Paso(nivel - 1)).ToDouble(), 0.6f);
            if (P.guion == "periodico" && nivel == 3) Lanzar(Periodico());
        }

        IEnumerator Alejar()
        {
            if (pila.Count == 0) yield break;
            var (a, k) = pila.Pop();
            baseA = a; nivel = k;
            lupaActiva = false;
            Refrescar();
            if (nivel == 0) yield return V.AnimarRango(P.RangoMin.ToDouble(), P.RangoMax.ToDouble(), 0.6f);
            else yield return V.AnimarRango(a.ToDouble(), (a + Paso(nivel - 1)).ToDouble(), 0.6f);
        }

        IEnumerator Periodico()
        {
            Central(P.lineasGuion[0]);
            yield return Ease.Pausa(2f);
            Central(P.lineasGuion[1] + "  (Frena en la marca 1/3)");
            elegido = P.Meta.Valor;
            V.marcas.Add(new Via.Marca { valor = P.Meta.Valor.ToDouble(), texto = "1/3", color = Paleta.Racionales, alto = 80, etiquetaArriba = true, tamTexto = 44 });
            Refrescar();
        }

        /// <summary>Marcas y tramos tocables del nivel actual.</summary>
        void DibujarMarcas(RectTransform capa)
        {
            if (numero == null) return;
            if (nivel == 0)
            {
                long a = (long)System.Math.Ceiling(V.min), b = (long)System.Math.Floor(V.max);
                for (long k = a; k < b; k++)
                {
                    long kk = k;
                    if (lupaActiva) Tramo(capa, k, k + 1, () => Lanzar(Zoom(new Racional(kk))));
                }
                return;
            }
            var paso = Paso(nivel);
            for (int j = 0; j <= 10; j++)
            {
                var v = baseA + j * paso;
                if (j < 10 && lupaActiva && nivel < 3) { var vv = v; Tramo(capa, v.ToDouble(), (v + paso).ToDouble(), () => Lanzar(Zoom(vv))); }
                var img = UI.Caja(capa, "Tic", Paleta.Decimales); img.raycastTarget = false;
                UI.Ubicar(img.rectTransform, Vector2.zero, new Vector2(V.X(v.ToDouble()), Via.YVia + 20), new Vector2(8, 52));
                MarcaBoton(capa, v, UI.M(v.ToDecimal(3)), true);
            }
        }

        void Tramo(RectTransform capa, double a, double b, UnityEngine.Events.UnityAction alClic)
        {
            var img = UI.Caja(capa, "Tramo", Paleta.ConAlfa(Paleta.Decimales, 0.22f));
            UI.Ubicar(img.rectTransform, Vector2.zero, new Vector2((V.X(a) + V.X(b)) / 2, Via.YVia + 130), new Vector2(Mathf.Abs(V.X(b) - V.X(a)) - 6, 200));
            var bt = img.gameObject.AddComponent<Button>();
            bt.onClick.AddListener(alClic);
        }

        void MarcaBoton(RectTransform capa, Racional v, string texto, bool chica)
        {
            if (v.ToDouble() < V.min || v.ToDouble() > V.max) return;
            bool sel = elegido.HasValue && elegido.Value == v;
            var b = UI.CrearBoton(capa, texto, chica ? 30 : 40, sel ? Paleta.Enteros : (chica ? Paleta.Decimales : Paleta.Naturales), () => { elegido = v; Refrescar(); });
            UI.Ubicar(b.rt, Vector2.zero, new Vector2(V.X(v.ToDouble()), Via.YVia - (chica ? 150 : 64)), chica ? new Vector2(Mathf.Min(140, V.PxPorUnidad * (float)Paso(nivel).ToDouble() - 6), 54) : new Vector2(104, 62));
        }
    }

    /// <summary>5.1 Compás: la diagonal del cuadrado se acuesta en la vía y cae en √2; ni el divisor ni la lupa calzan.</summary>
    public class PuzzleCompas : PuzzleBase
    {
        public override IEnumerator Jugar()
        {
            PrepararVia(false);
            V.salida = Racional.Cero; V.Redibujar();
            Pedido();
            float u = V.PxPorUnidad;
            var cuadrado = UI.Nodo(V.capaEncima, "Cuadrado");
            UI.Ubicar(cuadrado, Vector2.zero, new Vector2(V.X(0), Via.YVia + 6), new Vector2(u, u), Vector2.zero);
            var borde = UI.CajaConBorde(cuadrado, "Borde", Paleta.ConAlfa(Color.white, 0.6f), Paleta.Irracionales, 5);
            UI.Llenar(borde.rectTransform); borde.raycastTarget = false;
            var diag = UI.Caja(V.capaEncima, "Diagonal", Paleta.Irracionales);
            diag.raycastTarget = false;
            UI.Ubicar(diag.rectTransform, Vector2.zero, new Vector2(V.X(0), Via.YVia + 6), new Vector2(u * Mathf.Sqrt(2), 8), new Vector2(0, 0.5f));
            diag.rectTransform.localRotation = Quaternion.Euler(0, 0, 45);
            T.gameObject.SetActive(false);

            // paso 1: girar el compás
            Central(P.lineasGuion[0]);
            var s = new Senal();
            var b = BotonPrincipal("Girar compás", () => s.Dar());
            G.Captura(P.id + "_compas");
            if (G.auto) s.Dar();
            yield return s.Esperar();
            Object.Destroy(b.button.gameObject);
            yield return Ease.Durante(1.6f, t => diag.rectTransform.localRotation = Quaternion.Euler(0, 0, 45 * (1 - Ease.SuaveInOut(t))));
            V.marcas.Add(new Via.Marca { valor = System.Math.Sqrt(2), texto = "?", color = Paleta.Irracionales, alto = 80, etiquetaArriba = true, tamTexto = 44 });
            V.Redibujar();

            // paso 2: el divisor no calza
            Central(P.lineasGuion[1]);
            yield return Mostrar("7/5 = 1,4: falta 0,014…   ·   17/12 = 1,416…: se pasa por 0,002…");
            // paso 3: la lupa
            Central(P.lineasGuion[2]);
            yield return Mostrar("1,4  →  1,41  →  1,414  →  1,4142  →  1,41421…  (no se repiten)");
            // paso 4: frenar en la marca
            Central(P.lineasGuion[3]);
            T.gameObject.SetActive(true);
            T.Poner(Racional.Cero);
            var sf = new Senal();
            var fr = BotonPrincipal("Frenar en la marca", () => sf.Dar());
            fr.texto.fontSize = 40;
            if (G.auto) sf.Dar();
            yield return sf.Esperar();
            fr.Activo = false;
            yield return Linea0.Sala.ApuestaAnden.Correr(G, "Cuadrillas: ¿entre qué estaciones para? ¡Escríbanlo!");
            var arco = V.CrearArco(0, System.Math.Sqrt(2), "√2", Paleta.Irracionales, 130);
            yield return T.Mover(Posicion.Raiz2, -1, arco.Progreso);
            V.destino = Posicion.Raiz2; V.letreroDestino = "√2";
            yield return Llegada();
            yield return Resultado(true, "");
        }

        IEnumerator Mostrar(string s)
        {
            UI.Limpiar(Pa.panel);
            Etiqueta(Pa.panel, s, 44, Paleta.Irracionales, new Vector2(0.5f, 0.5f), new Vector2(-180, -6), new Vector2(1400, 90));
            var sen = new Senal();
            BotonPrincipal("Seguir", () => sen.Dar());
            if (G.auto) sen.Dar();
            yield return sen.Esperar();
            UI.Limpiar(Pa.panel);
        }
    }
}
