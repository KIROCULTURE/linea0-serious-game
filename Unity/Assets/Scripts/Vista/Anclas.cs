using System;
using System.Collections.Generic;
using UnityEngine;

namespace Linea0.Vista
{
    /// <summary>Objetivos de la guía por nombre ("destino", "boleto:0", "partir"…). Cada uno entrega sus 4 esquinas en el mundo.</summary>
    public static class Anclas
    {
        static readonly Dictionary<string, Func<Vector3[]>> mapa = new Dictionary<string, Func<Vector3[]>>();

        public static void Poner(string nombre, Func<Vector3[]> esquinas) => mapa[nombre] = esquinas;

        public static void Poner(string nombre, RectTransform rt) => mapa[nombre] = () => Esquinas(rt);

        /// <summary>Varios objetos juntos (por ejemplo las casillas de la ruta).</summary>
        public static void Poner(string nombre, params RectTransform[] rts) => mapa[nombre] = () =>
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue), max = new Vector3(float.MinValue, float.MinValue);
            foreach (var rt in rts)
            {
                var c = Esquinas(rt);
                if (c == null) continue;
                min = Vector3.Min(min, c[0]); max = Vector3.Max(max, c[2]);
            }
            return min.x > max.x ? null : new[] { min, new Vector3(min.x, max.y), max, new Vector3(max.x, min.y) };
        };

        public static Vector3[] Esquinas(RectTransform rt)
        {
            if (rt == null || !rt.gameObject.activeInHierarchy) return null;
            var c = new Vector3[4];
            rt.GetWorldCorners(c);
            return c;
        }

        public static void Quitar(string nombre) => mapa.Remove(nombre);
        public static void Limpiar() => mapa.Clear();

        /// <summary>Rectángulo del objetivo en coordenadas locales de 'en' (null si no está o no se ve).</summary>
        public static Rect? Rect(string nombre, RectTransform en)
        {
            if (!mapa.TryGetValue(nombre, out var f)) return null;
            Vector3[] c;
            try { c = f(); } catch (MissingReferenceException) { return null; }
            if (c == null) return null;
            Vector2 a = en.InverseTransformPoint(c[0]), b = en.InverseTransformPoint(c[2]);
            return UnityEngine.Rect.MinMaxRect(Mathf.Min(a.x, b.x), Mathf.Min(a.y, b.y), Mathf.Max(a.x, b.x), Mathf.Max(a.y, b.y));
        }
    }
}
