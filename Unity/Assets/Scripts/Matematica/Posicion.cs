using System;

namespace Linea0.Matematica
{
    public enum TipoPosicion { Racional, Raiz2, Pi }

    /// <summary>Lugar de la vía: un racional exacto o un irracional marcado. El double es solo para dibujar.</summary>
    public readonly struct Posicion
    {
        public readonly TipoPosicion Tipo;
        public readonly Racional Valor;

        Posicion(TipoPosicion t, Racional v) { Tipo = t; Valor = v; }

        public static Posicion De(Racional r) => new Posicion(TipoPosicion.Racional, r);
        public static readonly Posicion Raiz2 = new Posicion(TipoPosicion.Raiz2, Racional.Cero);
        public static readonly Posicion Pi = new Posicion(TipoPosicion.Pi, Racional.Cero);

        public static implicit operator Posicion(Racional r) => De(r);

        public bool EsRacional => Tipo == TipoPosicion.Racional;

        public double ToDouble()
        {
            switch (Tipo)
            {
                case TipoPosicion.Raiz2: return Math.Sqrt(2);
                case TipoPosicion.Pi: return Math.PI;
                default: return Valor.ToDouble();
            }
        }

        /// <summary>Acepta un racional, "√2" / "raiz2" o "π" / "pi".</summary>
        public static Posicion Parse(string s)
        {
            string t = (s ?? "").Trim().ToLowerInvariant();
            if (t == "√2" || t == "raiz2") return Raiz2;
            if (t == "π" || t == "pi") return Pi;
            return De(Racional.Parse(s));
        }

        public string ToTexto(bool preferirDecimal)
        {
            switch (Tipo)
            {
                case TipoPosicion.Raiz2: return "√2";
                case TipoPosicion.Pi: return "π";
                default: return Valor.ToTexto(preferirDecimal);
            }
        }

        public bool Igual(Posicion o) => Tipo == o.Tipo && (Tipo != TipoPosicion.Racional || Valor == o.Valor);
    }
}
