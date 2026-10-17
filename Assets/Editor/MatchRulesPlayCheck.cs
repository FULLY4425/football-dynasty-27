using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace FootballDynasty.Editor {
 public static class MatchRulesPlayCheck {
  [MenuItem("Football Dynasty/Comprobar reglas en Play (partido de prueba)")]
  public static void Run(){
   var game=UnityEngine.Object.FindFirstObjectByType<FootballSimulator>();
   if(!EditorApplication.isPlaying||!game||game.Career==null||game.Career.Coach==null){Debug.LogError("Inicia Play y crea o carga una carrera antes de comprobar las reglas.");return;}
   if(game.Replay.IsPlaying||game.Running){Debug.LogError("Vuelve a carrera antes de iniciar esta prueba. No se interrumpe un partido activo.");return;}
   var report=new StringBuilder();int checks=0;
   Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception(label);checks++;report.AppendLine("PASS "+label);};
   try{
    game.StartMatch();game.ConfirmMatch();if(!game.Running)throw new Exception("No hay partido de liga disponible.");
    var tackler=game.Agents.First(a=>a.Team==0&&a.Data.position!="POR");
    var victim=game.Agents.First(a=>a.Team==1&&a.Data.position!="POR");
    Action reset=()=>{game.Referee.Reset();foreach(var a in game.Agents){a.CancelSlide();a.Body.position=new Vector3(a.Team==0?-20:20,.92f,26);a.Body.linearVelocity=Vector3.zero;}game.Ball.position=new Vector3(0,.16f,15);game.Ball.linearVelocity=Vector3.zero;};
    reset();victim.Body.position=new Vector3(2,.92f,0);tackler.Body.position=new Vector3(-2,.92f,0);game.Ball.position=new Vector3(0,.16f,0);bool won=false;
    game.Referee.SlideContact(tackler,new Vector3(-2,.92f,0),new Vector3(3,.92f,0),ref won);
    check(won&&!game.Referee.HasRestart&&game.Ball.linearVelocity.magnitude>0,"barrida limpia mueve el Rigidbody del balón sin pitar falta");
    reset();victim.Body.position=new Vector3(10,.92f,0);game.Ball.position=new Vector3(20,.16f,0);won=false;
    game.Referee.SlideContact(tackler,new Vector3(8,.92f,0),new Vector3(12,.92f,0),ref won);
    check(game.Referee.HasRestart&&game.Referee.Kind==RestartKind.FreeKick,"contacto primero con rival fuera del área: falta y reanudación");
    reset();victim.Body.position=new Vector3(-40,.92f,0);game.Ball.position=new Vector3(-30,.16f,0);won=false;
    game.Referee.SlideContact(tackler,new Vector3(-42,.92f,0),new Vector3(-38,.92f,0),ref won);
    check(game.Referee.HasRestart&&game.Referee.Kind==RestartKind.Penalty&&Mathf.Abs(game.Ball.position.x+41.5f)<.01f,"falta del defensor en su área: penal en el punto de once metros");
    reset();won=false;game.Referee.SlideContact(tackler,new Vector3(-2,.92f,0),new Vector3(3,.92f,0),ref won);
    check(!won&&!game.Referee.HasRestart,"barrida fallida sin contacto no inventa una falta");
    reset();var passer=tackler;var receiver=game.Agents.First(a=>a.Team==0&&a!=passer&&a.Data.position!="POR");
    var defenders=game.Agents.Where(a=>a.Team==1).ToArray();foreach(var a in defenders)a.Body.position=new Vector3(10,.92f,20);
    defenders[0].Body.position=new Vector3(50,.92f,0);defenders[1].Body.position=new Vector3(40,.92f,10);
    passer.Body.position=new Vector3(30,.92f,0);receiver.Body.position=new Vector3(43,.92f,0);game.Ball.position=new Vector3(30,.16f,0);
    check(game.Referee.Kick(passer)&&!game.Referee.HasRestart,"posición adelantada no se sanciona hasta intervenir");receiver.Body.position=new Vector3(25,.92f,0);
    check(!game.Referee.Touch(receiver,true)&&game.Referee.Kind==RestartKind.IndirectFreeKick,"receptor adelantado al pase: tiro indirecto al tocar, aunque regrese");
    reset();tackler.TrySlide(Vector3.right);check(tackler.IsSliding,"acción barrida activa el estado y animación del jugador");
    reset();var own=game.Agents.Where(a=>a.Data.club==game.ClubId).ToArray();game.Controlled=own[0];game.Ball.position=own[1].Body.position-Vector3.up*.76f;game.Referee.Touch(own[1],true);
    check(game.Controlled==own[1],"recepción del compañero cambia el jugador controlado");
    reset();game.Referee.OutOfPlay(new BoundaryEvent{Kind=RestartKind.ThrowIn,Team=1,Spot=new PitchPoint(0,34)});
    var thrower=game.Agents.Where(a=>a.Team==1&&a.Data.position!="POR").OrderBy(a=>(a.Body.position-game.Ball.position).sqrMagnitude).First();game.Referee.Tick(2);
    thrower.Kick(true);check(game.Referee.LastRelease==RestartKind.ThrowIn&&!game.Referee.GoalAllowed,"saque de banda se ejecuta desde FootballAgent y no permite gol directo");
    reset();game.Referee.SetKickoff(0);game.Ball.position=new Vector3(0,.16f,0);game.Referee.OfficialContact();game.Referee.Tick(2);game.Referee.Tick(.02f);
    check(game.Referee.DropInFlight&&!game.Referee.HasRestart,"choque con árbitro suelta un balón neutral que debe caer al suelo");
    game.Ball.position=new Vector3(0,.16f,0);game.Referee.Tick(.02f);var dropReceiver=game.Agents.Where(a=>a.Team==0).OrderBy(a=>(a.Body.position-game.Ball.position).sqrMagnitude).First();game.Referee.Touch(dropReceiver,true);
    check(!game.Referee.GoalAllowed,"balón neutral no admite gol con un único jugador");var second=game.Agents.First(a=>a!=dropReceiver);game.Referee.Touch(second,true);
    check(game.Referee.GoalAllowed,"balón neutral admite gol después de dos jugadores distintos");
    reset();game.Referee.OutOfPlay(new BoundaryEvent{Kind=RestartKind.GoalKick,Team=1,Spot=new PitchPoint(47,0)});game.Referee.Tick(2);var keeper=game.Agents.First(a=>a.Team==1&&a.Data.position=="POR");
    keeper.Kick(true);check(game.Referee.LastRelease==RestartKind.GoalKick&&game.Referee.GoalIsValid(1)&&!game.Referee.GoalIsValid(0),"saque de meta se ejecuta con portero y no permite autogol directo");
    reset();game.Referee.Award(RestartKind.FreeKick,0,Vector3.zero,"Prueba doble toque");game.Referee.Tick(2);var taker=game.Agents.Where(a=>a.Team==0&&a.Data.position!="POR").OrderBy(a=>(a.Body.position-game.Ball.position).sqrMagnitude).First();game.Referee.Kick(taker);game.Ball.position=new Vector3(5,.16f,0);game.Referee.Tick(.02f);
    check(!game.Referee.Touch(taker,true)&&game.Referee.Kind==RestartKind.IndirectFreeKick,"doble toque del ejecutor produce tiro indirecto");
    report.AppendLine("SUCCESS "+checks+" runtime checks; geometry uses live Unity actors and Rigidbodies, not a full timed match.");Debug.Log(report.ToString());
   }catch(Exception error){report.AppendLine("FAIL "+error.Message);Debug.LogError(report.ToString());}
   finally{if(game.Referee)game.Referee.Reset();foreach(var a in game.Agents){a.CancelSlide();a.ResetPosition();}game.Running=false;game.Finished=false;game.Screen="career";Time.timeScale=1;game.Status="Prueba de reglas finalizada; consulta RulesPlayCheck-result.txt.";File.WriteAllText(Path.Combine(Application.dataPath,"../RulesPlayCheck-result.txt"),report.ToString());}
  }
 }
}
