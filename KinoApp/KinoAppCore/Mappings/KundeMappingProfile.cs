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
    public class KundeProfile : Profile
    {
        public KundeProfile()
        {
            CreateMap<Kunde, FullKundeDTO>().ReverseMap();
            CreateMap<Kunde, GetKundeDTO>().ReverseMap();
            
        }
    }
}
