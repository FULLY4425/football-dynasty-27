using System;
using System.Linq;
using UnityEngine;

namespace FootballDynasty {
    // Presentación separada de la simulación. Sin escudos ni imágenes de terceros.
    public sealed class PreMatchScreen : MonoBehaviour {
        FootballSimulator game;
        MatchPreview preview;
        GUIStyle heading, subheading, body, small, number, button;
        readonly Color background = new Color(.025f, .055f, .11f);
        readonly Color panel = new Color(.055f, .12f, .20f);
        readonly Color accent = new Color(.16f, .88f, .72f);
        public void Initialize(FootballSimulator simulator) { game = simulator; }
        public void Open(MatchPreview match) { preview = match; game.Screen = "prematch"; }
        void Styles() {
            if (heading != null) return;
            heading = Text(32, FontStyle.Bold); subheading = Text(22, FontStyle.Bold);
            body = Text(18); small = Text(15); number = Text(42, FontStyle.Bold);
            button = new GUIStyle(GUI.skin.button) { fontSize = 21, fontStyle = FontStyle.Bold, wordWrap = true };
        }
        static GUIStyle Text(int size, FontStyle weight = FontStyle.Normal) {
            var style = new GUIStyle(GUI.skin.label) { fontSize = size, fontStyle = weight, wordWrap = true };
            style.normal.textColor = Color.white; return style;
        }
        static void Fill(Rect area, Color color) {
            var old = GUI.color; GUI.color = color; GUI.DrawTexture(area, Texture2D.whiteTexture); GUI.color = old;
        }
        void Label(Rect area, string text, GUIStyle style, Color? color = null) {
            var old = GUI.color; GUI.color = color ?? Color.white; GUI.Label(area, text, style); GUI.color = old;
        }
        void Card(Rect area, string caption) {
            Fill(area, panel); Fill(new Rect(area.x, area.y, area.width, 5), accent);
            Label(new Rect(area.x + 22, area.y + 20, area.width - 44, 32), caption, subheading);
        }
        void Crest(Rect area, Club club) {
            ColorUtility.TryParseHtmlString(club.color, out var color);
            Fill(area, color); Fill(new Rect(area.x, area.yMax - 6, area.width, 6), accent);
            var initials = string.IsNullOrWhiteSpace(club.shortCode) ? club.name.Substring(0, Math.Min(3, club.name.Length)).ToUpperInvariant() : club.shortCode;
            var centered = new GUIStyle(subheading) { alignment = TextAnchor.MiddleCenter };
            // Sombra para que las iniciales sean legibles también sobre camisetas claras.
            Label(new Rect(area.x + 1, area.y + 1, area.width, area.height), initials, centered, Color.black);
            Label(area, initials, centered);
        }
        void Team(Rect area, string id, string caption) {
            var club = game.Career.Clubs[id];
            Crest(new Rect(area.x, area.y + 30, 70, 78), club);
            Label(new Rect(area.x, area.y, area.width, 25), caption, small, accent);
            Label(new Rect(area.x + 88, area.y + 26, area.width - 88, 64), club.name, subheading);
            int rating = MatchPreview.Rating(game.Career, id);
            Label(new Rect(area.x + 88, area.y + 94, area.width - 88, 30), "VALORACIÓN DEL ONCE  " + rating, body);
            Label(new Rect(area.x, area.y + 142, area.width, 32), "Forma: " + preview.Form(game.Career, id), small);
        }
        void Fixture(Rect area) {
            Card(area, preview.League.name.ToUpperInvariant());
            Label(new Rect(area.x + 22, area.y + 62, area.width - 44, 30), "JORNADA " + (preview.Fixture.round + 1) + "  ·  " + game.Career.Season + "/" + (game.Career.Season + 1), small, accent);
            float width = (area.width - 76) / 2;
            Team(new Rect(area.x + 22, area.y + 105, width, 190), preview.Fixture.home, "LOCAL");
            Team(new Rect(area.x + area.width / 2 + 16, area.y + 105, width, 190), preview.Fixture.away, "VISITANTE");
            Label(new Rect(area.center.x - 20, area.y + 185, 40, 40), "VS", body, accent);
        }
        void Standings(Rect area) {
            Card(area, "CLASIFICACIÓN");
            int managed = Array.FindIndex(preview.Table, row => row.Club == game.ClubId);
            int start = Mathf.Clamp(managed - 2, 0, Math.Max(0, preview.Table.Length - 5));
            Label(new Rect(area.x + 22, area.y + 62, area.width - 44, 26), "#     EQUIPO", small, accent);
            Label(new Rect(area.xMax - 94, area.y + 62, 80, 26), "PJ   PTS", small, accent);
            for (int i = start; i < Math.Min(start + 5, preview.Table.Length); i++) {
                var row = preview.Table[i]; float y = area.y + 98 + (i - start) * 40;
                if (row.Club == game.ClubId) Fill(new Rect(area.x + 14, y - 2, area.width - 28, 38), new Color(.08f, .34f, .37f));
                Label(new Rect(area.x + 22, y, 36, 32), (i + 1).ToString(), body);
                var club = game.Career.Clubs[row.Club];
                Label(new Rect(area.x + 62, y, area.width - 164, 36), string.IsNullOrEmpty(club.shortName) ? club.name : club.shortName, small);
                Label(new Rect(area.xMax - 94, y, 80, 32), row.Played + "    " + row.Points, body);
            }
        }
        void Info(Rect area, string caption, string first, string second) {
            Card(area, caption); Label(new Rect(area.x + 22, area.y + 65, area.width - 44, 36), first, subheading);
            Label(new Rect(area.x + 22, area.y + 108, area.width - 44, 58), second, body);
        }
        void Competitions(Rect area) {
            Card(area,"COMPETICIONES DEL CLUB");
            var cups=CompetitionMenu.ForClub(game.Career,game.ClubId);
            if(cups.Length==0)Label(new Rect(area.x+22,area.y+66,area.width-44,50),"Temporada de liga",body);
            for(int i=0;i<cups.Length;i++)Label(new Rect(area.x+22,area.y+62+i*32,area.width-44,32),CompetitionMenu.Name(cups[i])+" · "+CompetitionMenu.State(cups[i],game.ClubId),small);
        }
        void OnGUI() {
            if (!game || game.Screen != "prematch" || preview == null) return;
            Styles(); var oldMatrix = GUI.matrix; var oldColor = GUI.color;
            float w = UnityEngine.Screen.width, h = UnityEngine.Screen.height;
            bool portrait = h > w; float canvasW = portrait ? 720 : 1280, canvasH = portrait ? 1430 : 820;
            float scale = Mathf.Min(w / canvasW, h / canvasH);
            GUI.matrix = Matrix4x4.identity; Fill(new Rect(0, 0, w, h), background);
            GUI.matrix = Matrix4x4.TRS(new Vector3((w - canvasW * scale) / 2, (h - canvasH * scale) / 2), Quaternion.identity, Vector3.one * scale);
            Label(new Rect(32, 22, canvasW - 64, 28), "FOOTBALL DYNASTY 27  /  CENTRO DE PARTIDO", small, accent);
            Label(new Rect(32, 60, canvasW - 64, 48), "Tu próximo desafío", heading);
            if (portrait) {
                Fixture(new Rect(32, 128, 656, 320)); Standings(new Rect(32, 468, 656, 320));
                Info(new Rect(32, 808, 656, 170), "TU EQUIPO", game.Career.Clubs[game.ClubId].name, "Formación " + game.Career.Team(game.ClubId).formation + "  ·  " + (preview.IsHome ? "Juegas como local" : "Juegas como visitante"));
                Competitions(new Rect(32,998,656,170));
            } else {
                Fixture(new Rect(32, 128, 770, 320)); Standings(new Rect(822, 128, 426, 320));
                Info(new Rect(32, 468, 600, 170), "TU EQUIPO", game.Career.Clubs[game.ClubId].name, "Formación " + game.Career.Team(game.ClubId).formation + "  ·  " + (preview.IsHome ? "Juegas como local" : "Juegas como visitante"));
                Competitions(new Rect(652,468,596,170));
            }
            float footer = portrait ? 1190 : 658;
            if (GUI.Button(new Rect(32, footer, portrait ? 312 : 250, 48), "Volver a carrera", button)) game.Screen = "career";
            if (GUI.Button(new Rect(portrait ? 364 : 302, footer, portrait ? 324 : 250, 48), "Revisar once", button)) game.Screen = "squad";
            if (GUI.Button(new Rect(portrait ? 32 : 822, portrait ? 1255 : footer, portrait ? 656 : 426, 60 - (portrait ? 0 : 12)), "JUGAR AHORA  →", button)) game.ConfirmMatch();
            float difficultyY=portrait?1340:738;
            Label(new Rect(32,difficultyY,180,48),"DIFICULTAD",small,accent);
            var levels=new[]{MatchDifficulty.Facil,MatchDifficulty.Medio,MatchDifficulty.Dificil};
            for(int i=0;i<3;i++)if(GUI.Button(new Rect(210+i*(portrait?160:200),difficultyY,portrait?150:180,44),(game.Career.Difficulty==levels[i]?"● ":"")+levels[i],button))game.SetDifficulty(levels[i]);
            GUI.matrix = oldMatrix; GUI.color = oldColor;
        }
    }
}
