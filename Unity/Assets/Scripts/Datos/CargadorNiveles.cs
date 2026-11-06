using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Linea0.Datos
{
    public static class CargadorNiveles
    {
        static DatosNiveles cache;

        public static DatosNiveles Datos
        {
            get
            {
                if (cache == null)
                {
                    var ta = Resources.Load<TextAsset>("niveles");
                    cache = DesdeTexto(ta.text);
                }
                return cache;
            }
        }

        public static DatosNiveles DesdeTexto(string json) => JsonUtility.FromJson<DatosNiveles>(json);

        public static Zona Zona(int n) => Datos.zonas.First(z => z.numero == n);

        public static List<Puzzle> PuzzlesDeZona(int zona, bool conRefuerzo) =>
            Datos.puzzles.Where(p => p.zona == zona && (p.esencial || conRefuerzo)).ToList();

        public static Puzzle Puzzle(string id) => Datos.puzzles.First(p => p.id == id);
    }
}
