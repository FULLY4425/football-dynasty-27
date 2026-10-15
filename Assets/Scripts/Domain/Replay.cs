using System;
using System.Collections.Generic;
using System.Linq;
namespace FootballDynasty {
 [Serializable] public class ReplayPose {
  public float x,y,z,qx,qy,qz,qw=1,leftLeg,rightLeg;
 }
 [Serializable] public class ReplayActor {
  public string id,name,position,color;public int team;
 }
 [Serializable] public class ReplayFrame {
  public float time;public int homeGoals,awayGoals;public ReplayPose ball;public ReplayPose[] players;
 }
 [Serializable] public class ReplayClip {
  public int version=1;public string title,createdUtc,home,away;public ReplayActor[] actors;public ReplayFrame[] frames;
  public float Duration=>frames==null||frames.Length<2?0:frames[frames.Length-1].time-frames[0].time;
 }
 // A time bounded buffer. Exported clips are deep copies and survive new kickoffs/matches.
 public sealed class ReplayBuffer {
  private readonly Queue<ReplayFrame> frames=new Queue<ReplayFrame>();
  public int Count=>frames.Count;
  public void Clear()=>frames.Clear();
  public void Add(ReplayFrame frame){
   if(frame==null||!ReplayMath.Finite(frame.time))throw new ArgumentException("Fotograma inválido.");
   if(frames.Count>0&&frame.time<=frames.Last().time)throw new ArgumentException("Tiempo de repetición fuera de orden.");
   frames.Enqueue(frame);while(frames.Count>1&&frame.time-frames.Peek().time>10.0001f)frames.Dequeue();
  }
  public ReplayClip Export(string title,string home,string away,ReplayActor[] actors){
   var clip=new ReplayClip{title=title,createdUtc=DateTime.UtcNow.ToString("o"),home=home,away=away,
    actors=actors.Select(a=>new ReplayActor{id=a.id,name=a.name,position=a.position,color=a.color,team=a.team}).ToArray(),
    frames=frames.Select(ReplayMath.Copy).ToArray()};ReplayMath.Validate(clip);return clip;
  }
 }
 public static class ReplayMath {
  public static bool Finite(float n)=>!float.IsNaN(n)&&!float.IsInfinity(n);
  static ReplayPose CopyPose(ReplayPose p)=>new ReplayPose{x=p.x,y=p.y,z=p.z,qx=p.qx,qy=p.qy,qz=p.qz,qw=p.qw,leftLeg=p.leftLeg,rightLeg=p.rightLeg};
  public static ReplayFrame Copy(ReplayFrame f)=>new ReplayFrame{time=f.time,homeGoals=f.homeGoals,awayGoals=f.awayGoals,ball=CopyPose(f.ball),players=f.players.Select(CopyPose).ToArray()};
  static bool ValidPose(ReplayPose p)=>p!=null&&new[]{p.x,p.y,p.z,p.qx,p.qy,p.qz,p.qw,p.leftLeg,p.rightLeg}.All(Finite)&&Math.Abs(p.x)<300&&Math.Abs(p.y)<100&&Math.Abs(p.z)<300&&(p.qx*p.qx+p.qy*p.qy+p.qz*p.qz+p.qw*p.qw)>.1f;
  public static void Validate(ReplayClip clip){
   if(clip==null||clip.version!=1||string.IsNullOrWhiteSpace(clip.home)||string.IsNullOrWhiteSpace(clip.away)||clip.home==clip.away||clip.actors==null||clip.actors.Length!=22||clip.frames==null||clip.frames.Length<2||clip.frames.Length>1002)throw new ArgumentException("Repetición incompatible o incompleta.");
   if(clip.actors.Any(a=>a==null||string.IsNullOrWhiteSpace(a.id)||string.IsNullOrWhiteSpace(a.name)||a.name.Length>120||a.color==null||a.color.Length>16||a.team<0||a.team>1||!new[]{"POR","DEF","MED","DEL"}.Contains(a.position))||clip.actors.Select(a=>a.id).Distinct().Count()!=22||clip.actors.Count(a=>a.team==0)!=11)throw new ArgumentException("Jugadores de repetición inválidos.");
   float previous=-1;foreach(var f in clip.frames){if(f==null||!Finite(f.time)||f.time<0||f.time<=previous||f.homeGoals<0||f.awayGoals<0||f.homeGoals>99||f.awayGoals>99||!ValidPose(f.ball)||f.players==null||f.players.Length!=22||!f.players.All(ValidPose))throw new ArgumentException("Fotogramas inválidos.");previous=f.time;}
   if(clip.Duration>10.001f)throw new ArgumentException("La repetición excede diez segundos.");
  }
  // Returns neighboring frames for interpolation; time is relative to clip start.
  public static void Sample(ReplayClip clip,float seconds,out ReplayFrame a,out ReplayFrame b,out float mix){
   float target=clip.frames[0].time+Math.Max(0,Math.Min(clip.Duration,seconds));int low=0,high=clip.frames.Length-1;
   while(low+1<high){int mid=(low+high)/2;if(clip.frames[mid].time<=target)low=mid;else high=mid;}
   a=clip.frames[low];b=clip.frames[high];mix=Math.Max(0,Math.Min(1,(target-a.time)/(b.time-a.time)));
  }
 }
 public static class GoalRules {
  // Ball must cross completely. Interpolation prevents fast shots skipping the mouth test.
  public static int CrossedGoal(float px,float py,float pz,float x,float y,float z){
   const float plane=52.61f;foreach(int side in new[]{-1,1})if(px*side<=plane&&x*side>plane){float t=(side*plane-px)/(x-px);float cy=py+(y-py)*t,cz=pz+(z-pz)*t;if(Math.Abs(cz)<=3.55f&&cy>=.11f&&cy<=2.33f)return side;}
   return 0;
  }
 }
}
