using System.Collections.Generic;
using System.Linq;
using Linea0.Datos;
using Linea0.Matematica;
using NUnit.Framework;

public class NivelesTests
{
    static DatosNiveles D => CargadorNiveles.Datos;
    static Racional R(string s) => Racional.Parse(s);
    static IEnumerable<Puzzle> DeTipo(TipoPuzzle t) => D.puzzles.Where(p => p.Tipo == t);

    static readonly string[] Ids =
    {
        "0.1", "0.2", "1.1", "1.2", "1.3", "1.4", "1.5", "1.6", "1.7",
        "2.1", "2.2", "2.3", "2.4", "2.5", "2.6", "2.7",
        "3.1", "3.2", "3.3", "3.4", "3.5", "3.6", "3.7", "3.8",
        "4.1", "4.2", "4.3", "4.4", "4.5", "4.6", "4.7", "4.8", "4.9",
        "5.1", "5.2", "5.3", "6.1", "6.2", "6.3", "6.4", "6.5", "6.6"
    };

    static readonly string[] Refuerzos = { "1.2", "1.3", "1.6", "2.2", "2.6", "3.2", "3.4", "4.3", "4.6", "4.8", "6.3" };

    // Respuesta correcta según el GDD (no solo el índice).
    static readonly Dictionary<string, string> RespuestaGdd = new Dictionary<string, string>
    {
        { "1.1", "5" }, { "1.5", "4" }, { "1.6", "3" }, { "1.7", "3" }, { "2.7", "−7" }, { "3.7", "8" },
        { "3.8", "3/4" }, { "4.8", "6" }, { "4.9", "0,75" }, { "6.4", "12,5" }, { "6.6", "−1" }
    };

    [Test] public void TodosLosPuzzlesDelGdd()
    {
        CollectionAssert.AreEquivalent(Ids, D.puzzles.Select(p => p.id).ToArray());
        foreach (var p in D.puzzles)
        {
            Assert.AreEqual(!Refuerzos.Contains(p.id), p.esencial, p.id + " esencial/refuerzo");
            Assert.AreEqual(p.zona.ToString(), p.id.Split('.')[0], p.id);
            Assert.DoesNotThrow(() => { var _ = p.Tipo; }, p.id);
            Assert.AreEqual(3, p.pistas.Length, p.id + " pistas");
            Assert.IsFalse(string.IsNullOrEmpty(p.frasePedido), p.id + " pedido");
            if (p.alternativas != null && p.alternativas.Length > 0)
            {
                Assert.AreEqual(4, p.alternativas.Length, p.id);
                Assert.That(p.correcta, Is.InRange(0, 3), p.id);
            }
        }
        Assert.AreEqual(7, D.zonas.Length);
    }

    [Test] public void RespuestasSonLasDelGdd()
    {
        foreach (var kv in RespuestaGdd)
        {
            var p = CargadorNiveles.Puzzle(kv.Key);
            Assert.AreEqual(kv.Value, p.alternativas[p.correcta], kv.Key);
        }
    }

    /// <summary>Todas las rutas posibles con la mano: permutaciones sin repetición de largo 1..casillas,
    /// cada boleto con o sin palanca si el puzzle la tiene.</summary>
    static IEnumerable<(List<int> usados, Racional fin)> Rutas(Puzzle p)
    {
        int n = p.mano.Length;
        var res = new List<(List<int>, Racional)>();
        void Rec(List<int> usados, Racional pos)
        {
            if (usados.Count > 0) res.Add((new List<int>(usados), pos));
            if (usados.Count == p.casillas) return;
            for (int i = 0; i < n; i++)
            {
                if (usados.Contains(i)) continue;
                usados.Add(i);
                Rec(usados, p.mano[i].Aplicar(pos, false));
                if (p.palanca && p.mano[i].Invertible) Rec(usados, p.mano[i].Aplicar(pos, true));
                usados.RemoveAt(usados.Count - 1);
            }
        }
        Rec(new List<int>(), p.Inicio);
        return res;
    }

    [Test] public void BoletosSolucionUnica()
    {
        foreach (var p in DeTipo(TipoPuzzle.Boletos))
        {
            var meta = p.Meta.Valor;
            var pos = p.Inicio;
            for (int k = 0; k < p.solucion.Length; k++)
                pos = p.mano[p.solucion[k]].Aplicar(pos, p.invertidos != null && p.invertidos.Contains(k));
            Assert.AreEqual(meta, pos, p.id + ": la solución del GDD no llega");

            var esperado = p.solucion.OrderBy(x => x).ToList();
            foreach (var (usados, fin) in Rutas(p))
                if (fin == meta)
                    CollectionAssert.AreEqual(esperado, usados.OrderBy(x => x).ToList(), p.id + ": solución accidental");
        }
    }

    [Test] public void PalancaNecesariaEn24()
    {
        var p = CargadorNiveles.Puzzle("2.4");
        Assert.AreEqual(R("-1"), p.mano[0].Aplicar(p.Inicio, false));
        Assert.AreEqual(R("5"), p.mano[0].Aplicar(p.Inicio, true));
    }

