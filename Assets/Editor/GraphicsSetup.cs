using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace FootballDynasty.Editor
{
    public static class GraphicsSetup
    {
        private const string Folder = "Assets/Resources/Graphics";
        [MenuItem("Football Dynasty/Preparar gráficos URP")]
        public static void Prepare()
        {
            Directory.CreateDirectory(Folder);
            Directory.CreateDirectory("Assets/Settings");
            AssetDatabase.Refresh();
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Settings/DynastyRenderer.asset");
            if (!renderer)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, "Assets/Settings/DynastyRenderer.asset");
            }

            var pc = Profile("PC_High", renderer, false);
            Profile("Mobile_60", renderer, true);
            GraphicsSettings.defaultRenderPipeline = pc;
            QualitySettings.renderPipeline = pc;
            if (!AssetDatabase.LoadAssetAtPath<Material>(Folder + "/StadiumTurf.mat"))
                CreateTurf();
            AssetDatabase.SaveAssets();
        }

        static UniversalRenderPipelineAsset Profile(string name, UniversalRendererData renderer, bool mobile)
        {
            string path = Folder + "/" + name + ".asset";
            var asset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(path);
            if (asset)
                return asset;
            asset = UniversalRenderPipelineAsset.Create(renderer);
            asset.name = name;
            asset.renderScale = mobile ? .85f : 1;
            asset.msaaSampleCount = mobile ? 2 : 4;
            asset.supportsHDR = !mobile;
            asset.shadowDistance = mobile ? 80 : 110;
            asset.shadowCascadeCount = mobile ? 2 : 4;
            asset.mainLightShadowmapResolution = mobile ? 2048 : 4096;
            // URP exposes these flags with internal setters. Configure serialized asset fields in Editor only.
            var serialized = new SerializedObject(asset);
            serialized.FindProperty("m_MainLightShadowsSupported").boolValue = true;
            serialized.FindProperty("m_SoftShadowsSupported").boolValue = true;
            serialized.FindProperty("m_SoftShadowQuality").intValue = (int)(mobile ? SoftShadowQuality.Low : SoftShadowQuality.High);
            serialized.FindProperty("m_AdditionalLightsRenderingMode").intValue = (int)LightRenderingMode.Disabled;
            serialized.FindProperty("m_AdditionalLightShadowsSupported").boolValue = false;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            AssetDatabase.CreateAsset(asset, path);
            return asset;
        }

        static void CreateTurf()
        {
            const int size = 512;
            var stripes = new Texture2D(640, 64, TextureFormat.RGBA32, false);
            for (int y = 0; y < 64; y++)
                for (int x = 0; x < 640; x++)
                    stripes.SetPixel(x, y, x / 64 % 2 == 0 ? new Color(.20f, .39f, .13f) : new Color(.15f, .31f, .10f));
            var detail = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var normal = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var heights = new float[size * size];
            var random = new System.Random(2727);
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float grain = (float)random.NextDouble();
                    heights[y * size + x] = Mathf.Sin(x * .8f + y * .13f) * .05f + grain * .12f;
                    float c = .46f + grain * .08f;
                    detail.SetPixel(x, y, new Color(c, c + .01f, c - .01f));
                }

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = heights[y * size + (x + 1) % size] - heights[y * size + (x + size - 1) % size];
                    float dy = heights[((y + 1) % size) * size + x] - heights[((y + size - 1) % size) * size + x];
                    var n = new Vector3(-dx * 2, -dy * 2, 1).normalized;
                    normal.SetPixel(x, y, new Color(n.x * .5f + .5f, n.y * .5f + .5f, n.z * .5f + .5f));
                }

            var baseMap = SaveTexture(stripes, "GrassStripes", false, true);
            var detailMap = SaveTexture(detail, "GrassDetail", false, false);
            var detailNormal = SaveTexture(normal, "GrassDetailNormal", true, false);
            var flat = new Texture2D(4, 4, TextureFormat.RGBA32, false);
            for (int y = 0; y < 4; y++)
                for (int x = 0; x < 4; x++)
                    flat.SetPixel(x, y, new Color(.5f, .5f, 1));
            var flatNormal = SaveTexture(flat, "GrassFlatNormal", true, false);
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit"))
            {
                name = "Stadium Turf"
            };
            material.SetTexture("_BaseMap", baseMap);
            material.SetColor("_BaseColor", Color.white);
            material.SetFloat("_Metallic", 0);
            material.SetFloat("_Smoothness", .12f);
            material.SetTexture("_BumpMap", flatNormal);
            material.SetFloat("_BumpScale", 1);
            material.EnableKeyword("_NORMALMAP");
            material.SetTexture("_DetailAlbedoMap", detailMap);
            material.SetTexture("_DetailNormalMap", detailNormal);
            material.SetFloat("_DetailNormalMapScale", .55f);
            material.SetTextureScale("_DetailAlbedoMap", new Vector2(70, 45.3333f));
            material.EnableKeyword("_DETAIL_MULX2");
            material.SetFloat("_ReceiveShadows", 1);
            material.enableInstancing = true;
            AssetDatabase.CreateAsset(material, Folder + "/StadiumTurf.mat");
        }

        static Texture2D SaveTexture(Texture2D texture, string name, bool normal, bool srgb)
        {
            texture.Apply();
            string path = Folder + "/" + name + ".png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(path);
            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = normal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            importer.sRGBTexture = srgb;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Trilinear;
            importer.mipmapEnabled = true;
            importer.anisoLevel = 8;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.maxTextureSize = 1024;
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "Android", overridden = true, maxTextureSize = 512, format = TextureImporterFormat.ASTC_6x6 });
            importer.SetPlatformTextureSettings(new TextureImporterPlatformSettings { name = "iPhone", overridden = true, maxTextureSize = 512, format = TextureImporterFormat.ASTC_6x6 });
            importer.SaveAndReimport();
            return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        }
    }
}
