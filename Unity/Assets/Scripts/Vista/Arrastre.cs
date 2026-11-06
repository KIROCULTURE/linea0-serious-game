using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Linea0.Vista
{
    /// <summary>Avisa cada movimiento del mouse mientras se arrastra este elemento de UI, y cuando se suelta.</summary>
    public class Arrastre : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public Action<PointerEventData> alArrastrar, alSoltar;
        public void OnBeginDrag(PointerEventData e) => alArrastrar?.Invoke(e);
        public void OnDrag(PointerEventData e) => alArrastrar?.Invoke(e);
        public void OnEndDrag(PointerEventData e) => alSoltar?.Invoke(e);
    }
}
