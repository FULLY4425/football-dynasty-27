using UnityEngine;

namespace FootballDynasty
{
    [RequireComponent(typeof(RectTransform))]
    public sealed class SafeAreaPanel : MonoBehaviour
    {
        private RectTransform panel;
        private Rect last;
        private int width, height;
        void Awake()
        {
            panel = (RectTransform)transform;
        }

        void OnEnable() => Apply();
        void Update()
        {
            if (last != UnityEngine.Screen.safeArea || width != UnityEngine.Screen.width || height != UnityEngine.Screen.height)
                Apply();
        }

        void Apply()
        {
            if (!panel)
                panel = (RectTransform)transform;
            width = UnityEngine.Screen.width;
            height = UnityEngine.Screen.height;
            if (width <= 0 || height <= 0)
                return;
            last = UnityEngine.Screen.safeArea;
            panel.anchorMin = new Vector2(last.xMin / width, last.yMin / height);
            panel.anchorMax = new Vector2(last.xMax / width, last.yMax / height);
            panel.offsetMin = panel.offsetMax = Vector2.zero;
        }
    }
}
