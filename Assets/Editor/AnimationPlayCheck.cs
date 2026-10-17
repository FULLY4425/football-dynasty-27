using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace FootballDynasty.Editor {
 public static class AnimationPlayCheck {
  static FootballSimulator game;static double started;static int stage,checks;static float goalClock;static readonly StringBuilder report=new StringBuilder();
  [MenuItem("Football Dynasty/Comprobar animaciones y gol (15 s)")]
  public static void Run(){
   game=UnityEngine.Object.FindFirstObjectByType<FootballSimulator>();if(!EditorApplication.isPlaying||!game||game.Career?.Coach==null||game.Running||game.Replay.IsPlaying){Debug.LogWarning("Abre carrera en Play sin un partido activo.");return;}
   report.Clear();stage=checks=0;game.StartMatch();game.ConfirmMatch();if(!game.Running)return;started=EditorApplication.timeSinceStartup;EditorApplication.update-=Tick;EditorApplication.update+=Tick;
  }
  static void Check(bool value,string label){if(!value)throw new Exception(label);checks++;report.AppendLine("PASS "+label);}
  static void Tick(){
   if(!EditorApplication.isPlaying||!game){EditorApplication.update-=Tick;return;}
   try{double elapsed=EditorApplication.timeSinceStartup-started;
    if(stage==0&&elapsed>.5){Check(game.Agents.Count==22&&game.Agents.All(a=>a.Animation!=null),"los 22 jugadores tienen un modelo articulado");foreach(var a in game.Agents)a.Animation.Trigger(PlayerAction.Shoot);stage=1;started=EditorApplication.timeSinceStartup;}
    else if(stage==1&&elapsed>.2){Check(game.Agents.Any(a=>Math.Abs(a.Animation.Pose.rightLeg)>15),"animación de golpeo modifica la pierna");game.Referee.Reset();game.Ball.position=new Vector3(52.3f,1,0);game.Ball.linearVelocity=Vector3.zero;game.ResetBallTracking();game.Ball.position=new Vector3(52.9f,1,0);stage=2;started=EditorApplication.timeSinceStartup;}
    else if(stage==2&&elapsed>.3){Check(game.HomeGoals==1&&game.IsCelebrating&&game.CelebratingTeam==0,"gol físico activa celebración del equipo anotador");goalClock=game.Clock;stage=3;started=EditorApplication.timeSinceStartup;}
    else if(stage==3&&elapsed>.6&&!game.LineReview.IsPlaying){Check(game.Agents.Where(a=>a.Team==0).All(a=>Math.Abs(a.Animation.Pose.spread)>0||a.Animation.Pose.leftArm<0),"equipo anotador celebra con brazos y gestos");Check(game.Agents.Where(a=>a.Team==1).All(a=>a.Animation.Pose.lean==12),"rival reacciona sin celebrar");Check(Math.Abs(game.Clock-goalClock)<.001f,"reloj deportivo detenido durante la celebración");stage=4;}
    else if(stage==4&&elapsed>4.7&&!game.IsCelebrating){Check(!game.IsCelebrating&&game.Referee.LastTouchTeam==1,"celebración termina y rival saca de centro");var clip=game.Replay.CaptureLatest();ReplayMath.Validate(clip);Check(clip.frames.Any(f=>f.players.Any(p=>p.spread>50||p.leftArm<-100)),"replay conserva los gestos de celebración");Check(clip.frames.Zip(clip.frames.Skip(1),(a,b)=>b.time>a.time).All(v=>v),"replay mantiene tiempo creciente aunque el reloj esté detenido");report.AppendLine("SUCCESS "+checks+" Unity runtime animation checks.");Finish();}
    if(elapsed>18)throw new Exception("Tiempo de espera agotado.");
   }catch(Exception e){report.AppendLine("FAIL "+e.Message);Finish();}
  }
  static void Finish(){EditorApplication.update-=Tick;game.Running=false;game.Finished=false;game.Screen="career";game.Referee.Reset();foreach(var a in game.Agents)a.ResetPosition();Time.timeScale=1;game.Status="Comprobación de animaciones terminada. Consulta AnimationPlayCheck-result.txt.";File.WriteAllText(Path.Combine(Application.dataPath,"../AnimationPlayCheck-result.txt"),report.ToString());Debug.Log(report.ToString());}
 }
}
