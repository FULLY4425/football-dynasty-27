using System;
namespace FootballDynasty {
 public enum VarIncidentKind { Goal,GoalLine,Offside,Foul,Penalty,CleanTackle }
 // Immutable evidence from the original event; reviews do not reroll randomness.
 public sealed class VarIncident {
  public readonly VarIncidentKind Kind;public readonly string Decision,Reason;public readonly float Created;
  public VarIncident(VarIncidentKind kind,string decision,string reason,float created){Kind=kind;Decision=decision;Reason=reason;Created=created;}
  public bool Available(float now)=>now>=Created&&now-Created<=12;
 }
 public static class VarDecisions {
  public static string Goal(bool crossed,bool legal)=>crossed?(legal?"GOL CONFIRMADO":"GOL ANULADO"):"NO GOL CONFIRMADO";
  public static string Contact(bool bodyFirst,bool insideArea)=>!bodyFirst?"SIN FALTA · DECISIÓN CONFIRMADA":insideArea?"PENAL CONFIRMADO":"FALTA CONFIRMADA";
 }
}
