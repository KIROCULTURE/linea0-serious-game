using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Linea0.Juego
{
    /// <summary>Lo que pasa en una clase: cuadrillas, puntos, controles, intentos y tiempo.</summary>
    public class Sesion
    {
        public const int NumCuadrillas = 4;
        public static readonly string[] Niveles = { "Casi nadie", "Menos de la mitad", "Más de la mitad", "Casi todos" };

        public enum Modo { Curso, Solo }
        public bool conCuadrillas = true;
        /// <summary>GDD 12.3: Curso (cuadrillas, controles votados) o Solo (sin cuadrillas, controles respondidos por el jugador).</summary>
        public Modo modo { get => conCuadrillas ? Modo.Curso : Modo.Solo; set => conCuadrillas = value == Modo.Curso; }
        public bool[] activas = { true, true, true, true };
        public int[] puntos = new int[NumCuadrillas];
        public int turno;                       // índice de la cuadrilla de turno
        public int[] entrada = { -1, -1, -1, -1, -1, -1 };
        public int[] salida = { -1, -1, -1, -1, -1, -1 };
        public HashSet<int> refuerzoForzado = new HashSet<int>();
        public bool forzarTodoRefuerzo;
        public float inicio = Time.realtimeSinceStartup;

        public class RegistroPuzzle
        {
            public string id;
            public int zona;
            public int intentos;
            public int pistas;
            public bool logrado;
            public bool primerIntento;
            public bool saltado;
            public List<int> aciertosPrediccion = new List<int>(); // por viaje: cuántas cuadrillas acertaron
        }

        public List<RegistroPuzzle> registros = new List<RegistroPuzzle>();
        public HashSet<int> zonasJugadasConRefuerzo = new HashSet<int>();

        public bool Activa(int c) => activas[c];
        public int CuantasActivas => activas.Count(a => a);

        public void PrimerTurno()
        {
            turno = 0;
            while (!activas[turno]) turno = (turno + 1) % NumCuadrillas;
        }

        /// <summary>Rota A → B → C → D saltando las que no juegan.</summary>
        public void SiguienteTurno()
        {
            for (int i = 0; i < NumCuadrillas; i++)
            {
                turno = (turno + 1) % NumCuadrillas;
                if (activas[turno]) return;
            }
        }

        /// <summary>Tabla 4.2: +2 al primer intento, +1 al segundo.</summary>
        public static int PuntosPorLlegar(int intento, bool usoPista)
        {
            if (intento <= 1 && !usoPista) return 2;
            if (intento <= 2) return 1;
            return 0;
        }

        public void Sumar(int cuadrilla, int n)
        {
            if (conCuadrillas && cuadrilla >= 0 && cuadrilla < NumCuadrillas) puntos[cuadrilla] += n;
        }

        /// <summary>Regla 6.14: la zona lleva refuerzo si su pregunta de entrada quedó en "Casi nadie" o "Menos de la mitad".</summary>
        public bool ConRefuerzo(int zona)
        {
            if (forzarTodoRefuerzo || refuerzoForzado.Contains(zona)) return true;
            int q = zona - 1; // pregunta n ↔ zona n
            return q >= 0 && q < entrada.Length && entrada[q] >= 0 && entrada[q] <= 1;
        }

        public RegistroPuzzle Registro(string id, int zona)
        {
            var r = registros.FirstOrDefault(x => x.id == id);
            if (r == null) { r = new RegistroPuzzle { id = id, zona = zona }; registros.Add(r); }
            return r;
        }

        public int Ganadora()
        {
            int mejor = -1;
            for (int i = 0; i < NumCuadrillas; i++) if (activas[i] && (mejor < 0 || puntos[i] > puntos[mejor])) mejor = i;
            return mejor;
        }

        public List<int> Ganadoras()
        {
            int g = Ganadora();
            if (g < 0) return new List<int>();
            return Enumerable.Range(0, NumCuadrillas).Where(i => activas[i] && puntos[i] == puntos[g]).ToList();
        }

        public float MinutosJugados => (Time.realtimeSinceStartup - inicio) / 60f;

        /// <summary>Datos de ejemplo para el Modo demo.</summary>
        public void CargarEjemplo()
        {
            puntos = new[] { 6, 4, 5, 3 };
            entrada = new[] { 3, 1, 1, 0, 0, 1 };
            salida = new[] { 3, 3, 2, 2, 2, 2 };
        }
    }
}
