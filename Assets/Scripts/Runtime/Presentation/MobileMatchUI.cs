using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FootballDynasty
{
    public sealed class MobileMatchUI : MonoBehaviour
    {
        [SerializeField]
        private bool previewInEditor;
        private FootballSimulator game;
        private GameObject canvasRoot;
        private Sprite circle;
        private Texture2D circleTexture;
        public VirtualJoystick Joystick { get; private set; }
        public TouchActionButton Shoot { get; private set; }
        public TouchActionButton Pass { get; private set; }
        public TouchActionButton Switch { get; private set; }
        public TouchActionButton Slide {get;private set;}
        public bool IsVisible => canvasRoot && canvasRoot.activeSelf;

        public void Initialize(FootballSimulator owner)
        {
            game = owner;
            if (!Application.isMobilePlatform && !Application.isEditor)
                return;
            if (!FindFirstObjectByType<EventSystem>())
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            canvasRoot = new GameObject("Mobile Controls · 1920 × 1080", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasRoot.transform.SetParent(transform, false);
            var canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;
            var scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = .5f;
            var safe = new GameObject("Safe Area", typeof(RectTransform), typeof(SafeAreaPanel)).GetComponent<RectTransform>();
            safe.SetParent(canvasRoot.transform, false);
            circle = CreateCircle();
            var pad = CreateImage("Joystick", safe, new Vector2(0, 0), new Vector2(200, 200), 270, new Color(.06f, .1f, .13f, .38f));
            var knob = CreateImage("Handle", pad, new Vector2(.5f, .5f), Vector2.zero, 105, new Color(.92f, .96f, 1, .5f));
            knob.GetComponent<Image>().raycastTarget = false;
            Joystick = pad.gameObject.AddComponent<VirtualJoystick>();
            Joystick.Configure(knob);
            Shoot = CreateAction(safe, "A", new Vector2(-110, 180), new Color(.22f, .72f, .42f, .48f));
            Pass = CreateAction(safe, "B", new Vector2(-265, 135), new Color(.22f, .5f, .92f, .48f));
            Switch = CreateAction(safe, "C", new Vector2(-420, 180), new Color(.92f, .65f, .22f, .48f));
            Slide=CreateAction(safe,"D",new Vector2(-265,310),new Color(.85f,.25f,.22f,.6f));canvasRoot.SetActive(false);
        }

        RectTransform CreateImage(string name, Transform parent, Vector2 anchor, Vector2 position, float size, Color color)
        {
            var rect = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = Vector2.one * size;
            var image = rect.GetComponent<Image>();
            image.sprite = circle;
            image.color = color;
            return rect;
        }

        TouchActionButton CreateAction(Transform parent, string label, Vector2 position, Color color)
        {
            var rect = CreateImage(label, parent, new Vector2(1, 0), position, 130, color);
            var text = new GameObject("Label", typeof(RectTransform), typeof(Text)).GetComponent<Text>();
            text.transform.SetParent(rect, false);
            var t = (RectTransform)text.transform;
            t.anchorMin = Vector2.zero;
            t.anchorMax = Vector2.one;
            t.offsetMin = t.offsetMax = Vector2.zero;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label;
            text.fontSize = 58;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.raycastTarget = false;
            return rect.gameObject.AddComponent<TouchActionButton>();
        }

        Sprite CreateCircle()
        {
            circleTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            circleTexture.filterMode = FilterMode.Bilinear;
            circleTexture.wrapMode = TextureWrapMode.Clamp;
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 64; x++)
                {
                    float distance = new Vector2(x - 31.5f, y - 31.5f).magnitude;
                    circleTexture.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(31.5f - distance)));
                }

            circleTexture.Apply();
            return Sprite.Create(circleTexture, new Rect(0, 0, 64, 64), new Vector2(.5f, .5f), 64);
        }

        void Update()
        {
            if (!canvasRoot || !game)
                return;
            bool show = (Application.isMobilePlatform || (Application.isEditor && previewInEditor)) && game.Screen == "match" && game.Running && !game.Replay.IsPlaying;
            if (show != canvasRoot.activeSelf)
                canvasRoot.SetActive(show);
        }

        void OnApplicationPause(bool paused)
        {
            if (paused)
            {
                Joystick?.ResetInput();
                Shoot?.ResetInput();
                Pass?.ResetInput();
                Switch?.ResetInput();Slide?.ResetInput();
            }
        }

        void OnDisable()
        {
            if (canvasRoot)
                canvasRoot.SetActive(false);
        }

        void OnDestroy()
        {
            if (canvasRoot)
                Destroy(canvasRoot);
            if (circle)
                Destroy(circle);
            if (circleTexture)
                Destroy(circleTexture);
        }
    }
}
