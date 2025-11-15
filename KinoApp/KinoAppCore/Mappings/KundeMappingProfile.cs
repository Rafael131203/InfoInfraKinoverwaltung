using AutoMapper;
using KinoAppCore.Entities;
using KinoAppShared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KinoAppCore.Mappings
{
    public class KundeMappingProfile : Profile
    {
        public KundeMappingProfile()
        {
            CreateMap<Kunde, FullKundeDTO>().ReverseMap()
                .ForMember(dest => dest.Warenkorb, opt => opt.MapFrom(src => src.Warenkorb));
            CreateMap<Kunde, GetKundeDTO>().ReverseMap();
            
        }
    }
}
