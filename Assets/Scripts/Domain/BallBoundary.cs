using System;
namespace FootballDynasty {
 public struct BoundaryEvent {public RestartKind Kind;public int Team;public PitchPoint Spot;public float Fraction;}
 public static class BallBoundary {
  // The entire 22 cm ball must cross. At diagonal exits the first crossed line wins.
  public static BoundaryEvent Evaluate(PitchPoint previous,PitchPoint current,int lastTouchTeam) {
   var result=new BoundaryEvent{Kind=RestartKind.None,Fraction=2};if(lastTouchTeam<0||lastTouchTeam>1)return result;
   for(int side=-1;side<=1;side+=2){
    if(previous.Z*side<=34.11f&&current.Z*side>34.11f){float t=(side*34.11f-previous.Z)/(current.Z-previous.Z);if(t<result.Fraction){result=new BoundaryEvent{Kind=RestartKind.ThrowIn,Team=1-lastTouchTeam,Fraction=t,Spot=new PitchPoint(Math.Max(-52.5f,Math.Min(52.5f,previous.X+(current.X-previous.X)*t)),side*34)};}}
    if(previous.X*side<=52.61f&&current.X*side>52.61f){float t=(side*52.61f-previous.X)/(current.X-previous.X);if(t<result.Fraction){int defending=side>0?1:0;float z=previous.Z+(current.Z-previous.Z)*t;bool corner=lastTouchTeam==defending;result=new BoundaryEvent{Kind=corner?RestartKind.CornerKick:RestartKind.GoalKick,Team=corner?1-defending:defending,Fraction=t,Spot=corner?new PitchPoint(side*52.5f,z>=0?34:-34):new PitchPoint(side*47,0)};}}
   }return result;
  }
 }
 public static class ControlSelection {
  public static bool CanReceive(int playerTeam,int receiverTeam,bool deliberate,float speed,float now,float manualUntil) => playerTeam==receiverTeam&&now>=manualUntil&&(deliberate||speed<12);
 }
 public static class RestartPlacement {
  public static PitchPoint KeepAway(PitchPoint spot,PitchPoint player,float distance,float xLimit=51.5f,float zLimit=33) {
   var bounded=new PitchPoint(Math.Max(-xLimit,Math.Min(xLimit,player.X)),Math.Max(-zLimit,Math.Min(zLimit,player.Z)));
   if(PitchPoint.DistanceSquared(spot,bounded)>=distance*distance)return bounded;
   var best=new PitchPoint(0,0);float cost=float.MaxValue;
   for(int i=0;i<64;i++){float angle=i*(float)Math.PI/32;var candidate=new PitchPoint(spot.X+(float)Math.Cos(angle)*distance,spot.Z+(float)Math.Sin(angle)*distance);if(Math.Abs(candidate.X)>xLimit||Math.Abs(candidate.Z)>zLimit)continue;float d=PitchPoint.DistanceSquared(candidate,player);if(d<cost){cost=d;best=candidate;}}
   return best;
  }
 }
 public static class Passing {
  public static float Score(PitchPoint passer,PitchPoint target,int team,TacticalPlayer[] players) {
   float dx=target.X-passer.X,dz=target.Z-passer.Z,length=dx*dx+dz*dz;
   float score=(float)Math.Sqrt(length)-(team==0?1:-1)*dx*.6f;
   foreach(var p in players)if(p.Team!=team){float t=Math.Max(0,Math.Min(1,((p.Position.X-passer.X)*dx+(p.Position.Z-passer.Z)*dz)/Math.Max(.01f,length)));
    if(t>.05f&&t<.98f&&PitchPoint.DistanceSquared(p.Position,new PitchPoint(passer.X+dx*t,passer.Z+dz*t))<4)score+=25;
    if(PitchPoint.DistanceSquared(p.Position,target)<16)score+=10;
   }return score;
  }
 }
}
