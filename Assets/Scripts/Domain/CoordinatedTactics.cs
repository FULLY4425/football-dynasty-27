using System;
namespace FootballDynasty {
 public static class CoordinatedTactics {
  static float Clamp(float x,float a,float b)=>Math.Max(a,Math.Min(b,x));
  public static TacticalDecision Evaluate(int index,TacticalPlayer[] players,PitchPoint ball,PossessionTracker possession,MatchDifficulty difficulty,PitchPoint ballVelocity=default) {
   var self=players[index];var d=TacticalBrain.Evaluate(index,players,ball,possession);
   if(self.Role==TacticalRole.Portero)return d;
   if(possession.Carrier==index){
    float forward=self.Team==0?1:-1;bool blocked=false;foreach(var p in players)if(p.Team!=self.Team&&(p.Position.X-self.Position.X)*forward>0&&(p.Position.X-self.Position.X)*forward<9&&Math.Abs(p.Position.Z-self.Position.Z)<4)blocked=true;
    if(blocked){var left=new PitchPoint(self.Position.X+forward*7,Clamp(self.Position.Z-6,-27,27));var right=new PitchPoint(left.X,Clamp(self.Position.Z+6,-27,27));d.Target=Passing.Score(self.Position,left,self.Team,players)<=Passing.Score(self.Position,right,self.Team,players)?left:right;}
    d.Sprint=!blocked;return d;
   }
   float dir=self.Team==0?1:-1;bool attack=possession.PhaseFor(self.Team)==GamePhase.Ofensivo;
   float center=Clamp(ball.X*dir*.65f+(attack?4:-8),-28,24);
   float depth=self.Role==TacticalRole.CentralDeCobertura?-13:self.Role==TacticalRole.MediocentroDefensivo?-5:self.Role==TacticalRole.CreadorDeJuego?3:14;
   float width=difficulty==MatchDifficulty.Dificil?.68f:difficulty==MatchDifficulty.Facil?.88f:.77f;
   if(d.Action==TacticalAction.Presionar||d.Action==TacticalAction.Recuperar){float anticipation=difficulty==MatchDifficulty.Dificil?.32f:difficulty==MatchDifficulty.Facil?.10f:.22f;d.Target=new PitchPoint(ball.X+Clamp(ballVelocity.X*anticipation,-5,5),ball.Z+Clamp(ballVelocity.Z*anticipation,-5,5));}
   if(d.Action!=TacticalAction.Presionar&&d.Action!=TacticalAction.Recuperar&&d.Action!=TacticalAction.ApoyoCorto&&d.Action!=TacticalAction.DiagonalInterior)
    d.Target=new PitchPoint(dir*(center+depth),self.Anchor.Z*width+ball.Z*.22f);
   if(d.Action!=TacticalAction.Presionar&&d.Action!=TacticalAction.Recuperar){
    d.Target.X=dir*Clamp(d.Target.X*dir,center-17,center+19);
    // Avoid standing beyond the second-last opponent before a pass.
    if(attack&&d.Target.X*dir>0){float first=-1000,second=-1000;foreach(var p in players)if(p.Team!=self.Team){float x=p.Position.X*dir;if(x>first){second=first;first=x;}else if(x>second)second=x;}
     if(second>-999)d.Target.X=dir*Math.Min(d.Target.X*dir,Math.Max(ball.X*dir,second)-.8f);
    }
   }
   // Local separation preserves passing lanes instead of piling up at identical role targets.
   for(int i=0;i<players.Length;i++)if(i!=index&&players[i].Team==self.Team){float x=self.Position.X-players[i].Position.X,z=self.Position.Z-players[i].Position.Z;float distance=x*x+z*z;
    if(distance<9){if(distance<.01f)z=index>i?1:-1;float length=(float)Math.Sqrt(Math.Max(.01f,x*x+z*z));d.Target.X+=x/length*.8f;d.Target.Z+=z/length*.8f;}}
   d.Target=new PitchPoint(Clamp(d.Target.X,-50,50),Clamp(d.Target.Z,-29,29));return d;
  }
 }
}
