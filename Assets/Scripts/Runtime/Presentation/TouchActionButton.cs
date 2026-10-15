using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FootballDynasty
{
    [RequireComponent(typeof(Image))]
    public sealed class TouchActionButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        private int? pointer;
        private bool pending;
        private Image image;
        private Color normal;
        void Awake()
        {
            image = GetComponent<Image>();
            normal = image.color;
        }

        public void OnPointerDown(PointerEventData e)
        {
            if (pointer.HasValue)
                return;
            pointer = e.pointerId;
            pending = true;
            image.color = new Color(normal.r, normal.g, normal.b, .8f);
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (pointer != e.pointerId)
                return;
            pointer = null;
            image.color = normal;
        }

        // Consumed by the match Update, so a tap is retained across EventSystem update order.
        public bool Consume()
        {
            bool result = pending;
            pending = false;
            return result;
        }

        public void ResetInput()
        {
            pending = false;
            pointer = null;
            if (image)
                image.color = normal;
        }

        void OnDisable() => ResetInput();
        void OnApplicationFocus(bool focused)
        {
            if (!focused)
                ResetInput();
        }
    }
}
