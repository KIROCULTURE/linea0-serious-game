using System.Collections;
using Linea0.Juego;
using Linea0.Vista;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Sala
{
    /// <summary>"Cómo jugar": 4 pantallas con un dibujo de primitivas y una línea de texto.</summary>
    public static class ComoJugar
    {
        static readonly string[] Textos =
        {
            "La vía es la recta numérica. Lleva a cada pasajero a su estación.",
            "La cuadrilla de turno decide el viaje. El maquinista hace clic.",
            "Antes de que parta el tren, las otras cuadrillas anotan en su hoja dónde va a parar y la levantan.",
            "Puntos: +2 si la cuadrilla de turno llega al primer intento, +1 por cada predicción correcta."
        };

        public static IEnumerator Mostrar(GameManager g)
        {
            var raiz = UI.Llenar(UI.Nodo(g.P.overlay, "ComoJugar"));
            var fondo = UI.Caja(raiz, "Fondo", Paleta.Cielo); UI.Llenar(fondo.rectTransform);
            var titulo = UI.Texto(raiz, "Cómo jugar", 72, Paleta.Texto);
            UI.Ubicar(titulo.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(1200, 90));
            var dibujo = UI.Nodo(raiz, "Dibujo");
            UI.Ubicar(dibujo, new Vector2(0.5f, 0.5f), new Vector2(0, 90), new Vector2(1400, 520));
            var texto = UI.Texto(raiz, "", 46, Paleta.Texto, TextAnchor.MiddleCenter, false);
            UI.Ubicar(texto.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 250), new Vector2(1760, 130));
            var pagina = UI.Texto(raiz, "", 32, Paleta.Gris);
            UI.Ubicar(pagina.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 80), new Vector2(300, 50));

            int i = 0; bool fin = false;
            var ant = UI.BotonContorno(raiz, "Anterior", 42, () => i = Mathf.Max(0, i - 1));
            UI.Ubicar(ant.rt, new Vector2(0.5f, 0), new Vector2(-520, 80), new Vector2(340, 100));
            var sig = UI.CrearBoton(raiz, "Siguiente", 42, Paleta.Texto, () => { if (i < 3) i++; else fin = true; });
            UI.Ubicar(sig.rt, new Vector2(0.5f, 0), new Vector2(520, 80), new Vector2(340, 100));
            var saltar = UI.CrearBoton(raiz, "Saltar", 32, Color.white, () => fin = true, Paleta.Gris);
            UI.Ubicar(saltar.rt, new Vector2(1, 1), new Vector2(-30, -30), new Vector2(160, 64), new Vector2(1, 1));

            int dibujada = -1;
            while (!fin)
            {
                if (i != dibujada)
                {
                    dibujada = i;
                    UI.Limpiar(dibujo);
                    Dibujar(dibujo, i);
                    texto.text = Textos[i];
                    pagina.text = (i + 1) + " / 4";
                    ant.Activo = i > 0;
                    sig.texto.text = i < 3 ? "Siguiente" : "Empezar";
                    g.Captura("comojugar" + (i + 1));
                }
                if (g.auto) { yield return null; yield return null; if (i < 3) i++; else fin = true; }
                yield return null;
            }
            Object.Destroy(raiz.gameObject);
        }

        static Image R(Transform p, Color c, Vector2 pos, Vector2 tam, Sprite s = null)
        {
            var img = UI.Caja(p, "d", c, s);
            img.raycastTarget = false;
            UI.Ubicar(img.rectTransform, new Vector2(0.5f, 0.5f), pos, tam);
            return img;
        }

        static void T(Transform p, string s, int tam, Color c, Vector2 pos, Vector2 caja)
        {
            var t = UI.Texto(p, s, tam, c);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 0.5f), pos, caja);
        }

        static void Dibujar(RectTransform d, int i)
        {
            switch (i)
            {
                case 0: // vía con estaciones, tren y destino
                    R(d, Paleta.Via, new Vector2(0, -60), new Vector2(1300, 10));
                    for (int k = 0; k <= 5; k++)
                    {
                        float x = -600 + k * 240;
                        if (k == 4) { R(d, Paleta.Error, new Vector2(x, -130), new Vector2(130, 84), UI.Pildora); R(d, Color.white, new Vector2(x, -130), new Vector2(118, 72), UI.Pildora); }
                        var p = R(d, k == 0 ? Paleta.Texto : Paleta.Naturales, new Vector2(x, -130), new Vector2(104, 62), UI.Pildora);
                        T(p.transform, k.ToString(), 44, Paleta.TextoSobre(p.color), Vector2.zero, new Vector2(100, 60));
                    }
                    T(d, "DESTINO", 30, Paleta.Error, new Vector2(360, -190), new Vector2(240, 40));
                    Tren(d, new Vector2(-360, -54));
                    for (int k = 0; k <= 12; k++) // arco punteado hacia el destino
                    {
                        float t = k / 12f;
                        R(d, Paleta.Naturales, new Vector2(Mathf.Lerp(-360, 360, t), 70 + 260 * t * (1 - t)), new Vector2(14, 14), UI.Circulo);
                    }
                    Pantalla.Persona(d, Paleta.Hex("#9B6CD9"), 1f).anchoredPosition = new Vector2(500, 60);
                    break;
                case 1: // cuadrilla de turno y maquinista con el mouse
                    for (int k = 0; k < 4; k++)
                    {
                        var pos = new Vector2(-510 + k * 250, 150);
                        if (k == 1) { R(d, Paleta.Texto, pos, new Vector2(232, 98), UI.Pildora); T(d, "DE TURNO", 28, Paleta.Texto, pos + new Vector2(0, -78), new Vector2(240, 36)); }
                        Marcador.Pildora(d, k, "", pos, new Vector2(210, 82), 44);
                    }
                    for (int k = 0; k < 3; k++) Pantalla.Persona(d, Paleta.Cuadrillas[1], 0.9f).anchoredPosition = new Vector2(-380 + k * 90, -110);
                    // globo "¡+3!" de la cuadrilla
                    R(d, Color.white, new Vector2(-290, -5), new Vector2(180, 80), UI.Pildora);
                    T(d, "¡+3!", 44, Paleta.Texto, new Vector2(-290, -5), new Vector2(180, 80));
                    // maquinista con el mouse
                    Pantalla.Persona(d, Paleta.Hex("#F28C38"), 1.1f).anchoredPosition = new Vector2(200, -100);
                    R(d, Paleta.Texto, new Vector2(380, -150), new Vector2(90, 130), UI.Pildora);
                    R(d, Color.white, new Vector2(380, -112), new Vector2(6, 44));
                    R(d, Paleta.Texto, new Vector2(300, -150), new Vector2(120, 8));
                    break;
                case 2: // las otras cuadrillas levantan su hoja
                    int[] otras = { 0, 2, 3 };
                    for (int k = 0; k < 3; k++)
                    {
                        float x = -420 + k * 420;
                        Pantalla.Persona(d, Paleta.Cuadrillas[otras[k]], 1.1f).anchoredPosition = new Vector2(x, -150);
                        R(d, Paleta.Texto, new Vector2(x, 70), new Vector2(230, 170));
                        R(d, Color.white, new Vector2(x, 70), new Vector2(220, 160));
                        var f = UI.Forma(d, otras[k], Paleta.Cuadrillas[otras[k]], 40);
                        UI.Ubicar(f.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x - 80, 125), f.rectTransform.sizeDelta);
                        T(d, new[] { UI.M("-2"), UI.M("-2"), "2" }[k], 80, Paleta.Texto, new Vector2(x, 60), new Vector2(200, 110));
                        R(d, Paleta.Via, new Vector2(x + 40, -40), new Vector2(10, 70)); // brazo
                    }
                    break;
                default: // puntos
                    Marcador.Pildora(d, 1, "", new Vector2(-330, 120), new Vector2(240, 96), 48);
                    T(d, "+2", 150, Paleta.Racionales, new Vector2(-330, -60), new Vector2(300, 170));
                    T(d, "llega al primer intento", 34, Paleta.Via, new Vector2(-330, -170), new Vector2(500, 50));
                    Marcador.Pildora(d, 0, "", new Vector2(250, 120), new Vector2(200, 86), 44);
                    Marcador.Pildora(d, 3, "", new Vector2(480, 120), new Vector2(200, 86), 44);
                    T(d, "+1", 150, Paleta.Racionales, new Vector2(365, -60), new Vector2(300, 170));
                    T(d, "predicción correcta", 34, Paleta.Via, new Vector2(365, -170), new Vector2(500, 50));
                    break;
            }
        }

        static void Tren(Transform p, Vector2 pos)
        {
            foreach (float cx in new[] { -70f, 70f })
            {
                R(p, Paleta.Texto, pos + new Vector2(cx, 39), new Vector2(134, 78));
                R(p, Color.white, pos + new Vector2(cx, 39), new Vector2(124, 68));
                R(p, Paleta.Naturales, pos + new Vector2(cx, 26), new Vector2(124, 12));
                for (int w = 0; w < 3; w++) R(p, Paleta.Hex("#A9C8EE"), pos + new Vector2(cx - 40 + w * 30, 58), new Vector2(24, 22));
            }
            foreach (float wx in new[] { -112f, -28f, 28f, 112f }) R(p, Paleta.Texto, pos + new Vector2(wx, 0), new Vector2(26, 26), UI.Circulo);
        }
    }
}
