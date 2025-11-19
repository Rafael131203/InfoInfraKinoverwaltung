using AutoMapper;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Mappings
{
    public class SitzplatzMappingProfile : Profile
    {
        public SitzplatzMappingProfile()
        {
            // NMF-Kunde <-> DTO (kannst du so lassen wie du willst)
            CreateMap<SitzplatzEntity, CreateSitzplatzDTO>().ReverseMap();
        }
    }
}
