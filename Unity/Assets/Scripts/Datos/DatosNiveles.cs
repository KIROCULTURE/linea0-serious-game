using System;
using Linea0.Matematica;

namespace Linea0.Datos
{
    public enum TipoPuzzle { Contar, Boletos, Repartir, Divisor, Lupa, ContarSaltos, SinVia, Compas, Rueda, Clasificar, Jerarquia, Serpiente }

    [Serializable]
    public class DatosNiveles
    {
        public Zona[] zonas;
        public Puzzle[] puzzles;
        public PreguntaControl[] control;
        public string[] preguntasCierre;
    }

    [Serializable]
    public class Zona
    {
        public int numero;
        public string nombre;
        public string conjunto;
        public string color;
        public string hora;
        public string anuncio;
    }

    [Serializable]
    public class Boleto
    {
        public string operacion;   // suma, resta, mult, div, expreso, espejo
        public string valor;       // racional en texto
        public int repeticiones;   // solo expreso

        public Racional Valor => string.IsNullOrEmpty(valor) ? Racional.Cero : Racional.Parse(valor);

        /// <summary>Lo que hace el boleto con la posición. invertido = palanca Restar activa.</summary>
        public Racional Aplicar(Racional p, bool invertido = false)
        {
            switch (operacion)
            {
                case "suma": return invertido ? p - Valor : p + Valor;
                case "resta": return invertido ? p + Valor : p - Valor;
                case "mult": return p * Valor;
                case "div": return p / Valor;
                case "expreso": return invertido ? p - repeticiones * Valor : p + repeticiones * Valor;
                case "espejo": return -p;
            }
            throw new ArgumentException("Operación desconocida: " + operacion);
        }

        public bool Invertible => operacion == "suma" || operacion == "resta" || operacion == "expreso";

        static string Bonito(string v) => v.Replace('-', Racional.Menos);

        /// <summary>Texto del boleto: "+3", "−5", "× (−1)", "3 saltos de 4".</summary>
        public string Etiqueta()
        {
            string v = Bonito(valor ?? "");
            switch (operacion)
            {
                case "suma": return v.StartsWith(Racional.Menos.ToString()) ? v : "+" + v;
                case "resta": return Racional.Menos + v;
                case "mult": return "× " + (Valor.Signo < 0 ? "(" + v + ")" : v);
                case "div": return ": " + v;
                case "expreso": return repeticiones + " saltos de " + (Valor.Signo < 0 ? "(" + v + ")" : v);
                case "espejo": return "× (" + Racional.Menos + "1)";
            }
            return v;
        }

        /// <summary>Trozo de cuenta escrita: " + 3", " − 5", " − (−3)", " + 3 × 4", " × (−1)".</summary>
        public string Cuenta(bool invertido)
        {
            string v = Bonito(valor ?? "");
            string abs = v.TrimStart(Racional.Menos);
            bool neg = Valor.Signo < 0;
            switch (operacion)
            {
                case "suma":
                    if (invertido) return " " + Racional.Menos + " " + (neg ? "(" + v + ")" : v);
                    return neg ? " " + Racional.Menos + " " + abs : " + " + v;
                case "resta":
                    return invertido ? " + " + v : " " + Racional.Menos + " " + v;
                case "mult": return " × " + (neg ? "(" + v + ")" : v);
                case "div": return " : " + v;
                case "expreso":
                    string s = repeticiones + " × " + (neg ? "(" + v + ")" : v);
                    return (invertido ? " " + Racional.Menos + " " : " + ") + s;
                case "espejo": return " × (" + Racional.Menos + "1)";
            }
            return "";
        }
    }

    [Serializable]
    public class Puzzle
    {
        public string id;
        public int zona;
        public string tipo;
        public bool esencial;
        public bool sinPuntos;

        public string inicio;
        public string meta;
        public string letreroMeta;
        public Boleto[] mano;
        public int casillas;
        public string[] rangoVia;
        public int subdivision;
        public string estiloEstacion;

        public string hablante;
        public string frasePedido;
        public string fraseNombre;
        public string cuentaEscrita;
        public string[] pistas;
        public string[] lineasGuion;

        public string[] alternativas;
        public int correcta = -1; // índice desde 0

        public int[] solucion;    // índices de la mano, en orden
        public int[] invertidos;  // posiciones de la solución que van con la palanca Restar
        public bool palanca;

        public int dialFijo;
        public int partesSolucion;
        public int avanceSolucion;

        public string guion;
        public string salto;
        public string[] tramo;
        public string expresion;
        public string resultadoErrado;

        public int cantidad;
        public int vagones;
        public string[] cartas;
        public string[] regiones;
        public string comprobacion;

        public TipoPuzzle Tipo => (TipoPuzzle)Enum.Parse(typeof(TipoPuzzle), tipo);
        public Racional Inicio => string.IsNullOrEmpty(inicio) ? Racional.Cero : Racional.Parse(inicio);
        public Posicion Meta => Posicion.Parse(meta);
        public Racional RangoMin => rangoVia != null && rangoVia.Length == 2 ? Racional.Parse(rangoVia[0]) : -1;
        public Racional RangoMax => rangoVia != null && rangoVia.Length == 2 ? Racional.Parse(rangoVia[1]) : 6;

        /// <summary>Se escribe con decimales si alguno de sus números usa coma.</summary>
        public bool UsaDecimales
        {
            get
            {
                if ((inicio ?? "").Contains(",") || (meta ?? "").Contains(",") || (expresion ?? "").Contains(",") || (salto ?? "").Contains(",")) return true;
                if (mano != null) foreach (var b in mano) if ((b.valor ?? "").Contains(",")) return true;
                return false;
            }
        }
    }

    [Serializable]
    public class PreguntaControl
    {
        public int numero;
        public string texto;
        public string[] alternativas;
        public int correcta; // índice desde 0
    }
}
