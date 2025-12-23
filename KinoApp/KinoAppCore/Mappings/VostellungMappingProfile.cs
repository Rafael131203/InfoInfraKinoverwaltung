using AutoMapper;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Vorstellung;

namespace KinoAppCore.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping showings (Vorstellungen) between DTOs and database entities.
    /// </summary>
    public class VorstellungMappingProfile : Profile
    {
        /// <summary>
        /// Initializes the showing mappings.
        /// </summary>
        public VorstellungMappingProfile()
        {
            CreateMap<CreateVorstellungDTO, VorstellungEntity>().ReverseMap();
            CreateMap<VorstellungDTO, VorstellungEntity>().ReverseMap();
        }
    }
}
