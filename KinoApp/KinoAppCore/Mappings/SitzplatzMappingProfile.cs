using AutoMapper;
using KinoAppCore.Entities;       // Sitzplatz (NMF model)
using KinoAppDB.Entities;        // SitzplatzEntity (EF entity)
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppCore.Mappings
{
    public class SitzplatzMappingProfile : Profile
    {
        public SitzplatzMappingProfile()
        {
            // 1) DTO -> domain model (Sitzplatz)
            //    DTO likely has: Reihe, Nummer, Typ, etc.
            //    Id is DB-generated, so ignore it.
            CreateMap<CreateSitzplatzDTO, Sitzplatz>()
                .ForMember(s => s.Id, opt => opt.Ignore());

            // 2) Domain model -> EF entity
            CreateMap<Sitzplatz, SitzplatzEntity>()
                .ForMember(e => e.Id,
                    opt => opt.MapFrom(src => src.Id ?? 0));  // NMF Id may be nullable

            // 3) EF entity -> DTO (for reading back)
            CreateMap<SitzplatzEntity, CreateSitzplatzDTO>();
        }
    }
}
