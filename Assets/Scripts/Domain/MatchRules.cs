using System;
namespace FootballDynasty {
 public enum MatchDifficulty { Medio, Facil, Dificil }
 public enum RestartKind { None, FreeKick, Penalty, IndirectFreeKick, ThrowIn, GoalKick, CornerKick, DroppedBall }
 public static class DifficultySettings {
  public static float Reaction(MatchDifficulty d) => d==MatchDifficulty.Facil?.48f:d==MatchDifficulty.Dificil?.08f:.22f;
  public static float AimError(MatchDifficulty d) => d==MatchDifficulty.Facil?1.7f:d==MatchDifficulty.Dificil?.45f:1f;
  public static float TackleInterval(MatchDifficulty d) => d==MatchDifficulty.Facil?2.2f:d==MatchDifficulty.Dificil?.65f:1.2f;
 }
 public static class RefereeRules {
  public static bool InPenaltyArea(int defendingTeam, PitchPoint contact) {
   float ownDirection=defendingTeam==0?-1:1;
   return contact.X*ownDirection>=36f && contact.X*ownDirection<=52.5f && Math.Abs(contact.Z)<=20.16f;
  }
  public static RestartKind Foul(int defendingTeam, PitchPoint contact) => InPenaltyArea(defendingTeam,contact)?RestartKind.Penalty:RestartKind.FreeKick;
  // Positions are sampled at the teammate's kick; being in this position alone is not an offence.
  public static bool OffsidePosition(int team, PitchPoint attacker, PitchPoint ball, TacticalPlayer[] players) {
   float dir=team==0?1:-1, first=-1000, second=-1000;int opponents=0;
   foreach(var p in players)if(p.Team!=team){float x=p.Position.X*dir;opponents++;if(x>first){second=first;first=x;}else if(x>second)second=x;}
   return opponents>=2 && attacker.X*dir>0 && attacker.X*dir>ball.X*dir+.05f && attacker.X*dir>second+.05f;
  }
  // Earliest swept circular contact on a ground-plane segment. -1 means no contact.
  public static float ContactFraction(PitchPoint from,PitchPoint to,PitchPoint target,float radius) {
   float dx=to.X-from.X,dz=to.Z-from.Z,ox=from.X-target.X,oz=from.Z-target.Z;
   float c=ox*ox+oz*oz-radius*radius;if(c<=0)return 0;
   float a=dx*dx+dz*dz;if(a<.000001f)return -1;
   float b=2*(ox*dx+oz*dz),discriminant=b*b-4*a*c;if(discriminant<0)return -1;
   float t=(-b-(float)Math.Sqrt(discriminant))/(2*a);return t>=0&&t<=1?t:-1;
  }
 }
 public sealed class OffsideTracker {
  bool[] flagged=Array.Empty<bool>();int attacking=-1;
  public void Reset(){attacking=-1;Array.Clear(flagged,0,flagged.Length);}
  public void Capture(int kicker,TacticalPlayer[] players,PitchPoint ball) {
   if(flagged.Length!=players.Length)flagged=new bool[players.Length];
   attacking=players[kicker].Team;
   for(int i=0;i<players.Length;i++)flagged[i]=i!=kicker&&players[i].Team==attacking&&RefereeRules.OffsidePosition(attacking,players[i].Position,ball,players);
  }
  public bool Touch(int index,int team,bool deliberate) {
   if(team==attacking&&index>=0&&index<flagged.Length&&flagged[index])return true;
   if(team!=attacking&&deliberate)Reset();return false;
  }
 }
}
