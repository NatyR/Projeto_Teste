using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Users.API.Common.Annotations.Validations;
using Users.API.Dto.Sistema;
using Users.API.Interfaces.Services;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Users.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SistemasController : ControllerBase
    {
        private readonly ISistemaService _sistemaService;
        public SistemasController(ISistemaService sistemaService)
        {
            _sistemaService = sistemaService;
        }
              

        [HttpPost]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] SistemaAddDto sistema)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newSistema = await _sistemaService.Add(sistema);
            return CreatedAtAction("Get", new { id = newSistema.Id }, sistema);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(int id, [FromBody] SistemaUpdateDto sistema)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != sistema.Id)
            {
                return BadRequest();
            }
            await _sistemaService.Update(sistema);
            return Ok(sistema);
        }

        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _sistemaService.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var sistema = await _sistemaService.Get(id);
            if (sistema == null)
            {
                return NotFound();
            }
            return Ok(sistema);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Get()
        {
            var sistemas = await _sistemaService.GetAll();
            return Ok(sistemas);
        }



    }
}
