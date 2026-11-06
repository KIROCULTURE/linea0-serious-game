using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Linea0.Vista
{
    /// <summary>Avisa cada movimiento del mouse mientras se arrastra este elemento de UI.</summary>
    public class Arrastre : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public Action<PointerEventData> alArrastrar;
        public void OnBeginDrag(PointerEventData e) => alArrastrar?.Invoke(e);
        public void OnDrag(PointerEventData e) => alArrastrar?.Invoke(e);
    }
}
