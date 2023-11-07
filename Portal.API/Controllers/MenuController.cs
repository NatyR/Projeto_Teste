using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Portal.API.Common.Annotations.Validations;
using Portal.API.Common.Dto;
using Portal.API.Dto.Menu;
using Portal.API.Entities;
using Portal.API.Interfaces.Repositories;
using Portal.API.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Portal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenuController : Controller
    {
        private readonly IMenuService _menuService;
        public MenuController(IMenuService menuService)
        {
            _menuService = menuService;
        }
        // GET: api/<ConfigurationController>
        [HttpGet("sistema/{id}")]
        [Authorize]
        public async Task<List<GroupMenuDto>> GetBySistema(long id)
        {
            return await _menuService.GetBySistemaId(id);
        }

        [HttpGet("grupos/{sistema_id}")]
        [Authorize]
        public async Task<List<MenuDto>> GetGroupsBySistema(long sistema_id)
        {
            return await _menuService.GetGroupsBySistemaId(sistema_id);
        }

        [HttpGet("user/{convenio_id?}")]
        [Authorize]
        public async Task<List<GroupMenuDto>> GetByUserAndConvenio(long? convenio_id)
        {
            long.TryParse(User.Claims.First(c => c.Type == "id").Value, out long user_id);
            long.TryParse(User.Claims.First(c => c.Type == "sistema").Value, out long sistema_id);
            long.TryParse(User.Claims.First(c => c.Type == ClaimTypes.Role).Value, out long profile_id);
            return await _menuService.GetByUserAndConvenio(user_id,sistema_id,profile_id,convenio_id);
        }

        [HttpPost]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Post([FromBody] MenuAddDto menu)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var newRecord = await _menuService.Create(menu);
            return CreatedAtAction("Get", new { id = newRecord.Id }, menu);
        }

        [HttpPut("{id}")]
        [Authorize]
        [ValidForm]
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponsePortal), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Put(long id, [FromBody] MenuUpdateDto menu)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != menu.Id)
            {
                return BadRequest();
            }

            await _menuService.Update(menu);
            return Ok(menu);
        }
       
        [HttpDelete("{id}")]
        [Authorize]
        public async Task<IActionResult> Delete(int id)
        {
            await _menuService.Delete(id);
            return NoContent();
        }

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> Get(int id)
        {
            var faq = await _menuService.Get(id);
            if (faq == null)
            {
                return NotFound();
            }
            return Ok(faq);
        }



        [HttpPost("paged")]
        [Authorize]
        [ProducesResponseType(typeof(List<MenuDto>), 200)]
        public async Task<ActionResult<ResponseDto<MenuDto>>> GetAllPaged([FromQuery] int limit, [FromQuery] int skip, [FromQuery] string order, [FromBody] MenuFilterDto filter)
        {
            var response = await _menuService.GetAllPaged(limit, skip, order, filter);
            return Ok(response);
        }
    }
}
