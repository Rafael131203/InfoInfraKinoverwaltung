using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppCore.Entities;
using KinoAppCore.Services;
using KinoAppShared.DTOs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KinoAppService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KundenController : ControllerBase
    {
        private readonly IKundeRepository _repo;
        private readonly KundeService _service;
        private readonly IMapper _mapper;

        public KundenController(IKundeRepository repo, IMapper mapper, KundeService service)
        {
            _repo = repo;
            _mapper = mapper;
            _service = service;
        }

        // GET api/<KundenController>/5
        [HttpGet("{id}")]
        public ActionResult<FullKundeDTO>  GetKundeById(int id)
        {
            //var kunde = _repo.GetByIdAsync(id); // Kunde
            //if (kunde == null) return NotFound();

            //var kundeDTO = _mapper.Map<FullKundeDTO>(kunde);
            return Ok(_service.GetByIdAsync(id));
        }

        // GET api/<KundenController>/5
        [HttpGet("{Email}")]
        public ActionResult<FullKundeDTO> GetKundeByEmail(string email)
        {
            return Ok(_service.FindKundeByEmail(email));
        }


        [HttpGet]
        public ActionResult<IEnumerable<FullKundeDTO>> GetAll()
        {
            //var kunden = _repo.GetAllAsync(); // IEnumerable<Kunde>
            //var kundenDTOListe = _mapper.Map<IEnumerable<FullKundeDTO>>(kunden);
            return Ok(_service.GetAllAsync());
        }


        // POST api/<KundenController>
        [HttpPost]
        public async Task Post([FromBody] FullKundeDTO kunde)
        {
            var entity = _mapper.Map<Kunde>(kunde);
            await _service.AddAsync(entity);
        }


        // PUT api/<KundenController>/5
        [HttpPut("{id}")]
        public ActionResult<FullKundeDTO> Put([FromBody] FullKundeDTO kundeDTO)
        {
            if (kundeDTO == null)
                return BadRequest();

            var entity = _mapper.Map<Kunde>(kundeDTO);

            return Ok(_service.UpdateAsync(entity));
        }


        // DELETE api/<KundenController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(FullKundeDTO kundeDTO)
        {
            var entity = _mapper.Map<Kunde>(kundeDTO);

            _service.DeleteAsync(entity).Wait();

            return NoContent();
        }
    }
}
