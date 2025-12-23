using AutoMapper;
using KinoAppCore.Components;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Imdb;

namespace KinoAppCore.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping film-related models between IMDb DTOs, domain models, and database entities.
    /// </summary>
    /// <remarks>
    /// This profile centralizes transformations so the rest of the application can work with strongly typed
    /// domain objects without leaking external API shapes or persistence concerns.
    /// </remarks>
    public class FilmMappingProfile : Profile
    {
        /// <summary>
        /// Initializes the mappings for film models.
        /// </summary>
        public FilmMappingProfile()
        {
            // IMDb search result -> DB entity
            CreateMap<ImdbMovieSearchResult, FilmEntity>()
                .ForMember(e => e.Id, opt => opt.MapFrom(src => src.Id)) // e.g. "tt1234567"
                .ForMember(e => e.Titel, opt => opt.MapFrom(src => src.Title ?? string.Empty))
                .ForMember(e => e.Beschreibung, opt => opt.MapFrom(src => src.Plot))
                .ForMember(e => e.Dauer, opt => opt.MapFrom(src => src.RuntimeSeconds))
                .ForMember(e => e.Genre, opt => opt.MapFrom(src =>
                    src.Genres != null && src.Genres.Count > 0 ? src.Genres[0] : null))
                .ForMember(e => e.ImageURL, opt => opt.MapFrom(src => src.PosterUrl))
                .ForMember(e => e.Fsk, opt => opt.Ignore()); // set later via certificate data

            // IMDb title API DTO -> DB entity
            CreateMap<ImdbTitleApiDto, FilmEntity>()
                .ForMember(e => e.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(e => e.Titel, opt => opt.MapFrom(src => src.PrimaryTitle ?? string.Empty))
                .ForMember(e => e.Beschreibung, opt => opt.MapFrom(src => src.Plot))
                .ForMember(e => e.Dauer, opt => opt.MapFrom(src => src.RuntimeSeconds))
                .ForMember(e => e.Genre, opt => opt.MapFrom(src =>
                    src.Genres != null && src.Genres.Count > 0 ? src.Genres[0] : null))
                .ForMember(e => e.ImageURL, opt => opt.MapFrom(src => src.PrimaryImage.Url))
                .ForMember(e => e.Fsk, opt => opt.Ignore()); // set later via certificate data

            // IMDb details -> domain film (partial)
            CreateMap<ImdbMovieDetails, Film>()
                .ForMember(f => f.Titel, opt => opt.MapFrom(src => src.Title))
                .ForMember(f => f.Beschreibung, opt => opt.MapFrom(src => src.Plot))
                .ForMember(f => f.Dauer, opt => opt.Ignore())
                .ForMember(f => f.Genre, opt => opt.Ignore())
                .ForMember(f => f.Fsk, opt => opt.Ignore())
                .ForMember(f => f.Id, opt => opt.Ignore());

            // IMDb search result -> domain film
            CreateMap<ImdbMovieSearchResult, Film>()
                .ForMember(f => f.Id, opt => opt.MapFrom(src => src.Id ?? string.Empty))
                .ForMember(f => f.Titel, opt => opt.MapFrom(src => src.Title ?? string.Empty))
                .ForMember(f => f.Beschreibung, opt => opt.MapFrom(src => src.Plot ?? string.Empty))
                .ForMember(f => f.Dauer, opt => opt.MapFrom(src => src.RuntimeSeconds))
                .ForMember(f => f.Genre, opt => opt.MapFrom(src =>
                    src.Genres != null && src.Genres.Count > 0 ? src.Genres[0] : string.Empty))
                .ForMember(f => f.Fsk, opt => opt.Ignore()); // set later via certificate data

            // Domain film -> DB entity
            CreateMap<Film, FilmEntity>()
                .ForMember(e => e.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(e => e.Titel, opt => opt.MapFrom(src => src.Titel))
                .ForMember(e => e.Beschreibung, opt => opt.MapFrom(src => src.Beschreibung))
                .ForMember(e => e.Dauer, opt => opt.MapFrom(src => src.Dauer))
                .ForMember(e => e.Fsk, opt => opt.MapFrom(src => src.Fsk))
                .ForMember(e => e.Genre, opt => opt.MapFrom(src => src.Genre));

            // DB entity -> domain film
            CreateMap<FilmEntity, Film>()
                .ForMember(f => f.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(f => f.Titel, opt => opt.MapFrom(src => src.Titel))
                .ForMember(f => f.Beschreibung, opt => opt.MapFrom(src => src.Beschreibung))
                .ForMember(f => f.Dauer, opt => opt.MapFrom(src => src.Dauer))
                .ForMember(f => f.Fsk, opt => opt.MapFrom(src => src.Fsk))
                .ForMember(f => f.Genre, opt => opt.MapFrom(src => src.Genre));

            // DB entity <-> shared DTO
            CreateMap<FilmEntity, FilmDto>().ReverseMap();

            // Note: there is a duplicate ImdbMovieSearchResult -> Film mapping below in the original file.
            // Keeping this line would override/merge configuration depending on AutoMapper version and setup.
            // Prefer a single mapping definition per source/target pair to avoid surprises.
            CreateMap<ImdbMovieSearchResult, Film>()
                .ForMember(f => f.Id, opt => opt.MapFrom(src => src.Id));
        }
    }
}
