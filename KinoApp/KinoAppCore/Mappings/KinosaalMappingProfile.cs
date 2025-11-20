using AutoMapper;
using KinoAppCore.Entities;      // Kinosaal (NMF Domain)
using KinoAppDB.Entities;        // KinosaalEntity (EF)
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppCore.Mappings
{
    public class KinosaalMappingProfile : Profile
    {
        public KinosaalMappingProfile()
        {
            // DTO -> Domain
            CreateMap<CreateKinosaalDTO, Kinosaal>()
                .ForMember(x => x.Id, opt => opt.Ignore()); // Domain ID handled elsewhere

            // Domain -> Entity
            CreateMap<Kinosaal, KinosaalEntity>()
                .ForMember(e => e.Id, opt => opt.MapFrom(src => src.Id ?? 0));

            // Entity -> DTO (reading data)
            CreateMap<KinosaalEntity, CreateKinosaalDTO>();

            CreateMap<CreateKinosaalDTO, KinosaalEntity>()
                .ForMember(e => e.Id, opt => opt.Ignore());

        }
    }
}
