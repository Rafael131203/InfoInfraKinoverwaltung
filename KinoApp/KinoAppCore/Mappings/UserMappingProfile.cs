using AutoMapper;
using KinoAppCore.Entities;          // NMF Kunde
using KinoAppDB.Entities;            // EF KundeEntity
using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Authentication;
// using KinoAppShared.DTOs.Kinosaal; // Evtl. nicht gebraucht

namespace KinoAppCore.Mappings
{
    public class UserMappingProfile : Profile
    {
        public UserMappingProfile()
        {
            // 1) Domain <-> DTOs

            // Domain Kunde <-> FullKundeDTO
            CreateMap<Kunde, FullKundeDTO>().ReverseMap();

            // Domain Kunde <-> GetKundeDTO
            CreateMap<Kunde, GetKundeDTO>().ReverseMap();

            // 2) Domain <-> Entity

            // Kunde (domain) -> KundeEntity (EF)
            // WICHTIG: Keine Warenkorb-Mappings mehr!
            CreateMap<Kunde, UserEntity>();

            // KundeEntity (EF) -> Kunde (domain)
            CreateMap<UserEntity, Kunde>();

            // 3) Registration flow

            // RegisterRequestDTO -> KundeEntity
            CreateMap<RegisterRequestDTO, UserEntity>()
                .ForMember(dest => dest.Passwort, opt => opt.Ignore());

            // KundeEntity -> RegisterResponseDTO
            CreateMap<UserEntity, RegisterResponseDTO>();
        }
    }
}