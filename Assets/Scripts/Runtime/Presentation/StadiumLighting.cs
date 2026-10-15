using UnityEngine;
using UnityEngine.Rendering;

namespace FootballDynasty
{
    public sealed class StadiumLighting : MonoBehaviour
    {
        public void Configure(Light sun)
        {
            sun.type = LightType.Directional;
            sun.color = new Color(1, .96f, .88f);
            sun.intensity = 1.15f;
            sun.shadows = LightShadows.Soft;
            sun.shadowStrength = .8f;
            sun.shadowBias = .03f;
            sun.shadowNormalBias = .25f;
            sun.transform.rotation = Quaternion.Euler(48, -28, 0);
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(.42f, .53f, .66f);
            RenderSettings.ambientEquatorColor = new Color(.24f, .30f, .34f);
            RenderSettings.ambientGroundColor = new Color(.12f, .17f, .1f);
            RenderSettings.ambientIntensity = 1;
        // Broad stadium geometry should use baked lightmaps/probes once authored assets exist.
        }
    }
}
