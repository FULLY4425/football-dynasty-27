using System;
using System.Linq;
using UnityEngine;
namespace FootballDynasty {
 public sealed class MatchReferee : MonoBehaviour {
  FootballSimulator game;readonly OffsideTracker offside=new OffsideTracker();
  public FootballAgent LastKicker{get;private set;}
  FootballAgent kicker,indirectKicker;Vector3 restartSpot;float wait,readyTime;bool restart;
  AudioSource sound;AudioClip whistle;
  public int LastTouchTeam {get;private set;}=-1;
  public RestartKind LastRelease {get;private set;}
  FootballAgent lastRestarter,firstDropTouch;RestartKind restrictedGoal;int restartTakenTeam=-1;bool doubleTouchArmed,offsideExempt,dropWaiting;
  public bool DropInFlight=>dropWaiting&&game.Ball&&game.Ball.position.y>.23f;
  public void SetKickoff(int team){LastTouchTeam=team;}
  public bool GoalIsValid(int scoringTeam)=>GoalAllowed&&(restartTakenTeam<0||scoringTeam==restartTakenTeam);

  public bool IsStopped=>restart&&wait>0;
  public bool HasRestart=>restart;
  public RestartKind Kind {get;private set;}
  public bool GoalAllowed=>indirectKicker==null&&restrictedGoal==RestartKind.None;
  public void Initialize(FootballSimulator owner){game=owner;sound=gameObject.AddComponent<AudioSource>();sound.spatialBlend=0;sound.volume=.3f;
   whistle=AudioClip.Create("Silbato",8820,1,44100,false);var data=new float[8820];for(int i=0;i<data.Length;i++)data[i]=(float)Math.Sin(i*2*Math.PI*2300/44100)*.4f*(1-i/(float)data.Length);whistle.SetData(data,0);
  }
  public void Reset(){LastKicker=null;offside.Reset();restart=false;wait=readyTime=0;kicker=indirectKicker=lastRestarter=firstDropTouch=null;Kind=LastRelease=restrictedGoal=RestartKind.None;LastTouchTeam=restartTakenTeam=-1;doubleTouchArmed=offsideExempt=dropWaiting=false;}
  TacticalPlayer[] Snapshot()=>game.Agents.Select(a=>new TacticalPlayer{Team=a.Team,Role=a.Role,Position=new PitchPoint(a.Body.position.x,a.Body.position.z)}).ToArray();
  public bool Touch(FootballAgent actor,bool deliberate){
   if(game.Hands&&game.Hands.IsHolding&&game.Hands.Owner!=actor)return false;
   if(restart&&(wait>0||actor!=kicker))return false;
   if(DropInFlight)return false;
   if(!restart&&actor==lastRestarter&&doubleTouchArmed){Award(RestartKind.IndirectFreeKick,1-actor.Team,game.Ball.position,"DOBLE TOQUE · tiro libre indirecto");return false;}
   if(!offsideExempt&&offside.Touch(game.Agents.IndexOf(actor),actor.Team,deliberate)){
    Award(RestartKind.IndirectFreeKick,1-actor.Team,actor.Body.position,"FUERA DE JUEGO · tiro libre indirecto");return false;
   }
   if(restrictedGoal==RestartKind.DroppedBall){if(!firstDropTouch)firstDropTouch=actor;else if(actor!=firstDropTouch)restrictedGoal=RestartKind.None;}
   else if(indirectKicker&&actor!=indirectKicker){indirectKicker=null;restrictedGoal=RestartKind.None;}
   if(actor!=lastRestarter){if(restrictedGoal!=RestartKind.DroppedBall)restartTakenTeam=-1;lastRestarter=null;doubleTouchArmed=false;offsideExempt=false;}
   LastTouchTeam=actor.Team;game.NotifyReception(actor,deliberate);
   if(deliberate&&!restart&&!offsideExempt)offside.Capture(game.Agents.IndexOf(actor),Snapshot(),new PitchPoint(game.Ball.position.x,game.Ball.position.z));return true;
  }
  public bool Kick(FootballAgent actor){
   if(!Touch(actor,true))return false;
   LastKicker=actor;LastRelease=restart?Kind:RestartKind.None;
   if(restart){if(game.Var)game.Var.Clear();restart=false;restartTakenTeam=actor.Team;lastRestarter=actor;doubleTouchArmed=false;
    restrictedGoal=Kind==RestartKind.IndirectFreeKick||Kind==RestartKind.ThrowIn?Kind:RestartKind.None;
    indirectKicker=restrictedGoal!=RestartKind.None?actor:null;
    offsideExempt=Kind==RestartKind.ThrowIn||Kind==RestartKind.GoalKick||Kind==RestartKind.CornerKick;Kind=RestartKind.None;
   }
   if(offsideExempt)offside.Reset();else offside.Capture(game.Agents.IndexOf(actor),Snapshot(),new PitchPoint(game.Ball.position.x,game.Ball.position.z));return true;
  }
  public bool WouldBeOffside(FootballAgent receiver,FootballAgent passer)=>!(restart&&(Kind==RestartKind.ThrowIn||Kind==RestartKind.GoalKick||Kind==RestartKind.CornerKick))&&RefereeRules.OffsidePosition(passer.Team,new PitchPoint(receiver.Body.position.x,receiver.Body.position.z),new PitchPoint(game.Ball.position.x,game.Ball.position.z),Snapshot());
  public void Tick(float dt){
   if(!restart){if(lastRestarter&&(game.Ball.position-lastRestarter.Body.position).sqrMagnitude>1.8f)doubleTouchArmed=true;if(dropWaiting){foreach(var a in game.Agents)a.Body.linearVelocity=Vector3.zero;if(game.Ball.position.y<=.23f)dropWaiting=false;}return;}
   if(wait>0){wait=Mathf.Max(0,wait-dt);Freeze();return;}
   readyTime+=dt;
   if(Kind==RestartKind.DroppedBall){restart=false;Kind=RestartKind.None;restrictedGoal=RestartKind.DroppedBall;restartTakenTeam=kicker.Team;firstDropTouch=null;dropWaiting=true;game.Ball.position=restartSpot+Vector3.up*1.3f;game.Ball.linearVelocity=Vector3.zero;game.ResetBallTracking();return;}
   // Dead-ball placement and distance remain protected until the designated taker kicks.
   game.Ball.position=restartSpot;game.Ball.linearVelocity=Vector3.zero;game.Ball.angularVelocity=Vector3.zero;
   foreach(var a in game.Agents)a.Body.linearVelocity=Vector3.zero;
   if(kicker!=game.Controlled&&readyTime>.6f)kicker.Kick(Kind!=RestartKind.Penalty);
  }
  void Freeze(){foreach(var a in game.Agents)a.Body.linearVelocity=Vector3.zero;game.Ball.linearVelocity=Vector3.zero;game.Ball.angularVelocity=Vector3.zero;}
  public void Award(RestartKind kind,int team,Vector3 contact,string text){
   if(restart||!game.Running)return;
   if(game.Var){if(kind==RestartKind.Penalty||kind==RestartKind.FreeKick)game.Var.Register(kind==RestartKind.Penalty?VarIncidentKind.Penalty:VarIncidentKind.Foul,VarDecisions.Contact(true,kind==RestartKind.Penalty),"El contacto con el rival fue anterior al balón; se conserva la ubicación registrada.");else if(kind==RestartKind.IndirectFreeKick&&text.Contains("FUERA"))game.Var.Register(VarIncidentKind.Offside,"FUERA DE JUEGO CONFIRMADO","Posición adelantada registrada al pase e intervención posterior en el balón.");}
   restart=true;Kind=kind;wait=1.8f;readyTime=0;offside.Reset();indirectKicker=lastRestarter=firstDropTouch=null;restrictedGoal=RestartKind.None;restartTakenTeam=-1;doubleTouchArmed=offsideExempt=dropWaiting=false;
   game.Tactics.ResetPossession();game.Status=text+" · "+game.Career.Clubs[team==0?game.ActiveFixture.home:game.ActiveFixture.away].name;
   float dir=team==0?1:-1;
   restartSpot=kind==RestartKind.Penalty?new Vector3(dir*41.5f,.16f,0):new Vector3(Mathf.Clamp(contact.x,-52.5f,52.5f),.16f,Mathf.Clamp(contact.z,-34,34));
   bool goalkeeper=kind==RestartKind.GoalKick||(kind==RestartKind.DroppedBall&&RefereeRules.InPenaltyArea(team,new PitchPoint(restartSpot.x,restartSpot.z)));
   kicker=game.Agents.Where(a=>a.Team==team&&(goalkeeper?a.Data.position=="POR":a.Data.position!="POR")).OrderBy(a=>(a.Body.position-restartSpot).sqrMagnitude).First();
   foreach(var a in game.Agents){a.CancelSlide();var p=a.Body.position;
    if(kind==RestartKind.Penalty){if(a.Data.position=="POR"&&a.Team!=team)p=new Vector3(dir*52.3f,.92f,0);else if(a!=kicker){p.x=dir*Mathf.Min(p.x*dir,30);p.z=Mathf.Clamp(p.z,-27,27);}}
    else if(a!=kicker&&(a.Team!=team||kind==RestartKind.DroppedBall)){
     float distance=kind==RestartKind.ThrowIn?2.2f:kind==RestartKind.DroppedBall?4.2f:9.3f;
     var delta=p-restartSpot;delta.y=0;if(delta.magnitude<distance){if(delta.sqrMagnitude<.01f)delta=new Vector3(-dir,0,1);p=restartSpot+delta.normalized*distance;p.y=.92f;}
     if(kind==RestartKind.GoalKick&&a.Team!=team&&RefereeRules.InPenaltyArea(team,new PitchPoint(p.x,p.z)))p.x=(team==0?-1:1)*34;
    }
    float limit=a.Data.position=="POR"?52.4f:51.5f;p.x=Mathf.Clamp(p.x,-limit,limit);p.z=Mathf.Clamp(p.z,-33,33);
    if(kind!=RestartKind.Penalty&&a!=kicker&&(a.Team!=team||kind==RestartKind.DroppedBall)){float minimum=kind==RestartKind.ThrowIn?2.2f:kind==RestartKind.DroppedBall?4.2f:9.3f;var legal=RestartPlacement.KeepAway(new PitchPoint(restartSpot.x,restartSpot.z),new PitchPoint(p.x,p.z),minimum,limit);p.x=legal.X;p.z=legal.Z;}
    a.Body.position=p;
   }
   kicker.Body.position=kind==RestartKind.ThrowIn?new Vector3(restartSpot.x,.92f,Mathf.Sign(restartSpot.z)*34.3f):restartSpot-new Vector3(dir*.85f,-.76f,0);kicker.transform.forward=new Vector3(dir,0,0);kicker.PrepareRestart();
   if(kicker.Data.club==game.ClubId){game.Controlled=kicker;game.Status+=" · K tiro / J pase";}
   Freeze();game.Ball.position=restartSpot;game.ResetBallTracking();sound.PlayOneShot(whistle);
  }
  // Swept contacts stop a fast slide tunnelling through the ball or the opponent's capsule.
  public void SlideContact(FootballAgent tackler,Vector3 from,Vector3 to,ref bool wonBall){
   if(restart)return;
   var a=new PitchPoint(from.x,from.z);var b=new PitchPoint(to.x,to.z);
   float ballTime=game.Ball.position.y<.8f?RefereeRules.ContactFraction(a,b,new PitchPoint(game.Ball.position.x,game.Ball.position.z),.70f):-1;
   FootballAgent victim=null;float bodyTime=2;
   foreach(var other in game.Agents)if(other.Team!=tackler.Team&&Mathf.Abs(other.Body.position.y-from.y)<1.5f){float t=RefereeRules.ContactFraction(a,b,new PitchPoint(other.Body.position.x,other.Body.position.z),.65f);if(t>=0&&t<bodyTime){bodyTime=t;victim=other;}}
   if(victim&&!wonBall&&(ballTime<0||bodyTime<ballTime)){
    var at=Vector3.Lerp(from,to,bodyTime);Award(RefereeRules.Foul(tackler.Team,new PitchPoint(at.x,at.z)),victim.Team,at,RefereeRules.InPenaltyArea(tackler.Team,new PitchPoint(at.x,at.z))?"PENAL · barrida al rival antes del balón":"FALTA · barrida al rival antes del balón");return;
   }
   if(ballTime>=0&&!wonBall){if(!Touch(tackler,true))return;wonBall=true;if(game.Var&&victim)game.Var.Register(VarIncidentKind.CleanTackle,VarDecisions.Contact(false,false),"El contacto registrado con el balón fue anterior al cuerpo rival.");offside.Reset();var direction=to-from;direction.y=0;game.Ball.linearVelocity=direction.normalized*9+Vector3.up*.5f;game.Status="Barrida limpia · balón recuperado";}
  }
  public void RejectIndirectGoal(int receivingTeam){
   bool ownGoal=restartTakenTeam==receivingTeam;int side=receivingTeam==0?-1:1;indirectKicker=null;
   Award(ownGoal?RestartKind.CornerKick:RestartKind.GoalKick,ownGoal?1-receivingTeam:receivingTeam,new Vector3(side*(ownGoal?52.5f:47),.16f,ownGoal?34:0),"SIN GOL · reanudación directa no válida");
  }
  public void OutOfPlay(BoundaryEvent exit){Award(exit.Kind,exit.Team,new Vector3(exit.Spot.X,.16f,exit.Spot.Z),exit.Kind==RestartKind.ThrowIn?"SAQUE DE BANDA":exit.Kind==RestartKind.CornerKick?"CÓRNER":"SAQUE DE META");}
  public void OfficialContact(){
   if(!game.Running||restart||game.Replay.IsPlaying)return;
   var p=game.Ball.position;
   if(Mathf.Abs(p.x)>52.61f||Mathf.Abs(p.z)>34.11f)return;
   int team=LastTouchTeam<0?0:LastTouchTeam;
   if(RefereeRules.InPenaltyArea(0,new PitchPoint(p.x,p.z)))team=0;else if(RefereeRules.InPenaltyArea(1,new PitchPoint(p.x,p.z)))team=1;
   Award(RestartKind.DroppedBall,team,p,"SAQUE NEUTRAL · contacto con el árbitro");
  }
  void OnDestroy(){if(whistle)Destroy(whistle);}
 }
 public sealed class BallRefereeContact:MonoBehaviour {
  public MatchReferee Referee;
  void OnCollisionEnter(Collision hit){var a=hit.collider.GetComponent<FootballAgent>();if(a)Referee.Touch(a,false);}
  void OnCollisionStay(Collision hit){var a=hit.collider.GetComponent<FootballAgent>();if(a)Referee.Touch(a,false);}
 }
}
