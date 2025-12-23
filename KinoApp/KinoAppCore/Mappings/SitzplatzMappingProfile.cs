using AutoMapper;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppCore.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping seat (Sitzplatz) models between DTOs, domain models, and database entities.
    /// </summary>
    public class SitzplatzMappingProfile : Profile
    {
        /// <summary>
        /// Initializes the mappings for seat models.
        /// </summary>
        public SitzplatzMappingProfile()
        {
            // DTO -> domain
            CreateMap<SitzplatzDTO, Sitzplatz>()
                .ForMember(s => s.Id, opt => opt.Ignore());

            // Domain -> EF entity
            CreateMap<Sitzplatz, SitzplatzEntity>()
                .ForMember(e => e.Id, opt => opt.MapFrom(src => src.Id ?? 0));

            // EF entity -> DTO
            CreateMap<SitzplatzEntity, SitzplatzDTO>();
        }
    }
}
