using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
namespace FootballDynasty {
 public sealed class MatchReplay:MonoBehaviour {
  public bool IsPlaying=>playing!=null;
  private FootballSimulator game;private readonly ReplayBuffer buffer=new ReplayBuffer();
  private ReplayClip playing,lastGoal;private Transform ghostRoot,ghostBall;private readonly List<ReplayFigure> figures=new List<ReplayFigure>();
  private readonly Dictionary<Renderer,bool> hidden=new Dictionary<Renderer,bool>();
  private float playhead,speed=1,priorTimeScale;private bool paused;private int angle;private string message="";
  private Vector3 cameraPosition;private Quaternion cameraRotation;private float cameraFov;
  private Vector2 galleryScroll;private string[] files=Array.Empty<string>();
  private string Folder=>Path.Combine(Application.persistentDataPath,"replays");
  public void Initialize(FootballSimulator owner){game=owner;Refresh();}
  public void NewMatch(){Stop();buffer.Clear();lastGoal=null;}
  public void Record(){if(IsPlaying||!game.Ball||game.Agents.Count!=22)return;buffer.Add(new ReplayFrame{time=game.Clock,homeGoals=game.HomeGoals,awayGoals=game.AwayGoals,ball=Pose(game.Ball),players=game.Agents.Select(a=>{var p=Pose(a.Body);p.leftLeg=a.LeftLegAngle;p.rightLeg=a.RightLegAngle;return p;}).ToArray()});}
  static ReplayPose Pose(Rigidbody t){var p=t.position;var q=t.rotation;return new ReplayPose{x=p.x,y=p.y,z=p.z,qx=q.x,qy=q.y,qz=q.z,qw=q.w};}
  ReplayClip Latest(string title){return buffer.Export(title,game.Career.Clubs[game.ActiveFixture.home].name,game.Career.Clubs[game.ActiveFixture.away].name,game.Agents.Select(a=>new ReplayActor{id=a.Data.id,name=a.Data.name,position=a.Data.position,team=a.Team,color=game.Career.Clubs[a.Data.club].color}).ToArray());}
  public void Goal(){if(buffer.Count>=2)lastGoal=Latest("Gol · "+game.HomeGoals+"–"+game.AwayGoals);}
  void TryLatest(){try{Begin(Latest("Última jugada"));}catch(Exception e){game.Status=e.Message;}}
  public void MatchButtons(){GUILayout.BeginHorizontal();GUI.enabled=buffer.Count>=2;if(GUILayout.Button("Ver últimos 10 segundos (R)"))TryLatest();GUI.enabled=lastGoal!=null;if(GUILayout.Button("Repetir último gol"))Begin(lastGoal);GUI.enabled=true;GUILayout.EndHorizontal();GUILayout.Label(game.Status);}
  void Refresh(){try{files=Directory.Exists(Folder)?Directory.GetFiles(Folder,"*.fdreplay.json").OrderByDescending(f=>Path.GetFileName(f)).ToArray():Array.Empty<string>();}catch(Exception e){message=e.Message;}}
  public void OpenGallery(){game.Screen="replays";Refresh();}
  void Begin(ReplayClip clip){
   ReplayMath.Validate(clip);if(IsPlaying)Stop();playing=clip;playhead=0;paused=false;message="";priorTimeScale=Time.timeScale;Time.timeScale=0;
   cameraPosition=game.MatchCamera.transform.position;cameraRotation=game.MatchCamera.transform.rotation;cameraFov=game.MatchCamera.fieldOfView;if(game.Broadcast)game.Broadcast.SuspendForReplay(true);
   hidden.Clear();foreach(var a in game.Agents)foreach(var r in a.GetComponentsInChildren<Renderer>()){hidden[r]=r.enabled;r.enabled=false;}
   if(game.Ball)foreach(var r in game.Ball.GetComponentsInChildren<Renderer>()){hidden[r]=r.enabled;r.enabled=false;}
   ghostRoot=new GameObject("Repetición · actores sin física").transform;
   try{foreach(var actor in clip.actors){ColorUtility.TryParseHtmlString(actor.color,out var color);figures.Add(new ReplayFigure(ghostRoot,color,game));}
   ghostBall=Primitive(ghostRoot,PrimitiveType.Sphere,Vector3.zero,Vector3.one*.22f,Color.white,game);Destroy(ghostBall.GetComponent<Renderer>().sharedMaterial);ghostBall.GetComponent<Renderer>().sharedMaterial=FootballAppearance.BallMaterial(game);ApplyFrame();}catch{Stop();throw;}
  }
  public void Stop(){if(!IsPlaying)return;foreach(var item in hidden)if(item.Key)item.Key.enabled=item.Value;hidden.Clear();if(ghostRoot){ghostRoot.gameObject.SetActive(false);if(ghostBall){var ballMaterial=ghostBall.GetComponent<Renderer>().sharedMaterial;if(ballMaterial.HasProperty("_BaseMap"))Destroy(ballMaterial.GetTexture("_BaseMap"));}foreach(var material in ghostRoot.GetComponentsInChildren<Renderer>().Select(r=>r.sharedMaterial).Distinct())Destroy(material);Destroy(ghostRoot.gameObject);}figures.Clear();playing=null;Time.timeScale=priorTimeScale;game.MatchCamera.transform.SetPositionAndRotation(cameraPosition,cameraRotation);game.MatchCamera.fieldOfView=cameraFov;if(game.Broadcast)game.Broadcast.SuspendForReplay(false);}
  void OnDisable(){if(game)Stop();}
  void Update(){if(IsPlaying){if(Input.GetKeyDown(KeyCode.Escape)){Stop();return;}if(!paused)playhead=Mathf.Min(playing.Duration,playhead+Time.unscaledDeltaTime*speed);if(playhead>=playing.Duration)paused=true;ApplyFrame();}else if(game.Screen=="match"&&Input.GetKeyDown(KeyCode.R))TryLatest();}
  static void ApplyPose(Transform t,ReplayPose a,ReplayPose b,float mix){t.position=Vector3.Lerp(new Vector3(a.x,a.y,a.z),new Vector3(b.x,b.y,b.z),mix);t.rotation=Quaternion.Slerp(new Quaternion(a.qx,a.qy,a.qz,a.qw).normalized,new Quaternion(b.qx,b.qy,b.qz,b.qw).normalized,mix);}
  void ApplyFrame(){ReplayMath.Sample(playing,playhead,out var a,out var b,out var mix);ApplyPose(ghostBall,a.ball,b.ball,mix);for(int i=0;i<figures.Count;i++){ApplyPose(figures[i].Root,a.players[i],b.players[i],mix);figures[i].Legs[0].localRotation=Quaternion.Euler(Mathf.LerpAngle(a.players[i].leftLeg,b.players[i].leftLeg,mix),0,0);figures[i].Legs[1].localRotation=Quaternion.Euler(Mathf.LerpAngle(a.players[i].rightLeg,b.players[i].rightLeg,mix),0,0);}}
  void LateUpdate(){if(!IsPlaying)return;var target=ghostBall.position;Vector3 offset=angle==0?new Vector3(0,20,30):angle==1?new Vector3(target.x>=0?14:-14,6,10):new Vector3(target.x>=0?62:-62,9,0)-target;game.MatchCamera.transform.position=target+offset;game.MatchCamera.transform.LookAt(target);game.MatchCamera.fieldOfView=angle==0?50:60;}
  void SaveClip(){try{ReplayMath.Validate(playing);Directory.CreateDirectory(Folder);string label=new string((playing.title+"-"+playing.home+"-"+playing.away).Where(c=>char.IsLetterOrDigit(c)||c=='-').Take(80).ToArray());string file=Path.Combine(Folder,DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-fff")+"-"+label+"-"+Guid.NewGuid().ToString("N").Substring(0,8)+".fdreplay.json");File.WriteAllText(file+".tmp",JsonUtility.ToJson(playing));File.Move(file+".tmp",file);message="Repetición guardada. Disponible en Repeticiones guardadas.";Refresh();}catch(Exception e){message="No se pudo guardar: "+e.Message;}}
  void LoadClip(string file){try{var info=new FileInfo(file);if(info.Length>12*1024*1024)throw new ArgumentException("Archivo demasiado grande.");var clip=JsonUtility.FromJson<ReplayClip>(File.ReadAllText(file));Begin(clip);}catch(Exception e){message="No se pudo abrir la repetición: "+e.Message;}}
  void OnGUI(){if(!game||(!IsPlaying&&game.Screen!="replays"))return;var matrix=GUI.matrix;GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(UnityEngine.Screen.width/1280f,UnityEngine.Screen.height/720f,1));
   if(IsPlaying){GUILayout.BeginArea(new Rect(15,12,1250,200),GUI.skin.box);GUILayout.Label("REPETICIÓN · "+playing.title+" · "+playing.home+" vs "+playing.away);ReplayMath.Sample(playing,playhead,out var a,out var b,out var mix);var displayed=mix>=1?b:a;GUILayout.Label($"{displayed.homeGoals} : {displayed.awayGoals} · {playhead:F1} / {playing.Duration:F1} s");float seek=GUILayout.HorizontalSlider(playhead,0,playing.Duration);if(Mathf.Abs(seek-playhead)>.001f){playhead=seek;ApplyFrame();}GUILayout.BeginHorizontal();if(GUILayout.Button(paused?"Reproducir":"Pausar")){if(playhead>=playing.Duration)playhead=0;paused=!paused;}if(GUILayout.Button("Desde el inicio")){playhead=0;paused=false;}foreach(float rate in new[]{.25f,.5f,1f,2f})if(GUILayout.Button(rate+"×"))speed=rate;foreach(string label in new[]{"Televisión","Cerca del balón","Desde portería"})if(GUILayout.Button(label))angle=Array.IndexOf(new[]{"Televisión","Cerca del balón","Desde portería"},label);if(GUILayout.Button("Guardar repetición"))SaveClip();if(GUILayout.Button("Volver (Esc)"))Stop();GUILayout.EndHorizontal();GUILayout.Label(message);GUILayout.EndArea();}
   else{GUILayout.BeginArea(new Rect(15,12,1250,695),GUI.skin.box);GUILayout.Label("REPETICIONES GUARDADAS · disponibles después de cerrar el juego");if(GUILayout.Button("Volver a carrera"))game.Screen="career";GUILayout.Label(message);galleryScroll=GUILayout.BeginScrollView(galleryScroll);foreach(var file in files){GUILayout.BeginHorizontal();GUILayout.Label(Path.GetFileName(file));if(GUILayout.Button("Ver",GUILayout.Width(100)))LoadClip(file);GUILayout.EndHorizontal();}if(files.Length==0)GUILayout.Label("Aún no tienes repeticiones. Abre una jugada o un gol y pulsa Guardar repetición.");GUILayout.EndScrollView();GUILayout.EndArea();}GUI.matrix=matrix;
  }
  static Transform Primitive(Transform parent,PrimitiveType type,Vector3 position,Vector3 scale,Color color,FootballSimulator game){var obj=GameObject.CreatePrimitive(type);obj.transform.SetParent(parent,false);obj.transform.localPosition=position;obj.transform.localScale=scale;obj.GetComponent<Renderer>().sharedMaterial=game.Material(color);var collider=obj.GetComponent<Collider>();collider.enabled=false;Destroy(collider);return obj.transform;}
  sealed class ReplayFigure {
   public readonly Transform Root;public readonly Transform[] Legs=new Transform[2];
   public ReplayFigure(Transform parent,Color kit,FootballSimulator game){Root=new GameObject("Jugador de repetición").transform;Root.SetParent(parent);Primitive(Root,PrimitiveType.Capsule,new Vector3(0,.26f,0),new Vector3(.55f,.45f,.4f),kit,game);Primitive(Root,PrimitiveType.Sphere,new Vector3(0,.84f,0),Vector3.one*.36f,new Color(.57f,.38f,.25f),game);Primitive(Root,PrimitiveType.Sphere,new Vector3(0,.98f,0),new Vector3(.37f,.15f,.36f),new Color(.11f,.08f,.07f),game);for(int i=0;i<2;i++){float side=i==0?-1:1;Legs[i]=Primitive(Root,PrimitiveType.Capsule,new Vector3(side*.15f,-.48f,0),new Vector3(.17f,.3f,.17f),Color.white,game);Primitive(Legs[i],PrimitiveType.Cube,new Vector3(0,-1.1f,.3f),new Vector3(1.12f,.43f,2.06f),new Color(.12f,.13f,.14f),game);Primitive(Root,PrimitiveType.Capsule,new Vector3(side*.38f,.22f,0),new Vector3(.14f,.32f,.14f),kit,game);}}
  }
 }
}
