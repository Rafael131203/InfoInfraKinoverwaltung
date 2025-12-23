using AutoMapper;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Authentication;

namespace KinoAppCore.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping user/customer models between domain objects, EF entities, and authentication DTOs.
    /// </summary>
    /// <remarks>
    /// Password hashing is handled outside of AutoMapper; therefore password fields are intentionally ignored
    /// where appropriate.
    /// </remarks>
    public class UserMappingProfile : Profile
    {
        /// <summary>
        /// Initializes the user mappings.
        /// </summary>
        public UserMappingProfile()
        {
            CreateMap<Kunde, UserEntity>();
            CreateMap<UserEntity, Kunde>();

            CreateMap<RegisterRequestDTO, UserEntity>()
                .ForMember(dest => dest.Passwort, opt => opt.Ignore());

            CreateMap<UserEntity, RegisterResponseDTO>();
        }
    }
}
