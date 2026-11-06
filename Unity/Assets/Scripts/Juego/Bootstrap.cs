using UnityEngine;

namespace Linea0.Juego
{
    /// <summary>Único objeto de la escena: arma todo por código.</summary>
    public class Bootstrap : MonoBehaviour
    {
        void Awake()
        {
            Application.targetFrameRate = 60;
            gameObject.AddComponent<GameManager>();
        }
    }
}
