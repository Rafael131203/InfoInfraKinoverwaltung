using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppCore.Entities;
using KinoAppShared.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KinoAppCore.Services
{
    public class KundeService : GenericService<Kunde>
    {
        private readonly IKundeRepository _repository;
        private readonly IMapper _mapper;
        public KundeService(IKundeRepository repository, IMapper mapper) : base (repository) 
        {
            this._repository = repository;
            this._mapper = mapper;
        }

        public async Task<FullKundeDTO?> FindKundeByEmail(string email)
        {
            var kunde = _repository.FindByEmailAsync(email); // Kunde
            //if (kunde == null) return NotFound();

            var kundeDTO = _mapper.Map<FullKundeDTO>(kunde);
            return kundeDTO;
        }
    }
}
