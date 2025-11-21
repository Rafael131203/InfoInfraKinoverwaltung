using AutoMapper;
using KinoAppCore.Entities;       // Sitzreihe (NMF model)
using KinoAppDB.Entities;        // SitzreiheEntity (EF entity)
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppCore.Mappings
{
    public class SitzreiheMappingProfile : Profile
    {
        public SitzreiheMappingProfile()
        {
            // 1) DTO -> Domain (Sitzreihe)
            CreateMap<SitzreiheDTO, Sitzreihe>()
                .ForMember(r => r.Id, opt => opt.Ignore());    // DB-generated

            // 2) Domain -> EF Entity
            CreateMap<Sitzreihe, SitzreiheEntity>()
                .ForMember(e => e.Id,
                    opt => opt.MapFrom(src => src.Id ?? 0));  // safe conversion for nullable Id

            // 3) EF Entity -> DTO (reading back to frontend)
            CreateMap<SitzreiheEntity, SitzreiheDTO>();
        }
    }
}
