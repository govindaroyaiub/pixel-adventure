using UnityEngine;
using UnityEngine.EventSystems;

namespace PixelAdventure.Controls
{
    /// <summary>On-screen stick. Value is -1..1 on both axes.</summary>
    public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField] RectTransform knob;
        [SerializeField] float radius = 110f;

        RectTransform rect;

        public Vector2 Value { get; private set; }

        void Awake() => rect = (RectTransform)transform;

        public void OnPointerDown(PointerEventData e) => OnDrag(e);

        public void OnDrag(PointerEventData e)
        {
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, e.position, e.pressEventCamera, out Vector2 local)) return;
            Vector2 clamped = Vector2.ClampMagnitude(local, radius);
            knob.anchoredPosition = clamped;
            Value = clamped / radius;
        }

        public void OnPointerUp(PointerEventData e)
        {
            knob.anchoredPosition = Vector2.zero;
            Value = Vector2.zero;
        }
    }
}
