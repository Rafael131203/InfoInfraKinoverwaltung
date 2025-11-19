using AutoMapper;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppShared.DTOs;
using KinoAppShared.DTOs.Authentication;
using KinoAppShared.DTOs.Kinosaal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Mappings
{
    public class KinosaalMappingProfile : Profile
    {
        public KinosaalMappingProfile()
        {
            // NMF-Kunde <-> DTO (kannst du so lassen wie du willst)
            CreateMap<KinosaalEntity, CreateKinosaalDTO>().ReverseMap();
        }

    }
}
