using UnityEngine;
namespace FootballDynasty {
 [DefaultExecutionOrder(100)]
 public sealed class NeutralReferee:MonoBehaviour {
  FootballSimulator game;Rigidbody body;Transform model;Renderer[] parts;readonly Transform[] legs=new Transform[2],arms=new Transform[2];Vector3 varStart,varTarget;Quaternion varRotation;bool inVar;public float VarWalkDuration{get;private set;}
  public void Initialize(FootballSimulator owner){game=owner;body=gameObject.AddComponent<Rigidbody>();body.isKinematic=true;body.interpolation=RigidbodyInterpolation.Interpolate;var capsule=gameObject.AddComponent<CapsuleCollider>();capsule.height=1.8f;capsule.radius=.28f;
   model=new GameObject("Árbitro · uniforme amarillo").transform;model.SetParent(transform,false);
   Part(PrimitiveType.Capsule,new Vector3(0,.2f,0),new Vector3(.5f,.4f,.35f),Color.yellow);
   Part(PrimitiveType.Sphere,new Vector3(0,.8f,0),Vector3.one*.35f,new Color(.65f,.44f,.3f));
   foreach(float side in new[]{-1f,1f}){int index=side<0?0:1;legs[index]=Part(PrimitiveType.Capsule,new Vector3(side*.15f,-.48f,0),new Vector3(.17f,.3f,.17f),Color.black);arms[index]=Part(PrimitiveType.Capsule,new Vector3(side*.36f,.15f,0),new Vector3(.13f,.3f,.13f),Color.yellow);}
   parts=GetComponentsInChildren<Renderer>();body.position=new Vector3(-7,.92f,8);
  }
  Transform Part(PrimitiveType type,Vector3 position,Vector3 scale,Color color){var p=GameObject.CreatePrimitive(type);p.transform.SetParent(model,false);p.transform.localPosition=position;p.transform.localScale=scale;p.GetComponent<Renderer>().sharedMaterial=game.Material(color);var c=p.GetComponent<Collider>();c.enabled=false;Destroy(c);return p.transform;}
  void Update(){if(game==null||parts==null)return;bool show=game.Screen=="match"&&game.Running&&!game.Replay.IsPlaying;foreach(var p in parts)p.enabled=show;var c=GetComponent<CapsuleCollider>();c.enabled=show&&!inVar;}
  void FixedUpdate(){if(!game||!game.Running||game.IsCelebrating||game.Referee.HasRestart||game.Replay.IsPlaying||!game.Ball)return;
   float dir=game.Tactics.Possession.Team==1?-1:1;var ball=game.Ball.position;var target=new Vector3(Mathf.Clamp(ball.x-dir*8,-40,40),.92f,Mathf.Clamp(ball.z+(ball.z>0?-7:7),-25,25));
   foreach(var a in game.Agents){var delta=target-a.Body.position;delta.y=0;if(delta.sqrMagnitude<4)target+=delta.sqrMagnitude<.01f?Vector3.forward*2:delta.normalized*2;}
   var step=Vector3.MoveTowards(body.position,target,4.5f*Time.fixedDeltaTime);var face=ball-body.position;face.y=0;if(face.sqrMagnitude>.1f)body.MoveRotation(Quaternion.LookRotation(face));body.MovePosition(step);
  }
  public void BeginVar(Vector3 target){inVar=true;varStart=body.position;varRotation=body.rotation;varTarget=target;VarWalkDuration=Mathf.Clamp(Vector3.Distance(varStart,target)/5,3,12);GetComponent<CapsuleCollider>().enabled=false;}
  public void AnimateVar(float progress,bool checking,bool decision){if(!inVar)return;body.position=Vector3.Lerp(varStart,varTarget,progress);transform.position=body.position;var face=varTarget-varStart;face.y=0;if(face.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(face);float swing=checking?0:Mathf.Sin(Time.unscaledTime*11)*28;for(int i=0;i<2;i++){legs[i].localRotation=Quaternion.Euler(i==0?swing:-swing,0,0);arms[i].localRotation=Quaternion.Euler(decision?-80:checking?-35:i==0?-swing:swing,0,decision?(i==0?-55:55):0);}model.localRotation=Quaternion.Euler(checking&&!decision?12:0,0,0);}
  public void EndVar(){if(!inVar)return;inVar=false;body.position=varStart;body.rotation=varRotation;transform.SetPositionAndRotation(varStart,varRotation);model.localRotation=Quaternion.identity;foreach(var t in legs)t.localRotation=Quaternion.identity;foreach(var t in arms)t.localRotation=Quaternion.identity;GetComponent<CapsuleCollider>().enabled=true;}
  void OnCollisionEnter(Collision hit){if(game&&game.Ball&&hit.rigidbody==game.Ball)game.Referee.OfficialContact();}
  void OnDestroy(){if(parts!=null)foreach(var p in parts)if(p&&p.sharedMaterial)Destroy(p.sharedMaterial);}
 }
}
