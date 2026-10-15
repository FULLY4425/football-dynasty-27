using System.Collections.Generic;

namespace FootballDynasty
{
    public sealed class ClubHistory
    {
        public int Founded;
        public string City, Story, Source;
    }

    public static class ClubIdentity
    {
        // Short original summaries; dates/origins checked against the club's own history.
        public static readonly Dictionary<string, ClubHistory> Histories = new Dictionary<string, ClubHistory>
        {
            ["madrid"] = new ClubHistory
            {
                Founded = 1902,
                City = "Madrid",
                Story = "Sus primeros estatutos y junta directiva se oficializaron el 6 de marzo de 1902 como Madrid Foot Ball Club. Su identidad blanca acompaña una larga trayectoria en el fútbol español y europeo.",
                Source = "https://www.realmadrid.com/es-ES/noticias/club/actualidad/124-anos-de-leyenda-06-03-2026"
            },
            ["barcelona"] = new ClubHistory
            {
                Founded = 1899,
                City = "Barcelona",
                Story = "Nació el 29 de noviembre de 1899. Su historia está vinculada a Barcelona y a la identidad azulgrana que ha acompañado a distintas generaciones de futbolistas.",
                Source = "https://www.fcbarcelona.com/en/club/news/1132934/114-anys-dhistoria"
            },
            ["club-132"] = new ClubHistory
            {
                Founded = 1900,
                City = "Múnich",
                Story = "Once fundadores, encabezados por Franz John, crearon el club en febrero de 1900 en el Café Gisela de Múnich. Ese origen local dio paso a su extensa trayectoria en el fútbol alemán.",
                Source = "https://fcbayern.com/de/club/historie/meilensteine-seit-1900/meilensteine-in-der-geschichte-des-fc-bayern"
            },
            ["club-360"] = new ClubHistory
            {
                Founded = 1878,
                City = "Manchester",
                Story = "Comenzó en 1878 como Newton Heath, vinculado a trabajadores ferroviarios. Adoptó el nombre Manchester United en 1902 y se trasladó a Old Trafford en 1910.",
                Source = "https://ir.manutd.com/company-information/history.aspx"
            },
            ["club-111"] = new ClubHistory
            {
                Founded = 1897,
                City = "Turín",
                Story = "Un grupo de estudiantes fundó el club el 1 de noviembre de 1897 en Turín. Su nombre procede del latín y significa juventud.",
                Source = "https://www.juventus.com/en/news/articles/juventus-is-born"
            },
            ["club-160"] = new ClubHistory
            {
                Founded = 1970,
                City = "París",
                Story = "Surgió en 1970 de la unión de Paris Football Club y Stade Saint-Germain. Su identidad combina las raíces de Saint-Germain-en-Laye con la capital francesa.",
                Source = "https://en.psg.fr/teams/club/content/50-yrs-the-clubs-beginnings-912-history-psg-paris-saint-germain"
            },
            ["club-227"] = new ClubHistory
            {
                Founded = 1916,
                City = "Ciudad de México",
                Story = "Fue fundado el 12 de octubre de 1916 tras la unión de los equipos Récord y Colón. Sus colores azul y crema formaron parte de esa identidad desde el inicio.",
                Source = "https://www.clubamerica.com.mx/el-club"
            }
        };
        public static string Story(Club club, League league) => Histories.TryGetValue(club.id, out var history) ? history.Story : club.name + " representa a " + club.country + " en " + league.name + ". En esta carrera podrás desarrollar su plantilla, disputar la liga y construir una nueva etapa del club. La reseña histórica ampliada aún no está disponible.";
    }
}
