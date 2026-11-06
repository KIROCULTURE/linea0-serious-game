using System;
using System.Collections.Generic;
using System.Text;

namespace Linea0.Matematica
{
    public enum TipoToken { Num, Op, Abre, Cierra }

    public class Token
    {
        public TipoToken Tipo;
        public Racional Valor;
        public char Op;              // '+', '-', '×', '·', ':'
        public bool ConParentesis;   // "(−2)" escrito así en la expresión original
        public int Id = -1;          // para seguir un operador mientras la expresión se reduce

        public bool EsSuma => Tipo == TipoToken.Op && (Op == '+' || Op == '-');
        public bool EsProducto => Tipo == TipoToken.Op && (Op == '×' || Op == '·' || Op == ':');

        public Token Clon() => (Token)MemberwiseClone();
    }

    /// <summary>
    /// Expresiones de la Zona 6 y de los "Sin vía". Las fracciones van como literal ("1/2"),
    /// la división es ":" y la multiplicación "×" o "·". Se puede evaluar con jerarquía,
    /// de izquierda a derecha (serpiente) o paso a paso en el orden que elija el jugador.
    /// </summary>
    public static class Expresion
    {
        public static List<Token> Tokenizar(string s)
        {
            var t = new List<Token>();
            int i = 0;
            s = s.Replace(Racional.Menos, '-');
            while (i < s.Length)
            {
                char c = s[i];
                if (char.IsWhiteSpace(c)) { i++; continue; }
                Token prev = t.Count > 0 ? t[t.Count - 1] : null;
                bool unario = c == '-' && (prev == null || prev.Tipo == TipoToken.Op || prev.Tipo == TipoToken.Abre)
                              && i + 1 < s.Length && char.IsDigit(s[i + 1]);
                if (char.IsDigit(c) || unario)
                {
                    int ini = i;
                    i++;
                    while (i < s.Length && (char.IsDigit(s[i]) || s[i] == ',' ||
                           (s[i] == '/' && i + 1 < s.Length && char.IsDigit(s[i + 1])))) i++;
                    t.Add(new Token { Tipo = TipoToken.Num, Valor = Racional.Parse(s.Substring(ini, i - ini)) });
                    continue;
                }
                switch (c)
                {
                    case '(': t.Add(new Token { Tipo = TipoToken.Abre }); break;
                    case ')': t.Add(new Token { Tipo = TipoToken.Cierra }); break;
                    case '+': case '-': case '×': case '·': case ':':
                        t.Add(new Token { Tipo = TipoToken.Op, Op = c }); break;
                    case '*': t.Add(new Token { Tipo = TipoToken.Op, Op = '×' }); break;
                    case '÷': t.Add(new Token { Tipo = TipoToken.Op, Op = ':' }); break;
                    default: throw new FormatException("Carácter inesperado '" + c + "' en " + s);
                }
                i++;
            }
            for (int k = 0; k < t.Count; k++) t[k].Id = k;
            ColapsarParentesis(t, true);
            return t;
        }

        /// <summary>Quita "( n )" alrededor de un número solo.</summary>
        static void ColapsarParentesis(List<Token> t, bool marcar)
        {
            for (int i = 0; i + 2 < t.Count; i++)
            {
                if (t[i].Tipo == TipoToken.Abre && t[i + 1].Tipo == TipoToken.Num && t[i + 2].Tipo == TipoToken.Cierra)
                {
                    var n = t[i + 1];
                    n.ConParentesis = marcar && n.Valor.Signo < 0;
                    t.RemoveAt(i + 2);
                    t.RemoveAt(i);
                    i = -1;
                }
            }
        }

        public static Racional Aplicar(Racional a, char op, Racional b)
        {
            switch (op)
            {
                case '+': return a + b;
                case '-': return a - b;
                case '×': case '·': return a * b;
                case ':': return a / b;
            }
            throw new ArgumentException("Operador " + op);
        }

        /// <summary>Índices de los operadores que se pueden hacer ahora (con un número a cada lado).</summary>
        public static List<int> Disponibles(List<Token> t)
        {
            var r = new List<int>();
            for (int i = 1; i + 1 < t.Count; i++)
                if (t[i].Tipo == TipoToken.Op && t[i - 1].Tipo == TipoToken.Num && t[i + 1].Tipo == TipoToken.Num)
                    r.Add(i);
            return r;
        }

