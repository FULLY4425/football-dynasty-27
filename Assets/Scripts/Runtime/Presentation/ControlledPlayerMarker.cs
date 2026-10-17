using UnityEngine;
namespace FootballDynasty {
 public sealed class ControlledPlayerMarker:MonoBehaviour {
  FootballSimulator game;GameObject root;Transform arrow;Material material;Mesh mesh;GUIStyle label;
  public void Initialize(FootballSimulator owner){game=owner;root=new GameObject("Jugador controlado · indicador");root.transform.SetParent(transform,false);
   var shader=Shader.Find("Universal Render Pipeline/Unlit");if(!shader)shader=Shader.Find("Unlit/Color");material=new Material(shader);material.color=new Color(.1f,1,.72f);
   var ring=root.AddComponent<LineRenderer>();ring.useWorldSpace=false;ring.loop=true;ring.positionCount=40;ring.startWidth=ring.endWidth=.065f;ring.sharedMaterial=material;
   for(int i=0;i<40;i++){float a=i*Mathf.PI*2/40;ring.SetPosition(i,new Vector3(Mathf.Cos(a)*.65f,.07f,Mathf.Sin(a)*.65f));}
   arrow=new GameObject("Flecha").transform;arrow.SetParent(root.transform,false);arrow.localPosition=new Vector3(0,2.5f,0);
   mesh=new Mesh{vertices=new[]{new Vector3(-.2f,.35f,0),new Vector3(.2f,.35f,0),Vector3.zero},triangles=new[]{0,1,2,2,1,0}};mesh.RecalculateNormals();arrow.gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;arrow.gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;root.SetActive(false);
  }
  void LateUpdate(){if(!game||!root)return;bool show=game.Controlled&&game.Screen=="match"&&!game.Replay.IsPlaying&&(!game.LineReview||!game.LineReview.IsPlaying)&&(!game.Var||!game.Var.IsPlaying);root.SetActive(show);if(!show)return;var p=game.Controlled.Body.position;root.transform.position=new Vector3(p.x,0,p.z);arrow.rotation=game.MatchCamera.transform.rotation;}
  void OnGUI(){if(!root||!root.activeSelf)return;if(label==null){label=new GUIStyle(GUI.skin.box){fontSize=18,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft};label.normal.textColor=new Color(.1f,1,.72f);}var old=GUI.matrix;GUI.matrix=Matrix4x4.identity;GUI.Label(new Rect(18,UnityEngine.Screen.height-48,Mathf.Min(470,UnityEngine.Screen.width-36),36),"  CONTROLAS: "+game.Controlled.Data.name,label);GUI.matrix=old;}
  void OnDestroy(){if(root)Destroy(root);if(material)Destroy(material);if(mesh)Destroy(mesh);}
 }
}
