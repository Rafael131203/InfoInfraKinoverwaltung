using AutoMapper;
using KinoAppDB.Entities;
using KinoAppShared.DTOs;
using System.Collections.Generic;

namespace KinoAppCore.Mappings
{
    /// <summary>
    /// AutoMapper profile for mapping ticket purchases between persistence entities and DTOs.
    /// </summary>
    /// <remarks>
    /// A single <see cref="TicketEntity"/> represents one reserved seat, while <see cref="BuyTicketDTO"/>
    /// groups one or more seat IDs for a single purchase request.
    /// </remarks>
    public class TicketMappingProfile : Profile
    {
        /// <summary>
        /// Initializes the ticket mappings.
        /// </summary>
        public TicketMappingProfile()
        {
            CreateMap<TicketEntity, BuyTicketDTO>()
                .ForMember(dest => dest.VorstellungId, opt => opt.MapFrom(src => src.VorstellungId))
                .ForMember(dest => dest.SitzplatzIds, opt => opt.MapFrom(src => new List<long> { src.SitzplatzId }));
        }
    }
}
