using UnityEngine;
using UnityEngine.UI;

namespace Linea0.Vista
{
    /// <summary>La pantalla de juego: barra superior, diálogo, vía con tren, panel inferior y capas encima.</summary>
    public class Pantalla
    {
        public Canvas canvas;
        public CanvasScaler escalador;
        public RectTransform raiz, juego, barra, dialogo, viaRT, panel, capaPuzzle, overlay;
        public Image zonaPill;
        public Text zonaTexto, reloj;
        public UI.Boton pista, saltar, ayuda;
        public RectTransform marcadorRT;
        public RectTransform hablanteIcono;
        public Text hablanteNombre, dialogoTexto;
        public Image dialogoCaja;
        public Via via;
        public Tren tren;
        public RectTransform letreroPrioridad;

        public const float AltoBarra = 104, AltoDialogo = 150, AltoPanel = 200;

        public static Pantalla Crear()
        {
            var p = new Pantalla();
            p.canvas = UI.Lienzo("Linea0", 0);
            p.escalador = p.canvas.GetComponent<CanvasScaler>();
            p.raiz = (RectTransform)p.canvas.transform;
            var fondo = UI.Caja(p.raiz, "Cielo", Paleta.Cielo);
            UI.Llenar(fondo.rectTransform);
            fondo.raycastTarget = false;

            p.juego = UI.Llenar(UI.Nodo(p.raiz, "Juego"));

            // vía (al fondo, ocupa el centro)
            p.viaRT = UI.Llenar(UI.Nodo(p.juego, "ViaArea"), 0, 0, AltoBarra + AltoDialogo - 30, AltoPanel);
            p.via = Via.Crear(p.viaRT);
            p.tren = Tren.Crear(p.via, Paleta.Naturales);
            p.capaPuzzle = UI.Llenar(UI.Nodo(p.juego, "CapaPuzzle"), 0, 0, AltoBarra, AltoPanel);

            // barra superior
            var barra = UI.Caja(p.juego, "Barra", Color.white);
            p.barra = barra.rectTransform;
            p.barra.anchorMin = new Vector2(0, 1); p.barra.anchorMax = new Vector2(1, 1);
            p.barra.pivot = new Vector2(0.5f, 1); p.barra.anchoredPosition = Vector2.zero; p.barra.sizeDelta = new Vector2(0, AltoBarra);
            var linea = UI.Caja(p.barra, "Linea", Paleta.Borde);
            UI.Ubicar(linea.rectTransform, new Vector2(0.5f, 0), Vector2.zero, new Vector2(0, 3), new Vector2(0.5f, 0));
            linea.rectTransform.anchorMin = new Vector2(0, 0); linea.rectTransform.anchorMax = new Vector2(1, 0);

            p.zonaPill = UI.Caja(p.barra, "Zona", Paleta.Enteros, UI.Pildora);
            UI.Ubicar(p.zonaPill.rectTransform, new Vector2(0, 0.5f), new Vector2(28, 0), new Vector2(580, 70), new Vector2(0, 0.5f));
            p.zonaTexto = UI.Texto(p.zonaPill.transform, "ZONA", 34, Color.white);
            UI.Llenar(p.zonaTexto.rectTransform, 22, 22, 4, 4);

            p.marcadorRT = UI.Nodo(p.barra, "Marcador");
            UI.Ubicar(p.marcadorRT, new Vector2(0.5f, 0.5f), new Vector2(40, 4), new Vector2(680, 80));

            p.reloj = UI.Texto(p.barra, "7:00", 52, Paleta.Texto);
            UI.Ubicar(p.reloj.rectTransform, new Vector2(1, 0.5f), new Vector2(-405, 0), new Vector2(170, 70));
            p.ayuda = UI.BotonContorno(p.barra, "?", 44, null);
            UI.Ubicar(p.ayuda.rt, new Vector2(1, 0.5f), new Vector2(-338, 0), new Vector2(72, 72));

            p.pista = UI.BotonContorno(p.barra, "Pista", 40, null);
            UI.Ubicar(p.pista.rt, new Vector2(1, 0.5f), new Vector2(-140, 0), new Vector2(180, 72));

            p.saltar = UI.CrearBoton(p.barra, "Saltar", 28, Color.white, null, Paleta.Gris);
            UI.Ubicar(p.saltar.rt, new Vector2(1, 0.5f), new Vector2(-14, 0), new Vector2(110, 56), new Vector2(1, 0.5f));

            // diálogo
            p.dialogo = UI.Nodo(p.juego, "Dialogo");
            p.dialogo.anchorMin = new Vector2(0, 1); p.dialogo.anchorMax = new Vector2(1, 1);
            p.dialogo.pivot = new Vector2(0.5f, 1);
            p.dialogo.anchoredPosition = new Vector2(0, -AltoBarra - 6);
            p.dialogo.sizeDelta = new Vector2(0, AltoDialogo);
            p.hablanteIcono = UI.Nodo(p.dialogo, "Icono");
            UI.Ubicar(p.hablanteIcono, new Vector2(0, 0.5f), new Vector2(40, -6), new Vector2(90, 130), new Vector2(0, 0.5f));
            p.hablanteNombre = UI.Texto(p.dialogo, "CENTRAL", 26, Paleta.Via, TextAnchor.MiddleLeft);
            UI.Ubicar(p.hablanteNombre.rectTransform, new Vector2(0, 1), new Vector2(156, -8), new Vector2(500, 34), new Vector2(0, 1));
            p.dialogoCaja = UI.CajaConBorde(p.dialogo, "Caja", Color.white, Paleta.Borde, 3);
            var cr = p.dialogoCaja.rectTransform;
            cr.anchorMin = new Vector2(0, 0); cr.anchorMax = new Vector2(1, 0);
            cr.pivot = new Vector2(0, 0);
            cr.offsetMin = new Vector2(146, 4); cr.offsetMax = new Vector2(-300, 102);
            p.dialogoTexto = UI.Texto(cr, "", 42, Paleta.Texto, TextAnchor.MiddleLeft, false);
            UI.Llenar(p.dialogoTexto.rectTransform, 28, 20, 6, 6);

            p.letreroPrioridad = UI.Nodo(p.juego, "Prioridad");
            p.letreroPrioridad.gameObject.SetActive(false);

            // panel inferior
            var panel = UI.Caja(p.juego, "Panel", Color.white);
            p.panel = panel.rectTransform;
            p.panel.anchorMin = new Vector2(0, 0); p.panel.anchorMax = new Vector2(1, 0);
            p.panel.pivot = new Vector2(0.5f, 0); p.panel.anchoredPosition = Vector2.zero; p.panel.sizeDelta = new Vector2(0, AltoPanel);
            var lp = UI.Caja(p.panel, "Linea", Paleta.Borde);
            lp.rectTransform.anchorMin = new Vector2(0, 1); lp.rectTransform.anchorMax = new Vector2(1, 1);
            lp.rectTransform.pivot = new Vector2(0.5f, 1); lp.rectTransform.sizeDelta = new Vector2(0, 3); lp.rectTransform.anchoredPosition = Vector2.zero;

            p.overlay = UI.Llenar(UI.Nodo(p.raiz, "Overlay"));
            Canvas.ForceUpdateCanvases();
            p.via.Redibujar();
            return p;
        }

