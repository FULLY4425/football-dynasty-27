using UnityEngine;
namespace FootballDynasty {
 public sealed class MatchVar:MonoBehaviour {
  FootballSimulator game;NeutralReferee official;VarIncident incident,reviewed;float timer,priorScale,fov,walk;Vector3 cameraAt;Quaternion cameraRotation;GameObject station;Material screenMaterial;ReplayClip footage;Texture2D monitorTexture;Color[] pixels;float nextMonitorFrame;
  public bool IsPlaying{get;private set;}public string LastDecision{get;private set;}="";
  public bool CanRequest=>!IsPlaying&&incident!=null&&incident.Available(Time.unscaledTime)&&game.Running&&game.Screen=="match"&&!game.Replay.IsPlaying&&!game.LineReview.IsPlaying;
  public void Initialize(FootballSimulator owner,NeutralReferee referee){game=owner;official=referee;CreateMonitor();}
  void CreateMonitor(){station=new GameObject("Monitor VAR · banda técnica");station.transform.SetParent(transform,false);station.transform.position=new Vector3(0,0,37);Piece(new Vector3(0,.65f,0),new Vector3(.13f,1.3f,.13f),Color.gray);Piece(new Vector3(0,1.6f,0),new Vector3(1.3f,.85f,.16f),Color.black);var display=Piece(new Vector3(0,1.6f,-.095f),new Vector3(1.13f,.67f,.02f),new Color(.05f,.7f,.55f));screenMaterial=display.GetComponent<Renderer>().sharedMaterial;}
  GameObject Piece(Vector3 at,Vector3 size,Color color){var p=GameObject.CreatePrimitive(PrimitiveType.Cube);p.transform.SetParent(station.transform,false);p.transform.localPosition=at;p.transform.localScale=size;p.GetComponent<Renderer>().sharedMaterial=game.Material(color);p.GetComponent<Collider>().enabled=false;return p;}
  public void Register(VarIncidentKind kind,string decision,string reason){if(IsPlaying)return;incident=new VarIncident(kind,decision,reason,Time.unscaledTime);}
  public void Clear(){incident=null;}
  public bool Request(){if(!CanRequest)return false;reviewed=incident;incident=null;timer=0;LastDecision="";IsPlaying=true;priorScale=Time.timeScale;Time.timeScale=0;cameraAt=game.MatchCamera.transform.position;cameraRotation=game.MatchCamera.transform.rotation;fov=game.MatchCamera.fieldOfView;game.Broadcast.SuspendForReplay(true);float side=official.transform.position.z<0?-1:1;station.transform.position=new Vector3(Mathf.Clamp(official.transform.position.x,-40,40),0,side*37);station.transform.rotation=Quaternion.Euler(0,side<0?180:0,0);try{footage=game.Replay.CaptureLatest();}catch{footage=null;}nextMonitorFrame=0;official.BeginVar(station.transform.position+new Vector3(0,.92f,-side*1.2f));walk=official.VarWalkDuration;return true;}
  void Update(){station.SetActive(game.Running&&game.Screen=="match");if(!IsPlaying){if(CanRequest&&Input.GetKeyDown(KeyCode.V))Request();return;}if(!game.Running||game.Screen!="match"){Stop();return;}timer+=Time.unscaledDeltaTime;
   // Walk is animated in real time while match physics and its clock stay frozen.
   UpdateMonitor();official.AnimateVar(Mathf.Clamp01(timer/walk),timer>=walk,timer>=walk+3);
   if(timer>=walk+3){LastDecision=reviewed.Decision;screenMaterial.color=new Color(.2f,.9f,.6f);}
   if(timer>=walk+6)Stop();
  }
  void UpdateMonitor(){if(footage==null||timer<nextMonitorFrame)return;nextMonitorFrame=timer+.12f;if(!monitorTexture){monitorTexture=new Texture2D(128,80,TextureFormat.RGB24,false);pixels=new Color[128*80];screenMaterial.SetTexture("_BaseMap",monitorTexture);screenMaterial.color=Color.white;}
   for(int y=0;y<80;y++)for(int x=0;x<128;x++)pixels[y*128+x]=(x==2||x==125||y==2||y==77||x==64)?Color.white:new Color(.04f,.3f,.16f);
   ReplayMath.Sample(footage,(timer*.6f)%Mathf.Max(.01f,footage.Duration),out var a,out var b,out var mix);foreach(var p in a.players)Dot(p.x,p.z,new Color(.9f,.65f,.15f));Dot(Mathf.Lerp(a.ball.x,b.ball.x,mix),Mathf.Lerp(a.ball.z,b.ball.z,mix),Color.white);monitorTexture.SetPixels(pixels);monitorTexture.Apply();
  }
  void Dot(float x,float z,Color color){int px=Mathf.Clamp(Mathf.RoundToInt((x+55)/110*127),1,126),py=Mathf.Clamp(Mathf.RoundToInt((z+36)/72*79),1,78);for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)pixels[(py+dy)*128+px+dx]=color;}
  void LateUpdate(){if(!IsPlaying)return;var target=official.transform.position+Vector3.up*.6f;game.MatchCamera.transform.position=target+new Vector3(4,2.2f,-5);game.MatchCamera.transform.LookAt(target);game.MatchCamera.fieldOfView=42;}
  public void Stop(){if(!IsPlaying)return;IsPlaying=false;official.EndVar();Time.timeScale=priorScale;game.MatchCamera.transform.SetPositionAndRotation(cameraAt,cameraRotation);game.MatchCamera.fieldOfView=fov;game.Broadcast.SuspendForReplay(false);if(!string.IsNullOrEmpty(LastDecision))game.Status="VAR · "+LastDecision+" · "+reviewed.Reason;}
  void OnDisable(){Stop();}void OnDestroy(){if(monitorTexture)Destroy(monitorTexture);if(station)foreach(var r in station.GetComponentsInChildren<Renderer>())Destroy(r.sharedMaterial);}
  void OnGUI(){if(!game||!game.Running||game.Screen!="match"||game.Replay.IsPlaying||game.LineReview.IsPlaying)return;var old=GUI.matrix;GUI.matrix=Matrix4x4.identity;float w=Screen.width,h=Screen.height;
   if(!IsPlaying){GUI.enabled=CanRequest;if(GUI.Button(new Rect(w-235,h-58,220,44),"SOLICITAR VAR (V)"))Request();GUI.enabled=true;}
   else{float width=Mathf.Min(650,w-24);var box=new Rect((w-width)/2,16,width,170);GUI.Box(box,GUIContent.none);var style=new GUIStyle(GUI.skin.label){fontSize=21,fontStyle=FontStyle.Bold,wordWrap=true,alignment=TextAnchor.MiddleCenter};GUI.Label(new Rect(box.x+12,box.y+10,width-24,55),timer<walk?"VAR · EL ÁRBITRO VA AL MONITOR":timer<walk+3?"REVISANDO LA JUGADA":LastDecision,style);GUI.Label(new Rect(box.x+20,box.y+78,width-40,52),timer<walk+3?"Se revisa: "+(reviewed.Kind==VarIncidentKind.Goal?"gol":reviewed.Kind==VarIncidentKind.GoalLine?"línea de gol":reviewed.Kind==VarIncidentKind.Offside?"fuera de juego":reviewed.Kind==VarIncidentKind.Penalty?"penal":reviewed.Kind==VarIncidentKind.CleanTackle?"contacto con el balón":"falta"):reviewed.Reason);if(timer>=walk+3&&GUI.Button(new Rect(box.x+20,box.y+132,width-40,30),"Continuar"))Stop();}
   GUI.matrix=old;
  }
 }
}
