using UnityEngine;

namespace Linea0.Vista
{
    /// <summary>Colores del GDD, sección 10.</summary>
    public static class Paleta
    {
        public static Color Hex(string h) { ColorUtility.TryParseHtmlString(h, out var c); return c; }

        public static readonly Color Cielo = Hex("#EAF4FB");
        public static readonly Color Ciudad = Hex("#CFDCE6");
        public static readonly Color Tunel = Hex("#D6E6FA");
        public static readonly Color Via = Hex("#5B6470");
        public static readonly Color Texto = Hex("#1E2228");
        public static readonly Color Panel = Color.white;
        public static readonly Color Borde = Hex("#D3DDE6");
        public static readonly Color Naturales = Hex("#F2A900");
        public static readonly Color Enteros = Hex("#2F80ED");
        public static readonly Color Racionales = Hex("#27AE60");
        public static readonly Color Decimales = Hex("#6FCF97");
        public static readonly Color Irracionales = Hex("#D6336C");
        public static readonly Color Error = Hex("#E5484D");
        public static readonly Color Gris = Hex("#8A94A0");
        public static readonly Color GrisClaro = Hex("#EEF2F6");
        public static readonly Color Sol = Hex("#FDE08B");
        public static readonly Color[] Cuadrillas = { Hex("#F2C94C"), Hex("#2F80ED"), Hex("#27AE60"), Hex("#E5484D") };
        public static readonly string[] Formas = { "▲", "●", "■", "◆" };
        public static readonly string[] Letras = { "A", "B", "C", "D" };

        public static Color ConAlfa(Color c, float a) { c.a = a; return c; }

        /// <summary>Color de texto legible sobre un fondo (oscuro sobre ámbar y verde claro).</summary>
        public static Color TextoSobre(Color fondo)
        {
            float l = 0.299f * fondo.r + 0.587f * fondo.g + 0.114f * fondo.b;
            return l > 0.62f ? Texto : Color.white;
        }
    }
}
