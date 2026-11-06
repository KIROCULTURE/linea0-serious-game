using System;
using System.Collections;
using UnityEngine;

namespace Linea0.Juego
{
    /// <summary>Espera simple entre corrutinas: alguien llama Dar() y el que espera sigue.</summary>
    public class Senal
    {
        public bool listo;
        public int valor = -1;
        public void Dar(int v = 0) { valor = v; listo = true; }
        public void Reset() { listo = false; valor = -1; }
        public IEnumerator Esperar() { while (!listo) yield return null; }
    }

    public static class Ease
    {
        public static float SuaveInOut(float t) { t = Mathf.Clamp01(t); return t * t * (3 - 2 * t); }
        public static float Salida(float t) { t = Mathf.Clamp01(t); return 1 - (1 - t) * (1 - t); }
        public static float Rebote(float t) { t = Mathf.Clamp01(t); return 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2); }

        /// <summary>Corre f(0..1) durante d segundos.</summary>
        public static IEnumerator Durante(float d, Action<float> f)
        {
            float t = 0;
            while (t < d) { f(t / d); t += Time.deltaTime; yield return null; }
            f(1);
        }

        public static IEnumerator Pausa(float s)
        {
            float t = 0;
            while (t < s) { t += Time.deltaTime; yield return null; }
        }
    }
}
