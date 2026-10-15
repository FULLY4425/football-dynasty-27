using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace FootballDynasty
{
    public sealed class CoachOnboarding : MonoBehaviour
    {
        static readonly Color Navy = new Color(.035f, .065f, .14f), Panel = new Color(.08f, .14f, .22f, .96f), Accent = new Color(.25f, .95f, .65f), Muted = new Color(.65f, .76f, .82f);
        static readonly string[] Steps =
        {
            "01  PERSONAJE",
            "02  ATUENDO",
            "03  INFORMACIÓN",
            "04  CLUB"
        };
        static readonly string[] AvatarNames =
        {
            "Alex",
            "Mateo",
            "Samira"
        };
        static readonly string[] GenderNames =
        {
            "Masculino",
            "Femenino",
            "No binario"
        };
        static readonly string[] GenderCodes =
        {
            "male",
            "female",
            "nonbinary"
        };
        private FootballSimulator game;
        private CoachProfile draft = new CoachProfile();
        private CoachAvatarPreview preview;
        private RectTransform canvasRoot, content;
        private Font font;
        private Text error;
        private int step, leagueIndex;
        private string clubId;
        private Texture2D gradient;
        private bool closing;
        public void Initialize(FootballSimulator owner)
        {
            game = owner;
            draft = game.Career.Coach?.Copy() ?? new CoachProfile();
            clubId = game.Career.ManagedClub ?? game.Career.Data.leagues[0].clubs[0];
            if (!FindFirstObjectByType<EventSystem>())
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            canvasRoot = new GameObject("Career creation · Coach and club", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)).GetComponent<RectTransform>();
            canvasRoot.SetParent(transform, false);
            var canvas = canvasRoot.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = .5f;
            var raw = RawPanel(canvasRoot, 0, 0, 1, 1);
            gradient = new Texture2D(128, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 128; x++)
                {
                    float glow = Mathf.Clamp01((x / 127f) * .85f + (y / 63f) * .15f);
                    gradient.SetPixel(x, y, Color.Lerp(Navy, new Color(.02f, .32f, .28f), glow));
                }

            gradient.Apply();
            raw.texture = gradient;
            raw.raycastTarget = false;
            content = Box(canvasRoot, 0, 0, 1, 1, Color.clear);
            content.gameObject.AddComponent<SafeAreaPanel>();
            preview = gameObject.AddComponent<CoachAvatarPreview>();
            preview.Initialize(game);
            game.Screen = "onboarding";
            Render();
        }

        RectTransform Box(Transform parent, float x, float y, float right, float top, Color color)
        {
            var rect = new GameObject("Panel", typeof(RectTransform), typeof(Image)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x, y);
            rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image = rect.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }

        RawImage RawPanel(Transform parent, float x, float y, float right, float top)
        {
            var rect = new GameObject("Preview", typeof(RectTransform), typeof(RawImage)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x, y);
            rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var raw = rect.GetComponent<RawImage>();
            raw.raycastTarget = false;
            return raw;
        }

        Text Label(Transform parent, string text, float x, float y, float right, float top, int size = 28, Color? color = null, TextAnchor align = TextAnchor.MiddleLeft)
        {
            var rect = new GameObject("Text", typeof(RectTransform), typeof(Text)).GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(x, y);
            rect.anchorMax = new Vector2(right, top);
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var label = rect.GetComponent<Text>();
            label.font = font;
            label.text = text;
            label.fontSize = size;
            label.color = color ?? Color.white;
            label.alignment = align;
            label.raycastTarget = false;
            label.supportRichText = false;
            return label;
        }

        Button Action(Transform parent, string caption, float x, float y, float right, float top, UnityAction clicked, bool selected = false)
        {
            var rect = Box(parent, x, y, right, top, selected ? new Color(.12f, .38f, .34f) : new Color(.13f, .21f, .29f));
            rect.name = caption;
            rect.GetComponent<Image>().raycastTarget = true;
            var button = rect.gameObject.AddComponent<Button>();
            button.targetGraphic = rect.GetComponent<Image>();
            var colors = button.colors;
            colors.highlightedColor = new Color(.75f, 1, .92f);
            colors.pressedColor = new Color(.4f, .8f, .68f);
            button.colors = colors;
            button.onClick.AddListener(clicked);
            if (selected)
            {
                var outline = rect.gameObject.AddComponent<Outline>();
                outline.effectColor = Accent;
                outline.effectDistance = new Vector2(2, -2);
            }

            Label(rect, caption, .03f, .03f, .97f, .97f, 25, selected ? Accent : Color.white, TextAnchor.MiddleCenter);
            return button;
        }

        void Render()
        {
            foreach (Transform child in content)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            Label(content, "FOOTBALL DYNASTY 27", .04f, .935f, .8f, .985f, 25, Accent);
            Label(content, new[] { "Elige a tu entrenador", "Define tu estilo", "Tu historia comienza aquí", "Elige tu próximo equipo" }[step], .04f, .85f, .95f, .935f, 46);
            for (int i = 0; i < 4; i++)
                Label(content, Steps[i], .04f + i * .235f, .79f, .26f + i * .235f, .84f, 23, i == step ? Accent : Muted);
            var left = Box(content, .04f, .15f, .61f, .76f, Panel);
            if (step == 0)
                BuildAvatars(left);
            else if (step == 1)
                BuildOutfit(left);
            else if (step == 2)
                BuildInfo(left);
            else
                BuildClubs(left);
            if (step < 3)
            {
                var panel = Box(content, .64f, .15f, .96f, .76f, new Color(.025f, .18f, .2f, .9f));
                var frame = Box(panel, .02f, .10f, .98f, .98f, Color.clear);
                var image = RawPanel(frame, 0, 0, 1, 1);
                var aspect = image.gameObject.AddComponent<AspectRatioFitter>();
                aspect.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                aspect.aspectRatio = 2f / 3f;
                preview.Show(image, draft);
                Label(panel, AvatarNames[draft.avatar] + " · " + new[] { "Deportivo", "Americana", "Chaleco" }[draft.outfit], .06f, .015f, .94f, .09f, 25, Accent, TextAnchor.MiddleCenter);
            }
            else
            {
                preview.Hide();
                BuildClubDetails();
            }

            error = Label(content, "", .22f, .025f, .76f, .12f, 23, new Color(1, .77f, .45f));
            if (step > 0)
                Action(content, "‹  Atrás", .04f, .035f, .18f, .105f, () =>
                {
                    step--;
                    Render();
                });
            Action(content, step == 3 ? "Comenzar carrera  ›" : "Continuar  ›", .78f, .035f, .96f, .105f, Next, true);
        }

        void Next()
        {
            if (step == 2)
            {
                var problem = draft.Validate(game.Career.Season);
                if (problem != null)
                {
                    error.text = problem;
                    return;
                }
            }

            if (step < 3)
            {
                step++;
                Render();
                return;
            }

            try
            {
                if (!game.FinishManagerSetup(draft, clubId))
                {
                    error.text = game.Status;
                    return;
                }

                closing = true;
                preview.Hide();
                canvasRoot.gameObject.SetActive(false);
                Destroy(canvasRoot.gameObject);
                Destroy(preview);
                Destroy(this);
            }
            catch (Exception e)
            {
                error.text = e.Message;
            }
        }

        void BuildAvatars(RectTransform parent)
        {
            Label(parent, "TRES PERSONAJES ORIGINALES", .04f, .86f, .96f, .97f, 27, Muted);
            for (int i = 0; i < 3; i++)
            {
                int selected = i;
                float x = .035f + i * .32f;
                var button = Action(parent, "", x, .18f, x + .29f, .81f, () =>
                {
                    draft.avatar = selected;
                    Render();
                }, draft.avatar == i);
                var body = Box(button.transform, .18f, .17f, .82f, .52f, CoachAvatarPreview.ShirtColors[i]);
                Box(button.transform, .34f, .49f, .66f, .79f, i == 0 ? new Color(.66f, .43f, .29f) : i == 1 ? new Color(.39f, .24f, .16f) : new Color(.78f, .58f, .42f));
                Box(button.transform, .33f, .75f, .67f, .84f, i == 2 ? new Color(.28f, .13f, .08f) : new Color(.08f, .07f, .06f));
                Label(button.transform, AvatarNames[i], .02f, .02f, .98f, .15f, 29, Color.white, TextAnchor.MiddleCenter);
            }

            Label(parent, "El personaje es independiente del nombre y género que elijas después.", .04f, .025f, .96f, .14f, 24, Muted);
        }

        void BuildOutfit(RectTransform parent)
        {
            Label(parent, "ATUENDO", .04f, .85f, .96f, .97f, 27, Muted);
            for (int i = 0; i < 3; i++)
            {
                int value = i;
                Action(parent, new[] { "Deportivo", "Americana", "Chaleco" }[i], .04f + i * .31f, .70f, .32f + i * .31f, .83f, () =>
                {
                    draft.outfit = value;
                    Render();
                }, draft.outfit == i);
            }

            Label(parent, "Color superior", .04f, .57f, .40f, .68f, 25);
            for (int i = 0; i < 4; i++)
            {
                int value = i;
                var b = Action(parent, (i + 1).ToString(), .43f + i * .135f, .58f, .54f + i * .135f, .68f, () =>
                {
                    draft.shirtColor = value;
                    Render();
                }, draft.shirtColor == i);
                b.GetComponent<Image>().color = CoachAvatarPreview.ShirtColors[i];
            }

            Label(parent, "Pantalón", .04f, .42f, .38f, .54f, 25);
            for (int i = 0; i < 3; i++)
            {
                int value = i;
                Action(parent, new[] { "Oscuro", "Gris", "Arena" }[i], .40f + i * .19f, .43f, .57f + i * .19f, .54f, () =>
                {
                    draft.trousersColor = value;
                    Render();
                }, draft.trousersColor == i);
            }

            Label(parent, "Calzado", .04f, .27f, .38f, .39f, 25);
            for (int i = 0; i < 2; i++)
            {
                int value = i;
                Action(parent, i == 0 ? "Negro" : "Blanco", .40f + i * .285f, .28f, .66f + i * .285f, .39f, () =>
                {
                    draft.shoes = value;
                    Render();
                }, draft.shoes == i);
            }

            Action(parent, "Gafas: " + (draft.glasses ? "Sí" : "No"), .04f, .07f, .47f, .21f, () =>
            {
                draft.glasses = !draft.glasses;
                Render();
            }, draft.glasses);
            Action(parent, "Corbata: " + (draft.tie ? "Sí" : "No"), .52f, .07f, .95f, .21f, () =>
            {
                draft.tie = !draft.tie;
                Render();
            }, draft.tie);
        }

        void Input(Transform parent, string caption, string value, float y, UnityAction<string> changed, int limit = 40, float x = .40f, float right = .95f)
        {
            Label(parent, caption, .04f, y, .38f, y + .10f, 25, Muted);
            var rect = Box(parent, x, y, right, y + .10f, new Color(.035f, .09f, .16f));
            rect.GetComponent<Image>().raycastTarget = true;
            var text = Label(rect, value, .04f, 0, .96f, 1, 27);
            var field = rect.gameObject.AddComponent<InputField>();
            field.textComponent = text;
            field.targetGraphic = rect.GetComponent<Image>();
            field.lineType = InputField.LineType.SingleLine;
            field.characterLimit = limit;
            field.text = value;
            field.onValueChanged.AddListener(changed);
        }

        void BuildInfo(RectTransform parent)
        {
            Label(parent, "INFORMACIÓN DEL ENTRENADOR", .04f, .87f, .96f, .98f, 26, Muted);
            Action(parent, "Género: " + GenderNames[Math.Max(0, Array.IndexOf(GenderCodes, draft.gender))], .04f, .73f, .95f, .85f, () =>
            {
                draft.gender = GenderCodes[(Array.IndexOf(GenderCodes, draft.gender) + 1) % 3];
                Render();
            });
            Input(parent, "Nombre", draft.firstName, .59f, v => draft.firstName = v, 32);
            Input(parent, "Apellido", draft.lastName, .46f, v => draft.lastName = v, 40);
            Input(parent, "Apelativo", draft.nickname, .33f, v => draft.nickname = v, 24);
            Input(parent, "País / región", draft.country, .20f, v => draft.country = v, 56);
            Label(parent, "Nacimiento", .04f, .05f, .38f, .16f, 25, Muted);
            DateField(parent, draft.birthDay, .40f, .55f, v => draft.birthDay = v, 2, "Día");
            DateField(parent, draft.birthMonth, .59f, .74f, v => draft.birthMonth = v, 2, "Mes");
            DateField(parent, draft.birthYear, .78f, .95f, v => draft.birthYear = v, 4, "Año");
        }

        void DateField(Transform parent, int value, float x, float right, System.Action<int> changed, int limit, string title)
        {
            var rect = Box(parent, x, .05f, right, .15f, new Color(.035f, .09f, .16f));
            rect.GetComponent<Image>().raycastTarget = true;
            var text = Label(rect, value.ToString(), .06f, 0, .94f, 1, 27);
            var field = rect.gameObject.AddComponent<InputField>();
            field.textComponent = text;
            field.targetGraphic = rect.GetComponent<Image>();
            field.contentType = InputField.ContentType.IntegerNumber;
            field.characterLimit = limit;
            field.text = value.ToString();
            field.onValueChanged.AddListener(v => changed(int.TryParse(v, out var number) ? number : 0));
            Label(parent, title, x, .005f, right, .045f, 18, Muted, TextAnchor.MiddleCenter);
        }

        void BuildClubs(RectTransform parent)
        {
            var leagues = game.Career.Data.leagues;
            var league = leagues[leagueIndex];
            Action(parent, "‹", .04f, .82f, .12f, .96f, () => ChangeLeague(-1));
            Label(parent, league.name, .15f, .82f, .85f, .96f, 30, Color.white, TextAnchor.MiddleCenter);
            Action(parent, "›", .88f, .82f, .96f, .96f, () => ChangeLeague(1));
            Label(parent, "LIGA " + (leagueIndex + 1) + " / " + leagues.Length + " · ELIGE UN CLUB", .04f, .73f, .96f, .81f, 22, Muted);
            for (int i = 0; i < league.clubs.Length; i++)
            {
                string id = league.clubs[i];
                int row = i / 2, column = i % 2;
                float y = .66f - row * .064f;
                Action(parent, game.Career.Clubs[id].name, .04f + column * .48f, y, .49f + column * .48f, y + .057f, () =>
                {
                    clubId = id;
                    Render();
                }, clubId == id);
            }
        }

        void ChangeLeague(int delta)
        {
            leagueIndex = (leagueIndex + delta + game.Career.Data.leagues.Length) % game.Career.Data.leagues.Length;
            clubId = game.Career.Data.leagues[leagueIndex].clubs[0];
            Render();
        }

        void BuildClubDetails()
        {
            var club = game.Career.Clubs[clubId];
            var league = game.Career.Data.leagues[leagueIndex];
            var panel = Box(content, .64f, .15f, .96f, .76f, Panel);
            ColorUtility.TryParseHtmlString(club.color, out var color);
            Label(panel, club.name, .05f, .85f, .95f, .98f, 35);
            Label(panel, club.country + " · " + league.name, .05f, .77f, .95f, .85f, 22, Muted);
            var badge = Box(panel, .06f, .55f, .36f, .75f, color);
            Label(badge, club.shortCode ?? club.name.Substring(0, Math.Min(3, club.name.Length)), 0, 0, 1, 1, 38, Color.white, TextAnchor.MiddleCenter);
            var shirt = Box(panel, .55f, .54f, .84f, .74f, color);
            Box(panel, .47f, .64f, .55f, .74f, color);
            Box(panel, .84f, .64f, .92f, .74f, color);
            Label(shirt, "LOCAL", 0, 0, 1, 1, 18, Color.white, TextAnchor.MiddleCenter);
            var squad = game.Career.Squad(clubId);
            Label(panel, $"€ {club.budget / 1000000:F1} M   ·   {squad.Length} jugadores", .05f, .44f, .95f, .53f, 25, Accent);
            Label(panel, "Valoración de plantilla: " + Math.Round(squad.Average(p => p.rating)) + " / 100 · valores del juego", .05f, .37f, .95f, .44f, 20, Muted);
            bool known = ClubIdentity.Histories.TryGetValue(club.id, out var history);
            Label(panel, known ? history.City + " · Fundado en " + history.Founded : "IDENTIDAD DEL CLUB", .05f, .29f, .95f, .37f, 24);
            Label(panel, ClubIdentity.Story(club, league), .05f, .035f, .95f, .29f, 23, Muted, TextAnchor.UpperLeft);
        }

        void OnDestroy()
        {
            if (gradient)
                Destroy(gradient);
            if (!closing)
            {
                if (canvasRoot)
                    Destroy(canvasRoot.gameObject);
                if (preview)
                    Destroy(preview);
            }
        }
    }
}
