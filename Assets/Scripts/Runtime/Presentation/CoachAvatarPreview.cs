using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

namespace FootballDynasty
{
    // Original procedural stand-ins. Replace with authored rigged prefabs when available.
    public sealed class CoachAvatarPreview : MonoBehaviour
    {
        public static readonly Color[] ShirtColors =
        {
            new Color(.08f, .15f, .25f),
            new Color(.13f, .36f, .32f),
            new Color(.42f, .16f, .22f),
            new Color(.75f, .76f, .70f)
        };
        public static readonly Color[] TrouserColors =
        {
            new Color(.08f, .10f, .14f),
            new Color(.25f, .27f, .3f),
            new Color(.49f, .40f, .28f)
        };
        private const int PreviewLayer = 30;
        private Transform stage, model;
        private Camera portrait;
        private RenderTexture target;
        private FootballSimulator game;
        private readonly Dictionary<Color, Material> materials = new Dictionary<Color, Material>();
        public void Initialize(FootballSimulator owner)
        {
            game = owner;
            stage = new GameObject("Coach preview stage").transform;
            stage.SetParent(transform);
            stage.position = new Vector3(0, -1000, 0);
            target = new RenderTexture(Application.isMobilePlatform ? 320 : 512, Application.isMobilePlatform ? 480 : 768, 24)
            {
                name = "Coach preview",
                antiAliasing = 2
            };
            target.Create();
            portrait = new GameObject("Coach portrait camera").AddComponent<Camera>();
            portrait.transform.SetParent(stage, false);
            portrait.transform.localPosition = new Vector3(0, .10f, 4.4f);
            portrait.transform.LookAt(stage.position + Vector3.up * .08f);
            portrait.fieldOfView = 31;
            portrait.nearClipPlane = .1f;
            portrait.farClipPlane = 12;
            portrait.cullingMask = 1 << PreviewLayer;
            portrait.clearFlags = CameraClearFlags.SolidColor;
            portrait.backgroundColor = new Color(.035f, .19f, .20f);
            portrait.targetTexture = target;
            portrait.allowHDR = false;
            portrait.GetUniversalAdditionalCameraData().renderPostProcessing = false;
            game.MatchCamera.cullingMask &= ~(1 << PreviewLayer);
            foreach (var light in FindObjectsByType<Light>(FindObjectsSortMode.None))
                light.cullingMask &= ~(1 << PreviewLayer);
            var key = new GameObject("Portrait key").AddComponent<Light>();
            key.transform.SetParent(stage, false);
            key.type = LightType.Directional;
            key.transform.localRotation = Quaternion.Euler(25, 155, 0);
            key.intensity = 1.2f;
            key.cullingMask = 1 << PreviewLayer;
        }

