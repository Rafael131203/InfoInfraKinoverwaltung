using AutoMapper;
using KinoAppCore.Entities;          // NMF Kunde
using KinoAppDB.Entities;            // EF KundeEntity
using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Authentication;
// using KinoAppShared.DTOs.Kinosaal; // Evtl. nicht gebraucht

namespace KinoAppCore.Mappings
{
    public class KundeMappingProfile : Profile
    {
        public KundeMappingProfile()
        {
            // 1) Domain <-> DTOs

            // Domain Kunde <-> FullKundeDTO
            CreateMap<Kunde, FullKundeDTO>().ReverseMap();

            // Domain Kunde <-> GetKundeDTO
            CreateMap<Kunde, GetKundeDTO>().ReverseMap();

            // 2) Domain <-> Entity

            // Kunde (domain) -> KundeEntity (EF)
            // WICHTIG: Keine Warenkorb-Mappings mehr!
            CreateMap<Kunde, KundeEntity>();

            // KundeEntity (EF) -> Kunde (domain)
            CreateMap<KundeEntity, Kunde>();

            // 3) Registration flow

            // RegisterRequestDTO -> KundeEntity
            CreateMap<RegisterRequestDTO, KundeEntity>()
                .ForMember(dest => dest.Passwort, opt => opt.Ignore());

            // KundeEntity -> RegisterResponseDTO
            CreateMap<KundeEntity, RegisterResponseDTO>();
        }
    }
}