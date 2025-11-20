using AutoMapper;
using KinoAppCore.Entities;
using KinoAppDB.Entities;
using KinoAppShared.DTOs.Kinosaal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Mappings
{
    public class PreisMappingProfile : Profile
    {
        public PreisMappingProfile()
        {
            CreateMap<SetPreisDTO, SetPreisDTO>().ReverseMap();
        }
    }
}