        public void Zona(string titulo, Color color, string hora)
        {
            zonaPill.color = color;
            zonaTexto.text = titulo.ToUpperInvariant();
            zonaTexto.fontSize = zonaTexto.resizeTextMaxSize = titulo.Length > 24 ? 28 : 34;
            zonaTexto.color = Paleta.TextoSobre(color);
            reloj.text = hora;
        }

        /// <summary>Muestra quién habla: la Central (radio) o un pasajero (cápsula de color).</summary>
        public void Dialogo(string hablante, string texto, Color colorPersona)
        {
            UI.Limpiar(hablanteIcono);
            bool central = string.IsNullOrEmpty(hablante) || hablante == "Central";
            hablanteNombre.text = central ? "CENTRAL" : hablante.ToUpperInvariant();
            dialogoTexto.text = texto ?? "";
            if (central) IconoRadio(hablanteIcono);
            else Persona(hablanteIcono, colorPersona, 1f);
        }

        public static void IconoRadio(RectTransform padre)
        {
            var antena = UI.Caja(padre, "Antena", Paleta.Texto); antena.raycastTarget = false;
            UI.Ubicar(antena.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(18, 38), new Vector2(7, 46));
            var cuerpo = UI.Caja(padre, "Radio", Paleta.Texto); cuerpo.raycastTarget = false;
            UI.Ubicar(cuerpo.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, -14), new Vector2(66, 86));
            var parlante = UI.Caja(cuerpo.transform, "Parlante", Paleta.Gris, UI.Circulo); parlante.raycastTarget = false;
            UI.Ubicar(parlante.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0, 12), new Vector2(40, 40));
            var luz = UI.Caja(cuerpo.transform, "Luz", Paleta.Error, UI.Circulo); luz.raycastTarget = false;
            UI.Ubicar(luz.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(-16, -26), new Vector2(14, 14));
        }

        /// <summary>Pasajero: silueta de cápsula (cabeza y cuerpo).</summary>
        public static RectTransform Persona(Transform padre, Color c, float escala)
        {
            var raiz = UI.Nodo(padre, "Persona");
            UI.Ubicar(raiz, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(60 * escala, 130 * escala));
            var cabeza = UI.Caja(raiz, "Cabeza", c, UI.Circulo); cabeza.raycastTarget = false;
            UI.Ubicar(cabeza.rectTransform, new Vector2(0.5f, 1), new Vector2(0, 0), new Vector2(46 * escala, 46 * escala), new Vector2(0.5f, 1));
            var cuerpo = UI.Caja(raiz, "Cuerpo", c, UI.Pildora); cuerpo.raycastTarget = false;
            UI.Ubicar(cuerpo.rectTransform, new Vector2(0.5f, 0), new Vector2(0, 0), new Vector2(60 * escala, 80 * escala), new Vector2(0.5f, 0));
            return raiz;
        }

        public void MostrarJuego(bool v) => juego.gameObject.SetActive(v);

        public void LimpiarPuzzle()
        {
            Anclas.Limpiar();
            Anclas.Poner("pista", pista.rt);
            Anclas.Poner("ayuda", ayuda.rt);
            UI.Limpiar(panel);
            var lp = UI.Caja(panel, "Linea", Paleta.Borde);
            lp.rectTransform.anchorMin = new Vector2(0, 1); lp.rectTransform.anchorMax = new Vector2(1, 1);
            lp.rectTransform.pivot = new Vector2(0.5f, 1); lp.rectTransform.sizeDelta = new Vector2(0, 3); lp.rectTransform.anchoredPosition = Vector2.zero;
            UI.Limpiar(capaPuzzle);
            via.LimpiarTodo();
            for (int i = via.capaTren.childCount - 1; i >= 0; i--)
                if (via.capaTren.GetChild(i) != tren.transform) Object.Destroy(via.capaTren.GetChild(i).gameObject);
            tren.gameObject.SetActive(true);
            tren.sacudida = 0;
            tren.Puertas(false);
        }

        /// <summary>Panel blanco centrado encima de todo (overlay).</summary>
        public RectTransform Ventana(Vector2 tam, bool velo = true)
        {
            var raizV = UI.Llenar(UI.Nodo(overlay, "Ventana"));
            if (velo)
            {
                var v = UI.Caja(raizV, "Velo", Paleta.ConAlfa(Paleta.Texto, 0.45f));
                UI.Llenar(v.rectTransform);
            }
            var caja = UI.CajaConBorde(raizV, "Caja", Color.white, Paleta.Borde, 4);
            UI.Ubicar(caja.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, tam);
            return raizV;
        }
    }
}
