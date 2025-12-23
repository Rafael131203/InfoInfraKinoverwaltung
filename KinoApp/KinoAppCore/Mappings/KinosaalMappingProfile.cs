using AutoMapper;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppCore.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping auditorium (Kinosaal) models between DTOs, domain models, and database entities.
    /// </summary>
    public class KinosaalMappingProfile : Profile
    {
        /// <summary>
        /// Initializes the mappings for auditorium models.
        /// </summary>
        public KinosaalMappingProfile()
        {
            CreateMap<CreateKinosaalDTO, Kinosaal>()
                .ForMember(x => x.Id, opt => opt.Ignore());

            CreateMap<Kinosaal, KinosaalEntity>()
                .ForMember(e => e.Id, opt => opt.MapFrom(src => src.Id ?? 0));

            CreateMap<KinosaalEntity, CreateKinosaalDTO>();

            CreateMap<CreateKinosaalDTO, KinosaalEntity>()
                .ForMember(e => e.Id, opt => opt.Ignore());

            CreateMap<KinosaalDTO, KinosaalEntity>().ReverseMap();
        }
    }
}
