using System;
namespace FootballDynasty {
 public static class GoalLineReviewRules {
  // Matches the match ball radius and the goal-mouth tolerances used by GoalRules.
  public const float Line=52.5f,Radius=.11f;
  public static bool NearMouth(float x,float y,float z)=>Math.Abs(Math.Abs(x)-Line)<.65f&&y>=Radius&&y<=2.33f&&Math.Abs(z)<=3.55f;
  public static bool ShouldReview(float px,float py,float pz,float x,float y,float z,float speed,bool goal){
   if(goal)return true;
   bool close=NearMouth(px,py,pz)||NearMouth(x,y,z);
   return close&&Math.Max(Math.Abs(px),Math.Abs(x))>=Line-.35f&&(Math.Abs(x)<Math.Abs(px)-.005f||speed<.4f);
  }
  public static float Clearance(float side,float x)=>side*x-Radius-Line;
 }
}
