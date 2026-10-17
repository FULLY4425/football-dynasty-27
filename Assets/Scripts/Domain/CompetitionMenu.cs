using System.Linq;
namespace FootballDynasty {
 public static class CompetitionMenu {
  public static string Name(Tournament t) => t.id=="cup-spain"?"Copa del Rey":t.id=="cup-england"?"FA Cup":t.id=="europa"?"Europa League":t.name;
  public static Tournament[] ForClub(Career career,string club) => career.Tournaments.Where(t =>
   (t.participants!=null&&t.participants.Contains(club))||(t.alive!=null&&t.alive.Contains(club))||
   (t.id.StartsWith("cup-")&&career.Data.leagues.Any(l=>t.id=="cup-"+l.id&&l.clubs.Contains(club)))).ToArray();
  public static string State(Tournament t,string club) {
   if(!string.IsNullOrEmpty(t.champion))return t.champion==club?"Campeón":"Finalizada";
   bool alive=(t.alive!=null&&t.alive.Contains(club))||(t.direct!=null&&t.direct.Contains(club));
   return alive?t.stage:"Eliminado";
  }
 }
}
