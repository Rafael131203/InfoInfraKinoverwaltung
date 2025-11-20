using AutoMapper;
using KinoAppCore.Entities;          // Kunde (NMF / domain model)
using KinoAppDB.Entities;           // KundeEntity (EF entity)
using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Kinosaal;

namespace KinoAppCore.Mappings
{
    public class KundeMappingProfile : Profile
    {
        public KundeMappingProfile()
        {
            // 1) Domain <-> DTOs

            // Domain Kunde <-> FullKundeDTO
            // Warenkorb is explicitly mapped both ways.
            CreateMap<Kunde, FullKundeDTO>()
                .ForMember(dto => dto.Warenkorb,
                    opt => opt.MapFrom(src => src.Warenkorb))
                .ReverseMap()
                .ForMember(k => k.Warenkorb,
                    opt => opt.MapFrom(dto => dto.Warenkorb));

            // Domain Kunde <-> GetKundeDTO (no special handling needed)
            CreateMap<Kunde, GetKundeDTO>().ReverseMap();


            // 2) Domain <-> Entity

            // Kunde (domain) -> KundeEntity (EF)
            CreateMap<Kunde, KundeEntity>()
                .ForMember(ent => ent.Warenkorb,
                    opt => opt.MapFrom(src => src.Warenkorb))
                // FK is managed by EF / your own logic
                .ForMember(ent => ent.WarenkorbId,
                    opt => opt.Ignore());

            // KundeEntity (EF) -> Kunde (domain)
            CreateMap<KundeEntity, Kunde>()
                .ForMember(k => k.Warenkorb,
                    opt => opt.MapFrom(ent => ent.Warenkorb));


            // 3) Registration flow

            // RegisterRequestDTO -> KundeEntity (special case)
            // We keep this so you can continue hashing the password manually.
            CreateMap<RegisterRequestDTO, KundeEntity>()
                .ForMember(dest => dest.Passwort, opt => opt.Ignore());

            // KundeEntity -> RegisterResponseDTO
            CreateMap<KundeEntity, RegisterResponseDTO>();

            // NOTE:
            // We no longer need direct KundeEntity <-> FullKundeDTO / GetKundeDTO maps
            // for normal usage, because AutoMapper can now go:
            //
            //   FullKundeDTO -> Kunde -> KundeEntity
            //   GetKundeDTO  -> Kunde -> KundeEntity
            //
            // if you ever call _mapper.Map<KundeEntity>(fullKundeDto) or similar.
        }
    }
}
