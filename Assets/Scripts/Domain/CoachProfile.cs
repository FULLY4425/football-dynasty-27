using System;
using System.Linq;

namespace FootballDynasty
{
    [Serializable]
    public sealed class CoachProfile
    {
        public int avatar, outfit, shirtColor, trousersColor, shoes;
        public bool glasses, tie;
        public string firstName = "", lastName = "", nickname = "", country = "México", gender = "male";
        public int birthYear = 1985, birthMonth = 1, birthDay = 1;
        public CoachProfile Copy() => (CoachProfile)MemberwiseClone();
        public string DisplayName => (firstName + " " + lastName).Trim();

        public string Validate(int season)
        {
            if (avatar < 0 || avatar > 2 || outfit < 0 || outfit > 2 || shirtColor < 0 || shirtColor > 3 || trousersColor < 0 || trousersColor > 2 || shoes < 0 || shoes > 1)
                return "Elige un personaje y un atuendo válidos.";
            if (!Text(firstName, 32, true) || !Text(lastName, 40, true))
                return "Completa nombre y apellido (máximo 32 y 40 caracteres).";
            if (!Text(nickname, 24, false) || !Text(country, 56, true))
                return "Revisa el apelativo y el país o región.";
            if (!new[]
            {
                "male",
                "female",
                "nonbinary"
            }.Contains(gender))
                return "Selecciona un género.";
            try
            {
                var birth = new DateTime(birthYear, birthMonth, birthDay);
                var start = new DateTime(season, 7, 1);
                if (birth > start.AddYears(-18) || birth < start.AddYears(-100))
                    return "El entrenador debe tener entre 18 y 100 años al inicio de la temporada (1 de julio).";
            }
            catch (ArgumentOutOfRangeException)
            {
                return "La fecha de nacimiento no existe.";
            }

            return null;
        }

        static bool Text(string value, int max, bool required) => value != null && value.Trim().Length <= max && (!required || value.Trim().Length > 0) && !value.Any(char.IsControl);
        public CoachProfile CleanCopy()
        {
            var copy = Copy();
            copy.firstName = firstName.Trim();
            copy.lastName = lastName.Trim();
            copy.nickname = nickname.Trim();
            copy.country = country.Trim();
            return copy;
        }
    }
}
