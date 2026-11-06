using System.Collections;
using System.Collections.Generic;
using Linea0.Juego;
using Linea0.Matematica;
using Linea0.Vista;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Puzzles
{
    /// <summary>1.1 Contar: se toca cada pasajero (1, 2, 3…) y se elige cuántos son.</summary>
    public class PuzzleContar : PuzzleAlternativas
    {
        public override IEnumerator Jugar()
        {
            PrepararVia(false);
            Pedido();
            var gente = new List<RectTransform>();
            int contados = 0;
            var listo = new Senal();
            for (int i = 0; i < P.cantidad; i++)
            {
                var per = Pantalla.Persona(V.capaEncima, ColoresPasajero[i % ColoresPasajero.Length], 1.1f);
                UI.Ubicar(per, Vector2.zero, new Vector2(V.X(1.6 + i * 0.85), Via.YVia + 14), per.sizeDelta, new Vector2(0.5f, 0));
                var img = per.GetChild(1).GetComponent<Image>(); img.raycastTarget = true;
                var b = per.GetChild(1).gameObject.AddComponent<Button>();
                var num = UI.Texto(per, "", 56, Paleta.Texto);
                UI.Ubicar(num.rectTransform, new Vector2(0.5f, 1), new Vector2(0, 50), new Vector2(100, 70));
                b.onClick.AddListener(() =>
                {
                    if (num.text != "") return;
                    contados++;
                    num.text = contados.ToString();
                    if (contados == P.cantidad) listo.Dar();
                });
                gente.Add(per);
            }
            Etiqueta(Pa.panel, "Toca a cada pasajero para contarlo.", 40, Paleta.Via, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1400, 80));
            G.Captura(P.id + "_contar");
            if (G.auto) listo.Dar();
            yield return listo.Esperar();
            UI.Limpiar(Pa.panel);
            var s = new Senal();
            while (true)
            {
                yield return Elegir(s, new Vector2(0, -6));
                yield return Linea0.Sala.ApuestaAnden.Correr(G, "Cuadrillas: ¿cuántos son? ¡Escríbanlo!");
                bool ok = s.valor == P.correcta;
                if (ok)
                {
                    // suben al tren
                    var desde = new List<Vector2>();
                    foreach (var g in gente) desde.Add(g.anchoredPosition);
                    float xt = V.X(T.x);
                    yield return Ease.Durante(0.9f, t => { for (int i = 0; i < gente.Count; i++) gente[i].anchoredPosition = Vector2.Lerp(desde[i], new Vector2(xt, desde[i].y), Ease.SuaveInOut(t)); });
                    foreach (var g in gente) g.gameObject.SetActive(false);
                    T.Puertas(true);
                }
                yield return Resultado(ok, TextoEleccion(s.valor));
                BorrarBotones();
                if (ok) yield break;
                descartadas.Add(s.valor);
            }
        }
    }

    /// <summary>1.5 / 1.6 Repartir: cada clic manda un pasajero al siguiente vagón.</summary>
    public class PuzzleRepartir : PuzzleAlternativas
    {
        public static IEnumerator Animar(GameManager g, RectTransform capa, int cantidad, int vagones, bool porClic, Senal hecho)
        {
            var raiz = UI.Llenar(UI.Nodo(capa, "Repartir"));
            float ancho = 300, sep = 40, total = vagones * ancho + (vagones - 1) * sep;
            var conteo = new List<Text>();
            var vagRT = new List<RectTransform>();
            var cuenta = new int[vagones];
            for (int v = 0; v < vagones; v++)
            {
                var c = UI.CajaConBorde(raiz, "Vagon", Color.white, Paleta.Texto, 5);
                UI.Ubicar(c.rectTransform, new Vector2(0.5f, 0), new Vector2(-total / 2 + ancho / 2 + v * (ancho + sep) + 260, Via.YVia + 30), new Vector2(ancho, 150), new Vector2(0.5f, 0));
                var n = UI.Texto(c.transform, "0", 48, Paleta.Via);
                UI.Ubicar(n.rectTransform, new Vector2(0.5f, 1), new Vector2(0, 44), new Vector2(200, 56));
                conteo.Add(n);
                vagRT.Add(c.rectTransform);
            }
            var anden = new List<RectTransform>();
            for (int i = 0; i < cantidad; i++)
            {
                var per = Pantalla.Persona(raiz, PuzzleBase.ColoresPasajero[i % PuzzleBase.ColoresPasajero.Length], 0.55f);
                UI.Ubicar(per, new Vector2(0, 0), new Vector2(70 + (i % 6) * 46, Via.YVia + 40 + (i / 6) * 84), per.sizeDelta, new Vector2(0.5f, 0));
                anden.Add(per);
            }
            var sobra = UI.Texto(raiz, "", 44, Paleta.Error);
            UI.Ubicar(sobra.rectTransform, new Vector2(0, 0), new Vector2(200, Via.YVia + 300), new Vector2(360, 60));
            int siguiente = 0, mandados = 0;
            int repartibles = cantidad - cantidad % vagones;
            var clic = new Senal();
            Button boton = null;
            if (porClic)
            {
                var b = UI.CrearBoton(raiz, "Mandar pasajero", 44, Paleta.Naturales, () => clic.Dar());
                UI.Ubicar(b.rt, new Vector2(0, 0), new Vector2(230, Via.YVia - 60), new Vector2(420, 96));
                boton = b.button;
            }
            while (mandados < repartibles)
            {
                if (porClic && !g.auto) { clic.Reset(); yield return clic.Esperar(); }
                else yield return Ease.Pausa(0.12f);
                var per = anden[cantidad - 1 - mandados];
                int v = siguiente;
                var vag = vagRT[v];
                int k = cuenta[v];
                per.SetParent(vag, false);
                UI.Ubicar(per, new Vector2(0, 0), new Vector2(34 + (k % 6) * 46, 10 + (k / 6) * 70), per.sizeDelta, new Vector2(0.5f, 0));
                cuenta[v]++;
                conteo[v].text = cuenta[v].ToString();
                mandados++;
                siguiente = (siguiente + 1) % vagones;
            }
            if (boton != null) Object.Destroy(boton.gameObject);
            if (cantidad % vagones > 0) sobra.text = "Sobra " + (cantidad % vagones);
            hecho?.Dar();
        }

        public override IEnumerator Jugar()
        {
            PrepararVia(false);
            T.gameObject.SetActive(false);
            V.mostrarPildoras = false; V.salida = null; V.Redibujar();
            Pedido();
            var hecho = new Senal();
            G.Captura(P.id + "_repartir");
            yield return Animar(G, V.capaEncima, P.cantidad, P.vagones, true, hecho);
            var s = new Senal();
            while (true)
            {
                yield return Elegir(s, new Vector2(0, -6));
                yield return Linea0.Sala.ApuestaAnden.Correr(G, "Cuadrillas: ¿cuántos van por vagón? ¡Escríbanlo!");
                bool ok = s.valor == P.correcta;
                yield return Resultado(ok, TextoEleccion(s.valor));
                BorrarBotones();
                if (ok) yield break;
                descartadas.Add(s.valor);
            }
        }
    }
}
