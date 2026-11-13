using System.Linq;
using Linea0.Datos;
using Linea0.Sala;
using NUnit.Framework;

public class GuiasTests
{
    [Test] public void CadaGuiaEsDeUnPuzzleYTieneTextosCortos()
    {
        var ids = CargadorNiveles.Datos.puzzles.Select(p => p.id).ToList();
        foreach (var g in Guia.Datos.guias)
        {
            Assert.Contains(g.clave, ids, "guía sin puzzle: " + g.clave);
            Assert.IsNotEmpty(g.pasos, g.clave);
            foreach (var p in g.pasos)
            {
                Assert.IsFalse(string.IsNullOrEmpty(p.ancla), g.clave);
                Assert.That(p.texto.Length, Is.LessThanOrEqualTo(70), g.clave + ": " + p.texto); // GDD 12.1
                Assert.Contains(p.espera, new[] { "tocar", "accion", "seguir", "solo" }, g.clave);
            }
        }
    }

    [Test] public void GuiasDelGdd()
    {
        string[] claves = { "0.1", "0.2", "1.1", "1.4", "1.5", "2.1", "2.4", "2.5", "3.1", "3.7", "4.1", "5.1", "5.2", "5.3", "6.1" };
        CollectionAssert.AreEquivalent(claves, Guia.Datos.guias.Select(g => g.clave).ToArray());
        Assert.AreEqual(5, Guia.Buscar("0.1").pasos.Length);
        Assert.AreEqual(7, Guia.Datos.avisos.Length);
    }
}
