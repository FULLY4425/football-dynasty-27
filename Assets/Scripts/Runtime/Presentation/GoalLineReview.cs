using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace FootballDynasty {
 // Presentation only: freezes the live world and animates a non-physical ghost ball.
 public sealed class GoalLineReview:MonoBehaviour {
  FootballSimulator game;readonly Queue<Vector3> trail=new Queue<Vector3>();Vector3[] path;
  float timer,priorScale,cooldown,side;bool goal;Vector3 camPos,focus;Quaternion camRotation;float fov;GameObject ghost;Renderer liveBall;bool priorVisibility;Texture2D disc;
  public bool IsPlaying{get;private set;}public bool LastDecisionWasGoal{get;private set;}
  public void Initialize(FootballSimulator owner){game=owner;}
  public void NewMatch(){Stop();trail.Clear();cooldown=0;}
  public void Observe(Vector3 previous,Vector3 current,int crossed,bool validGoal){
   if(IsPlaying||(game.Var&&game.Var.IsPlaying))return;cooldown=Mathf.Max(0,cooldown-Time.fixedDeltaTime);trail.Enqueue(current);while(trail.Count>45)trail.Dequeue();
   if((cooldown>0&&crossed==0)||!GoalLineReviewRules.ShouldReview(previous.x,previous.y,previous.z,current.x,current.y,current.z,game.Ball.linearVelocity.magnitude,crossed!=0))return;
   // Never show a goal-mouth review for a ball that first went out over a touchline.
   if(crossed==0&&game.Var)game.Var.Register(VarIncidentKind.GoalLine,VarDecisions.Goal(false,true),"El borde posterior del balón no cruzó completamente el plano de gol.");
   Begin(previous,current,crossed!=0&&validGoal);
  }
  void Begin(Vector3 previous,Vector3 current,bool isGoal){
   path=trail.ToArray();if(path.Length<2)path=new[]{previous,current};goal=LastDecisionWasGoal=isGoal;side=Mathf.Sign(current.x);if(side==0)side=1;
   focus=path.OrderByDescending(p=>p.x*side).First();timer=0;IsPlaying=true;cooldown=8;
   priorScale=Time.timeScale;Time.timeScale=0;camPos=game.MatchCamera.transform.position;camRotation=game.MatchCamera.transform.rotation;fov=game.MatchCamera.fieldOfView;game.Broadcast.SuspendForReplay(true);
   liveBall=game.Ball.GetComponent<Renderer>();priorVisibility=liveBall.enabled;liveBall.enabled=false;
   ghost=GameObject.CreatePrimitive(PrimitiveType.Sphere);ghost.name="Revisión de línea · balón sin física";ghost.transform.localScale=Vector3.one*.22f;var collider=ghost.GetComponent<Collider>();collider.enabled=false;Destroy(collider);ghost.GetComponent<Renderer>().sharedMaterial=game.Material(Color.white);
  }
  void Update(){if(!IsPlaying)return;if(!game.Running||game.Screen!="match"){Stop();return;}timer+=Time.unscaledDeltaTime;if(timer>=4.8f||Input.GetKeyDown(KeyCode.Space))Stop();}
  void LateUpdate(){if(!IsPlaying)return;float t=Mathf.Clamp01(timer/2.2f)*(path.Length-1);int a=Mathf.FloorToInt(t),b=Mathf.Min(path.Length-1,a+1);ghost.transform.position=Vector3.Lerp(path[a],path[b],t-a);
   var target=new Vector3(side*GoalLineReviewRules.Line,Mathf.Clamp(focus.y,.3f,1.7f),focus.z);game.MatchCamera.transform.position=target+new Vector3(side*.5f,4.5f,5);game.MatchCamera.transform.LookAt(target);game.MatchCamera.fieldOfView=33;
  }
  public void Stop(){if(!IsPlaying)return;IsPlaying=false;if(liveBall)liveBall.enabled=priorVisibility;if(ghost){Destroy(ghost.GetComponent<Renderer>().sharedMaterial);Destroy(ghost);}if(game&&game.MatchCamera){game.MatchCamera.transform.SetPositionAndRotation(camPos,camRotation);game.MatchCamera.fieldOfView=fov;game.Broadcast.SuspendForReplay(false);}Time.timeScale=priorScale;}
  void OnDisable(){Stop();}void OnDestroy(){if(disc)Destroy(disc);}
  static void Fill(Rect r,Color color){var old=GUI.color;GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=old;}
  void OnGUI(){if(!IsPlaying)return;var old=GUI.matrix;GUI.matrix=Matrix4x4.identity;float w=Screen.width,h=Screen.height;float width=w>=850?420:w-24,height=Mathf.Min(300,h-24);var box=new Rect(w>=850?16:(w-width)/2,h-height-12,width,height);Fill(box,new Color(.025f,.055f,.11f,.96f));
   var title=new GUIStyle(GUI.skin.label){fontSize=18,wordWrap=true,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleCenter};title.normal.textColor=Color.white;
   GUI.Label(new Rect(box.x,box.y+6,width,40),timer<2.2f?"REVISIÓN · LÍNEA DE GOL":goal?"GOL · BALÓN COMPLETAMENTE DENTRO":"NO GOL · NO CRUZÓ COMPLETAMENTE",title);
   float line=box.center.x,cy=box.y+130;Fill(new Rect(line-2,box.y+60,4,125),Color.white);GUI.Label(new Rect(line-145,box.y+55,140,28),"CAMPO");GUI.Label(new Rect(line+16,box.y+55,140,28),"PORTERÍA");
   if(!disc){disc=new Texture2D(64,64,TextureFormat.RGBA32,false);for(int y=0;y<64;y++)for(int x=0;x<64;x++)disc.SetPixel(x,y,(new Vector2(x-31.5f,y-31.5f)).magnitude<31?Color.white:Color.clear);disc.Apply(false,true);}
   float radius=25;float at=(timer<2.2f?ghost.transform.position.x:focus.x)*side;float bx=line+Mathf.Clamp((at-GoalLineReviewRules.Line)/GoalLineReviewRules.Radius*radius,-width*.4f,width*.4f);
   GUI.DrawTexture(new Rect(bx-radius,cy-radius,radius*2,radius*2),disc);
   float gap=GoalLineReviewRules.Clearance(side,focus.x)*100;GUI.Label(new Rect(box.x+20,box.y+198,width-40,30),"Vista lateral ampliada · borde posterior del balón: "+gap.ToString("F1")+" cm respecto a la línea",new GUIStyle(GUI.skin.label){fontSize=12,wordWrap=true});
   if(GUI.Button(new Rect(box.x+20,box.yMax-44,width-40,32),"Continuar (Espacio)"))Stop();GUI.matrix=old;
  }
 }
}
