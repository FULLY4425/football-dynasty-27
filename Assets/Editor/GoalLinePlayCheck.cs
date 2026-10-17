using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace FootballDynasty.Editor {
 public static class GoalLinePlayCheck {
  static FootballSimulator game;static int stage,checks;static double at;static float clock;static readonly StringBuilder report=new StringBuilder();
  [MenuItem("Football Dynasty/Comprobar gol fantasma (12 s)")]
  public static void Run(){game=UnityEngine.Object.FindFirstObjectByType<FootballSimulator>();if(!EditorApplication.isPlaying||!game||game.Career?.Coach==null||game.Running||game.Replay.IsPlaying)return;report.Clear();stage=checks=0;game.StartMatch();game.ConfirmMatch();at=EditorApplication.timeSinceStartup;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
  static void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;report.AppendLine("PASS "+label);}
  static void Tick(){if(!EditorApplication.isPlaying||!game){EditorApplication.update-=Tick;return;}try{double elapsed=EditorApplication.timeSinceStartup-at;
   if(stage==0&&elapsed>.4){game.Ball.position=new Vector3(52.48f,.25f,0);game.Ball.linearVelocity=Vector3.zero;game.ResetBallTracking();game.LineReview.Observe(new Vector3(52.4f,.25f,0),game.Ball.position,0,false);clock=game.Clock;Check(game.LineReview.IsPlaying&&!game.LineReview.LastDecisionWasGoal,"balón detenido sobre la raya muestra NO GOL");Check(Time.timeScale==0,"revisión detiene la física");stage=1;at=EditorApplication.timeSinceStartup;}
   else if(stage==1&&elapsed>2.8){Check(game.HomeGoals==0&&game.AwayGoals==0&&Math.Abs(game.Clock-clock)<.001f,"revisión NO GOL conserva resultado y reloj");game.LineReview.Stop();Check(Time.timeScale==1,"continuar restaura la velocidad de juego");game.LineReview.NewMatch();game.Referee.Reset();game.Ball.position=new Vector3(52.3f,1,0);game.ResetBallTracking();game.Ball.position=new Vector3(52.9f,1,0);stage=2;at=EditorApplication.timeSinceStartup;}
   else if(stage==2&&elapsed>.4){Check(game.LineReview.IsPlaying&&game.LineReview.LastDecisionWasGoal&&game.HomeGoals==1,"cruce completo activa revisión GOL y cuenta una vez");stage=3;at=EditorApplication.timeSinceStartup;}
   else if(stage==3&&elapsed>5){Check(!game.LineReview.IsPlaying&&Time.timeScale==1,"revisión GOL termina automáticamente");Check(game.IsCelebrating,"tras la revisión se continúa con la celebración");report.AppendLine("SUCCESS "+checks+" Unity goal-line review checks.");Finish();}
   if(elapsed>15)throw new Exception("Tiempo de espera agotado.");
  }catch(Exception e){report.AppendLine("FAIL "+e.Message);Finish();}}
  static void Finish(){EditorApplication.update-=Tick;game.LineReview.Stop();game.Running=false;game.Finished=false;game.Screen="career";game.Referee.Reset();foreach(var a in game.Agents)a.ResetPosition();Time.timeScale=1;game.Status="Prueba de revisión finalizada. Consulta GoalLinePlayCheck-result.txt.";File.WriteAllText(Path.Combine(Application.dataPath,"../GoalLinePlayCheck-result.txt"),report.ToString());Debug.Log(report.ToString());}
 }
}
