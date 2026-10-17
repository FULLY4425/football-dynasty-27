using System;
namespace FootballDynasty {
 public enum PlayerAction { None,Pass,Shoot,Receive,Save,ThrowIn,Catch,Throw }
 [Serializable] public struct PlayerPose {
  public float leftLeg,rightLeg,leftArm,rightArm,spread,lean,bob,twist,leftKnee,rightKnee,leftElbow,rightElbow,sway;
 }
 // Deterministic, allocation-free animation poses; independent of Unity physics.
 public static class PlayerMotion {
  public static PlayerPose Sample(float time,float speed,int seed,PlayerAction action,float actionTime,bool sliding,bool celebrating,float celebrationTime,bool winner){
   float phase=time+(seed%31)*.37f;
   float stride=Math.Min(42,Math.Max(0,speed)*5),wave=(float)Math.Sin(phase);
   var p=new PlayerPose{leftLeg=wave*stride,rightLeg=-wave*stride,leftArm=-wave*stride*.65f,rightArm=wave*stride*.65f,lean=Math.Min(10,speed),bob=Math.Abs((float)Math.Sin(phase))*.035f*Math.Min(1,speed/4)};
   p.leftKnee=Math.Max(0,-wave)*65*Math.Min(1,speed/5);p.rightKnee=Math.Max(0,wave)*65*Math.Min(1,speed/5);p.leftElbow=p.rightElbow=-35-Math.Min(35,speed*4);p.twist=wave*Math.Min(8,speed);p.sway=wave*Math.Min(3,speed*.4f);
   if(speed<.2f){p.bob=(float)Math.Sin(time*2+seed)*.008f;p.spread=3;}
   if(action!=PlayerAction.None&&actionTime>=0&&actionTime<.6f){float kick=(float)Math.Sin(actionTime/.6f*Math.PI);
    if(action==PlayerAction.Pass||action==PlayerAction.Shoot){p.rightLeg=-kick*(action==PlayerAction.Shoot?78:45);p.leftArm=-kick*30;p.rightArm=kick*40;p.twist=kick*12;}
    if(action==PlayerAction.Receive){p.rightLeg=-kick*22;p.lean=-kick*8;}
    if(action==PlayerAction.Save){p.spread=kick*85;p.leftArm=p.rightArm=-kick*55;}
    if(action==PlayerAction.Catch){p.leftArm=p.rightArm=-55;p.leftElbow=p.rightElbow=-65;p.lean=15*(1-kick);}
    if(action==PlayerAction.Throw){p.rightArm=-130*kick;p.rightElbow=-60*(1-kick);p.leftArm=-30;}
    if(action==PlayerAction.ThrowIn){p.leftArm=p.rightArm=-kick*160;p.lean=-kick*12;}
   }
   if(sliding)p=new PlayerPose{lean=65,leftLeg=-25,rightLeg=12,leftArm=-35,rightArm=35,spread=25};
   if(celebrating){float pulse=(float)Math.Sin(celebrationTime*7);p=new PlayerPose();
    if(winner){switch(Math.Abs(seed%3)){case 0:p.spread=115;p.bob=Math.Max(0,pulse)*.22f;p.leftLeg=-pulse*12;p.rightLeg=pulse*12;break;case 1:p.leftArm=p.rightArm=-155;p.bob=Math.Max(0,pulse)*.16f;p.twist=pulse*10;break;default:p.spread=80;p.leftArm=-50;p.rightArm=-110;p.twist=pulse*18;break;}}
    else{p.lean=12;p.leftArm=p.rightArm=15;}
   }
   return p;
  }
  public static int StableSeed(string id){unchecked{int hash=17;foreach(char c in id??"")hash=hash*31+c;return hash&int.MaxValue;}}
 }
}
