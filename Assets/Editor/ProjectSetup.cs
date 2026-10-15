using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace FootballDynasty.Editor {
 [InitializeOnLoad] public static class ProjectSetup {
  static ProjectSetup(){EditorApplication.delayCall+=EnsureScene;}
  [MenuItem("Football Dynasty/Crear escena de simulación")]
  public static void EnsureScene(){if(EditorApplication.isPlayingOrWillChangePlaymode||EditorApplication.isCompiling)return;System.IO.Directory.CreateDirectory("Assets/Scenes");System.IO.Directory.CreateDirectory("Assets/Settings");if(!AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>("Assets/Settings/DynastyURP.asset")){var renderer=ScriptableObject.CreateInstance<UniversalRendererData>();AssetDatabase.CreateAsset(renderer,"Assets/Settings/DynastyRenderer.asset");var pipeline=UniversalRenderPipelineAsset.Create(renderer);pipeline.name="Dynasty URP";pipeline.renderScale=1;pipeline.msaaSampleCount=4;AssetDatabase.CreateAsset(pipeline,"Assets/Settings/DynastyURP.asset");GraphicsSettings.defaultRenderPipeline=pipeline;QualitySettings.renderPipeline=pipeline;AssetDatabase.SaveAssets();}GraphicsSetup.Prepare();if(!AssetDatabase.LoadAssetAtPath<Material>("Assets/Resources/DynastyLit.mat")){var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.name="Dynasty Lit";AssetDatabase.CreateAsset(material,"Assets/Resources/DynastyLit.mat");AssetDatabase.SaveAssets();}if(!System.IO.File.Exists("Assets/Scenes/Football.unity")){var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);new GameObject("Football Dynasty · Simulador").AddComponent<FootballSimulator>();EditorSceneManager.SaveScene(scene,"Assets/Scenes/Football.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Football.unity",true)};AssetDatabase.SaveAssets();}}
  [MenuItem("Football Dynasty/Exportar Windows")]
  public static void BuildWindows(){EnsureScene();System.IO.Directory.CreateDirectory("Builds/Windows");var report=BuildPipeline.BuildPlayer(new[]{"Assets/Scenes/Football.unity"},"Builds/Windows/FootballDynasty27.exe",BuildTarget.StandaloneWindows64,BuildOptions.None);if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new System.Exception("La exportación falló.");}
 }
}
