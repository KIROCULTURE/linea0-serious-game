using System;
using System.Text;

namespace Linea0.Matematica
{
    /// <summary>Número racional exacto, siempre reducido y con denominador positivo.</summary>
    public readonly struct Racional : IEquatable<Racional>, IComparable<Racional>
    {
        public const char Menos = '−'; // signo menos real para pantalla

        readonly long num;
        readonly long den; // 0 solo en default(Racional): se lee como 1

        public long Num => num;
        public long Den => den == 0 ? 1 : den;

        public Racional(long n, long d = 1)
        {
            if (d == 0) throw new DivideByZeroException("Denominador cero");
            if (d < 0) { n = -n; d = -d; }
            long g = Mcd(Math.Abs(n), d);
            if (g > 1) { n /= g; d /= g; }
            num = n;
            den = d;
        }

        public static readonly Racional Cero = new Racional(0);
        public static readonly Racional Uno = new Racional(1);

        static long Mcd(long a, long b)
        {
            while (b != 0) { long t = a % b; a = b; b = t; }
            return a == 0 ? 1 : a;
        }

        public bool EsEntero => Den == 1;
        public int Signo => Math.Sign(num);
        public Racional Abs => new Racional(Math.Abs(num), Den);
        public double ToDouble() => (double)num / Den;

        public static implicit operator Racional(long n) => new Racional(n);

        public static Racional operator +(Racional a, Racional b) => new Racional(checked(a.num * b.Den + b.num * a.Den), checked(a.Den * b.Den));
        public static Racional operator -(Racional a, Racional b) => new Racional(checked(a.num * b.Den - b.num * a.Den), checked(a.Den * b.Den));
        public static Racional operator -(Racional a) => new Racional(-a.num, a.Den);
        public static Racional operator *(Racional a, Racional b) => new Racional(checked(a.num * b.num), checked(a.Den * b.Den));
        public static Racional operator /(Racional a, Racional b)
        {
            if (b.num == 0) throw new DivideByZeroException("División por cero");
            return new Racional(checked(a.num * b.Den), checked(a.Den * b.num));
        }

        public static bool operator ==(Racional a, Racional b) => a.num == b.num && a.Den == b.Den;
        public static bool operator !=(Racional a, Racional b) => !(a == b);
        public static bool operator <(Racional a, Racional b) => a.CompareTo(b) < 0;
        public static bool operator >(Racional a, Racional b) => a.CompareTo(b) > 0;
        public static bool operator <=(Racional a, Racional b) => a.CompareTo(b) <= 0;
        public static bool operator >=(Racional a, Racional b) => a.CompareTo(b) >= 0;

        public int CompareTo(Racional o) => (checked(num * o.Den)).CompareTo(checked(o.num * Den));
        public bool Equals(Racional o) => this == o;
        public override bool Equals(object obj) => obj is Racional r && this == r;
        public override int GetHashCode() => ((num * 397) ^ Den).GetHashCode();

        /// <summary>Acepta "3", "-2", "−2", "3/4", "-3/4", "0,35", "1,5" (también punto).</summary>
        public static Racional Parse(string s)
        {
            if (!TryParse(s, out var r)) throw new FormatException("No es un racional: '" + s + "'");
            return r;
        }

        public static bool TryParse(string s, out Racional r)
        {
            r = Cero;
            if (string.IsNullOrWhiteSpace(s)) return false;
            s = s.Replace(" ", "").Replace(Menos, '-').Replace('.', ',');
            if (s.StartsWith("(") && s.EndsWith(")")) s = s.Substring(1, s.Length - 2);
            if (s.StartsWith("+")) s = s.Substring(1);
            int barra = s.IndexOf('/');
            if (barra >= 0)
            {
                if (!TryDecimal(s.Substring(0, barra), out var a) || !TryDecimal(s.Substring(barra + 1), out var b) || b.num == 0) return false;
                r = a / b;
                return true;
            }
            return TryDecimal(s, out r);
        }

        static bool TryDecimal(string s, out Racional r)
        {
            r = Cero;
            if (s.Length == 0) return false;
            bool neg = s[0] == '-';
            if (neg) s = s.Substring(1);
            if (s.Length == 0) return false;
            int coma = s.IndexOf(',');
            string ent = coma >= 0 ? s.Substring(0, coma) : s;
            string frac = coma >= 0 ? s.Substring(coma + 1) : "";
            if (ent.Length == 0) ent = "0";
            if (frac.Length > 15) return false;
            foreach (char c in ent + frac) if (c < '0' || c > '9') return false;
            long pot = 1;
            for (int i = 0; i < frac.Length; i++) pot *= 10;
            long n = long.Parse(ent) * pot + (frac.Length > 0 ? long.Parse(frac) : 0);
            r = new Racional(neg ? -n : n, pot);
            return true;
        }

        /// <summary>"3/4", "−2", "5". Con menos real.</summary>
        public string ToFraccion()
        {
            string s = Den == 1 ? Math.Abs(num).ToString() : Math.Abs(num) + "/" + Den;
            return num < 0 ? Menos + s : s;
        }

        /// <summary>Decimal con coma, truncado a maxCifras; agrega "…" si no termina. 1/3 → "0,333…".</summary>
        public string ToDecimal(int maxCifras = 3)
        {
            long a = Math.Abs(num), d = Den;
            var sb = new StringBuilder();
            if (num < 0) sb.Append(Menos);
            sb.Append(a / d);
            long resto = a % d;
            if (resto == 0) return sb.ToString();
            sb.Append(',');
            for (int i = 0; i < maxCifras && resto != 0; i++)
            {
                resto *= 10;
                sb.Append(resto / d);
                resto %= d;
            }
            if (resto != 0) sb.Append('…');
            return sb.ToString();
        }

        /// <summary>True si tiene decimal finito con a lo más maxCifras.</summary>
        public bool DecimalFinito(int maxCifras = 6)
        {
            long d = Den;
            long pot = 1;
            for (int i = 0; i < maxCifras; i++) { pot *= 10; if (pot % d == 0) return true; }
            return d == 1;
        }

        /// <summary>Texto para pantalla: decimal si se pide y es finito, si no fracción.</summary>
        public string ToTexto(bool preferirDecimal) =>
            preferirDecimal && DecimalFinito(6) ? ToDecimal(6) : ToFraccion();

        public override string ToString() => ToFraccion();
    }
}
