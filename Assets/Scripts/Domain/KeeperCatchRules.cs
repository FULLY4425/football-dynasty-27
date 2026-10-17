using System;
namespace FootballDynasty {
 public static class KeeperCatchRules {
  public static bool CanCatch(int team,PitchPoint keeper,PitchPoint ball,float height,float distance,float speed,int handling,bool teammateKick)=>!teammateKick&&RefereeRules.InPenaltyArea(team,keeper)&&RefereeRules.InPenaltyArea(team,ball)&&height>=.1f&&height<=2.15f&&distance<=1.55f&&speed<=14+Math.Max(0,Math.Min(100,handling))*.14f;
 }
}
