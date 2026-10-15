using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FootballDynasty
{
    [DefaultExecutionOrder(-200)]
    public sealed class QualityManager : MonoBehaviour
    {
        [SerializeField]
        private UniversalRenderPipelineAsset pcHigh;
        [SerializeField]
        private UniversalRenderPipelineAsset mobile60;
        private UniversalRenderPipelineAsset runtimePipeline;
        private RenderPipelineAsset previousQuality;
        private float elapsed;
        private int samples, slowWindows, fastWindows;
        private float warmup = 5;
        public bool IsMobile { get; private set; }

        void Awake()
        {
            IsMobile = Application.isMobilePlatform;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            QualitySettings.anisotropicFiltering = IsMobile ? AnisotropicFiltering.Enable : AnisotropicFiltering.ForceEnable;
            QualitySettings.lodBias = IsMobile ? .75f : 1.5f;
            if (!pcHigh)
                pcHigh = Resources.Load<UniversalRenderPipelineAsset>("Graphics/PC_High");
            if (!mobile60)
                mobile60 = Resources.Load<UniversalRenderPipelineAsset>("Graphics/Mobile_60");
            var source = IsMobile ? mobile60 : pcHigh;
            if (!source)
            {
                Debug.LogWarning("Falta el perfil URP. Ejecuta Football Dynasty > Preparar gráficos URP.");
                return;
            }

            // Clone to avoid modifying the project asset or quality profiles in Play Mode.
            previousQuality = QualitySettings.renderPipeline;
            runtimePipeline = Instantiate(source);
            runtimePipeline.name = source.name + " (runtime)";
            QualitySettings.renderPipeline = runtimePipeline;
        }

        void Update()
        {
            if (!IsMobile || !runtimePipeline || Time.timeScale == 0)
                return;
            if (warmup > 0)
            {
                warmup -= Time.unscaledDeltaTime;
                return;
            }

            elapsed += Time.unscaledDeltaTime;
            samples++;
            if (elapsed < 2)
                return;
            float averageMs = elapsed * 1000 / samples;
            elapsed = 0;
            samples = 0;
            if (averageMs > 19)
            {
                slowWindows++;
                fastWindows = 0;
            }
            else if (averageMs < 16.8f)
            {
                fastWindows++;
                slowWindows = 0;
            }
            else
                slowWindows = fastWindows = 0;
            // Sustained samples and slow recovery avoid visible resolution oscillation.
            if (slowWindows >= 2)
            {
                runtimePipeline.renderScale = Mathf.Max(.65f, runtimePipeline.renderScale - .05f);
                slowWindows = 0;
            }

            if (fastWindows >= 5)
            {
                runtimePipeline.renderScale = Mathf.Min(.85f, runtimePipeline.renderScale + .05f);
                fastWindows = 0;
            }
        }

        void OnApplicationPause(bool paused)
        {
            warmup = 5;
            elapsed = 0;
            samples = slowWindows = fastWindows = 0;
        }

        void OnDestroy()
        {
            if (runtimePipeline)
            {
                if (QualitySettings.renderPipeline == runtimePipeline)
                    QualitySettings.renderPipeline = previousQuality;
                Destroy(runtimePipeline);
            }
        }
    }
}
