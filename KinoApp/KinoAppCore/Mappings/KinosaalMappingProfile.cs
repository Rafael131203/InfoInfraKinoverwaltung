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
            // 1) DTO -> Domain
            CreateMap<CreateKinosaalDTO, Kinosaal>()
                .ForMember(x => x.Id, opt => opt.Ignore()); // Domain ID handled elsewhere

            // 2) Domain -> Entity
            CreateMap<Kinosaal, KinosaalEntity>()
                .ForMember(e => e.Id, opt => opt.MapFrom(src => src.Id ?? 0));

            // 3) Entity -> DTO (reading data)
            CreateMap<KinosaalEntity, CreateKinosaalDTO>();
        }
    }
}
