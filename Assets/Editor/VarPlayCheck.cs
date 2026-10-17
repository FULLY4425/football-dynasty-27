using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace FootballDynasty.Editor {
 public static class VarPlayCheck {
  static FootballSimulator game;static NeutralReferee referee;static int stage,checks;static double at;static float clock;static Vector3 start;static readonly StringBuilder report=new StringBuilder();
  [MenuItem("Football Dynasty/Comprobar VAR solicitado (20 s)")]
  public static void Run(){game=UnityEngine.Object.FindFirstObjectByType<FootballSimulator>();if(!EditorApplication.isPlaying||!game||game.Career?.Coach==null||game.Running||game.Replay.IsPlaying)return;report.Clear();stage=checks=0;game.StartMatch();game.ConfirmMatch();referee=UnityEngine.Object.FindFirstObjectByType<NeutralReferee>();at=EditorApplication.timeSinceStartup;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
  static void Check(bool ok,string label){if(!ok)throw new Exception(label);checks++;report.AppendLine("PASS "+label);}
  static void Tick(){if(!EditorApplication.isPlaying||!game){EditorApplication.update-=Tick;return;}try{double elapsed=EditorApplication.timeSinceStartup-at;
   if(stage==0&&elapsed>.5){Check(!game.Var.CanRequest&&!game.Var.Request(),"no se solicita VAR sin un incidente");game.Referee.Award(RestartKind.Penalty,1,new Vector3(-40,.92f,0),"PENAL · prueba de contacto");Check(game.Var.CanRequest,"penal registrado habilita solicitud del usuario");clock=game.Clock;start=referee.transform.position;Check(game.Var.Request()&&!game.Var.Request()&&Time.timeScale==0,"solicitud pausa el partido y rechaza duplicados");stage=1;at=EditorApplication.timeSinceStartup;}
   else if(stage==1&&elapsed>1){Check(Vector3.Distance(start,referee.transform.position)>.5f&&Math.Abs(game.Clock-clock)<.001f,"árbitro se desplaza al monitor con reloj detenido");stage=2;}
   else if(stage==2&&!string.IsNullOrEmpty(game.Var.LastDecision)){Check(game.Var.LastDecision=="PENAL CONFIRMADO"&&game.Referee.Kind==RestartKind.Penalty,"VAR anuncia el penal registrado y conserva la reanudación");game.Var.Stop();Check(Time.timeScale==1&&Vector3.Distance(start,referee.transform.position)<.1f&&!game.Var.CanRequest,"continuar restaura árbitro y velocidad sin permitir repetir el caso");report.AppendLine("SUCCESS "+checks+" Unity user-requested VAR checks.");Finish();}
   if(elapsed>25)throw new Exception("Tiempo de espera agotado.");
  }catch(Exception e){report.AppendLine("FAIL "+e.Message);Finish();}}
  static void Finish(){EditorApplication.update-=Tick;game.Var.Stop();game.Running=false;game.Finished=false;game.Screen="career";game.Referee.Reset();foreach(var a in game.Agents)a.ResetPosition();Time.timeScale=1;game.Status="Prueba VAR terminada; consulta VarPlayCheck-result.txt.";File.WriteAllText(Path.Combine(Application.dataPath,"../VarPlayCheck-result.txt"),report.ToString());Debug.Log(report.ToString());}
 }
}
