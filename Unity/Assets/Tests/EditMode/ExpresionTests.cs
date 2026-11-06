using System.Collections.Generic;
using Linea0.Matematica;
using NUnit.Framework;

public class ExpresionTests
{
    static Racional R(string s) => Racional.Parse(s);

    [TestCase("2 + 3 × 4", "14")]
    [TestCase("(2 + 3) × 4", "20")]
    [TestCase("−8 + 12 : 4", "-5")]
    [TestCase("1/2 · (−4) + 0,5 · 6", "1")]
    [TestCase("(−2) · 3 + 10 : 2", "-1")]
    [TestCase("12 : 4", "3")]
    [TestCase("−4 − 3", "-7")]
    [TestCase("3 × 1/4", "3/4")]
    [TestCase("0,5 + 0,25", "0,75")]
    [TestCase("6 − 2 × 3", "0")]
    [TestCase("2 − (−3)", "5")]
    public void Jerarquia(string e, string esperado) => Assert.AreEqual(R(esperado), Expresion.EvaluarJerarquia(e));

    [Test] public void Serpiente()
    {
        var pasos = new List<Racional>();
        Assert.AreEqual(R("12,5"), Expresion.EvaluarIzqDer("−10 : 5 − 2 · (−3) + 0,5", pasos));
        CollectionAssert.AreEqual(new[] { R("-2"), R("-4"), R("12"), R("12,5") }, pasos);
    }

    [Test] public void OrdenEquivocado61Da20()
    {
        // primero la suma (índice 1), después el producto que queda en el índice 1
        Assert.AreEqual(R("20"), Expresion.EvaluarEnOrden("2 + 3 × 4", new[] { 1, 1 }));
        Assert.AreEqual(R("14"), Expresion.EvaluarEnOrden("2 + 3 × 4", new[] { 3, 1 }));
    }

    [Test] public void IzquierdaADerecha66Da2() => Assert.AreEqual(R("2"), Expresion.EvaluarIzqDer("(−2) · 3 + 10 : 2"));

    [Test] public void Parentesis62SoloPermiteLaSuma()
    {
        var t = Expresion.Tokenizar("(2 + 3) × 4");
        CollectionAssert.AreEqual(new[] { 2 }, Expresion.Disponibles(t));
        t = Expresion.Paso(t, 2, out var r);
        Assert.AreEqual(R("5"), r);
        Assert.AreEqual("5 × 4", Expresion.Texto(t, false));
    }

    [Test] public void MultiplicacionesEnCualquierOrden65()
    {
        var e = "1/2 · (−4) + 0,5 · 6";
        Assert.AreEqual(R("1"), Expresion.EvaluarEnOrden(e, new[] { 1, 3, 1 }));
        Assert.AreEqual(R("1"), Expresion.EvaluarEnOrden(e, new[] { 5, 1, 1 }));
    }

    [Test] public void TextoConMenosReal()
    {
        Assert.AreEqual("(−2) · 3 + 10 : 2", Expresion.Texto(Expresion.Tokenizar("(-2) · 3 + 10 : 2"), false));
        Assert.AreEqual("−4 − 3", Expresion.Texto(Expresion.Tokenizar("-4 - 3"), false));
    }
}
