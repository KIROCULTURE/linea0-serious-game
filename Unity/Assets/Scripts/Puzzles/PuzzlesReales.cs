using System.Collections;
using System.Collections.Generic;
using Linea0.Juego;
using Linea0.Matematica;
using Linea0.Sala;
using Linea0.Vista;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Puzzles
{
    /// <summary>5.2 Rueda: una rueda de diámetro 1 rueda una vuelta sin resbalar y el punto rojo cae en π.</summary>
    public class PuzzleRueda : PuzzleBase
    {
        public override IEnumerator Jugar()
        {
            PrepararVia(false);
            V.salida = Racional.Cero; V.Redibujar();
            Pedido();
            T.gameObject.SetActive(false);
            float d = V.PxPorUnidad, r = d / 2;
            var centro = UI.Nodo(V.capaEncima, "Rueda");
            UI.Ubicar(centro, Vector2.zero, new Vector2(V.X(0), Via.YVia + r), new Vector2(d, d));
            var borde = UI.Caja(centro, "Borde", Paleta.Texto, UI.Circulo); UI.Llenar(borde.rectTransform);
            var dentro = UI.Caja(centro, "Dentro", Color.white, UI.Circulo); UI.Llenar(dentro.rectTransform, 12, 12, 12, 12);
            for (int i = 0; i < 4; i++)
            {
                var rayo = UI.Caja(centro, "Rayo", Paleta.Gris);
                UI.Ubicar(rayo.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(d - 24, 6));
                rayo.rectTransform.localRotation = Quaternion.Euler(0, 0, i * 45);
            }
            var punto = UI.Caja(centro, "Punto", Paleta.Error, UI.Circulo);
            UI.Ubicar(punto.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -r + 10), new Vector2(30, 30));
            var eje = UI.Caja(centro, "Eje", Paleta.Texto, UI.Circulo);
            UI.Ubicar(eje.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(24, 24));
            foreach (var g in centro.GetComponentsInChildren<Image>()) g.raycastTarget = false;

            Central(P.lineasGuion[0]);
            var s = new Senal();
            var b = BotonPrincipal("Rodar una vuelta", () => s.Dar());
            b.texto.fontSize = b.texto.resizeTextMaxSize = 40;
            G.Captura(P.id + "_rueda");
            if (G.auto) s.Dar();
            yield return s.Esperar();
            Object.Destroy(b.button.gameObject);
            // rodar sin resbalar: avanza π·d mientras gira 360°
            var huella = new List<Image>();
            yield return Ease.Durante(3f, t =>
            {
                float e = Ease.SuaveInOut(t);
                double x = Mathf.PI * e;
                centro.anchoredPosition = new Vector2(V.X(x), Via.YVia + r);
                centro.localRotation = Quaternion.Euler(0, 0, -360 * e);
            });
            V.marcas.Add(new Via.Marca { valor = System.Math.PI, texto = "?", color = Paleta.Irracionales, alto = 80, etiquetaArriba = true, tamTexto = 44 });
            V.Redibujar();
            yield return Mostrar("Con la lupa:  3,1  →  3,14  →  3,141  →  3,1415  →  3,14159…  (sin patrón)");
            Central(P.lineasGuion[1]);
            Object.Destroy(centro.gameObject);
            T.gameObject.SetActive(true);
            T.Poner(Racional.Cero);
            var sf = new Senal();
            var fr = BotonPrincipal("Frenar en la marca", () => sf.Dar());
            fr.texto.fontSize = fr.texto.resizeTextMaxSize = 40;
            if (G.auto) sf.Dar();
            yield return sf.Esperar();
            fr.Activo = false;
            yield return ApuestaAnden.Correr(G, "Cuadrillas: ¿entre qué estaciones para? ¡Escríbanlo!");
            var arco = V.CrearArco(0, System.Math.PI, "π", Paleta.Irracionales, 130);
            yield return T.Mover(Posicion.Pi, -1, arco.Progreso);
            V.destino = Posicion.Pi; V.letreroDestino = "π";
            yield return Llegada();
            yield return Resultado(true, "");
        }

        IEnumerator Mostrar(string s)
        {
            UI.Limpiar(Pa.panel);
            Etiqueta(Pa.panel, s, 42, Paleta.Irracionales, new Vector2(0, 0.5f), new Vector2(40, -6), new Vector2(1420, 90)).GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            var sen = new Senal();
            BotonPrincipal("Seguir", () => sen.Dar());
            if (G.auto) sen.Dar();
            yield return sen.Esperar();
            UI.Limpiar(Pa.panel);
        }
    }

    /// <summary>5.3 Clasificar: cada tarjeta va a la línea más chica del plano que la contiene. Se elige la tarjeta y luego la región.</summary>
    public class PuzzleClasificar : PuzzleBase
    {
        static readonly string[] Codigos = { "I", "Q", "Z", "N" };
        static readonly string[] Nombres = { "ℝ Reales · franja de los Irracionales", "ℚ Racionales", "ℤ Enteros", "ℕ Naturales" };
        static readonly Color[] Colores = { Paleta.Irracionales, Paleta.Racionales, Paleta.Enteros, Paleta.Naturales };
        static readonly string[] Tambien = { "", "también es real", "también es racional y real", "también es entero, racional y real" };

        readonly List<Image> regiones = new List<Image>();
        readonly int[] llenas = new int[4];

        public override IEnumerator Jugar()
        {
            PrepararVia(false);
            T.gameObject.SetActive(false);
            Pedido();
            var tapa = UI.Caja(V.capaEncima, "Tapa", Paleta.Cielo); UI.Llenar(tapa.rectTransform);
            var plano = UI.Nodo(V.capaEncima, "Plano");
            UI.Ubicar(plano, new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(1500, 600));
            int elegida = -1, region = -1;
            for (int i = 0; i < 4; i++)
            {
                int k = i;
                float w = 1500 - i * 300, h = 600 - i * 130;
                var b = UI.CajaConBorde(plano, Nombres[i], Color.white, Colores[i], 10);
                UI.Ubicar(b.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -i * 55), new Vector2(w, h));
                b.transform.GetChild(0).GetComponent<Image>().color = Paleta.ConAlfa(Colores[i], 0.08f) + new Color(0.92f, 0.92f, 0.92f, 1);
                var t = UI.Texto(b.transform, Nombres[i], 34, Colores[i] == Paleta.Naturales ? Paleta.Hex("#B07A00") : Colores[i], TextAnchor.UpperCenter);
                UI.Llenar(t.rectTransform, 10, 10, 14, 0);
                b.raycastTarget = true;
                b.gameObject.AddComponent<Button>().onClick.AddListener(() => region = k);
                regiones.Add(b);
            }
            // tarjetas
            var tarjetas = new List<UI.Boton>();
            float x = -((P.cartas.Length - 1) * 230) / 2f;
            for (int i = 0; i < P.cartas.Length; i++)
            {
                int k = i;
                var c = UI.CrearBoton(Pa.panel, UI.M(P.cartas[i]), 60, Paleta.Texto, () => elegida = k, Paleta.Texto);
                c.fondo.sprite = UI.Cuadrado; c.fondo.type = Image.Type.Simple;
                var dentro = UI.Caja(c.rt, "Dentro", Color.white); dentro.raycastTarget = false;
                dentro.transform.SetAsFirstSibling();
                UI.Llenar(dentro.rectTransform, 5, 5, 5, 5);
                c.fondo = dentro; // el color de selección va en el interior
                UI.Ubicar(c.rt, new Vector2(0.5f, 0.5f), new Vector2(x + i * 230, -6), new Vector2(200, 130));
                // también se puede arrastrar: al soltarla sobre una línea, cuenta como elegir esa región
                var inicioPos = c.rt.anchoredPosition;
                var arr = c.rt.gameObject.AddComponent<Arrastre>();
                arr.alArrastrar = ev =>
                {
                    if (!c.button.interactable) return;
                    elegida = k;
                    RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)c.rt.parent, ev.position, ev.pressEventCamera, out var lp);
                    c.rt.anchoredPosition = lp - ((RectTransform)c.rt.parent).rect.center;
                };
                arr.alSoltar = ev =>
                {
                    if (!c.button.interactable) return;
                    c.rt.anchoredPosition = inicioPos;
                    for (int r = 3; r >= 0; r--)
                        if (RectTransformUtility.RectangleContainsScreenPoint(regiones[r].rectTransform, ev.position, ev.pressEventCamera)) { elegida = k; region = r; break; }
                };
                tarjetas.Add(c);
            }
            G.Captura(P.id + "_plano");
            int restantes = P.cartas.Length;
            while (restantes > 0)
            {
                int intentos = 0;
                Central("Cuadrilla " + Paleta.Letras[G.S.turno] + ": elige una tarjeta y después su línea.");
                if (!G.S.conCuadrillas) Central(P.frasePedido);
                elegida = -1; region = -1;
                while (true)
                {
                    if (G.auto) { for (int i = 0; i < tarjetas.Count; i++) if (tarjetas[i].button.interactable) { elegida = i; break; } }
                    while (elegida < 0) yield return null;
                    foreach (var t in tarjetas) if (t.button.interactable) t.fondo.color = Color.white;
                    tarjetas[elegida].fondo.color = Paleta.Sol;
                    region = -1;
                    if (G.auto)
                    {
                        int destino = (System.Array.IndexOf(Codigos, P.regiones[elegida]) + (G.fallar && intentos == 0 ? 1 : 0)) % 4;
                        if (G.fallar)
                        {
                            // prueba automática del arrastre: suelta la tarjeta sobre la franja de esa región
                            var rDest = regiones[destino].rectTransform;
                            var punto = destino == 3 ? Vector2.zero : new Vector2(-rDest.rect.width / 2 + 60, 0);
                            var pant = RectTransformUtility.WorldToScreenPoint(null, rDest.TransformPoint(punto));
                            tarjetas[elegida].rt.GetComponent<Arrastre>().alSoltar(new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current) { position = pant });
                            if (region != destino) Debug.LogError("Arrastre de tarjeta: región " + region + " en vez de " + destino);
                        }
                        region = destino;
                    }
                    while (region < 0)
                    {
                        // se puede cambiar de tarjeta antes de elegir región
                        if (tarjetas[elegida].fondo.color != Paleta.Sol) { foreach (var t in tarjetas) if (t.button.interactable) t.fondo.color = Color.white; tarjetas[elegida].fondo.color = Paleta.Sol; }
                        yield return null;
                    }
                    intentos++;
                    if (Codigos[region] == P.regiones[elegida]) break;
                    Central("No va ahí. Busca la línea más chica que contiene a " + UI.M(P.cartas[elegida]) + ".");
                    region = -1;
                }
                // acierto: la tarjeta entra a su región y se encienden los anillos de afuera
                var tc = tarjetas[elegida];
                tc.button.interactable = false;
                tc.rt.SetParent(regiones[region].transform, false);
                int n = llenas[region]++;
                var rr = regiones[region].rectTransform;
                float w = rr.sizeDelta.x, h = rr.sizeDelta.y;
                // las tarjetas van en la franja lateral de su región (la de adentro tapa el centro)
                var pos = region == 3 ? new Vector2(n * 160, -30) : new Vector2((n % 2 == 0 ? -1 : 1) * (w / 2 - 82), -h / 2 + 70);
                UI.Ubicar(tc.rt, new Vector2(0.5f, 0.5f), pos, new Vector2(140, 90));
                tc.fondo.color = Color.white;
                string tambien = Tambien[region];
                Central(UI.M(P.cartas[elegida]) + (tambien != "" ? " " + tambien + "." : " es irracional: no es fracción."));
                int pts = intentos == 1 ? 2 : intentos == 2 ? 1 : 0;
                R.intentos += intentos;
                G.S.Sumar(G.S.turno, pts);
                yield return Ease.Durante(0.8f, t =>
                {
                    for (int i = 0; i <= region; i++)
                        regiones[i].color = Color.Lerp(Paleta.Sol, Colores[i], Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2)));
                });
                for (int i = 0; i < 4; i++) regiones[i].color = Colores[i];
                restantes--;
                if (restantes > 0 && G.S.conCuadrillas) G.S.SiguienteTurno();
                Marcador.Refrescar(Pa, G.S);
                yield return Ease.Pausa(G.auto ? 0.1f : 1f);
            }
            R.logrado = true;
            R.primerIntento = R.intentos == P.cartas.Length && R.pistas == 0;
            intento = 1;
            yield return Cartel(true, UI.M(P.cuentaEscrita), P.fraseNombre, "", "Siguiente");
        }
    }
}
