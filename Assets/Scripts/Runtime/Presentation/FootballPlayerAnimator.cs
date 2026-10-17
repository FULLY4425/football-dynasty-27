using UnityEngine;
namespace FootballDynasty {
 // Pivot joints animate the existing original capsule models; no downloaded clips needed.
 public sealed class PlayerRig {
  public readonly Transform Root;readonly Transform[] legs=new Transform[2],arms=new Transform[2],knees=new Transform[2],elbows=new Transform[2],hands=new Transform[2];
  public Vector3 HandPosition=>(hands[0].position+hands[1].position)*.5f;
  public PlayerPose Pose{get;private set;}
  public PlayerRig(Transform parent,Color kit,FootballSimulator game,bool keeper=false){
   Root=new GameObject("Modelo articulado").transform;Root.SetParent(parent,false);
   Part(Root,PrimitiveType.Capsule,new Vector3(0,.26f,0),new Vector3(.55f,.45f,.4f),kit,game);
   Part(Root,PrimitiveType.Sphere,new Vector3(0,.84f,0),Vector3.one*.36f,new Color(.57f,.38f,.25f),game);
   Part(Root,PrimitiveType.Sphere,new Vector3(0,.98f,0),new Vector3(.37f,.15f,.36f),new Color(.11f,.08f,.07f),game);
   for(int i=0;i<2;i++){float side=i==0?-1:1;legs[i]=Joint("Cadera",new Vector3(side*.15f,-.18f,0));arms[i]=Joint("Hombro",new Vector3(side*.32f,.5f,0));
    Part(legs[i],PrimitiveType.Capsule,new Vector3(0,-.16f,0),new Vector3(.17f,.16f,.17f),Color.white,game);
    knees[i]=ChildJoint(legs[i],"Rodilla",new Vector3(0,-.32f,0));Part(knees[i],PrimitiveType.Capsule,new Vector3(0,-.16f,0),new Vector3(.14f,.16f,.14f),Color.white,game);
    Part(knees[i],PrimitiveType.Cube,new Vector3(0,-.32f,.08f),new Vector3(.19f,.13f,.35f),new Color(.12f,.13f,.14f),game);
    Part(arms[i],PrimitiveType.Capsule,new Vector3(0,-.135f,0),new Vector3(.14f,.135f,.14f),kit,game);
    elbows[i]=ChildJoint(arms[i],"Codo",new Vector3(0,-.27f,0));Part(elbows[i],PrimitiveType.Capsule,new Vector3(0,-.13f,0),new Vector3(.12f,.13f,.12f),new Color(.57f,.38f,.25f),game);
    hands[i]=ChildJoint(elbows[i],"Mano",new Vector3(0,-.27f,0));Part(hands[i],PrimitiveType.Sphere,Vector3.zero,new Vector3(.13f,.14f,.12f),keeper?Color.white:new Color(.57f,.38f,.25f),game);
   }
  }
  static Transform ChildJoint(Transform parent,string name,Vector3 at){var t=new GameObject(name).transform;t.SetParent(parent,false);t.localPosition=at;return t;}
  Transform Joint(string name,Vector3 at){var t=new GameObject(name).transform;t.SetParent(Root,false);t.localPosition=at;return t;}
  static void Part(Transform parent,PrimitiveType type,Vector3 at,Vector3 scale,Color color,FootballSimulator game){var p=GameObject.CreatePrimitive(type);p.transform.SetParent(parent,false);p.transform.localPosition=at;p.transform.localScale=scale;p.GetComponent<Renderer>().sharedMaterial=game.Material(color);var c=p.GetComponent<Collider>();c.enabled=false;Object.Destroy(c);}
  public void Apply(PlayerPose pose){Pose=pose;Root.localPosition=Vector3.up*pose.bob;Root.localRotation=Quaternion.Euler(pose.lean,pose.twist,pose.sway);legs[0].localRotation=Quaternion.Euler(pose.leftLeg,0,0);legs[1].localRotation=Quaternion.Euler(pose.rightLeg,0,0);arms[0].localRotation=Quaternion.Euler(pose.leftArm,0,-pose.spread);arms[1].localRotation=Quaternion.Euler(pose.rightArm,0,pose.spread);knees[0].localRotation=Quaternion.Euler(pose.leftKnee,0,0);knees[1].localRotation=Quaternion.Euler(pose.rightKnee,0,0);elbows[0].localRotation=Quaternion.Euler(pose.leftElbow,0,0);elbows[1].localRotation=Quaternion.Euler(pose.rightElbow,0,0);}
 }
 public sealed class FootballPlayerAnimator:MonoBehaviour {
  FootballAgent actor;FootballSimulator game;PlayerAction action;float actionAt=-10,motionTime,stridePhase,blendedSpeed;int seed;PlayerRig rig;
  public Vector3 HandPosition=>rig.HandPosition;
  public PlayerPose Pose=>rig==null?default:rig.Pose;
  public void Initialize(FootballAgent player,FootballSimulator owner,PlayerRig model){actor=player;game=owner;rig=model;seed=PlayerMotion.StableSeed(actor.Data.id);}
  public void Trigger(PlayerAction next){action=next;actionAt=motionTime;}
  public void ResetPose(){action=PlayerAction.None;actionAt=-10;rig.Apply(default);}
  void LateUpdate(){if(!actor||rig==null||game.Replay.IsPlaying||(game.LineReview&&game.LineReview.IsPlaying)||(game.Var&&game.Var.IsPlaying))return;if(game.Screen!="match"||!game.Running){rig.Apply(default);return;}
   motionTime+=Time.deltaTime;var velocity=actor.Body.linearVelocity;float speed=new Vector2(velocity.x,velocity.z).magnitude;
   blendedSpeed=Mathf.Lerp(blendedSpeed,speed,1-Mathf.Exp(-Time.deltaTime*9));stridePhase+=Time.deltaTime*Mathf.Lerp(3,15,Mathf.Clamp01(blendedSpeed/9));
   var pose=PlayerMotion.Sample(stridePhase,blendedSpeed,seed,action,motionTime-actionAt,actor.IsSliding,game.IsCelebrating,game.CelebrationTime,actor.Team==game.CelebratingTeam);
   if(game.Hands.Owner==actor){float grip=Mathf.SmoothStep(0,1,game.Hands.HoldTime/.45f);pose=new PlayerPose{leftArm=Mathf.Lerp(-80,-40,grip),rightArm=Mathf.Lerp(-80,-40,grip),leftElbow=Mathf.Lerp(-15,-65,grip),rightElbow=Mathf.Lerp(-15,-65,grip),spread=Mathf.Lerp(-8,-25,grip),lean=Mathf.Lerp(18,0,grip)};}
   rig.Apply(pose);
  }
 }
}
