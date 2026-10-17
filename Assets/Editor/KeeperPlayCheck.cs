using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace FootballDynasty.Editor {
 public static class KeeperPlayCheck {
  static FootballSimulator game;static FootballAgent keeper;static int stage,checks;static double at;static readonly StringBuilder report=new StringBuilder();
  [MenuItem("Football Dynasty/Comprobar portero con manos (6 s)")]
  public static void Run(){game=UnityEngine.Object.FindFirstObjectByType<FootballSimulator>();if(!EditorApplication.isPlaying||!game||game.Career?.Coach==null||game.Running||game.Replay.IsPlaying)return;report.Clear();stage=checks=0;game.StartMatch();game.ConfirmMatch();keeper=game.Agents.First(a=>a.Data.club==game.ClubId&&a.Data.position=="POR");at=EditorApplication.timeSinceStartup;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
  static void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;report.AppendLine("PASS "+label);}
  static void Tick(){if(!EditorApplication.isPlaying||!game){EditorApplication.update-=Tick;return;}try{double elapsed=EditorApplication.timeSinceStartup-at;
   if(stage==0&&elapsed>.5){game.Referee.Reset();keeper.Body.position=new Vector3(keeper.Team==0?-48:48,.92f,0);keeper.transform.forward=new Vector3(keeper.Team==0?1:-1,0,0);game.Ball.position=keeper.Body.position+keeper.transform.forward*.4f;game.Ball.linearVelocity=Vector3.zero;game.ResetBallTracking();Check(game.Hands.TryCatch(keeper),"portero atrapa balón cercano dentro de su área");Check(game.Hands.Owner==keeper&&game.Ball.isKinematic&&!game.Ball.GetComponent<Collider>().enabled,"balón sujeto sin rebotes ni colisiones");game.Broadcast.SuspendForReplay(true);game.MatchCamera.transform.position=keeper.Body.position+new Vector3(3,1.5f,3);game.MatchCamera.transform.LookAt(keeper.Body.position+Vector3.up*.3f);game.MatchCamera.fieldOfView=45;stage=1;at=EditorApplication.timeSinceStartup;}
   else if(stage==1&&elapsed>2){Check(game.Controlled==keeper&&Vector3.Distance(game.Ball.position,keeper.Animation.HandPosition)<.05f,"balón sigue las manos y portero recibe control");Check(keeper.Animation.Pose.leftElbow<-40&&keeper.Animation.Pose.rightElbow<-40,"pose de agarre flexiona ambos codos");Check(game.Agents.Any(a=>a!=keeper&&(a.Animation.Pose.leftKnee>5||a.Animation.Pose.rightKnee>5)),"jugadores corren con rodillas articuladas");Check(game.Hands.Release(true)&&!game.Hands.IsHolding&&!game.Ball.isKinematic&&game.Ball.GetComponent<Collider>().enabled&&game.Ball.linearVelocity.magnitude>10,"saque de mano devuelve física y velocidad al balón");Check(!game.Hands.TryCatch(keeper),"recuperación evita volver a atrapar su propio saque inmediatamente");stage=2;at=EditorApplication.timeSinceStartup;}
   else if(stage==2&&elapsed>.4){ReplayMath.Validate(game.Replay.CaptureLatest());Check(true,"repetición con extremidades articuladas sigue siendo válida");report.AppendLine("SUCCESS "+checks+" Unity goalkeeper checks.");Finish();}
   if(elapsed>12)throw new Exception("Tiempo de espera agotado.");
  }catch(Exception e){report.AppendLine("FAIL "+e.Message);Finish();}}
  static void Finish(){EditorApplication.update-=Tick;game.Hands.Drop();game.Broadcast.SuspendForReplay(false);game.Running=false;game.Finished=false;game.Screen="career";game.Referee.Reset();foreach(var a in game.Agents)a.ResetPosition();Time.timeScale=1;game.Status="Prueba del portero terminada; consulta KeeperPlayCheck-result.txt.";File.WriteAllText(Path.Combine(Application.dataPath,"../KeeperPlayCheck-result.txt"),report.ToString());Debug.Log(report.ToString());}
 }
}
