using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Linea0.Datos;
using Linea0.Juego;
using Linea0.Matematica;
using Linea0.Vista;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Puzzles
{
    /// <summary>Utilidades para mostrar expresiones como fila de vagones y recorrerlas con el tren.</summary>
    public static class Vagones
    {
        /// <summary>Dibuja la expresión; los operadores disponibles son botones si alClic no es null.</summary>
        public static RectTransform Dibujar(RectTransform padre, List<Token> t, bool dec, System.Action<int> alClic, float y)
        {
            var raiz = UI.Nodo(padre, "Vagones");
            UI.Ubicar(raiz, new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(1800, 120), new Vector2(0.5f, 1));
            var disp = Expresion.Disponibles(t);
            var anchos = new List<float>();
            for (int i = 0; i < t.Count; i++)
            {
                var k = t[i];
                string s = Expresion.TextoToken(k, i > 0 ? t[i - 1] : null, dec);
                anchos.Add(k.Tipo == TipoToken.Num ? Mathf.Max(110, s.Length * 34 + 40) : k.Tipo == TipoToken.Op ? 110 : 44);
            }
            float total = anchos.Sum() + 14 * (t.Count - 1), x = -total / 2;
            for (int i = 0; i < t.Count; i++)
            {
                var k = t[i];
                string s = Expresion.TextoToken(k, i > 0 ? t[i - 1] : null, dec);
                float w = anchos[i];
                Vector2 pos = new Vector2(x + w / 2, -60);
                if (k.Tipo == TipoToken.Num)
                {
                    var c = UI.CajaConBorde(raiz, "Num", Color.white, Paleta.Texto, 4);
                    UI.Ubicar(c.rectTransform, new Vector2(0.5f, 1), pos, new Vector2(w, 100));
                    UI.Texto(c.transform, s, 56, Paleta.Texto);
                }
                else if (k.Tipo == TipoToken.Op)
                {
                    bool prod = k.EsProducto;
                    var col = prod ? Paleta.Enteros : Paleta.Naturales;
                    bool clic = alClic != null && disp.Contains(i);
                    int id = k.Id;
                    var b = UI.CrearBoton(raiz, s, 64, col, clic ? () => alClic(id) : (UnityEngine.Events.UnityAction)null);
                    b.fondo.sprite = UI.Cuadrado; b.fondo.type = Image.Type.Simple;
                    UI.Ubicar(b.rt, new Vector2(0.5f, 1), pos, new Vector2(w, 100));
                    b.Activo = alClic == null || clic;
                    if (alClic != null && !clic) b.fondo.color = Paleta.ConAlfa(col, 0.35f);
                }
                else
                {
                    var p = UI.Texto(raiz, s, 80, Paleta.Texto);
                    UI.Ubicar(p.rectTransform, new Vector2(0.5f, 1), pos, new Vector2(w, 110));
                }
                x += w + 14;
            }
            return raiz;
        }

        /// <summary>Hace un paso y mueve el tren al resultado con un arco "3 × 4 = 12".</summary>
        public static IEnumerator Paso(PuzzleBase pz, List<Token> t, int indice, System.Action<List<Token>> listo)
        {
            var a = t[indice - 1].Valor; var b = t[indice + 1].Valor; char op = t[indice].Op;
            var n = Expresion.Paso(t, indice, out var r);
            string et = pz.Num(a).Replace('-', '−') + " " + Expresion.SimboloOp(op) + " " + (b.Signo < 0 ? "(" + pz.Num(b) + ")" : pz.Num(b)) + " = " + pz.Num(r);
            et = UI.M(et);
            var V = pz.G.P.via; var T = pz.G.P.tren;
            yield return V.Incluir(r.ToDouble());
            V.LimpiarArcos();
            var arco = V.CrearArco(T.x, r.ToDouble(), et, op == '+' || op == '-' ? Paleta.Naturales : Paleta.Enteros, 130);
            yield return T.Mover(r, -1, arco.Progreso);
            listo(n);
        }

        /// <summary>El índice del operador que va primero según la jerarquía.</summary>
        public static int SiguienteCorrecto(List<Token> t)
        {
            var d = Expresion.Disponibles(t);
            foreach (int i in d) if (t[i].EsProducto) return i;
            return d[0];
        }
    }

    /// <summary>Sin vía: solo la cuenta y 4 alternativas; después se levanta el letrero y el tren comprueba.</summary>
    public class PuzzleSinVia : PuzzleAlternativas
    {
        RectTransform letrero;

        public override IEnumerator Jugar()
        {
            PrepararVia(false);
            V.salida = null; V.Redibujar();
            Pedido();
            var s = new Senal();
            while (true)
            {
                Tapar();
                yield return Elegir(s, new Vector2(0, -6));
                yield return Linea0.Sala.ApuestaAnden.Correr(G, "Cuadrillas: ¿cuánto da? ¡Escríbanlo!");
                bool ok = s.valor == P.correcta;
                yield return Ease.Durante(0.6f, t => letrero.anchoredPosition = new Vector2(0, t * 900));
                Object.Destroy(letrero.gameObject);
                yield return Comprobar();
                if (ok) yield return Llegada();
                yield return Resultado(ok, TextoEleccion(s.valor));
                BorrarBotones();
                if (ok) yield break;
                descartadas.Add(s.valor);
                // vuelve el letrero y el tren a la partida
                V.LimpiarArcos(); UI.Limpiar(V.capaEncima); V.destinoIluminado = false;
                PrepararVia(false); V.salida = null; V.Redibujar();
            }
        }

        void Tapar()
        {
            T.gameObject.SetActive(false);
            letrero = UI.Nodo(V.capaEncima, "Mantencion");
            UI.Llenar(letrero);
            var fondo = UI.Caja(letrero, "Fondo", Paleta.Hex("#FFF4D6")); UI.Llenar(fondo.rectTransform);
            for (int i = 0; i < 14; i++)
            {
                var f = UI.Caja(letrero, "Franja", i % 2 == 0 ? Paleta.Naturales : Paleta.Texto);
                UI.Ubicar(f.rectTransform, new Vector2(0, 0), new Vector2(i * 150, 0), new Vector2(150, 34), new Vector2(0, 0));
                f.rectTransform.anchorMin = f.rectTransform.anchorMax = new Vector2(i / 14f, 0);
                var f2 = UI.Caja(letrero, "Franja", i % 2 == 0 ? Paleta.Texto : Paleta.Naturales);
                f2.rectTransform.anchorMin = f2.rectTransform.anchorMax = new Vector2(i / 14f, 1);
                f2.rectTransform.pivot = new Vector2(0, 1); f2.rectTransform.anchoredPosition = Vector2.zero; f2.rectTransform.sizeDelta = new Vector2(150, 34);
            }
            var t = UI.Texto(letrero, "VÍA EN MANTENCIÓN", 40, Paleta.Via);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(1200, 50));
            var e = UI.Texto(letrero, UI.M(Expresion.Texto(Expresion.Tokenizar(P.expresion), Dec)) + " = ?", 130, Paleta.Texto);
            UI.Ubicar(e.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(1700, 200));
        }

        IEnumerator Comprobar()
        {
            T.gameObject.SetActive(P.comprobacion != "repartir");
            switch (P.comprobacion)
            {
                case "repartir":
                    yield return PuzzleRepartir.Animar(G, V.capaEncima, P.cantidad, P.vagones, false, null);
                    break;
                case "jerarquia":
                    var t = Expresion.Tokenizar(P.expresion);
                    var vag = Vagones.Dibujar(V.capaEncima, t, Dec, null, -10);
                    T.Poner(t[0].Valor);
                    while (!Expresion.Terminada(t))
                    {
                        yield return Ease.Pausa(0.3f);
                        List<Token> n = null;
                        yield return Vagones.Paso(this, t, Vagones.SiguienteCorrecto(t), x => n = x);
                        t = n;
                        Object.Destroy(vag.gameObject);
                        vag = Vagones.Dibujar(V.capaEncima, t, Dec, null, -10);
                    }
                    break;
                default:
                    T.Poner(P.Inicio);
                    V.salida = P.Inicio; V.Redibujar();
                    var pos = P.Inicio;
                    foreach (var b in P.mano)
                    {
                        var nueva = b.Aplicar(pos);
                        if (b.operacion == "expreso")
                            for (int i = 0; i < b.repeticiones; i++)
                            {
                                var a = V.CrearArco((pos + i * b.Valor).ToDouble(), (pos + (i + 1) * b.Valor).ToDouble(), "+" + UI.M(b.valor), PuzzleBoletos.ColorBoleto(b, Dec), 130);
                                yield return T.Mover(pos + (i + 1) * b.Valor, -1, a.Progreso);
                            }
                        else
                        {
                            var a = V.CrearArco(pos.ToDouble(), nueva.ToDouble(), b.Etiqueta(), PuzzleBoletos.ColorBoleto(b, Dec), 130);
                            yield return T.Mover(nueva, -1, a.Progreso);
                        }
                        pos = nueva;
                    }
                    V.destino = pos; V.Redibujar();
                    break;
            }
        }
    }

    /// <summary>Hora punta: la cuadrilla elige qué vagón parte primero; si el orden es malo, "choque de horarios".</summary>
    public class PuzzleJerarquia : PuzzleBase
    {
        public override IEnumerator Jugar()
        {
            Pedido();
            var meta = Racional.Parse(P.meta);
            while (true)
            {
                var t = Expresion.Tokenizar(P.expresion);
                PrepararVia(false);
                V.salida = null;
                T.Poner(t[0].Valor);
                V.Redibujar();
                var vag = Vagones.Dibujar(V.capaEncima, t, Dec, null, -10);
                var sp = new Senal();
                var partir = BotonPrincipal("Partir", () => sp.Dar());
                Etiqueta(Pa.panel, "Decidan en voz alta el orden. Al partir, toquen el vagón que sale primero.", 34, Paleta.Via, new Vector2(0, 0.5f), new Vector2(40, -6), new Vector2(1420, 80));
                G.Captura(P.id + "_vagones");
                if (G.auto) sp.Dar();
                yield return sp.Esperar();
                UI.Limpiar(Pa.panel);
                yield return Linea0.Sala.ApuestaAnden.Correr(G, "Cuadrillas: ¿en qué número termina? ¡Escríbanlo!");
                Etiqueta(Pa.panel, "¿Qué vagón parte ahora? Toca un signo.", 44, Paleta.Texto, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400, 80));
                while (!Expresion.Terminada(t))
                {
                    var s = new Senal();
                    Object.Destroy(vag.gameObject);
                    vag = Vagones.Dibujar(V.capaEncima, t, Dec, id => s.Dar(id), -10);
                    if (G.auto) s.Dar(t[Vagones.SiguienteCorrecto(t)].Id);
                    yield return s.Esperar();
                    int idx = t.FindIndex(k => k.Id == s.valor);
                    Object.Destroy(vag.gameObject);
                    vag = Vagones.Dibujar(V.capaEncima, t, Dec, null, -10);
                    List<Token> n = null;
                    yield return Vagones.Paso(this, t, idx, x => n = x);
                    t = n;
                }
                Object.Destroy(vag.gameObject);
                vag = Vagones.Dibujar(V.capaEncima, t, Dec, null, -10);
                UI.Limpiar(Pa.panel);
                bool ok = t[0].Valor == meta;
                if (ok) { V.destino = meta; yield return Llegada(); }
                yield return Resultado(ok, "Choque de horarios: el expreso tenía prioridad. Te dio " + UI.M(Num(t[0].Valor)) + ".");
                if (ok) yield break;
                V.LimpiarArcos(); UI.Limpiar(V.capaEncima);
            }
        }
    }

    /// <summary>Serpiente (6.4): se elige la estación final y el tren recorre en orden, sin prioridad.</summary>
    public class PuzzleSerpiente : PuzzleAlternativas
    {
        public override IEnumerator Jugar()
        {
            Pedido();
            var s = new Senal();
            while (true)
            {
                var t = Expresion.Tokenizar(P.expresion);
                PrepararVia(false);
                T.Poner(t[0].Valor);
                var vag = Vagones.Dibujar(V.capaEncima, t, Dec, null, -10);
                yield return Elegir(s, new Vector2(0, -6));
                yield return Linea0.Sala.ApuestaAnden.Correr(G, "Cuadrillas: ¿dónde termina la serpiente? ¡Escríbanlo!");
                while (!Expresion.Terminada(t))
                {
                    List<Token> n = null;
                    yield return Vagones.Paso(this, t, Expresion.Disponibles(t)[0], x => n = x);
                    t = n;
                    Object.Destroy(vag.gameObject);
                    vag = Vagones.Dibujar(V.capaEncima, t, Dec, null, -10);
                }
                bool ok = s.valor == P.correcta;
                if (ok) { V.destino = t[0].Valor; yield return Llegada(); }
                yield return Resultado(ok, TextoEleccion(s.valor));
                BorrarBotones();
                if (ok) yield break;
                descartadas.Add(s.valor);
                V.LimpiarArcos(); UI.Limpiar(V.capaEncima);
            }
        }
    }

    /// <summary>Contar saltos (3.7, 4.8): cuántos saltos caben en el tramo.</summary>
    public class PuzzleContarSaltos : PuzzleAlternativas
    {
        public override IEnumerator Jugar()
        {
            Pedido();
            var ini = Racional.Parse(P.tramo[0]); var fin = Racional.Parse(P.tramo[1]); var salto = Racional.Parse(P.salto);
            var s = new Senal();
            while (true)
            {
                PrepararVia(false);
                V.salida = ini; V.destino = fin; V.Redibujar();
                T.Poner(ini);
                var info = UI.Texto(V.capaEncima, "Salto: " + UI.M(P.salto), 56, Paleta.Racionales);
                UI.Ubicar(info.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -30), new Vector2(800, 70));
                yield return Elegir(s, new Vector2(0, -6));
                yield return Linea0.Sala.ApuestaAnden.Correr(G, "Cuadrillas: ¿cuántos saltos caben? ¡Escríbanlo!");
                var contador = UI.Texto(V.capaEncima, "0", 150, Paleta.Texto);
                UI.Ubicar(contador.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -100), new Vector2(400, 170));
                var pos = ini; int n = 0;
                while (pos < fin)
                {
                    var a = V.CrearArco(pos.ToDouble(), (pos + salto).ToDouble(), "", Paleta.Racionales, 130);
                    yield return T.Mover(pos + salto, 0.45f, a.Progreso);
                    pos += salto; n++;
                    contador.text = n.ToString();
                }
                bool ok = s.valor == P.correcta;
                if (ok) yield return Llegada();
                yield return Resultado(ok, TextoEleccion(s.valor));
                BorrarBotones();
                if (ok) yield break;
                descartadas.Add(s.valor);
                V.LimpiarArcos(); UI.Limpiar(V.capaEncima);
            }
        }
    }
}
