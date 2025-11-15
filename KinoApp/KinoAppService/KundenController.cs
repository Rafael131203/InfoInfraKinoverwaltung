using AutoMapper;
using KinoAppCore.Abstractions;
using KinoAppCore.Entities;
using KinoAppShared.DTOs;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace KinoAppService
{
    [Route("api/[controller]")]
    [ApiController]
    public class KundenController : ControllerBase
    {
        private readonly IKundeRepository _repo;
        private readonly IMapper _mapper;

        public KundenController(IKundeRepository repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        // GET api/<KundenController>/5
        [HttpGet("{id}")]
        public ActionResult<FullKundeDTO>  Get(int id)
        {
            var kunde = _repo.GetByIdAsync(id); // Kunde
            if (kunde == null) return NotFound();

            var kundeDTO = _mapper.Map<FullKundeDTO>(kunde);
            return Ok(kundeDTO);
        }


        [HttpGet]
        public ActionResult<IEnumerable<FullKundeDTO>> GetAll()
        {
            var kunden = _repo.GetAllAsync(); // IEnumerable<Kunde>
            var kundenDTOListe = _mapper.Map<IEnumerable<FullKundeDTO>>(kunden);
            return Ok(kundenDTOListe);
        }


        // POST api/<KundenController>
        [HttpPost]
        public void Post([FromBody] FullKundeDTO kunde)
        {
            var entity = _mapper.Map<Kunde>(kunde);
            _repo.AddAsync(entity);
        }


        // PUT api/<KundenController>/5
        [HttpPut("{id}")]
        public ActionResult<FullKundeDTO> Put([FromBody] FullKundeDTO kundeDTO)
        {
            if (kundeDTO == null)
                return BadRequest();

            var entity = _mapper.Map<Kunde>(kundeDTO);
            var updated = _repo.UpdateAsync(entity);

            if (updated == null)
                return NotFound();

            return Ok(_mapper.Map<FullKundeDTO>(updated));
        }


        // DELETE api/<KundenController>/5
        [HttpDelete("{id}")]
        public IActionResult Delete(FullKundeDTO kundeDTO)
        {
            var entity = _mapper.Map<Kunde>(kundeDTO);

            _repo.DeleteAsync(entity);

            return NoContent();
        }
    }
}
