using AutoMapper;
using KinoAppDB.Entities;
using KinoAppShared.DTOs;
using System.Collections.Generic;

namespace KinoAppCore.Mappings
{
    public class TicketMappingProfile : Profile
    {
        public TicketMappingProfile()
        {
            // Entity -> DTO
            // Besonderheit: Entity hat EINE ID, DTO erwartet eine LISTE
            CreateMap<TicketEntity, BuyTicketDTO>()
                .ForMember(dest => dest.VorstellungId, opt => opt.MapFrom(src => src.VorstellungId))
                .ForMember(dest => dest.SitzplatzIds, opt => opt.MapFrom(src => new List<long> { src.SitzplatzId }));

            // DTO -> Entity (ReverseMap ist hier schwierig wegen der 1-zu-N Beziehung, 
            // daher lassen wir das ReverseMap() weg oder nutzen es nur für Einzelfälle)
        }
    }
}