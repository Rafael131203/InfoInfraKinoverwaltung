using AutoMapper;
using KinoAppCore.Entities;
using KinoAppShared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Mappings
{

    public class WarenkorbProfile : Profile
    {
        public WarenkorbProfile()
        {
            // DTO → konkrete Klasse
            CreateMap<WarenkorbDTO, Warenkorb>();

            // DTO → Interface (AutoMapper weiß sonst nicht, welche Klasse gemeint ist)
            CreateMap<WarenkorbDTO, IWarenkorb>()
                .ConstructUsing((dto, ctx) => ctx.Mapper.Map<Warenkorb>(dto));

        }
    }

}