        public void Show(RawImage image, CoachProfile profile)
        {
            stage.gameObject.SetActive(true);
            image.texture = target;
            if (model)
            {
                model.gameObject.SetActive(false);
                Destroy(model.gameObject);
            }

            model = new GameObject("Selected original coach").transform;
            model.SetParent(stage, false);
            model.localRotation = Quaternion.Euler(0, -12, 0);
            var skin = profile.avatar == 0 ? new Color(.66f, .43f, .29f) : profile.avatar == 1 ? new Color(.39f, .24f, .16f) : new Color(.78f, .58f, .42f);
            var hair = profile.avatar == 0 ? new Color(.16f, .12f, .1f) : profile.avatar == 1 ? new Color(.04f, .035f, .03f) : new Color(.22f, .10f, .06f);
            var cloth = ShirtColors[profile.shirtColor];
            var pants = TrouserColors[profile.trousersColor];
            Part(PrimitiveType.Cube, new Vector3(0, -.19f, 0), new Vector3(.56f, .20f, .3f), pants);
            Part(PrimitiveType.Cube, new Vector3(0, .18f, 0), new Vector3(.58f, .68f, .32f), profile.outfit == 2 ? Color.white : cloth);
            Part(PrimitiveType.Capsule, new Vector3(0, .61f, 0), new Vector3(.18f, .12f, .18f), skin);
            Part(PrimitiveType.Sphere, new Vector3(0, .88f, 0), new Vector3(.4f, .48f, .4f), skin);
            Part(PrimitiveType.Sphere, new Vector3(0, 1.08f, 0), new Vector3(.42f, profile.avatar == 1 ? .1f : .23f, .42f), hair);
            if (profile.avatar == 2)
                Part(PrimitiveType.Sphere, new Vector3(0, 1.06f, -.22f), Vector3.one * .25f, hair);
            if (profile.avatar == 0)
                Part(PrimitiveType.Cube, new Vector3(0, .72f, .16f), new Vector3(.24f, .08f, .05f), hair);
            foreach (int side in new[]
            {
                -1,
                1
            }

            )
            {
                Part(PrimitiveType.Capsule, new Vector3(side * .37f, .14f, 0), new Vector3(.16f, .34f, .18f), profile.outfit == 2 ? Color.white : cloth);
                Part(PrimitiveType.Sphere, new Vector3(side * .38f, -.26f, 0), new Vector3(.14f, .2f, .14f), skin);
                Part(PrimitiveType.Capsule, new Vector3(side * .16f, -.57f, 0), new Vector3(.23f, .33f, .25f), pants);
                Part(PrimitiveType.Cube, new Vector3(side * .16f, -.96f, .07f), new Vector3(.25f, .14f, .41f), profile.shoes == 0 ? new Color(.035f, .045f, .06f) : new Color(.85f, .87f, .85f));
                Part(PrimitiveType.Sphere, new Vector3(side * .09f, .91f, .18f), new Vector3(.035f, .027f, .015f), Color.black);
            }

            if (profile.outfit == 1)
            {
                Part(PrimitiveType.Cube, new Vector3(0, .28f, .17f), new Vector3(.2f, .46f, .03f), Color.white);
            }

            if (profile.outfit == 2)
            {
                Part(PrimitiveType.Cube, new Vector3(0, .17f, .04f), new Vector3(.61f, .62f, .37f), cloth);
            }

            if (profile.tie)
                Part(PrimitiveType.Cube, new Vector3(0, .28f, .24f), new Vector3(.055f, .34f, .035f), new Color(.3f, .07f, .12f));
            if (profile.glasses)
                foreach (int side in new[]
                {
                    -1,
                    1
                }

                )
                {
                    foreach (float dy in new[]
                    {
                        -.045f,
                        .045f
                    }

                    )
                        Part(PrimitiveType.Cube, new Vector3(side * .105f, .91f + dy, .205f), new Vector3(.16f, .02f, .025f), Color.black);
                    foreach (float dx in new[]
                    {
                        -.075f,
                        .075f
                    }

                    )
                        Part(PrimitiveType.Cube, new Vector3(side * .105f + dx, .91f, .205f), new Vector3(.02f, .09f, .025f), Color.black);
                    Part(PrimitiveType.Cube, new Vector3(0, .91f, .205f), new Vector3(.06f, .02f, .025f), Color.black);
                }
        }

        void Part(PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            var part = GameObject.CreatePrimitive(type);
            part.layer = PreviewLayer;
            part.transform.SetParent(model, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            if (!materials.TryGetValue(color, out var material))
            {
                material = game.Material(color);
                materials[color] = material;
            }

            part.GetComponent<Renderer>().sharedMaterial = material;
            var collider = part.GetComponent<Collider>();
            collider.enabled = false;
            Destroy(collider);
        }

        public void Hide()
        {
            if (stage)
                stage.gameObject.SetActive(false);
        }

        void OnDestroy()
        {
            if (stage)
                Destroy(stage.gameObject);
            if (target)
            {
                target.Release();
                Destroy(target);
            }

            foreach (var material in materials.Values)
                Destroy(material);
        }
    }
}