    [Test] public void SinViaYJerarquia()
    {
        foreach (var p in DeTipo(TipoPuzzle.SinVia))
        {
            var v = Expresion.EvaluarJerarquia(p.expresion);
            Assert.AreEqual(v, R(p.alternativas[p.correcta]), p.id);
            if (p.comprobacion == "viaje")
            {
                var pos = p.Inicio;
                foreach (var b in p.mano) pos = b.Aplicar(pos);
                Assert.AreEqual(v, pos, p.id + " comprobación");
            }
            if (p.comprobacion == "repartir") Assert.AreEqual(v, new Racional(p.cantidad, p.vagones), p.id);
        }
        foreach (var p in DeTipo(TipoPuzzle.Jerarquia))
            Assert.AreEqual(R(p.meta), Expresion.EvaluarJerarquia(p.expresion), p.id);
        Assert.AreEqual(R("20"), Expresion.EvaluarIzqDer(CargadorNiveles.Puzzle("6.1").expresion));
        Assert.AreEqual(R("2"), Expresion.EvaluarIzqDer(CargadorNiveles.Puzzle("6.6").expresion));
    }

    [Test] public void Serpiente()
    {
        foreach (var p in DeTipo(TipoPuzzle.Serpiente))
        {
            Assert.AreEqual(R(p.alternativas[p.correcta]), Expresion.EvaluarIzqDer(p.expresion), p.id);
            Assert.AreEqual(p.Inicio, Expresion.Tokenizar(p.expresion)[0].Valor, p.id);
        }
    }

    [Test] public void ContarSaltos()
    {
        foreach (var p in DeTipo(TipoPuzzle.ContarSaltos))
            Assert.AreEqual((R(p.tramo[1]) - R(p.tramo[0])) / R(p.salto), R(p.alternativas[p.correcta]), p.id);
    }

    [Test] public void RepartirYContar()
    {
        foreach (var p in DeTipo(TipoPuzzle.Repartir))
            Assert.AreEqual(new Racional(p.cantidad / p.vagones), R(p.alternativas[p.correcta]), p.id);
        foreach (var p in DeTipo(TipoPuzzle.Contar))
            Assert.AreEqual(new Racional(p.cantidad), R(p.alternativas[p.correcta]), p.id);
    }

    static bool DivisorAlcanza(Racional ini, Racional meta, int dialFijo, Racional max)
    {
        for (int partes = 1; partes <= 12; partes++)
        {
            if (dialFijo > 0 && partes != dialFijo) continue;
            for (int k = 0; ini + new Racional(k, partes) <= max; k++)
                if (ini + new Racional(k, partes) == meta) return true;
        }
        return false;
    }

    [Test] public void DivisorAlcanzable()
    {
        foreach (var p in DeTipo(TipoPuzzle.Divisor))
        {
            Assert.IsTrue(DivisorAlcanza(p.Inicio, p.Meta.Valor, p.dialFijo, p.RangoMax), p.id);
            Assert.AreEqual(p.Meta.Valor, p.Inicio + new Racional(p.avanceSolucion, p.partesSolucion), p.id + " solución");
            if (p.dialFijo > 0) Assert.AreEqual(p.dialFijo, p.partesSolucion, p.id);
        }
    }

    [Test] public void LupaAlcanzable()
    {
        foreach (var p in DeTipo(TipoPuzzle.Lupa))
        {
            var m = p.Meta.Valor;
            Assert.That(m >= p.RangoMin && m <= p.RangoMax, p.id + " fuera de la vía");
            if (p.guion == "periodico")
            {
                Assert.IsFalse((m * 1000).EsEntero, p.id + " debería no terminar");
                Assert.AreEqual(m, new Racional(p.avanceSolucion, p.partesSolucion), p.id);
            }
            else Assert.IsTrue((m * 1000).EsEntero, p.id + ": la lupa llega hasta milésimas");
        }
    }

    [Test] public void Clasificar()
    {
        foreach (var p in DeTipo(TipoPuzzle.Clasificar))
        {
            Assert.AreEqual(p.cartas.Length, p.regiones.Length);
            for (int i = 0; i < p.cartas.Length; i++)
            {
                var pos = Posicion.Parse(p.cartas[i]);
                string reg = !pos.EsRacional ? "I" : !pos.Valor.EsEntero ? "Q" : pos.Valor.Signo > 0 ? "N" : "Z";
                Assert.AreEqual(reg, p.regiones[i], p.id + " " + p.cartas[i]);
            }
        }
    }

    [Test] public void Controles()
    {
        string[] correctas = { "15", "2", "1/2", "0,3", "√2", "0" };
        Assert.AreEqual(6, D.control.Length);
        for (int i = 0; i < 6; i++)
        {
            var q = D.control[i];
            Assert.AreEqual(4, q.alternativas.Length);
            Assert.AreEqual(correctas[i], q.alternativas[q.correcta], "Control " + q.numero);
        }
        Assert.AreEqual(R("15"), 5 * R("3"));
        Assert.AreEqual(R("2"), R("-1") + 3);
        Assert.AreEqual(R("4/8"), R("1/2"));
        Assert.AreEqual(R("3/10"), R("0,3"));
        Assert.AreEqual(R("0"), Expresion.EvaluarJerarquia(D.control[5].texto.Replace("¿Cuánto es ", "").Replace("?", "")));
    }
}
