using AutoMapper;
using KinoAppCore.Components;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Imdb;

namespace KinoAppCore.Mappings
{
    public class FilmMappingProfile : Profile
    {
        public FilmMappingProfile()
        {

            // ImdbMovieSearchResult -> FilmEntity
            CreateMap<ImdbMovieSearchResult, FilmEntity>()
                .ForMember(e => e.Id,
                    opt => opt.MapFrom(src => src.Id)) // tt1234567
                .ForMember(e => e.Titel,
                    opt => opt.MapFrom(src => src.Title ?? string.Empty))
                .ForMember(e => e.Beschreibung,
                    opt => opt.MapFrom(src => src.Plot))
                // Dauer: just map the seconds
                .ForMember(e => e.Dauer,
                    opt => opt.MapFrom(src => src.RuntimeSeconds))
                // Genre: only the first genre
                .ForMember(e => e.Genre,
                    opt => opt.MapFrom(src =>
                        src.Genres != null && src.Genres.Count > 0
                            ? src.Genres[0]
                            : null))
                .ForMember(e => e.ImageURL,
                    opt => opt.MapFrom(src => src.PosterUrl))
                // FSK will be set after calling the certificates endpoint
                .ForMember(e => e.Fsk, opt => opt.Ignore());

            CreateMap<ImdbTitleApiDto, FilmEntity>()
                .ForMember(e => e.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(e => e.Titel, opt => opt.MapFrom(src => src.PrimaryTitle ?? string.Empty))
                .ForMember(e => e.Beschreibung, opt => opt.MapFrom(src => src.Plot))
                .ForMember(e => e.Dauer, opt => opt.MapFrom(src => src.RuntimeSeconds))
                .ForMember(e => e.Genre, opt => opt.MapFrom(src =>
                    src.Genres != null && src.Genres.Count > 0 ? src.Genres[0] : null))
                .ForMember(e => e.ImageURL,
                    opt => opt.MapFrom(src => src.PrimaryImage.Url))
                .ForMember(e => e.Fsk, opt => opt.Ignore());

            // From detailed IMDb movie (if you use it)
            CreateMap<ImdbMovieDetails, Film>()
                .ForMember(f => f.Titel,
                    opt => opt.MapFrom(src => src.Title))
                .ForMember(f => f.Beschreibung,
                    opt => opt.MapFrom(src => src.Plot))
                .ForMember(f => f.Dauer,
                    opt => opt.Ignore())
                .ForMember(f => f.Genre,
                    opt => opt.Ignore())
                .ForMember(f => f.Fsk,
                    opt => opt.Ignore())
                .ForMember(f => f.Id,
                    opt => opt.Ignore());

            // From search/list result → Film
            CreateMap<ImdbMovieSearchResult, Film>()
                .ForMember(f => f.Id,
                    opt => opt.MapFrom(src => src.Id ?? string.Empty))
                .ForMember(f => f.Titel,
                    opt => opt.MapFrom(src => src.Title ?? string.Empty))
                .ForMember(f => f.Beschreibung,
                    opt => opt.MapFrom(src => src.Plot ?? string.Empty))

                // Dauer: take the raw seconds from RuntimeSeconds
                .ForMember(f => f.Dauer,
                    opt => opt.MapFrom(src => src.RuntimeSeconds))

                // Genre: only the first genre, if any
                .ForMember(f => f.Genre,
                    opt => opt.MapFrom(src =>
                        src.Genres != null && src.Genres.Count > 0
                            ? src.Genres[0]
                            : string.Empty))

                // FSK will still be set later from certificates
                .ForMember(f => f.Fsk, opt => opt.Ignore());


            // Domain Film -> DB FilmEntity
            CreateMap<Film, FilmEntity>()
                .ForMember(e => e.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(e => e.Titel,
                    opt => opt.MapFrom(src => src.Titel))
                .ForMember(e => e.Beschreibung,
                    opt => opt.MapFrom(src => src.Beschreibung))
                .ForMember(e => e.Dauer,
                    opt => opt.MapFrom(src => src.Dauer))
                .ForMember(e => e.Fsk,
                    opt => opt.MapFrom(src => src.Fsk))
                .ForMember(e => e.Genre,
                    opt => opt.MapFrom(src => src.Genre));

            // FilmEntity -> Film
            CreateMap<FilmEntity, Film>()
                .ForMember(f => f.Id,
                    opt => opt.MapFrom(src => src.Id))
                .ForMember(f => f.Titel,
                    opt => opt.MapFrom(src => src.Titel))
                .ForMember(f => f.Beschreibung,
                    opt => opt.MapFrom(src => src.Beschreibung))
                .ForMember(f => f.Dauer,
                    opt => opt.MapFrom(src => src.Dauer))
                .ForMember(f => f.Fsk,
                    opt => opt.MapFrom(src => src.Fsk))
                .ForMember(f => f.Genre,
                    opt => opt.MapFrom(src => src.Genre));

            CreateMap<FilmEntity, FilmDto>().ReverseMap();


            CreateMap<ImdbMovieSearchResult, Film>()
                .ForMember(f => f.Id, opt => opt.MapFrom(src => src.Id)); // IMDb id

        }
    }
}
