using AutoMapper;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;
using KinoAppShared.DTOs.Vorstellung;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Mappings
{
    public class VorstellungMappingProfile : Profile
    {
        public VorstellungMappingProfile()
        {
            // DTO -> Domain
            CreateMap<CreateVorstellungDTO, VorstellungEntity>().ReverseMap();
            CreateMap<VorstellungDTO, VorstellungEntity>().ReverseMap();

        }
    }
}
