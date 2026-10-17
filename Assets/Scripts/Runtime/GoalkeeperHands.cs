using UnityEngine;
namespace FootballDynasty {
 [DefaultExecutionOrder(50)]
 public sealed class GoalkeeperHands:MonoBehaviour {
  FootballSimulator game;Collider ballCollider;bool wasKinematic,hadGravity,colliderEnabled;float heldFor,recovery;public FootballAgent Owner{get;private set;}public float HoldTime=>heldFor;public bool IsHolding=>Owner!=null;
  public void Initialize(FootballSimulator owner){game=owner;}
  public bool TryCatch(FootballAgent keeper){if(IsHolding||recovery>0||!game.Running||game.IsCelebrating||game.Referee.HasRestart||keeper.Data.position!="POR")return false;var b=game.Ball.position;var k=keeper.Body.position;
   bool backpass=game.Referee.LastKicker&&game.Referee.LastKicker.Team==keeper.Team&&game.Referee.LastKicker!=keeper;
   if(!KeeperCatchRules.CanCatch(keeper.Team,new PitchPoint(k.x,k.z),new PitchPoint(b.x,b.z),b.y,Vector3.Distance(k,b),game.Ball.linearVelocity.magnitude,keeper.Data.stats.MAN,backpass)||!game.Referee.Touch(keeper,true))return false;
   Owner=keeper;heldFor=0;wasKinematic=game.Ball.isKinematic;hadGravity=game.Ball.useGravity;ballCollider=game.Ball.GetComponent<Collider>();colliderEnabled=ballCollider.enabled;game.Ball.linearVelocity=Vector3.zero;game.Ball.angularVelocity=Vector3.zero;game.Ball.isKinematic=true;game.Ball.useGravity=false;ballCollider.enabled=false;keeper.Animation.Trigger(PlayerAction.Catch);if(keeper.Data.club==game.ClubId)game.Controlled=keeper;game.Status="El portero atrapa el balón · J pase / K despeje";game.ResetBallTracking();return true;
  }
  void FixedUpdate(){if(recovery>0)recovery-=Time.fixedDeltaTime;if(!IsHolding)return;if(!game.Running||game.Screen!="match"||game.IsCelebrating||game.Referee.HasRestart){Drop();return;}heldFor+=Time.fixedDeltaTime;if(heldFor>=(Owner==game.Controlled?6:1.7f))Release(true);}
  void LateUpdate(){if(!IsHolding||game.Replay.IsPlaying)return;game.Ball.position=Owner.Animation.HandPosition;game.Ball.rotation=Owner.transform.rotation;game.ResetBallTracking();}
  public bool Release(bool pass){if(!IsHolding)return false;var keeper=Owner;if(!game.Referee.Kick(keeper))return false;var forward=keeper.transform.forward;forward.y=0;if(forward.sqrMagnitude<.1f)forward=new Vector3(keeper.Team==0?1:-1,0,0);Restore();game.Ball.position=keeper.Body.position+forward.normalized*1.1f+Vector3.up*.35f;game.Ball.linearVelocity=forward.normalized*(pass?16:28)+Vector3.up*(pass?2:8);keeper.RecoverAfterRelease();keeper.Animation.Trigger(pass?PlayerAction.Throw:PlayerAction.Shoot);game.ResetBallTracking();game.Status=pass?"El portero distribuye con la mano":"El portero despeja";recovery=.9f;return true;}
  void Restore(){if(game.Ball){game.Ball.isKinematic=wasKinematic;game.Ball.useGravity=hadGravity;if(ballCollider)ballCollider.enabled=colliderEnabled;}Owner=null;}
  public void Drop(){if(!IsHolding)return;Restore();game.ResetBallTracking();}
  void OnDisable(){Drop();}
 }
}
