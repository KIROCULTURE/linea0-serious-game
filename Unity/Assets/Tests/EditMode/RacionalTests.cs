using Linea0.Matematica;
using NUnit.Framework;

public class RacionalTests
{
    static Racional R(string s) => Racional.Parse(s);

    [Test] public void Reduce() { var r = new Racional(6, -8); Assert.AreEqual(-3, r.Num); Assert.AreEqual(4, r.Den); }
    [Test] public void MitadIgualDosCuartos() => Assert.AreEqual(new Racional(1, 2), new Racional(2, 4));
    [Test] public void RestarNegativo() => Assert.AreEqual(new Racional(5), new Racional(2) - new Racional(-3));
    [Test] public void Suma() => Assert.AreEqual(R("3/4"), R("1/4") + R("1/2"));
    [Test] public void Producto() => Assert.AreEqual(R("3/4"), 3 * R("1/4"));
    [Test] public void Division() => Assert.AreEqual(new Racional(8), R("2") / R("1/4"));
    [Test] public void Signos() { Assert.AreEqual(new Racional(6), R("-6") * R("-1")); Assert.AreEqual(new Racional(-2), R("3") + R("-5")); }
    [Test] public void Comparacion() { Assert.IsTrue(R("1/3") < R("0,34")); Assert.IsTrue(R("-2") < R("0")); Assert.IsTrue(R("5/4") > 1); }
    [Test] public void DefaultEsCero() => Assert.AreEqual(Racional.Cero, default(Racional));
    [Test] public void DivisionPorCero() => Assert.Throws<System.DivideByZeroException>(() => { var _ = R("1") / R("0"); });

    [TestCase("3", 3, 1)]
    [TestCase("-2", -2, 1)]
    [TestCase("−2", -2, 1)]
    [TestCase("3/4", 3, 4)]
    [TestCase("0,35", 7, 20)]
    [TestCase("1,5", 3, 2)]
    [TestCase("-0,2", -1, 5)]
    [TestCase("+1/2", 1, 2)]
    [TestCase("(−3)", -3, 1)]
    public void Parse(string s, long n, long d) => Assert.AreEqual(new Racional(n, d), R(s));

    [Test] public void ParseInvalido() => Assert.IsFalse(Racional.TryParse("abc", out _));

    [Test] public void Decimales()
    {
        Assert.AreEqual("0,333…", new Racional(1, 3).ToDecimal(3));
        Assert.AreEqual("0,35", R("0,35").ToDecimal(3));
        Assert.AreEqual("2,25", R("9/4").ToDecimal());
        Assert.AreEqual("−12,5", R("-25/2").ToDecimal());
        Assert.AreEqual("7", R("7").ToDecimal());
    }

    [Test] public void Fracciones()
    {
        Assert.AreEqual("3/4", R("0,75").ToFraccion());
        Assert.AreEqual("−2", R("-2").ToFraccion());
        Assert.AreEqual("−1/2", R("-2/4").ToFraccion());
    }

    [Test] public void Texto()
    {
        Assert.AreEqual("0,75", R("3/4").ToTexto(true));
        Assert.AreEqual("1/3", R("1/3").ToTexto(true));
        Assert.AreEqual("3/4", R("3/4").ToTexto(false));
    }
}
