using System.Linq;

namespace FootballDynasty {
    // Consulta sin efectos secundarios: abrir o cancelar la previa no altera la carrera.
    public sealed class MatchPreview {
        public League League;
        public MatchResult Fixture;
        public Career.Standing[] Table;
        public string ManagedClub;
        public string Opponent => Fixture.home == ManagedClub ? Fixture.away : Fixture.home;
        public bool IsHome => Fixture.home == ManagedClub;
        public static MatchPreview Create(Career career, string club) {
            var league = career.Data.leagues.FirstOrDefault(l => l.clubs.Contains(club));
            if (league == null) return null;
            var pair = career.NextLeague(league.id).FirstOrDefault(p => p.Contains(club));
            if (pair == null) return null;
            return new MatchPreview {
                League = league, ManagedClub = club,
                Fixture = new MatchResult { home = pair[0], away = pair[1], competition = league.id, round = career.Round },
                Table = career.Table(league.clubs, career.Results.Where(m => m.competition == league.id))
            };
        }
        public static int Rating(Career career, string club) {
            var lineup = career.Team(club).lineup;
            return lineup.Length == 0 ? 0 : (int)System.Math.Round(lineup.Average(id => career.Players[id].rating));
        }
        public string Form(Career career, string club) {
            var games = career.Results.Where(m => m.competition == League.id && (m.home == club || m.away == club)).ToArray();
            if (games.Length == 0) return "Sin partidos disputados";
            return string.Join("  ", games.Skip(System.Math.Max(0, games.Length - 5)).Select(m => {
                int own = m.home == club ? m.hg : m.ag, other = m.home == club ? m.ag : m.hg;
                return own == other ? "E" : own > other ? "V" : "D";
            }));
        }
    }
}
