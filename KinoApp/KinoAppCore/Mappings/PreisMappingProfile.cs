using AutoMapper;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppCore.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping price category assignments.
    /// </summary>
    public class PreisMappingProfile : Profile
    {
        /// <summary>
        /// Initializes the mappings for price models.
        /// </summary>
        public PreisMappingProfile()
        {
            CreateMap<SetPreisDTO, PreisZuKategorieEntity>().ReverseMap();
        }
    }
}
