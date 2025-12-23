using AutoMapper;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppCore.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping seat row (Sitzreihe) models between DTOs, domain models, and database entities.
    /// </summary>
    public class SitzreiheMappingProfile : Profile
    {
        /// <summary>
        /// Initializes the mappings for seat row models.
        /// </summary>
        public SitzreiheMappingProfile()
        {
            CreateMap<SitzreiheDTO, Sitzreihe>()
                .ForMember(r => r.Id, opt => opt.Ignore());

            CreateMap<Sitzreihe, SitzreiheEntity>()
                .ForMember(e => e.Id, opt => opt.MapFrom(src => src.Id ?? 0));

            CreateMap<SitzreiheEntity, SitzreiheDTO>();
        }
    }
}
