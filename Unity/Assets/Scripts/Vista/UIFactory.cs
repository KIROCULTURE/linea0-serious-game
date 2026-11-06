using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Linea0.Vista
{
    /// <summary>Construye la UI por código con primitivas (cuadrado, círculo, píldora).</summary>
    public static class UI
    {
        static Font fuente, fuenteNormal;
        static Sprite cuadrado, circulo, pildora;

        public static Font Fuente
        {
            get
            {
                if (fuente == null)
                {
                    fuente = Resources.Load<Font>("Fuentes/AtkinsonHyperlegible-Bold");
                    if (fuente == null) fuente = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }
                return fuente;
            }
        }

        public static Font FuenteNormal
        {
            get
            {
                if (fuenteNormal == null)
                {
                    fuenteNormal = Resources.Load<Font>("Fuentes/AtkinsonHyperlegible-Regular");
                    if (fuenteNormal == null) fuenteNormal = Fuente;
                }
                return fuenteNormal;
            }
        }

        public static Sprite Cuadrado
        {
            get
            {
                if (cuadrado == null)
                {
                    var t = new Texture2D(4, 4);
                    var px = new Color[16];
                    for (int i = 0; i < 16; i++) px[i] = Color.white;
                    t.SetPixels(px); t.Apply();
                    cuadrado = Sprite.Create(t, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 4);
                }
                return cuadrado;
            }
        }

        static Texture2D TexturaCirculo(int n)
        {
            var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            float r = n / 2f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - r) * (x + 0.5f - r) + (y + 0.5f - r) * (y + 0.5f - r));
                    t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - d)));
                }
            t.Apply();
            return t;
        }

        public static Sprite Circulo
        {
            get
            {
                if (circulo == null) { var t = TexturaCirculo(128); circulo = Sprite.Create(t, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128); }
                return circulo;
            }
        }

        /// <summary>Píldora: círculo de 9 cortes (bordes = radio).</summary>
        public static Sprite Pildora
        {
            get
            {
                if (pildora == null)
                {
                    var t = TexturaCirculo(128);
                    pildora = Sprite.Create(t, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.5f), 128, 0, SpriteMeshType.FullRect, new Vector4(63, 63, 63, 63));
                }
                return pildora;
            }
        }

        static Sprite triangulo;
        public static Sprite Triangulo
        {
            get
            {
                if (triangulo == null)
                {
                    int n = 128;
                    var t = new Texture2D(n, n, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
                    for (int y = 0; y < n; y++)
                        for (int x = 0; x < n; x++)
                        {
                            float fy = (y + 0.5f) / n;              // 0 abajo, 1 arriba
                            float media = (1 - fy) * 0.5f;          // media base a esa altura
                            float dx = Mathf.Abs((x + 0.5f) / n - 0.5f);
                            float a = Mathf.Clamp01((media - dx) * n * 0.9f) * Mathf.Clamp01(fy * n);
                            t.SetPixel(x, y, new Color(1, 1, 1, a));
                        }
                    t.Apply();
                    triangulo = Sprite.Create(t, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), n);
                }
                return triangulo;
            }
        }

        /// <summary>Forma de cuadrilla o alternativa: 0 ▲, 1 ●, 2 ■, 3 ◆. Dibujada con primitivas.</summary>
        public static Image Forma(Transform padre, int i, Color c, float tam)
        {
            Image img;
            switch (i)
            {
                case 0: img = Caja(padre, "Triangulo", c, Triangulo); break;
                case 1: img = Caja(padre, "Circulo", c, Circulo); break;
                case 2: img = Caja(padre, "Cuadrado", c); tam *= 0.82f; break;
                default: img = Caja(padre, "Rombo", c); tam *= 0.72f; img.rectTransform.localRotation = Quaternion.Euler(0, 0, 45); break;
            }
            img.raycastTarget = false;
            img.rectTransform.sizeDelta = new Vector2(tam, tam);
            return img;
        }

        // ---------- estructura ----------

        public static Canvas Lienzo(string nombre, int orden)
        {
            var go = new GameObject(nombre, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = orden;
            var s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1920, 1080);
            s.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            return c;
        }

        public static void AsegurarEventSystem()
        {
            if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null) return;
            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        public static RectTransform Nodo(Transform padre, string nombre)
        {
            var go = new GameObject(nombre, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(padre, false);
            return rt;
        }

        /// <summary>Estira el rect a todo el padre (con márgenes opcionales).</summary>
        public static RectTransform Llenar(RectTransform rt, float izq = 0, float der = 0, float arr = 0, float aba = 0)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(izq, aba); rt.offsetMax = new Vector2(-der, -arr);
            return rt;
        }

        /// <summary>Ubica con un ancla (0..1) y posición/tamaño en píxeles de referencia.</summary>
        public static RectTransform Ubicar(RectTransform rt, Vector2 ancla, Vector2 pos, Vector2 tam, Vector2? pivote = null)
        {
            rt.anchorMin = rt.anchorMax = ancla;
            rt.pivot = pivote ?? (ancla == Vector2.zero ? new Vector2(0.5f, 0.5f) : ancla); // ancla cero = coordenadas de vía, centrado
            rt.anchoredPosition = pos;
            rt.sizeDelta = tam;
            return rt;
        }

        public static Image Caja(Transform padre, string nombre, Color color, Sprite sprite = null)
        {
            var rt = Nodo(padre, nombre);
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = sprite ?? Cuadrado;
            if (sprite == Pildora) img.type = Image.Type.Sliced;
            img.color = color;
            return img;
        }

        public static Image CajaConBorde(Transform padre, string nombre, Color fondo, Color borde, float grosor = 3)
        {
            var b = Caja(padre, nombre, borde);
            var f = Caja(b.transform, "Fondo", fondo);
            Llenar(f.rectTransform, grosor, grosor, grosor, grosor);
            f.raycastTarget = false;
            return b;
        }

        public static Text Texto(Transform padre, string texto, int tam, Color color, TextAnchor alin = TextAnchor.MiddleCenter, bool negrita = true)
        {
            var rt = Nodo(padre, "Texto");
            var t = rt.gameObject.AddComponent<Text>();
            t.font = negrita ? Fuente : FuenteNormal;
            t.text = texto;
            t.fontSize = tam;
            t.color = color;
            t.alignment = alin;
            t.supportRichText = true;
            t.raycastTarget = false;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.resizeTextForBestFit = true;
            t.resizeTextMinSize = Mathf.Max(14, tam / 2);
            t.resizeTextMaxSize = tam;
            Llenar(rt);
            return t;
        }

        public class Boton
        {
            public Button button;
            public Image fondo;
            public Text texto;
            public RectTransform rt => (RectTransform)button.transform;
            public bool Activo { get => button.interactable; set => button.interactable = value; }
            public void Mostrar(bool v) => button.gameObject.SetActive(v);
        }

        public static Boton CrearBoton(Transform padre, string texto, int tam, Color fondo, UnityAction alClic, Color? colorTexto = null, Sprite sprite = null)
        {
            var img = Caja(padre, "Boton " + texto, fondo, sprite ?? Pildora);
            var b = img.gameObject.AddComponent<Button>();
            var cols = b.colors;
            cols.highlightedColor = new Color(0.92f, 0.92f, 0.92f);
            cols.pressedColor = new Color(0.8f, 0.8f, 0.8f);
            cols.disabledColor = new Color(1, 1, 1, 0.4f);
            b.colors = cols;
            if (alClic != null) b.onClick.AddListener(alClic);
            var t = Texto(img.transform, texto, tam, colorTexto ?? Paleta.TextoSobre(fondo));
            Llenar(t.rectTransform, 16, 16, 4, 4);
            return new Boton { button = b, fondo = img, texto = t };
        }

        /// <summary>Botón con borde oscuro y fondo blanco (secundario).</summary>
        public static Boton BotonContorno(Transform padre, string texto, int tam, UnityAction alClic)
        {
            var borde = Caja(padre, "Boton " + texto, Paleta.Texto, Pildora);
            var b = borde.gameObject.AddComponent<Button>();
            if (alClic != null) b.onClick.AddListener(alClic);
            var f = Caja(borde.transform, "Fondo", Color.white, Pildora);
            Llenar(f.rectTransform, 4, 4, 4, 4);
            f.raycastTarget = false;
            var t = Texto(borde.transform, texto, tam, Paleta.Texto);
            Llenar(t.rectTransform, 16, 16, 4, 4);
            return new Boton { button = b, fondo = f, texto = t };
        }

        public static void Limpiar(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) UnityEngine.Object.Destroy(t.GetChild(i).gameObject);
        }

        /// <summary>"-3" → "−3" para pantalla.</summary>
        public static string M(string s) => s?.Replace('-', '−');
    }
}