        /// <summary>Hace la operación del índice dado y devuelve la expresión reducida.</summary>
        public static List<Token> Paso(List<Token> t, int indiceOp, out Racional resultado)
        {
            if (!Disponibles(t).Contains(indiceOp)) throw new InvalidOperationException("Ese vagón no puede partir todavía");
            resultado = Aplicar(t[indiceOp - 1].Valor, t[indiceOp].Op, t[indiceOp + 1].Valor);
            var n = new List<Token>();
            foreach (var x in t) n.Add(x.Clon());
            n.RemoveRange(indiceOp - 1, 3);
            n.Insert(indiceOp - 1, new Token { Tipo = TipoToken.Num, Valor = resultado });
            ColapsarParentesis(n, false);
            return n;
        }

        public static bool Terminada(List<Token> t) => t.Count == 1 && t[0].Tipo == TipoToken.Num;

        /// <summary>Evaluación correcta: paréntesis, luego × :, luego + −.</summary>
        public static Racional EvaluarJerarquia(string s) => EvaluarJerarquia(Tokenizar(s));

        public static Racional EvaluarJerarquia(List<Token> t)
        {
            int i = 0;
            var r = Suma(t, ref i);
            if (i != t.Count) throw new FormatException("Expresión mal formada");
            return r;
        }

        static Racional Suma(List<Token> t, ref int i)
        {
            var a = Producto(t, ref i);
            while (i < t.Count && t[i].EsSuma) { char op = t[i++].Op; a = Aplicar(a, op, Producto(t, ref i)); }
            return a;
        }

        static Racional Producto(List<Token> t, ref int i)
        {
            var a = Factor(t, ref i);
            while (i < t.Count && t[i].EsProducto) { char op = t[i++].Op; a = Aplicar(a, op, Factor(t, ref i)); }
            return a;
        }

        static Racional Factor(List<Token> t, ref int i)
        {
            if (i >= t.Count) throw new FormatException("Falta un número");
            var k = t[i];
            if (k.Tipo == TipoToken.Num) { i++; return k.Valor; }
            if (k.Tipo == TipoToken.Abre)
            {
                i++;
                var r = Suma(t, ref i);
                if (i >= t.Count || t[i].Tipo != TipoToken.Cierra) throw new FormatException("Falta ')'");
                i++;
                return r;
            }
            throw new FormatException("Se esperaba un número");
        }

        /// <summary>De izquierda a derecha, sin prioridad (la serpiente, o el error típico).</summary>
        public static Racional EvaluarIzqDer(string s, List<Racional> pasos = null)
        {
            var t = Tokenizar(s);
            while (!Terminada(t))
            {
                t = Paso(t, Disponibles(t)[0], out var r);
                pasos?.Add(r);
            }
            return t[0].Valor;
        }

        /// <summary>Evalúa haciendo los operadores en el orden dado (cada número es un índice de la expresión vigente).</summary>
        public static Racional EvaluarEnOrden(string s, IList<int> orden)
        {
            var t = Tokenizar(s);
            foreach (int k in orden) t = Paso(t, k, out _);
            if (!Terminada(t)) throw new InvalidOperationException("Faltan pasos");
            return t[0].Valor;
        }

        public static string SimboloOp(char op) => op == '-' ? Racional.Menos.ToString() : op.ToString();

        public static string TextoToken(Token k, Token anterior, bool preferirDecimal)
        {
            switch (k.Tipo)
            {
                case TipoToken.Abre: return "(";
                case TipoToken.Cierra: return ")";
                case TipoToken.Op: return SimboloOp(k.Op);
                default:
                    string n = k.Valor.ToTexto(preferirDecimal);
                    bool par = k.ConParentesis || (k.Valor.Signo < 0 && anterior != null && anterior.Tipo == TipoToken.Op);
                    return par ? "(" + n + ")" : n;
            }
        }

        public static string Texto(List<Token> t, bool preferirDecimal)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < t.Count; i++)
            {
                var k = t[i];
                var ant = i > 0 ? t[i - 1] : null;
                bool espacio = i > 0 && k.Tipo != TipoToken.Cierra && ant.Tipo != TipoToken.Abre;
                if (espacio) sb.Append(' ');
                sb.Append(TextoToken(k, ant, preferirDecimal));
            }
            return sb.ToString();
        }

        public static bool UsaDecimales(string s) => s.Contains(",");
    }
}
