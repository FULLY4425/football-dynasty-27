using UnityEngine;
using UnityEngine.EventSystems;

namespace FootballDynasty
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [SerializeField]
        private RectTransform handle;
        [SerializeField, Range(0, .4f)]
        private float deadZone = .12f;
        private RectTransform pad;
        private int? pointer;
        public Vector2 Value { get; private set; }

        void Awake()
        {
            pad = (RectTransform)transform;
        }

        public void Configure(RectTransform knob) => handle = knob;
        public void OnPointerDown(PointerEventData e)
        {
            if (pointer.HasValue)
                return;
            pointer = e.pointerId;
            OnDrag(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (pointer != e.pointerId || !handle)
                return;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(pad, e.position, e.pressEventCamera, out var local))
                return;
            float radius = Mathf.Max(1, (Mathf.Min(pad.rect.width, pad.rect.height) - handle.rect.width) * .5f);
            Vector2 raw = Vector2.ClampMagnitude(local / radius, 1);
            handle.anchoredPosition = raw * radius;
            ControlMath.Analog(raw.x, raw.y, deadZone, out var x, out var y);
            Value = new Vector2(x, y);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (pointer == e.pointerId)
                ResetInput();
        }

        public void ResetInput()
        {
            pointer = null;
            Value = Vector2.zero;
            if (handle)
                handle.anchoredPosition = Vector2.zero;
        }

        void OnDisable() => ResetInput();
        void OnApplicationFocus(bool focused)
        {
            if (!focused)
                ResetInput();
        }
    }
}
