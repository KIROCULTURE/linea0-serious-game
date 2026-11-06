using Linea0.Vista;
using UnityEngine;

namespace Linea0.Juego
{
    public class GameManager : MonoBehaviour
    {
        void Start()
        {
            UI.AsegurarEventSystem();
            var c = UI.Lienzo("Menu", 0);
            var fondo = UI.Caja(c.transform, "Fondo", Paleta.Cielo);
            UI.Llenar(fondo.rectTransform);
            var t = UI.Texto(c.transform, "Línea 0", 120, Paleta.Texto);
            UI.Ubicar(t.rectTransform, new Vector2(0.5f, 0.6f), Vector2.zero, new Vector2(1200, 160));
            var p = UI.Texto(c.transform, "Próximamente", 56, Paleta.Gris);
            UI.Ubicar(p.rectTransform, new Vector2(0.5f, 0.45f), Vector2.zero, new Vector2(1200, 80));
            var b = UI.CrearBoton(c.transform, "Salir", 48, Paleta.Texto, Application.Quit);
            UI.Ubicar(b.rt, new Vector2(0.5f, 0.25f), Vector2.zero, new Vector2(360, 100));
        }
    }
}
