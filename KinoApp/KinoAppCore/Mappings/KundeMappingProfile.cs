using AutoMapper;
using KinoAppCore.Entities;
using KinoAppDB.Entities;         // <-- add this
using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Authentication;

namespace KinoAppCore.Mappings
{
    public class KundeMappingProfile : Profile
    {
        public KundeMappingProfile()
        {
            // NMF-Kunde <-> DTO (kannst du so lassen wie du willst)
            CreateMap<Kunde, FullKundeDTO>().ReverseMap()
                .ForMember(dest => dest.Warenkorb, opt => opt.MapFrom(src => src.Warenkorb));
            CreateMap<Kunde, GetKundeDTO>().ReverseMap();

            CreateMap<RegisterRequestDTO, KundeEntity>()
            .ForMember(dest => dest.Passwort, opt => opt.Ignore()); // we hash manually

            CreateMap<KundeEntity, RegisterResponseDTO>();

            // EF-Entity <-> FullKundeDTO
            CreateMap<KundeEntity, FullKundeDTO>()
                // Richtung Entity -> DTO: DTO.Warenkorb ignorieren
                .ForMember(dto => dto.Warenkorb, opt => opt.Ignore())
                .ReverseMap()
                // ab hier gilt die Config für DTO -> Entity:
                .ForMember(ent => ent.Warenkorb, opt => opt.Ignore())
                .ForMember(ent => ent.WarenkorbId, opt => opt.Ignore());

            // EF-Entity <-> GetKundeDTO
            CreateMap<KundeEntity, GetKundeDTO>().ReverseMap();
        }

    }
}
