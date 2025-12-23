using System;

namespace KinoAppDB.Entities
{
    /// <summary>
    /// Database representation of a Film.
    /// Mirrors the core Film model (Titel, Beschreibung, Dauer, Fsk, Genre, Id).
    /// </summary>
    public class FilmEntity
    {
        /// <summary>
        /// Primary key.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Film title.
        /// </summary>
        public string Titel { get; set; } = string.Empty;

        /// <summary>
        /// Description / plot.
        /// </summary>
        public string? Beschreibung { get; set; }

        /// <summary>
        /// Duration in minutes.
        /// </summary>
        public int? Dauer { get; set; }

        /// <summary>
        /// Age rating (FSK).
        /// </summary>
        public int? Fsk { get; set; }

        /// <summary>
        /// Genre(s) as a comma-separated string.
        /// </summary>
        public string? Genre { get; set; }


        /// <summary>
        /// Image url to call from online 
        /// </summary>
        public string? ImageURL { get; set; }


        /// <summary>
        /// Multiples vorstellungs will show one film
        /// </summary>
        public ICollection<VorstellungEntity> Vorstellungen { get; set; } = new List<VorstellungEntity>();
    }
}
