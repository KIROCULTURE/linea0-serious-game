using System.Collections;
using System.Collections.Generic;
using Linea0.Juego;
using Linea0.Vista;
using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Sala
{
    /// <summary>Marcador de cuadrillas en la barra superior.</summary>
    public static class Marcador
    {
        public static void Refrescar(Pantalla p, Sesion s)
        {
            UI.Limpiar(p.marcadorRT);
            p.marcadorRT.gameObject.SetActive(s.conCuadrillas);
            if (!s.conCuadrillas) return;
            int n = 0;
            for (int i = 0; i < Sesion.NumCuadrillas; i++) if (s.activas[i]) n++;
            float x = -(n - 1) * 88f;
            for (int i = 0; i < Sesion.NumCuadrillas; i++)
            {
                if (!s.activas[i]) continue;
                bool turno = i == s.turno;
                if (turno)
                {
                    var b = UI.Caja(p.marcadorRT, "Turno", Paleta.Texto, UI.Pildora);
                    UI.Ubicar(b.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, 6), new Vector2(172, 78));
                    var dt = UI.Texto(p.marcadorRT, "DE TURNO", 20, Paleta.Texto);
                    UI.Ubicar(dt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(x, -44), new Vector2(160, 24));
                }
                Pildora(p.marcadorRT, i, s.puntos[i].ToString(), new Vector2(x, 6), new Vector2(160, 66));
                x += 176;
            }
        }

        /// <summary>Píldora de cuadrilla: color + forma + letra (+ texto).</summary>
        public static RectTransform Pildora(Transform padre, int i, string extra, Vector2 pos, Vector2 tam, int tamTexto = 38)
        {
            var c = Paleta.Cuadrillas[i];
            var img = UI.Caja(padre, "Cuadrilla " + Paleta.Letras[i], c, UI.Pildora);
            UI.Ubicar(img.rectTransform, new Vector2(0.5f, 0.5f), pos, tam);
            var f = UI.Forma(img.transform, i, Paleta.TextoSobre(c), tam.y * 0.45f);
            UI.Ubicar(f.rectTransform, new Vector2(0, 0.5f), new Vector2(tam.y * 0.55f, 0), f.rectTransform.sizeDelta);
            var t = UI.Texto(img.transform, Paleta.Letras[i] + (string.IsNullOrEmpty(extra) ? "" : " " + extra), tamTexto, Paleta.TextoSobre(c));
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.alignment = TextAnchor.MiddleLeft;
            UI.Llenar(t.rectTransform, tam.y * 0.95f, 8, 2, 2);
            return img.rectTransform;
        }
    }

    /// <summary>Apuesta del andén: cuenta regresiva de 8 s antes de que el tren se mueva. Espacio la salta.</summary>
    public static class ApuestaAnden
    {
        public static IEnumerator Correr(GameManager g, string pregunta = "Cuadrillas: ¿dónde para el tren? ¡Escríbanlo!")
        {
            if (!g.opciones.apuesta || !g.S.conCuadrillas) yield break;
            var raiz = UI.Llenar(UI.Nodo(g.P.overlay, "Apuesta"));
            var velo = UI.Caja(raiz, "Velo", Paleta.ConAlfa(Paleta.Texto, 0.55f));
            UI.Llenar(velo.rectTransform);
            var t = UI.Texto(raiz, pregunta, 64, Color.white);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 0.68f), Vector2.zero, new Vector2(1700, 100));
            var n = UI.Texto(raiz, "8", 260, Paleta.Sol);
            UI.Ubicar(n.rectTransform, new Vector2(0.5f, 0.42f), Vector2.zero, new Vector2(600, 300));
            var ayuda = UI.Texto(raiz, "Espacio: saltar", 30, Paleta.Borde);
            UI.Ubicar(ayuda.rectTransform, new Vector2(0.5f, 0.12f), Vector2.zero, new Vector2(600, 40));
            float dur = g.auto ? 0.3f : 8f, tt = 0;
            yield return null;
            while (tt < dur)
            {
                n.text = Mathf.CeilToInt(dur - tt).ToString();
                if (Input.GetKeyDown(KeyCode.Space)) break;
                tt += Time.deltaTime;
                yield return null;
            }
            t.text = "";
            n.text = "¡Arriba los papeles!";
            n.fontSize = 120; n.resizeTextMaxSize = 120;
            UI.Ubicar(n.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1700, 200));
            ayuda.text = "";
            yield return Ease.Pausa(g.auto ? 0.1f : 1.1f);
            Object.Destroy(raiz.gameObject);
        }
    }

    /// <summary>"¿Quién predijo bien?": se marcan las cuadrillas que acertaron y se presiona Listo.</summary>
    public static class QuienPredijo
    {
        public static IEnumerator Correr(GameManager g, Sesion.RegistroPuzzle r)
        {
            if (!g.opciones.apuesta || !g.S.conCuadrillas) yield break;
            var s = g.S;
            var v = g.P.Ventana(new Vector2(1200, 420), false);
            var caja = v.GetChild(0);
            ((RectTransform)caja).anchoredPosition = new Vector2(0, -220);
            var t = UI.Texto(caja, "¿Quién predijo bien?", 56, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(1100, 80));
            var marcadas = new bool[4];
            float x = -420;
            for (int i = 0; i < 4; i++)
            {
                if (!s.activas[i]) continue;
                int k = i;
                bool deTurno = i == s.turno;
                var b = UI.CrearBoton(caja, "", 40, Color.white, null, Paleta.Texto);
                UI.Ubicar(b.rt, new Vector2(0.5f, 0.5f), new Vector2(x, 0), new Vector2(230, 110));
                var pil = Marcador.Pildora(b.rt, i, deTurno ? "turno" : "", Vector2.zero, new Vector2(210, 90), 36);
                var chk = UI.Texto(b.rt, "", 60, Paleta.Racionales);
                UI.Ubicar(chk.rectTransform, new Vector2(1, 1), new Vector2(-10, 0), new Vector2(60, 60));
                if (deTurno) { b.Activo = false; pil.GetComponent<Image>().color = Paleta.ConAlfa(Paleta.Cuadrillas[i], 0.35f); }
                b.button.onClick.AddListener(() =>
                {
                    marcadas[k] = !marcadas[k];
                    b.fondo.color = marcadas[k] ? Paleta.Texto : Color.white;
                    chk.text = marcadas[k] ? "✓" : "";
                });
                x += 280;
            }
            var listo = new Senal();
            var bl = UI.CrearBoton(caja, "Listo", 48, Paleta.Texto, () => listo.Dar());
            UI.Ubicar(bl.rt, new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(300, 96));
            if (g.auto) { marcadas[(s.turno + 1) % 4] = true; listo.Dar(); }
            yield return listo.Esperar();
            int aciertos = 0;
            for (int i = 0; i < 4; i++) if (marcadas[i] && i != s.turno) { s.Sumar(i, 1); aciertos++; }
            r.aciertosPrediccion.Add(aciertos);
            Object.Destroy(v.gameObject);
            Marcador.Refrescar(g.P, s);
        }
    }

    /// <summary>Pantallas de sala: cambio de maquinista y controles de entrada/salida.</summary>
    public static class Pantallas
    {
        public static IEnumerator CambioMaquinista(GameManager g, string siguiente)
        {
            if (g.auto) yield break;
            var v = g.P.Ventana(new Vector2(1300, 520));
            var caja = v.GetChild(1);
            var t = UI.Texto(caja, "Cambio de maquinista", 72, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(1200, 100));
            var d = UI.Texto(caja, "Otra persona toma el mouse. Sigue: " + siguiente + ".", 44, Paleta.Via, TextAnchor.MiddleCenter, false);
            UI.Ubicar(d.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 10), new Vector2(1200, 80));
            var s = new Senal();
            var b = UI.CrearBoton(caja, "Listo", 52, Paleta.Texto, () => s.Dar());
            UI.Ubicar(b.rt, new Vector2(0.5f, 0), new Vector2(0, 60), new Vector2(320, 100));
            yield return s.Esperar();
            Object.Destroy(v.gameObject);
        }

        /// <summary>Control de entrada o salida: 6 preguntas, 10 s para votar, el profe marca cuántos acertaron.</summary>
        public static IEnumerator Control(GameManager g, bool salida)
        {
            var preguntas = Linea0.Datos.CargadorNiveles.Datos.control;
            var res = salida ? g.S.salida : g.S.entrada;
            var raiz = UI.Llenar(UI.Nodo(g.P.overlay, "Control"));
            var fondo = UI.Caja(raiz, "Fondo", Paleta.Cielo); UI.Llenar(fondo.rectTransform);
            var titulo = UI.Texto(raiz, salida ? "Control de salida" : "Control de entrada", 56, Paleta.Via);
            UI.Ubicar(titulo.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(1600, 70));
            var cuerpo = UI.Llenar(UI.Nodo(raiz, "Cuerpo"));
            for (int q = 0; q < preguntas.Length; q++)
            {
                UI.Limpiar(cuerpo);
                var pr = preguntas[q];
                var n = UI.Texto(cuerpo, "Pregunta " + pr.numero + " de " + preguntas.Length, 34, Paleta.Gris);
                UI.Ubicar(n.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -140), new Vector2(1600, 50));
                var tx = UI.Texto(cuerpo, pr.texto, 60, Paleta.Texto);
                UI.Ubicar(tx.rectTransform, new Vector2(0.5f, 1), new Vector2(0, -240), new Vector2(1760, 140));
                var botones = Alternativas.Crear(cuerpo, pr.alternativas, null, new Vector2(0.5f, 0.5f), new Vector2(0, 20));
                foreach (var b in botones) b.button.interactable = true;
                var cuenta = UI.Texto(cuerpo, "", 90, Paleta.Error);
                UI.Ubicar(cuenta.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 300), new Vector2(400, 110));
                var ayuda = UI.Texto(cuerpo, "Voten con 1 a 4 dedos o con tarjetas de color · Espacio: saltar", 30, Paleta.Gris, TextAnchor.MiddleCenter, false);
                UI.Ubicar(ayuda.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 240), new Vector2(1600, 40));
                float tt = 0, dur = g.auto ? 0.2f : 10f;
                yield return null;
                while (tt < dur && !Input.GetKeyDown(KeyCode.Space)) { cuenta.text = Mathf.CeilToInt(dur - tt).ToString(); tt += Time.deltaTime; yield return null; }
                cuenta.text = ""; ayuda.text = "";
                if (salida)
                {
                    for (int i = 0; i < botones.Count; i++)
                        if (i != pr.correcta) botones[i].fondo.color = Paleta.ConAlfa(botones[i].fondo.color, 0.25f);
                    var ok = UI.Texto(cuerpo, "Respuesta: " + pr.alternativas[pr.correcta], 50, Paleta.Racionales);
                    UI.Ubicar(ok.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 290), new Vector2(1200, 70));
                }
                var lbl = UI.Texto(cuerpo, "Profe: ¿cuántos acertaron?", 38, Paleta.Texto);
                UI.Ubicar(lbl.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 200), new Vector2(1200, 50));
                var sen = new Senal();
                for (int i = 0; i < 4; i++)
                {
                    int k = i;
                    var b = UI.BotonContorno(cuerpo, Sesion.Niveles[i], 34, () => sen.Dar(k));
                    UI.Ubicar(b.rt, new Vector2(0.5f, 0), new Vector2(-645 + i * 430, 90), new Vector2(400, 100));
                }
                if (g.auto) sen.Dar(salida ? 3 : (q % 2));
                yield return sen.Esperar();
                res[q] = sen.valor;
            }
            Object.Destroy(raiz.gameObject);
        }
    }

    /// <summary>Cuatro alternativas grandes con forma, color y número (▲ ● ■ ◆).</summary>
    public static class Alternativas
    {
        public static List<UI.Boton> Crear(Transform padre, string[] alts, System.Action<int> alElegir, Vector2 ancla, Vector2 centro, float ancho = 400, float alto = 130)
        {
            var lista = new List<UI.Boton>();
            float sep = ancho + 30;
            for (int i = 0; i < alts.Length; i++)
            {
                int k = i;
                var c = Paleta.Cuadrillas[i];
                var b = UI.CrearBoton(padre, (i + 1) + ")  " + UI.M(alts[i]), 56, c, () => alElegir?.Invoke(k));
                UI.Ubicar(b.rt, ancla, centro + new Vector2((i - (alts.Length - 1) / 2f) * sep, 0), new Vector2(ancho, alto));
                UI.Llenar(b.texto.rectTransform, alto * 0.75f, 16, 4, 4);
                var f = UI.Forma(b.rt, i, Paleta.TextoSobre(c), alto * 0.42f);
                UI.Ubicar(f.rectTransform, new Vector2(0, 0.5f), new Vector2(alto * 0.45f, 0), f.rectTransform.sizeDelta);
                if (alElegir == null) b.button.transition = Selectable.Transition.None;
                lista.Add(b);
            }
            return lista;
        }
    }
}
